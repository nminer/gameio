

using Newtonsoft.Json;
using server.mapObjects;
using System;
using System.Data.Entity.Core.Common.EntitySql;

namespace server.world
{
    class Storm
    {
        private static TimeSpan? NextStorm;

        private const int MAX_RAIN_AMOUNT = 200;
        private const int MIN_RAIN_AMOUNT = 20;

        private const int MAX_HOURS = 10;
        private const int MIN_HOURS = 2;
  
        public string skyColor = "#003323";

        private TimeSpan EndTime;

        private RandomCurve StormCurve;

        public bool Thundering = false;

        public bool Finished
        {
            get
            {
                TimeSpan worldTime = GameServer.GetWorldTime();
                return EndTime < worldTime && lastValue == 0.0;
            }
        }

        /// <summary>
        /// this counter is the x on the curve.
        /// this will be increased each time the storm is ticked.
        /// </summary>
        private double Counter = 0.0;

        private double CounterTick = 0.01;

        /// <summary>
        /// keep track of the current ramp up value.
        /// </summary>
        private double RampUp = 1.0;

        private double RampUpRate;
        private double RampDownRate;

        private double StartCurve = 0;

        private double lastValue = 0.0;

        private TimeSpan? NextStrike;

        public Storm()
        {
            Random rnd = new Random();
            int parts = rnd.Next(2, 5);
            int ranMax = rnd.Next(MIN_RAIN_AMOUNT + 20, MAX_RAIN_AMOUNT);
            //int ranMax = MAX_RAIN_AMOUNT;
            StormCurve = new RandomCurve(parts, MIN_RAIN_AMOUNT, ranMax, 15);
            TimeSpan worldTime = GameServer.GetWorldTime();          
            int hours = rnd.Next(MIN_HOURS, MAX_HOURS);
            int mins = rnd.Next(0, 60);
            TimeSpan stormRunTime = new TimeSpan(0, hours, mins, 0);
            EndTime = worldTime + stormRunTime;
            StartCurve = StormCurve.GetY(0.0);
            RampUpRate = rnd.NextDouble() * (1.0 - 0.002) + 0.002;
            RampDownRate = rnd.NextDouble() * (1.0 - 0.002) + 0.002;
            Thundering = Mods.Chance(0.5);
        }

        private bool timerCompleate()
        {
            TimeSpan worldTime = GameServer.GetWorldTime();
            return EndTime < worldTime;
        }

        public int GetStormAmount()
        {
            if (RampUp < StartCurve)
            {
                // ramp up.
                RampUp += RampUpRate;
                lastValue = RampUp;
                if (lastValue < 0)
                {
                    lastValue = 0;
                }
                return (int)lastValue;
            } else if (timerCompleate())
            {
                // ramp down.
                lastValue -= RampDownRate;
                if (lastValue < 0)
                {
                    lastValue = 0;
                }
                return (int)lastValue;
            } else
            {
                // return the curve.
                Counter += CounterTick;
                lastValue = StormCurve.GetY(Counter);
                if (lastValue < 0)
                {
                    lastValue = 0;
                }
                return (int)lastValue;
            }
        }

        public double GetLastAmount()
        {
            return lastValue;
        }

        public static Storm? GetNextStorm()
        {
            TimeSpan worldTime = GameServer.GetWorldTime();
            if (NextStorm is null)
            {
                Random rnd = new Random();
                int hours = rnd.Next(2, 60);
                //int hours = rnd.Next(0, 1);
                int mins = rnd.Next(0, 60);
                TimeSpan stormRunTime = new TimeSpan(0, hours, mins, 0);
                NextStorm = worldTime + stormRunTime;
            }
            if (NextStorm < worldTime)
            {
                NextStorm = null;
                return new Storm();                
            }
            return null;
        }

        public LightningStrike? GetLightning()
        {
            if (Finished || !Thundering) return null;
            TimeSpan worldTime = GameServer.GetWorldTime();
            if (NextStrike is null)
            {
                Random rnd = new Random();
                //int hours = rnd.Next(2, 60);
                int hours = 0;
                if (lastValue < 100)
                {
                    hours = rnd.Next(0, 1);
                }
                int mins = rnd.Next(10, 40);
                if (lastValue > 150)
                {
                    mins -= rnd.Next(10, 20);
                    if (mins < 0) { mins = 0; }
                }
                    TimeSpan stormRunTime = new TimeSpan(0, hours, mins, 0);
                NextStrike = worldTime + stormRunTime;
            }
            if (NextStrike < worldTime)
            {
                NextStrike = null;
                double volume = Mods.DoubleBetween(0.1, 1);
                string[] thunderlist = {
                    "sounds/login/thunder.wav",
                    "sounds/outside/thunder1.mp3",
                    "sounds/outside/thunder2.mp3",
                    "sounds/outside/thunder4.mp3",
                    "sounds/outside/thunder5.mp3",
                    "sounds/outside/thunder3rolling.mp3",
                    "sounds/outside/thunder5rolling.mp3",
                };
                FullMapSoundEffect thunder = new FullMapSoundEffect(thunderlist[Mods.IntBetween(0,6)], false, volume);
                double flashAmount = 0;
                if (volume > 0.35)
                {
                    flashAmount = volume;
                }
                return new LightningStrike(thunder, flashAmount);
            }
            return null;
        }

    }

    class LightningStrike
    {
        /// <summary>
        /// sound file for thunder.
        /// </summary>
        public FullMapSoundEffect Thunder;


        /// <summary>
        /// amount of light 0.0 to 1.0.
        /// </summary>
        public double FlashAmount = 0.8;

        /// <summary>
        /// true if flash is set.
        /// </summary>
        public bool HasFlash
        {
            get
            {
                return FlashAmount > 0;
            }
        }
        
        public LightningStrike(FullMapSoundEffect thunderSound, double flashAmount = 0.0)
        {
            FlashAmount = flashAmount;
            Thunder = thunderSound;
        }

        public string getJasonString()
        {
            return JsonConvert.SerializeObject(new { amount = FlashAmount });
        }
    }
}
