using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace SimpleSchedules
{
    internal interface IConfigurationLoader
    {
        Schedule[] LoadFrom(IConfiguration configuration);
        Schedule[] LoadFrom(IConfiguration configuration, string section);
        Schedule[] LoadFrom(IEnumerable<ScheduleConfig> scheduleConfigs);
    }
}