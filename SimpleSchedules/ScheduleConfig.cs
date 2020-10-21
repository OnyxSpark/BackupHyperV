using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleSchedules
{
    public class ScheduleConfig : IEquatable<ScheduleConfig>
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

        public bool Equals(ScheduleConfig other)
        {
            if (DaysOfWeek == null && other.DaysOfWeek != null)
                return false;

            if (DaysOfWeek != null && other.DaysOfWeek == null)
                return false;

            if (DaysOfWeek != null && other.DaysOfWeek != null
                && !Enumerable.SequenceEqual(DaysOfWeek, other.DaysOfWeek))
                return false;

            if (LaunchDays == null && other.LaunchDays != null)
                return false;

            if (LaunchDays != null && other.LaunchDays == null)
                return false;

            if (LaunchDays != null && other.LaunchDays != null
                && !Enumerable.SequenceEqual(LaunchDays, other.LaunchDays))
                return false;

            if (OccursOnceAt == other.OccursOnceAt
                && Type == other.Type
                && IntervalUnit == other.IntervalUnit
                && Interval == other.Interval
                && StartAt == other.StartAt
                && EndAt == other.EndAt
                && Description == other.Description
                && Enabled == other.Enabled)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(OccursOnceAt);
            hash.Add(Type);
            hash.Add(IntervalUnit);
            hash.Add(Interval);
            hash.Add(StartAt);
            hash.Add(EndAt);
            hash.Add(Description);
            hash.Add(Enabled);
            hash.Add(DaysOfWeek);
            hash.Add(LaunchDays);
            return hash.ToHashCode();
        }

        public static bool operator ==(ScheduleConfig left, ScheduleConfig right)
        {
            return EqualityComparer<ScheduleConfig>.Default.Equals(left, right);
        }

        public static bool operator !=(ScheduleConfig left, ScheduleConfig right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            var sc = obj as ScheduleConfig;

            if (sc == null)
                return false;

            return Equals(sc);
        }
    }
}
