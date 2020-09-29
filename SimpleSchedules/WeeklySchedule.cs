using System;
using System.Collections.Generic;

namespace SimpleSchedules
{
    public class WeeklySchedule : DailySchedule
    {
        /// <summary>
        /// Array of days when this schedule is active.
        /// </summary>
        public DayOfWeek[] LaunchDays { get { return days.ToArray(); } }

        private List<DayOfWeek> days;

        protected WeeklySchedule() { }

        private void InitWeeklySchedule(DayOfWeek[] launchDays)
        {
            days = ProcessArrayInput<DayOfWeek>(launchDays);
        }

        /// <summary>
        /// Sets a schedule, that will only fire event on active days, once a day, at the specified time
        /// </summary>
        /// <param name="launchDays">Array of days when this schedule is active.</param>
        /// <param name="occursOnceAt">A time, at which event will fire</param>
        /// <param name="enabled">Flag, indicating that schedule is enabled. Disabled schedules wil not fire events</param>
        /// <param name="description">Optional description of a schedule</param>
        public WeeklySchedule(DayOfWeek[] launchDays, Time occursOnceAt,
             bool enabled = true, string description = null) : base(occursOnceAt, enabled, description)
        {
            InitWeeklySchedule(launchDays);
        }

        /// <summary>
        /// Sets a schedule, that will fire event on active days, through the specified interval within active period
        /// </summary>
        /// <param name="launchDays">Array of days when this schedule is active.</param>
        /// <param name="intervalUnit">Unit of the interval</param>
        /// <param name="interval">Specifies the time interval for the event to occur</param>
        /// <param name="startAt">Starting point of active period, within which event will fire. If null, it will assume begin of the day (00:00:00)</param>
        /// <param name="endAt">Ending point of active period, within which event will fire. If null, it will assume end of the day (23:59:59)</param>
        /// <param name="enabled">Flag, indicating that schedule is enabled. Disabled schedules wil not fire events</param>
        /// <param name="description">Optional description of a schedule</param>
        public WeeklySchedule(DayOfWeek[] launchDays, DailyIntervalUnit intervalUnit, int interval, Time? startAt, Time? endAt,
                                bool enabled = true, string description = null)
                       : base(intervalUnit, interval, startAt, endAt, enabled, description)
        {
            InitWeeklySchedule(launchDays);
        }

        /// <summary>
        /// Intended to be used internally by ConfigurationLoader
        /// </summary>
        /// <param name="paramsObj">Filled object with init params</param>
        public WeeklySchedule(ScheduleParams paramsObj) : base(paramsObj)
        {
            InitWeeklySchedule(paramsObj.DaysOfWeek);
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

            TimeSpan next = GetNextInterval(currentDate);
            return GetNextOnCondition(currentDate, !IsActiveDay(currentDate) || next > SpanEnd);
        }

        private bool IsActiveDay(DateTime currentDate)
        {
            if (days.Contains(currentDate.DayOfWeek))
                return true;

            return false;
        }

        private DateTime? GetNextOnCondition(DateTime currentDate, bool needNextDay)
        {
            if (needNextDay)
                return base.GetNext(GetNextDay(currentDate));
            else
                return base.GetNext(currentDate);
        }

        private DateTime GetNextDay(DateTime currentDate)
        {
            int idx = -1;

            for (int i = 0; i < days.Count; i++)
            {
                if (days[i] > currentDate.DayOfWeek)
                {
                    idx = (int)days[i];
                    break;
                }
            }

            if (idx == -1) idx = (int)days[0];

            int daysToAdd = (idx - (int)currentDate.DayOfWeek + 7) % 7;
            var tmp = currentDate.AddDays(daysToAdd);

            return new DateTime(tmp.Year, tmp.Month, tmp.Day, SpanStart.Hours, SpanStart.Minutes, SpanStart.Seconds);
        }
    }
}
