using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleSchedules
{
    internal class ConfigurationLoader : IConfigurationLoader
    {
        private readonly string DEFAULT_SECTION = "SimpleSchedules";

        // used in ToTitleCase()
        private readonly CultureInfo cultureInfo;
        private readonly TextInfo textInfo;

        public ConfigurationLoader()
        {
            cultureInfo = CultureInfo.InvariantCulture;
            textInfo = cultureInfo.TextInfo;
        }

        public Schedule[] LoadFrom(IConfiguration configuration)
        {
            return LoadFrom(configuration, DEFAULT_SECTION);
        }

        public Schedule[] LoadFrom(IConfiguration configuration, string section)
        {
            var cfgList = BindConfigSection(configuration, section);
            return LoadFrom(cfgList);
        }

        public Schedule[] LoadFrom(IEnumerable<ScheduleConfig> scheduleConfigs)
        {
            var list = new List<Schedule>();

            foreach (var schConfig in scheduleConfigs)
            {
                var schType = FindType(schConfig.Type);

                if (schType == null)
                    throw new ArgumentException($"Could not find type \"{schConfig.Type}\", must be one of: SimpleSchedules.DailySchedule, SimpleSchedules.WeeklySchedule, SimpleSchedules.MonthlySchedule");

                var paramsObj = ConvertToScheduleParamsObject(schConfig);
                var sch = (Schedule)Activator.CreateInstance(schType, paramsObj);
                list.Add(sch);
            }

            return list.ToArray();
        }

        private Type FindType(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return type;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private List<ScheduleConfig> BindConfigSection(IConfiguration configuration, string section)
        {
            var schConfigs = new List<ScheduleConfig>();
            configuration.Bind(section, schConfigs);
            return schConfigs;
        }

        private DayOfWeek[] ConvertWeekDays(string[] DaysOfWeek)
        {
            List<DayOfWeek> weekDays = new List<DayOfWeek>();

            foreach (string day in DaysOfWeek)
            {
                var val = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), ToTitleCase(day));
                weekDays.Add(val);
            }

            return weekDays.ToArray();
        }

        private ScheduleParams ConvertToScheduleParamsObject(ScheduleConfig schConfig)
        {
            // ScheduleConfig is helper class, used to bind to section in IConfiguration
            // ScheduleParams holds checked, ready-to-use params in schedules constructors

            bool schModeOnce = !string.IsNullOrWhiteSpace(schConfig.OccursOnceAt);
            bool schModeRecurring = !string.IsNullOrWhiteSpace(schConfig.IntervalUnit);

            if (schModeOnce && schModeRecurring)
                throw new ArgumentException("You must choose only a single option, OccursOnceAt to fire single time of a day, or IntervalUnit for recurring event firing.");

            if (!schModeOnce && !schModeRecurring)
                throw new ArgumentException("You must choose a single option, OccursOnceAt or IntervalUnit (for recurring event firing).");

            var paramsObj = new ScheduleParams();

            if (schModeOnce)
            {
                paramsObj.OccursOnceAt = new Time(schConfig.OccursOnceAt);
            }
            else if (schModeRecurring)
            {
                paramsObj.Interval = schConfig.Interval;
                paramsObj.IntervalUnit = (DailyIntervalUnit)Enum.Parse(typeof(DailyIntervalUnit), ToTitleCase(schConfig.IntervalUnit));
                paramsObj.StartAt = string.IsNullOrWhiteSpace(schConfig.StartAt) ? (Time?)null : new Time(schConfig.StartAt);
                paramsObj.EndAt = string.IsNullOrWhiteSpace(schConfig.EndAt) ? (Time?)null : new Time(schConfig.EndAt);
            }

            paramsObj.Enabled = schConfig.Enabled;
            paramsObj.Description = schConfig.Description;

            if (schConfig.DaysOfWeek != null)
                paramsObj.DaysOfWeek = ConvertWeekDays(schConfig.DaysOfWeek);

            if (schConfig.LaunchDays != null)
                paramsObj.LaunchDays = schConfig.LaunchDays;

            return paramsObj;
        }

        private string ToTitleCase(string name)
        {
            return textInfo.ToTitleCase(name);
        }
    }
}
