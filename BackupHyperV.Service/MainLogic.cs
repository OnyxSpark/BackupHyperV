using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Common;
using Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private readonly IConfiguration _config;
        private readonly ILogger<MainLogic> _logger;
        private readonly ISchedulesManager _schManager;
        private readonly IVmExporter _vmExporter;
        private readonly IVmArchiver _vmArchiver;
        private readonly IBackupRemover _backupRemover;
        private readonly IProgressReporter _progressReporter;
        private readonly IBackupTaskService _backupTaskService;
        private readonly ICentralServer _centralServer;

        private BackupTask backupTask;
        private bool backupingNow;
        private ConcurrentQueue<VirtualMachine> vmsToBackup = new ConcurrentQueue<VirtualMachine>();

        public MainLogic(IConfiguration config
                       , ILogger<MainLogic> logger
                       , ISchedulesManager schManager
                       , IVmExporter vmExporter
                       , IVmArchiver vmArchiver
                       , IBackupRemover backupRemover
                       , IProgressReporter progressReporter
                       , IBackupTaskService backupTaskService
                       , ICentralServer centralServer)
        {
            _config = config;
            _logger = logger;
            _schManager = schManager;
            _vmArchiver = vmArchiver;
            _vmExporter = vmExporter;
            _backupRemover = backupRemover;
            _progressReporter = progressReporter;
            _backupTaskService = backupTaskService;
            _centralServer = centralServer;

            _schManager.EventOccurred += Schedules_EventOccurred;
            _backupTaskService.OnBackupTaskChange += BackupTaskChanged;

            LoadNewTask();

            if (_centralServer.PingSuccess)
                SendInfoToCentralServer();
        }

        private void SendInfoToCentralServer()
        {
            var hypervisor = new HttpPostHypervisor()
            {
                Hypervisor = Util.GetCurrentServerFQDN(),
                BackupTask = JsonConvert.SerializeObject(backupTask),
                VirtualMachines = Util.GetLocalVirtualMachines()
            };

            var result = _centralServer.UpdateHypervisor(hypervisor).Result;

            if (result.Success)
                _logger.LogInformation("Successfully sent hypervisor info to central server.");
            else
                _logger.LogError("Failed to sent hypervisor info to central server. Error was: {error}",
                                        result.Message);
        }

        private void BackupTaskChanged(object sender, EventArgs e)
        {
            LoadNewTask();
        }

        private void LoadNewTask()
        {
            backupTask = _backupTaskService.CurrentBackupTask;

            if (backupTask != null
             && backupTask.VirtualMachines != null
             && backupTask.VirtualMachines.Count > 0)
                _progressReporter.SendReportsFor(backupTask.VirtualMachines);

            LoadSchedulesFromBackupTask();

            _logger.LogInformation("New backup task was loaded.");
        }

        private void LoadSchedulesFromBackupTask()
        {
            CheckVirtualMachinesExist();
            DeleteCurrentSchedules();

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

        private void DeleteCurrentSchedules()
        {
            if (_schManager.Schedules != null && _schManager.Schedules.Count > 0)
                for (int i = _schManager.Schedules.Count - 1; i >= 0; i--)
                {
                    var sch = _schManager.Schedules[i];
                    _schManager.RemoveSchedule(sch);
                }
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
            vm.BackupStartDate = DateTime.Now;
            vm.BackupEndDate = null;

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
            vm.BackupEndDate = DateTime.Now;
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

        private void SetTimersIntervals()
        {
            int progressInterval = _config.GetValue<int>("ProgressReportIntervalSeconds");
            int refreshBackupTaskInterval = _config.GetValue<int>("RefreshBackupTaskIntervalSeconds");

            _logger.LogDebug("Setting report progress interval to {progress} seconds...", progressInterval);
            _progressReporter.SetReportFrequency(progressInterval * 1000);

            _logger.LogDebug("Setting refresh backup task interval to {refresh} seconds...", refreshBackupTaskInterval);
            _backupTaskService.SetUpdateFrequency(refreshBackupTaskInterval * 1000);
        }

        public Task StartAsync()
        {
            SetTimersIntervals();

            _progressReporter.StartReporting();
            _logger.LogDebug("{type} timer started.", nameof(IProgressReporter));

            _backupTaskService.StartUpdating();
            _logger.LogDebug("{type} timer started.", nameof(IBackupTaskService));

            var t = _schManager.StartAsync();
            _logger.LogDebug("{type} timer started.", nameof(ISchedulesManager));
            return t;
        }

        public Task StopAsync()
        {
            _progressReporter.StopReporting();
            _logger.LogDebug("{type} timer stopped.", nameof(IProgressReporter));

            _backupTaskService.StopUpdating();
            _logger.LogDebug("{type} timer stopped.", nameof(IBackupTaskService));

            var t = _schManager.StopAsync();
            _logger.LogDebug("{type} timer stopped.", nameof(ISchedulesManager));
            return t;
        }

        public void Dispose()
        {
            _schManager.Dispose();
            _progressReporter.Dispose();
            _backupTaskService.Dispose();
        }
    }
}
