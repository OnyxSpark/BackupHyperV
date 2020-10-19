using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DB
{
    public class VirtualMachine
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        public Hypervisor Hypervisor { get; set; }

        public int HypervisorId { get; set; }

        public List<BackupHistory> HistoryRecords { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime? StatusUpdated { get; set; }

        public int? PercentComplete { get; set; }
    }
}
