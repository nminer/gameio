

using Newtonsoft.Json;
using server.mapObjects;
using System;
using System.Data.Entity.Core.Common.EntitySql;

namespace server.world
{
    class Storm
    {
        /// <summary>
        /// gets set to when the next storm will happen.
        /// gets set to null when a storm is happening.
        /// </summary>
        private static TimeSpan? NextStorm;

        /// <summary>
        /// the max amount of rain.
        /// </summary>
        private const int MAX_RAIN_AMOUNT = 200;

        /// <summary>
        /// the min amount of rain.
        /// </summary>
        private const int MIN_RAIN_AMOUNT = 20;

        /// <summary>
        /// max time a storm can run. this is in game time.
        /// </summary>
        private const int MAX_HOURS = 10;

        /// <summary>
        /// min amount of time a storm can run. this is in game time.
        /// </summary>
        private const int MIN_HOURS = 2;
  
        /// <summary>
        /// the color of the sky when a storm is happening.
        /// </summary>
        public string skyColor = "#003323";

        /// <summary>
        /// the world time that the storm should start ramping down at.
        /// </summary>
        private TimeSpan EndTime;

        /// <summary>
        /// the curve is used for the amount of rain over the time of the storm.
        /// </summary>
        private RandomCurve StormCurve;

        /// <summary>
        /// set to true if the storm is going to have thunder.
        /// </summary>
        public bool Thundering = false;

        /// <summary>
        /// returns true when the storm is all finished.
        /// this includes ramping down is done.
        /// </summary>
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

        /// <summary>
        /// amount we move through the random curve at.
        /// used to increase the counter.
        /// </summary>
        private double CounterTick = 0.01;

        /// <summary>
        /// keep track of the current ramp up value.
        /// </summary>
        private double RampUp = 1.0;

        /// <summary>
        /// the amount we ramp up at to start the storm.
        /// we ramp up till the StartCurve value.
        /// </summary>
        private double RampUpRate;

        /// <summary>
        /// the rate we ramp down at the end of the storm.
        /// </summary>
        private double RampDownRate;

        /// <summary>
        /// gets set to the first value of the random curve.
        /// this is the amount we ramp up to.
        /// </summary>
        private double StartCurve = 0;

        /// <summary>
        /// gets set to the last amount of rain that was set.
        /// </summary>
        private double lastValue = 0.0;

        /// <summary>
        /// time for when next lightning strike should happen.
        /// </summary>
        private TimeSpan? NextStrike;

        /// <summary>
        /// create a new storm with random amount of rain and thunder.
        /// </summary>
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
            RampUpRate = rnd.NextDouble() * (0.1 - 0.002) + 0.002;
            RampDownRate = rnd.NextDouble() * (0.1 - 0.002) + 0.002;
            Thundering = Mods.Chance(0.5);
        }

        /// <summary>
        /// returns true if storm should be done and start to ramp down.
        /// </summary>
        /// <returns></returns>
        private bool timerCompleate()
        {
            TimeSpan worldTime = GameServer.GetWorldTime();
            return EndTime < worldTime;
        }

        /// <summary>
        /// returns a new rain amount after updating the amount.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// return the last amount (of rain) that was set.
        /// </summary>
        /// <returns></returns>
        public double GetLastAmount()
        {
            return lastValue;
        }

        /// <summary>
        /// returns the next storm or null if time is not up.
        /// </summary>
        /// <returns></returns>
        public static Storm? GetNextStorm()
        {
            TimeSpan worldTime = GameServer.GetWorldTime();
            if (NextStorm is null)
            {
                // set the time for the next storm if one is not set yet.
                Random rnd = new Random();
                int hours = rnd.Next(2, 60);
                //int hours = rnd.Next(0, 1);
                int mins = rnd.Next(0, 60);
                TimeSpan stormRunTime = new TimeSpan(0, hours, mins, 0);
                NextStorm = worldTime + stormRunTime;
            }
            if (NextStorm < worldTime)
            {
                // reset the next storm timer and return a new storm.  
                NextStorm = null; 
                return new Storm();               
            }
            return null;
        }

        /// <summary>
        /// returns a LightningStrike if the storm has thunder
        /// and if the storm is not finished.
        /// else it will return null.
        /// </summary>
        /// <returns></returns>
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
                // set the timer for the next lightning strike.
                TimeSpan stormRunTime = new TimeSpan(0, hours, mins, 0);
                NextStrike = worldTime + stormRunTime;
            }
            if (NextStrike < worldTime)
            {
                NextStrike = null; // reset the timer.
                // make the lightning.
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
                    // only flash if volume is up enough.
                    flashAmount = volume;
                }
                return new LightningStrike(thunder, flashAmount);
            }
            return null;
        }

    }

    /// <summary>
    /// class for lighting strikes.
    /// holds the sound and flash for the lighting.
    /// </summary>
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
        
        /// <summary>
        /// create a new lightning strike.
        /// </summary>
        /// <param name="thunderSound"></param>
        /// <param name="flashAmount"></param>
        public LightningStrike(FullMapSoundEffect thunderSound, double flashAmount = 0.0)
        {
            FlashAmount = flashAmount;
            Thunder = thunderSound;
        }

        /// <summary>
        /// returns the json string with the amount of the flash.
        /// </summary>
        /// <returns></returns>
        public string getJasonString()
        {
            return JsonConvert.SerializeObject(new { amount = FlashAmount });
        }
    }
}
