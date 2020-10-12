using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Microsoft.Extensions.Logging;
using SimpleSchedules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackupHyperV.Service
{
    public class MainLogic : IDisposable
    {
        private readonly ILogger<MainLogic> _logger;
        private readonly ISchedulesManager _schManager;
        private readonly IVmExporter _vmExporter;
        private readonly IVmArchiver _vmArchiver;
        private readonly IBackupRemover _backupRemover;
        private readonly IProgressReporter _progressReporter;
        private readonly IBackupTaskService _backupTaskService;

        private BackupTask backupTask;
        private bool backupingNow;
        private ConcurrentQueue<VirtualMachine> vmsToBackup = new ConcurrentQueue<VirtualMachine>();

        public MainLogic(ILogger<MainLogic> logger
                       , ISchedulesManager schManager
                       , IVmExporter vmExporter
                       , IVmArchiver vmArchiver
                       , IBackupRemover backupRemover
                       , IProgressReporter progressReporter
                       , IBackupTaskService backupTaskService)
        {
            _logger = logger;
            _schManager = schManager;
            _vmArchiver = vmArchiver;
            _vmExporter = vmExporter;
            _backupRemover = backupRemover;
            _progressReporter = progressReporter;
            _backupTaskService = backupTaskService;

            _schManager.EventOccurred += Schedules_EventOccurred;

            backupTask = _backupTaskService.GetBackupTask();
            _progressReporter.SendReportsFor(backupTask.VirtualMachines);

            LoadSchedulesFromBackupTask();
        }

        private void LoadSchedulesFromBackupTask()
        {
            CheckVirtualMachinesExist();

            foreach (var vm in backupTask.VirtualMachines)
                vm.LoadedSchedules = _schManager.LoadFrom(vm.SchedulesConfigs);
        }

        private void Schedules_EventOccurred(object sender, ScheduleEventArgs e)
        {
            foreach (var schedule in e.OccurredSchedules)
            {
                var vm = FindVirtualMachineBySchedule(schedule);
                if (vm != null)
                    vmsToBackup.Enqueue(vm);
            }

            if (vmsToBackup.Count > 0 && !backupingNow)
                DoBackup();
        }

        private void CheckVirtualMachinesExist()
        {
            if (backupTask == null
             || backupTask.VirtualMachines == null
             || backupTask.VirtualMachines.Count == 0)
                throw new ArgumentException("backupTask is null or does not contains Virtual Machines.");
        }

        private VirtualMachine FindVirtualMachineBySchedule(Schedule sch)
        {
            CheckVirtualMachinesExist();

            foreach (var vm in backupTask.VirtualMachines)
            {
                var found = vm.LoadedSchedules.Where(s => s == sch).SingleOrDefault();
                if (found != null)
                    return vm;
            }

            return null;
        }

        private void DoBackup()
        {
            backupingNow = true;
            backupTask.Status = BackupTaskStatus.Active;

            try
            {
                while (vmsToBackup.Count > 0)
                {
                    var vmsList = MoveFromQueueToList();
                    DoParallelBackups(vmsList);
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    _logger.LogError(ex, "Error occurred.");
                }
            }
            finally
            {
                backupingNow = false;
                backupTask.Status = BackupTaskStatus.Idle;
                backupTask.LastExecuted = DateTime.Now;
            }
        }

        private List<VirtualMachine> MoveFromQueueToList()
        {
            var vmsList = new List<VirtualMachine>();

            while (vmsToBackup.Count > 0)
            {
                if (vmsToBackup.TryDequeue(out VirtualMachine vm))
                    vmsList.Add(vm);
            }

            return vmsList;
        }

        private void DoParallelBackups(List<VirtualMachine> virtualMachines)
        {
            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = backupTask.ParallelBackups;

            var exceptionsQueue = new ConcurrentQueue<Exception>();

            Parallel.ForEach(virtualMachines, options, (vm) =>
            {
                try
                {
                    if (ExportVirtualMachine(vm))
                    {
                        if (CreateArchive(vm))
                        {
                            RemoveOldBackups(vm);
                        }
                    }
                }
                catch (Exception e)
                {
                    exceptionsQueue.Enqueue(e);
                }
            });

            if (exceptionsQueue.Count > 0)
                throw new AggregateException(exceptionsQueue);
        }

        private bool ExportVirtualMachine(VirtualMachine vm)
        {
            bool success = false;
            vm.Status = BackupJobStatus.Exporting;

            _logger.LogInformation("Start of export virtual machine '{name}'.", vm.Name);

            vm.CreateExportPathFromTemplate();
            success = _vmExporter.ExportVirtualSystem(vm, SnapshotExport.AllSnapshots);

            _logger.LogInformation("Export virtual machine '{name}' completed successfully. Files were placed here: {path}",
                                vm.Name, vm.ExportPath);

            vm.Status = BackupJobStatus.Idle;
            return success;
        }

        private bool CreateArchive(VirtualMachine vm)
        {
            bool success = true;

            if (!vm.CreateArchive) return success;

            _logger.LogInformation("Start of archiving virtual machine '{name}'.", vm.Name);

            vm.Status = BackupJobStatus.Archiving;

            vm.CreateArchivePathFromTemplate();
            success = _vmArchiver.CreateArchive(vm);

            if (success)
            {
                _logger.LogInformation("Virtual machine '{name}' archive created successfully. File was placed here: {path}",
                                    vm.Name, vm.ArchivePath);
            }

            vm.Status = BackupJobStatus.Idle;
            return success;
        }

        private void RemoveOldBackups(VirtualMachine vm)
        {
            if (vm.ExportRotateDays == 0 && vm.ArchiveRotateDays == 0)
                return;

            vm.Status = BackupJobStatus.Rotating;

            _logger.LogInformation("Start to remove old backups for virtual machine '{name}'. Export folders older {folders} day(s) and archive file(s) older {files} day(s) will be removed now.",
                vm.Name, vm.ExportRotateDays, vm.ArchiveRotateDays);

            _backupRemover.RemoveBackups(vm, out int deletedFolders, out int deletedFiles);

            _logger.LogInformation("Removing old backups completed successfully. Removed {folders} folder(s) and {files} file(s).",
                        deletedFolders, deletedFiles);

            vm.Status = BackupJobStatus.Idle;
        }

        public Task StartAsync()
        {
            _progressReporter.StartReporting();
            return _schManager.StartAsync();
        }

        public Task StopAsync()
        {
            _progressReporter.StopReporting();
            return _schManager.StopAsync();
        }

        public void Dispose()
        {
            _schManager.Dispose();
            _progressReporter.Dispose();
        }
    }
}
