using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleSchedules
{
    public class MonthlySchedule : DailySchedule
    {
        /// <summary>
        /// Array of days when this schedule is active. Integers from 1 to 31.
        /// </summary>
        public int[] LaunchDays { get { return days.ToArray(); } }

        private List<int> days;

        protected MonthlySchedule() { }

        private void InitMonthlySchedule(int[] launchDays)
        {
            days = ProcessArrayInput<int>(launchDays);
            CheckInput();
        }

        /// <summary>
        /// Sets a schedule, that will only fire event on active days, once a day, at the specified time
        /// </summary>
        /// <param name="launchDays">Array of days when this schedule is active. Integers from 1 to 31.</param>
        /// <param name="occursOnceAt">A time, at which event will fire</param>
        /// <param name="enabled">Flag, indicating that schedule is enabled. Disabled schedules wil not fire events</param>
        /// <param name="description">Optional description of a schedule</param>
        public MonthlySchedule(int[] launchDays, Time occursOnceAt, bool enabled = true,
              string description = null) : base(occursOnceAt, enabled, description)
        {
            InitMonthlySchedule(launchDays);
        }

        /// <summary>
        /// Sets a schedule, that will fire event on active days, through the specified interval within active period
        /// </summary>
        /// <param name="launchDays">Array of days when this schedule is active. Integers from 1 to 31.</param>
        /// <param name="intervalUnit">Unit of the interval</param>
        /// <param name="interval">Specifies the time interval for the event to occur</param>
        /// <param name="startAt">Starting point of active period, within which event will fire. If null, it will assume begin of the day (00:00:00)</param>
        /// <param name="endAt">Ending point of active period, within which event will fire. If null, it will assume end of the day (23:59:59)</param>
        /// <param name="enabled">Flag, indicating that schedule is enabled. Disabled schedules wil not fire events</param>
        /// <param name="description">Optional description of a schedule</param>
        public MonthlySchedule(int[] launchDays, DailyIntervalUnit intervalUnit, int interval, Time? startAt, Time? endAt,
                                bool enabled = true, string description = null)
                       : base(intervalUnit, interval, startAt, endAt, enabled, description)
        {
            InitMonthlySchedule(launchDays);
        }

        /// <summary>
        /// Intended to be used internally by ConfigurationLoader
        /// </summary>
        /// <param name="paramsObj">Filled object with init params</param>
        public MonthlySchedule(ScheduleParams paramsObj) : base(paramsObj)
        {
            InitMonthlySchedule(paramsObj.LaunchDays);
        }

        /// <summary>
        /// Returns next DateTime when this schedule need to fire event, relative to currentDate and current active days
        /// </summary>
        /// <param name="currentDate">Relative this value next date will return</param>
        /// <returns>DateTime when next event need to fire or null if it should not</returns>
        public override DateTime? GetNext(DateTime currentDate)
        {
            if (days.Count == 0) return null;

            if (Type == ScheduleType.Once)
            {
                bool alreadyFired = GetOccursOnceDateTime(currentDate) <= currentDate;
                return GetNextOnCondition(currentDate, !IsActiveDay(currentDate) || alreadyFired);
            }

            return GetNextOnCondition(currentDate, !IsActiveDay(currentDate));
        }

        private DateTime? GetNextOnCondition(DateTime currentDate, bool needNextDay)
        {
            if (needNextDay)
                return base.GetNext(GetNextDay(currentDate));
            else
                return base.GetNext(currentDate);
        }

        private void CheckInput()
        {
            foreach (int day in days)
            {
                if (day < 1 || day > 31)
                    throw new ArgumentOutOfRangeException("days", "day must be in range 1..31");
            }
        }

        private bool IsActiveDay(DateTime currentDate)
        {
            int idx = days.IndexOf(currentDate.Day);
            TimeSpan next = GetNextInterval(currentDate);

            if (idx == -1 || (idx > -1 && next > SpanEnd))
                return false;

            return true;
        }

        private int GetNextDayIndex(DateTime currentDate)
        {
            // at this point days list has unique items and already sorted asceding

            int idx = -1;

            for (int i = 0; i < days.Count; i++)
            {
                if (days[i] > currentDate.Day)
                {
                    idx = i;
                    break;
                }
            }

            return idx;
        }

        private DateTime GetFirstActiveDayOfNextMonth(DateTime currentDate)
        {
            int day = days[0];
            var tmp = currentDate.AddMonths(1);
            return new DateTime(tmp.Year, tmp.Month, day, SpanStart.Hours,
                            SpanStart.Minutes, SpanStart.Seconds);
        }

        private int[] CalcLastDayOfMonth(DateTime currentDate)
        {
            int lastDay = (int)DayOfMonth.LastDay;
            return days.Where(d => d == lastDay)
                       .Select(d =>
                       {
                           d = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                           return d;
                       })
                       .ToArray();
        }

        private DateTime GetNextDay(DateTime currentDate)
        {
            int dayIndex = -1;

            if (!IsActiveDay(currentDate))
            {
                dayIndex = GetNextDayIndex(currentDate);
                if (dayIndex > -1)
                {
                    int day = days[dayIndex];
                    int maxDays = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

                    // if month has less days then next requested day, jump to next month
                    if (day > maxDays)
                        return GetFirstActiveDayOfNextMonth(currentDate);

                    return new DateTime(currentDate.Year, currentDate.Month, day,
                                            SpanStart.Hours, SpanStart.Minutes, SpanStart.Seconds);
                }
            }

            // if this day is the last in array, then we pick first active day of next month

            if (dayIndex == -1 || dayIndex == days.Count - 1)
                return GetFirstActiveDayOfNextMonth(currentDate);

            // we are still in current month

            return new DateTime(currentDate.Year, currentDate.Month, days[dayIndex + 1],
                            SpanStart.Hours, SpanStart.Minutes, SpanStart.Seconds);
        }
    }
}
