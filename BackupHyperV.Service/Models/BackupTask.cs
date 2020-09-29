using System;
using System.Collections.Generic;

namespace BackupHyperV.Service.Models
{
    public class BackupTask
    {
        public DateTime? LastExecuted { get; set; }

        public BackupTaskStatus Status { get; set; }

        public int ParallelBackups { get; set; }

        public List<VirtualMachine> VirtualMachines { get; set; }
    }
}
