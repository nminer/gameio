

using System;

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

        public Storm()
        {
            Random rnd = new Random();
            int parts = rnd.Next(2, 5);
            StormCurve = new RandomCurve(parts, MIN_RAIN_AMOUNT, MAX_RAIN_AMOUNT, 7);
            TimeSpan worldTime = GameServer.GetWorldTime();          
            int hours = rnd.Next(MIN_HOURS, MAX_HOURS);
            int mins = rnd.Next(0, 60);
            TimeSpan stormRunTime = new TimeSpan(0, hours, mins, 0);
            EndTime = worldTime + stormRunTime;
            StartCurve = StormCurve.GetY(0.0);
            RampUpRate = rnd.NextDouble() * (1.0 - 0.002) + 0.002;
            RampDownRate = rnd.NextDouble() * (1.0 - 0.002) + 0.002;
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

        public static Storm? GetNextStorm()
        {
            TimeSpan worldTime = GameServer.GetWorldTime();
            if (NextStorm is null)
            {
                Random rnd = new Random();
                int hours = rnd.Next(2, 60);
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
    }
}
