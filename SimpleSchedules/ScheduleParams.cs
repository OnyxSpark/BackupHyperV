using System;

namespace SimpleSchedules
{
    public class ScheduleParams
    {
        // DailySchedule

        public Time? OccursOnceAt { get; set; }
        public DailyIntervalUnit? IntervalUnit { get; set; }
        public int? Interval { get; set; }
        public Time? StartAt { get; set; }
        public Time? EndAt { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; } = true;

        // WeeklySchedule

        public DayOfWeek[] DaysOfWeek { get; set; }

        // MonthlySchedule

        public int[] LaunchDays { get; set; }
    }
}
