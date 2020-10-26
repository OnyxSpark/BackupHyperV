using System;
using System.ComponentModel.DataAnnotations;

namespace DB
{
    public class BackupHistory
    {
        public int Id { get; set; }

        public DateTime BackupDateStart { get; set; }

        public DateTime? BackupDateEnd { get; set; }

        public VirtualMachine VirtualMachine { get; set; }

        public int? VirtualMachineId { get; set; }

        public bool Success { get; set; }

        [MaxLength(50)]
        public string LastKnownStatus { get; set; }

        [MaxLength(1000)]
        public string ExportedToFolder { get; set; }

        [MaxLength(1000)]
        public string ArchivedToFile { get; set; }
    }
}
