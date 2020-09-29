namespace SimpleSchedules
{
    public class ScheduleConfig
    {
        // DailySchedule

        public string OccursOnceAt { get; set; }
        public string Type { get; set; }
        public string IntervalUnit { get; set; }
        public int Interval { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; } = true;

        // WeeklySchedule

        public string[] DaysOfWeek { get; set; }

        // MonthlySchedule

        public int[] LaunchDays { get; set; }
    }
}
