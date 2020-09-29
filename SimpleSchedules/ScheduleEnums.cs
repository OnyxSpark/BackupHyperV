namespace SimpleSchedules
{
    public enum DailyIntervalUnit
    {
        Second,
        Minute,
        Hour
    }

    public enum DayOfMonth
    {
        LastDay = int.MaxValue
    }

    public enum ScheduleType
    {
        Once,
        Recurring
    }
}
