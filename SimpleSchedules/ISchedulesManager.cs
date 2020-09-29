using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSchedules
{
    public delegate void ScheduleEventHandler(object sender, ScheduleEventArgs e);

    public interface ISchedulesManager
    {
        /// <summary>
        /// Reads array of schedules from IConfiguration, using default section name "SimpleSchedules"
        /// </summary>
        /// <param name="configuration">Standard .Net Core IConfiguration object</param>
        /// <returns>Array of schedules, which were loaded into manager</returns>
        Schedule[] LoadFrom(IConfiguration configuration);

        /// <summary>
        /// Reads array of schedules from IConfiguration, using custom section name
        /// </summary>
        /// <param name="configuration">Standard .Net Core IConfiguration object</param>
        /// <param name="section">Custom section name</param>
        /// <returns>Array of schedules, which were loaded into manager</returns>
        Schedule[] LoadFrom(IConfiguration configuration, string section);

        /// <summary>
        /// Reads array of schedules from bunch of ScheduleConfig objects. Useful when deserealizing from JSON.
        /// </summary>
        /// <param name="scheduleConfigs">Collection of filled ScheduleConfig objects</param>
        /// <returns>Array of schedules, which were loaded into manager</returns>
        Schedule[] LoadFrom(IEnumerable<ScheduleConfig> scheduleConfigs);

        /// <summary>
        /// Add schedule to the list. This schedule's events will fire on the next timer tick (1 sec)
        /// </summary>
        /// <param name="schedule">Object of types: DailySchedule, WeeklySchedule, MonthlySchedule</param>
        void AddSchedule(Schedule schedule);

        /// <summary>
        /// Add several schedules to the list. These schedules events will fire on the next timer tick (1 sec)
        /// </summary>
        /// <param name="schedules">Several objects of types: DailySchedule, WeeklySchedule, MonthlySchedule</param>
        void AddSchedules(params Schedule[] schedules);

        /// <summary>
        /// Remove schedule from the list. This schedule's events will not fire any more
        /// </summary>
        /// <param name="schedule">Object of one of the childs (DailySchedule, etc)</param>
        void RemoveSchedule(Schedule schedule);

        /// <summary>
        /// Start the timer
        /// </summary>
        void Start();

        /// <summary>
        /// Start the timer. Intended to run in StartAsync() of the HostedService
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stop the timer
        /// </summary>
        void Stop();

        /// <summary>
        /// Stop the timer. Intended to run in StopAsync() of the HostedService
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Disposes internal timer
        /// </summary>
        void Dispose();

        /// <summary>
        /// Event, that will fire on current schedules
        /// </summary>
        event ScheduleEventHandler EventOccurred;
    }
}
