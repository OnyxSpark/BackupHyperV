using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackupHyperV.Service.Models
{
    public class BackupTask
    {
        [JsonIgnore]
        public DateTime? LastExecuted { get; set; }

        [JsonIgnore]
        public BackupTaskStatus Status { get; set; }

        public int ParallelBackups { get; set; }

        public List<VirtualMachine> VirtualMachines { get; set; }
    }
}
