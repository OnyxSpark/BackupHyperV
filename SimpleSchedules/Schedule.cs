using System;
using System.Collections.Generic;

namespace SimpleSchedules
{
    public abstract class Schedule
    {
        /// <summary>
        /// When this schedule wa created
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Flag, indicating that schedule is enabled. Disabled schedules wil not fire events.
        /// </summary>
        public bool Enabled { get; protected set; }

        /// <summary>
        /// Optional description of a schedule
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Returns next DateTime value relative to currentDate when event should fire. If it returns null, it means that there is no date to fire on (maybe because schedule is disabled)
        /// </summary>
        /// <param name="currentDate">DateTime value, relative it a next date to fire event will calculate</param>
        /// <returns>DateTime when next date exists or null when no date can be calculated (maybe because schedule is disabled)</returns>
        public abstract DateTime? GetNext(DateTime currentDate);

        protected void Init(bool enabled, string description)
        {
            CreatedAt = DateTime.Now;
            Description = description;
            Enabled = enabled;
        }

        protected List<T> ProcessArrayInput<T>(T[] arr)
        {
            // remove possible duplicates
            var set = new HashSet<T>(arr);

            var list = new List<T>(set);
            list.Sort();

            return list;
        }
    }
}
