using System.Diagnostics;

namespace server.mods
{
    /// <summary>
    /// This class is to speed up time to have a in game time.
    /// you get to set the amount of Milliseconds = one day.
    /// </summary>
    internal class GameDayTimer
    {

        /// <summary>
        ///  How many hours there are in a day
        /// </summary>
        public const int HOURSTODAYS = 24; 
        
        /// <summary>
        /// How many minutes there are in an hour
        /// </summary>
        public const int MINUTESTOTHEHOUR = 60;

        /// <summary>
        /// How many seconds there are in a minute
        /// </summary>
        public const int SECONDSTOTHEMINUTE = 60;

        /// <summary>
        /// the number of Milliseconds = one day, for in game time.
        /// </summary>
        public long DayLengthInMilliseconds;

        /// <summary>
        /// the number of Milliseconds for one in game hour
        /// </summary>
        public long HourLengthInMilliseconds;

        /// <summary>
        /// the number of Milliseconds for one in game minute
        /// </summary>
        public long MinuteLengthInMilliseconds;

        /// <summary>
        /// the Milliseconds in one game second.
        /// </summary>
        public long SecondsLengthInMilliseconds;

        /// <summary>
        /// how many in game hours the timer is starting at in Milliseconds.
        /// </summary>
        private long ModStartTimeHours;

        /// <summary>
        /// the amount of time the fame day timer has been running.
        /// this is in real time and then converted to game time in GetGameTime 
        /// function call.
        /// </summary>
        private Stopwatch dayTimer;

        /// <summary>
        /// takes in the amount of milliseconds = one in game day. 
        /// can take in the starting hour for the time. this should be in a range of 0-24.
        /// any < 0 amount will be a random starting hour.
        /// </summary>
        /// <param name="dayLengthInMillisecond"></param>
        /// <param name="startDayAtHour"></param>
        public GameDayTimer(long dayLengthInMillisecond, int startDayAtHour = -1) {
            DayLengthInMilliseconds = dayLengthInMillisecond;
            HourLengthInMilliseconds = DayLengthInMilliseconds / HOURSTODAYS;
            MinuteLengthInMilliseconds = HourLengthInMilliseconds / MINUTESTOTHEHOUR;
            SecondsLengthInMilliseconds = MinuteLengthInMilliseconds / SECONDSTOTHEMINUTE;
            if (startDayAtHour < 0)
            {
                Random rnd = new Random();
                startDayAtHour = rnd.Next(0, 23);
            }
            startDayAtHour = startDayAtHour % 24;
            ModStartTimeHours = startDayAtHour * HourLengthInMilliseconds;
            dayTimer = new Stopwatch();
            dayTimer.Start();
        }

        /// <summary>
        /// returns the in game time as a TimeSpan.
        /// the TimeSpan holds the in game days, hours, minutes and seconds that have passed.
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetGameTime()
        {
            // get the real time that has passed
            long realTimePassed = dayTimer.ElapsedMilliseconds;
            // add in the start time in hours
            realTimePassed += ModStartTimeHours;
            long days = realTimePassed / DayLengthInMilliseconds;
            realTimePassed -= days * DayLengthInMilliseconds;
            long hours = realTimePassed / HourLengthInMilliseconds;
            realTimePassed -= hours * HourLengthInMilliseconds;
            long minutes = realTimePassed / MinuteLengthInMilliseconds;
            realTimePassed -= minutes * MinuteLengthInMilliseconds;
            long seconds = realTimePassed / SecondsLengthInMilliseconds;
            TimeSpan gametime = new TimeSpan((int)days, (int)hours, (int)minutes, (int)seconds);
            return gametime;
        }

        public TimeSpan MillisecondsToGameTime(long milliseconds)
        {
            long seconds = milliseconds / SecondsLengthInMilliseconds;
            TimeSpan gametime = new TimeSpan(0, 0, 0, (int)seconds);
            return gametime;
        }

        /// <summary>
        /// takes in a game timespan and returns how many real milliseconds
        /// that game time would take.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long GameTimeSpanToMilliseconds(TimeSpan time)
        {
            return (long)(time.TotalSeconds * SecondsLengthInMilliseconds);
        }
    }
}
