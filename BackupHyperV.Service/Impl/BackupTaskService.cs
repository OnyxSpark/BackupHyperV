using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimpleSchedules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/*
Work logic:
1. If central server was detected, try to download backup task from it.
2. Central server NOT detected, try to use local backup task.
3. No local backup task file was found? Try to query Hyper-V WMI API
   and create new BackupTask.json with all found virtual machines.
   Their schedules will be disabled and human must check this file
   before use.
*/

namespace BackupHyperV.Service.Impl
{
    public class BackupTaskService : IBackupTaskService
    {
        public event EventHandler OnBackupTaskChange;

        public BackupTask CurrentBackupTask
        {
            get
            {
                lock (lockBackupTask)
                {
                    return _currentBackupTask;
                }
            }
            private set
            {
                lock (lockBackupTask)
                {
                    _currentBackupTask = value;
                }
            }
        }

        private readonly ILogger<BackupTaskService> _logger;
        private readonly IConfiguration _config;
        private readonly ICentralServer _centralServer;

        private Timer timer;
        private int timerFrequency = 5 * 60 * 1000;    // default 5 min
        private bool disposed = false;
        private object lockBackupTask = new object();
        private BackupTask _currentBackupTask;

        public BackupTaskService(ILogger<BackupTaskService> logger
                               , IConfiguration config
                               , ICentralServer centralServer)
        {
            timer = new Timer(new TimerCallback(TimerProc), null, Timeout.Infinite, Timeout.Infinite);

            _logger = logger;
            _config = config;
            _centralServer = centralServer;

            CurrentBackupTask = GetBackupTask();

            // task was downloaded from central server
            if (_centralServer.PingSuccess)
                SaveBackupTaskToDisk();
        }

        private void TimerProc(object state)
        {
            var task = GetBackupTask();

            if (task != CurrentBackupTask)
            {
                CurrentBackupTask = task;
                SaveBackupTaskToDisk();
                OnBackupTaskChange?.Invoke(this, new EventArgs());
            }
        }

        private BackupTask GetBackupTask()
        {
            if (_centralServer.PingSuccess)
            {
                _logger.LogDebug("Reading backup task from central server.");
                return GetBackupTaskFromCentralServer();
            }
            else
            {
                _logger.LogDebug("Reading backup task from local file.");
                return GetBackupTaskFromLocalFile();
            }
        }

        private void SaveBackupTaskToDisk()
        {
            string backupTaskFile = GetPathToLocalBackupTaskFile();
            string json = JsonConvert.SerializeObject(CurrentBackupTask, Formatting.Indented);
            File.WriteAllText(backupTaskFile, json);

            _logger.LogDebug("New backup task was saved to file: \"{file}\"", backupTaskFile);
        }

        private BackupTask GetBackupTaskFromCentralServer()
        {
            string hypervisor = Util.GetCurrentServerFQDN();
            var result = _centralServer.GetBackupTask(hypervisor).Result;

            if (result.Success)
                return JsonConvert.DeserializeObject<BackupTask>(result.Data);

            return null;
        }

        private BackupTask GetBackupTaskFromLocalFile()
        {
            string backupTaskFile = GetPathToLocalBackupTaskFile();

            if (!File.Exists(backupTaskFile))
            {
                _logger.LogInformation("Could not find Backup Task file \"{file}\".", backupTaskFile);
                _logger.LogInformation("Will try to generate a new file.");

                CreateNewBackupTaskFile(backupTaskFile);
            }

            string json = File.ReadAllText(backupTaskFile);
            return JsonConvert.DeserializeObject<BackupTask>(json);
        }

        private string GetPathToLocalBackupTaskFile()
        {
            string path = _config.GetValue<string>("BackupTaskFile");

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("BackupTaskFile config option is null or empty.");

            return path;
        }

        private void CreateNewBackupTaskFile(string path)
        {
            var localVMs = Util.GetLocalVirtualMachines();

            if (localVMs.Count == 0)
                throw new Exception("Could not find any virtual machines. Cannot continue.");

            var bt = new BackupTask();
            bt.ParallelBackups = 1;
            bt.VirtualMachines = CreateDefaultVMs(localVMs);

            string json = JsonConvert.SerializeObject(bt, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private List<VirtualMachine> CreateDefaultVMs(List<string> vmNames)
        {
            var result = new List<VirtualMachine>();

            var disabledSchedule = new ScheduleConfig()
            {
                Type = "SimpleSchedules.DailySchedule",
                OccursOnceAt = "00:00",
                Enabled = false,
                Description = "This schedule is disabled. Check Backup Task properties and after that enable this schedule."
            };

            foreach (string name in vmNames)
            {
                var vm = new VirtualMachine()
                {
                    Name = name,
                    ExportPathTemplate = "C:\\Backup\\{HV_HOST_FULL}\\{HV_GUEST_FULL}\\Daily_{YEAR}_{MONTH}_{DAY}_{HOUR}_{MINUTE}",
                    ExportRotateDays = 2,
                    CreateArchive = true,
                    ArchivePathTemplate = "C:\\Backup\\{HV_HOST_FULL}\\{HV_GUEST_FULL}\\{HV_HOST_SHORT}_HV_VM_{HV_GUEST_FULL}_export_{YEAR}.{MONTH}.{DAY}.{HOUR}.{MINUTE}.zip",
                    ArchiveCompressionLevel = 1,
                    ArchiveRotateDays = 5,
                    SchedulesConfigs = new List<ScheduleConfig>() { disabledSchedule }
                };

                result.Add(vm);
            }

            return result;
        }

        public void SetUpdateFrequency(int frequencyMilliseconds)
        {
            timerFrequency = frequencyMilliseconds;
            timer.Change(timerFrequency, timerFrequency);

            _logger.LogDebug("{type} timer frequency changed to {msec} milliseconds.",
                        nameof(BackupTaskService), frequencyMilliseconds);
        }

        public void StartUpdating()
        {
            timer.Change(timerFrequency, timerFrequency);
        }

        public void StopUpdating()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                timer.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
