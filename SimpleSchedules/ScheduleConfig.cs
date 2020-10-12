using Newtonsoft.Json;

namespace SimpleSchedules
{
    public class ScheduleConfig
    {
        // DailySchedule

        public string OccursOnceAt { get; set; }

        public string Type { get; set; }

        [JsonIgnore]
        public string IntervalUnit { get; set; }

        [JsonIgnore]
        public int Interval { get; set; }

        [JsonIgnore]
        public string StartAt { get; set; }

        [JsonIgnore]
        public string EndAt { get; set; }

        public string Description { get; set; }

        public bool Enabled { get; set; } = true;

        // WeeklySchedule

        [JsonIgnore]
        public string[] DaysOfWeek { get; set; }

        // MonthlySchedule

        [JsonIgnore]
        public int[] LaunchDays { get; set; }
    }
}
