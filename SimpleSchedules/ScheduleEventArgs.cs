namespace SimpleSchedules
{
    public class ScheduleEventArgs
    {
        /// <summary>
        /// Array of schedules that fired the event
        /// </summary>
        public Schedule[] OccurredSchedules { get; }

        public ScheduleEventArgs(Schedule[] schedules)
        {
            OccurredSchedules = schedules;
        }
    }
}