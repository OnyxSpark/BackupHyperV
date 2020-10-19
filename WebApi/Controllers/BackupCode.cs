using Common.Models;
using DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                vm.Status = state.State;
                vm.PercentComplete = state.PercentComplete;
                vm.StatusUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
