using Common;
using Common.Models;
using DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Controllers
{
    public partial class BackupController
    {
        private async Task<Hypervisor> FindHypervisorByName(string hypervisor)
        {
            return await _context.Hypervisors
                                 .Include(vm => vm.VirtualMachines)
                                 .FirstOrDefaultAsync(s => s.Name.ToLower() == hypervisor.ToLower());
        }

        private async Task UpdateHypervisorAsync(string hypervisor, List<string> virtualMachines,
                                                 string backupTask)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            var hyperV = await WriteHypervisorToDbAsync(hypervisor, backupTask);
            await WriteVirtualMachinesToDbAsync(hyperV, virtualMachines);

            await transaction.CommitAsync();
        }

        private async Task<Hypervisor> WriteHypervisorToDbAsync(string hypervisor, string backupTask)
        {
            var hyperV = await FindHypervisorByName(hypervisor);

            if (hyperV == null)
            {
                hyperV = new Hypervisor();
                hyperV.Name = hypervisor.ToLower();
                hyperV.BackupTask = backupTask;
                _context.Hypervisors.Add(hyperV);

                await _context.SaveChangesAsync();
            }

            return hyperV;
        }

        private async Task WriteVirtualMachinesToDbAsync(Hypervisor hyperV, List<string> virtualMachines)
        {
            if (virtualMachines == null || virtualMachines.Count == 0)
                return;

            if (hyperV.VirtualMachines != null && hyperV.VirtualMachines.Count > 0)
            {
                var machinesInDb = hyperV.VirtualMachines.Select(m => m.Name);
                virtualMachines = virtualMachines.Except(machinesInDb, StringComparer.OrdinalIgnoreCase)
                                                 .ToList();
            }

            foreach (var machine in virtualMachines)
            {
                var vm = new VirtualMachine();
                vm.Hypervisor = hyperV;
                vm.Name = machine;
                _context.VirtualMachines.Add(vm);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SendBackupProgressAsync(HttpPostBackupProgress progress)
        {
            var hyperV = await FindHypervisorByName(progress.Hypervisor);

            if (hyperV == null)
            {
                var vms = progress.BackupStates.Select(s => s.VmName).ToList();
                await UpdateHypervisorAsync(progress.Hypervisor, vms, null);
                hyperV = await FindHypervisorByName(progress.Hypervisor);
            }

            foreach (var state in progress.BackupStates)
            {
                var vm = hyperV.VirtualMachines.FirstOrDefault(v => v.Name.ToLower() == state.VmName.ToLower());

                if (vm == null)
                {
                    _logger.LogError("Could not find virtual machine with name \"{vm}\" for hypervisor \"{hv}\"",
                                            state.VmName, hyperV.Name);
                    return;
                }

                WriteCurrentStateToDB(vm, state);

                if (state.Status != BackupJobStatus.Idle)
                    await WriteHistoryAsync(vm, state);
            }

            await _context.SaveChangesAsync();
        }

        private void WriteCurrentStateToDB(VirtualMachine vm, BackupState bs)
        {
            vm.Status = bs.Status.ToString();
            vm.PercentComplete = bs.PercentComplete;
            vm.StatusUpdated = DateTime.Now;
            vm.LastBackup = bs.LastBackup;
        }

        private async Task WriteHistoryAsync(VirtualMachine vm, BackupState bs)
        {
            var history = await _context.History.FirstOrDefaultAsync(h =>
                                               h.VirtualMachine == vm
                                            && h.BackupDateStart == bs.BackupStartDate);

            if (history == null)
            {
                history = new BackupHistory();
                _context.History.Add(history);
            }

            history.VirtualMachine = vm;
            history.LastKnownStatus = bs.Status.ToString();
            history.ExportedToFolder = bs.ExportedToFolder;
            history.ArchivedToFile = bs.ArchivedToFile;

            if (bs.Status == BackupJobStatus.Completed)
                history.Success = true;
            else
                history.Success = false;

            if (bs.BackupStartDate.HasValue)
                history.BackupDateStart = bs.BackupStartDate.Value;

            history.BackupDateEnd = bs.BackupEndDate;
        }

        private async Task<string> GetVmStatesAsync()
        {
            var vmList = new List<VmState>();

            var hypers = await _context.Hypervisors.Include(h => h.VirtualMachines).ToListAsync();

            if (hypers == null || hypers.Count == 0)
                return null;

            foreach (var h in hypers)
            {
                foreach (var vm in h.VirtualMachines)
                {
                    var s = new VmState();

                    s.VmId = vm.Id;
                    s.Hypervisor = h.Name;
                    s.Name = vm.Name;
                    s.Status = vm.Status;
                    s.PercentComplete = vm.PercentComplete;
                    s.StatusUpdated = vm.StatusUpdated;
                    s.LastBackup = vm.LastBackup;

                    vmList.Add(s);
                }
            }

            return JsonConvert.SerializeObject(vmList);
        }

        private async Task<string> GetVmHistoryAsync(int vmid)
        {
            var history = new VmHistory();
            history.HistoryRecords = new List<HistoryRecord>();

            var records = await _context.History
                                        .Where(vm => vm.VirtualMachineId == vmid)
                                        .Include(v => v.VirtualMachine)
                                        .ThenInclude(h => h.Hypervisor)
                                        .ToListAsync();

            if (records == null || records.Count == 0)
                return null;

            foreach (var rec in records)
            {
                history.Hypervisor = rec.VirtualMachine.Hypervisor.Name;

                var h = new HistoryRecord();
                h.BackupDateStart = rec.BackupDateStart;
                h.BackupDateEnd = rec.BackupDateEnd;
                h.Success = rec.Success;
                h.LastKnownStatus = rec.LastKnownStatus;
                h.ExportedToFolder = rec.ExportedToFolder;
                h.ArchivedToFile = rec.ArchivedToFile;

                history.HistoryRecords.Add(h);
            }

            return JsonConvert.SerializeObject(history);
        }
    }
}
