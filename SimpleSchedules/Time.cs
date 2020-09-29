using System;

namespace SimpleSchedules
{
    /// <summary>
    /// Purpose of this struct is to hold only time of a single day
    /// </summary>
    public struct Time
    {
        private DateTime CurrentValue;

        private void Init(int hour, int minute, int second)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException("hour", "hour must be in range 0..23");

            if (minute < 0 || minute > 59)
                throw new ArgumentOutOfRangeException("minute", "minute must be in range 0..59");

            if (second < 0 || second > 59)
                throw new ArgumentOutOfRangeException("second", "second must be in range 0..59");

            CurrentValue = new DateTime(1, 1, 1, hour, minute, second);
        }

        /// <summary>
        /// Standard TimeSpan struct offers to much power, we need to keep only time within a single day here
        /// </summary>
        /// <param name="hour">Hour must be in range 0..23</param>
        /// <param name="minute">Minute must be in range 0..59</param>
        /// <param name="second">Second must be in range 0..59</param>
        public Time(int hour, int minute, int second) : this()
        {
            Init(hour, minute, second);
        }

        /// <summary>
        /// Standard TimeSpan struct offers to much power, we need to keep only time within a single day here
        /// </summary>
        /// <param name="time">Time, for example "6:12:14" </param>
        public Time(string time) : this()
        {
            // will throw standard TimeSpan exceptions if time format is wrong
            var span = TimeSpan.Parse(time);
            Init(span.Hours, span.Minutes, span.Seconds);
        }

        /// <summary>
        /// Returns TimeSpan for Time object
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetCurrentValue()
        {
            return CurrentValue.TimeOfDay;
        }

        public static bool operator >(Time o1, Time o2)
        {
            return o1.GetCurrentValue() > o2.GetCurrentValue();
        }

        public static bool operator <(Time o1, Time o2)
        {
            return o1.GetCurrentValue() < o2.GetCurrentValue();
        }

        public static bool operator >=(Time o1, Time o2)
        {
            return o1.GetCurrentValue() >= o2.GetCurrentValue();
        }

        public static bool operator <=(Time o1, Time o2)
        {
            return o1.GetCurrentValue() <= o2.GetCurrentValue();
        }
    }
}
