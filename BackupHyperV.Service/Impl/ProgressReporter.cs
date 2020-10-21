using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BackupHyperV.Service.Impl
{
    public class ProgressReporter : IProgressReporter, IDisposable
    {
        private readonly ILogger<ProgressReporter> _logger;

        private Timer timer;
        private int timerFrequency = 5000;    // default 5 sec
        private bool disposed = false;
        private IList<VirtualMachine> monitored;

        public ProgressReporter(ILogger<ProgressReporter> logger)
        {
            _logger = logger;

            timer = new Timer(new TimerCallback(TimerProc), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerProc(object state)
        {
            if (monitored == null || monitored.Count == 0)
                return;

            foreach (var vm in monitored)
            {
                ReportToLog(vm);
                ReportToCentralServer(vm);
            }
        }

        private void ReportToLog(VirtualMachine vm)
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

        private void ReportToCentralServer(VirtualMachine vm)
        {
            // TODO: add reporting to central server
        }

        public void SendReportsFor(IList<VirtualMachine> virtualMachines)
        {
            monitored = virtualMachines;
        }

        public void SetReportFrequency(int frequencyMilliseconds)
        {
            timerFrequency = frequencyMilliseconds;
            timer.Change(0, timerFrequency);

            _logger.LogDebug("{type} timer frequency changed to {msec} milliseconds.",
                        nameof(BackupTaskService), frequencyMilliseconds);
        }

        public void StartReporting()
        {
            timer.Change(0, timerFrequency);
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
