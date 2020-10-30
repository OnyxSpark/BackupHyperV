using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Common;
using Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BackupHyperV.Service.Impl
{
    public class ProgressReporter : IProgressReporter, IDisposable
    {
        private readonly ILogger<ProgressReporter> _logger;
        private readonly ICentralServer _centralServer;

        private Timer timer;
        private int timerFrequency = 5000;    // default 5 sec
        private bool disposed = false;
        private IList<VirtualMachine> monitoredVMs;

        private HashSet<VirtualMachine> activeVMs = new HashSet<VirtualMachine>();

        public ProgressReporter(ILogger<ProgressReporter> logger
                              , ICentralServer centralServer)
        {
            _logger = logger;
            _centralServer = centralServer;

            timer = new Timer(new TimerCallback(TimerProc), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerProc(object state)
        {
            if (monitoredVMs == null || monitoredVMs.Count == 0)
                return;

            RefreshVMsStatuses();

            ReportToLocalLog();

            if (_centralServer.PingSuccess)
                ReportToCentralServer();
        }

        private void RefreshVMsStatuses()
        {
            foreach (var vm in monitoredVMs)
            {
                switch (vm.Status)
                {
                    case BackupJobStatus.Exporting:
                    case BackupJobStatus.Archiving:
                        activeVMs.Add(vm);
                        break;

                    case BackupJobStatus.Completed:
                        vm.Status = BackupJobStatus.Idle;
                        break;

                    default:
                        if (activeVMs.Contains(vm))
                        {
                            activeVMs.Remove(vm);
                            vm.Status = BackupJobStatus.Completed;
                            vm.LastBackup = DateTime.Now;
                        }
                        break;
                }
            }
        }

        private void ReportToLocalLog()
        {
            foreach (var vm in monitoredVMs)
            {
                switch (vm.Status)
                {
                    case BackupJobStatus.Exporting:
                        _logger.LogDebug("Export '{name}': {percent}% completed.", vm.Name, vm.ExportPercentComplete);
                        break;

                    case BackupJobStatus.Archiving:
                        _logger.LogDebug("Archive '{name}': {percent}% completed.", vm.Name, vm.ArchivePercentComplete);
                        break;
                }
            }
        }

        private void ReportToCentralServer()
        {
            var states = new List<BackupState>();

            foreach (var vm in monitoredVMs)
            {
                int percent = 0;

                switch (vm.Status)
                {
                    case BackupJobStatus.Exporting:
                        percent = vm.ExportPercentComplete;
                        break;

                    case BackupJobStatus.Archiving:
                        percent = vm.ArchivePercentComplete;
                        break;
                }

                var state = new BackupState()
                {
                    VmName = vm.Name,
                    BackupStartDate = vm.BackupStartDate,
                    BackupEndDate = vm.BackupEndDate,
                    Status = vm.Status,
                    PercentComplete = percent,
                    ExportedToFolder = vm.ExportPath,
                    ArchivedToFile = vm.ArchivePath,
                    LastBackup = vm.LastBackup
                };

                states.Add(state);
            }

            var progress = new HttpPostBackupProgress();
            progress.Hypervisor = Util.GetCurrentServerFQDN();
            progress.BackupStates = states;

            var result = _centralServer.SendBackupProgress(progress).Result;

            if (!result.Success)
                _logger.LogError("Error occurred while send backup progress to central server. Error was: {err}",
                    result.Message);
        }

        public void SendReportsFor(IList<VirtualMachine> virtualMachines)
        {
            monitoredVMs = virtualMachines;
        }

        public void SetReportFrequency(int frequencyMilliseconds)
        {
            timerFrequency = frequencyMilliseconds;
            timer.Change(timerFrequency, timerFrequency);

            _logger.LogDebug("{type} timer frequency changed to {msec} milliseconds.",
                        nameof(BackupTaskService), frequencyMilliseconds);
        }

        public void StartReporting()
        {
            timer.Change(timerFrequency, timerFrequency);
        }

        public void StopReporting()
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
