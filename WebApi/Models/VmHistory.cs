using System;
using System.Collections.Generic;

namespace WebApi.Models
{
    public class VmHistory
    {
        public string Hypervisor { get; set; }

        public List<HistoryRecord> HistoryRecords { get; set; }
    }

    public class HistoryRecord
    {
        public DateTime BackupDateStart { get; set; }

        public DateTime? BackupDateEnd { get; set; }

        public bool Success { get; set; }

        public string LastKnownStatus { get; set; }

        public string ExportedToFolder { get; set; }

        public string ArchivedToFile { get; set; }
    }
}
