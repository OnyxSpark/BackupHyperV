using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class BackupState
    {
        public string VmName { get; set; }

        public DateTime Started { get; set; }

        public string State { get; set; }

        public int PercentComplete { get; set; }

        public string ExportedToFolder { get; set; }

        public string ArchivedToFile { get; set; }
    }

    public class HttpPostBackupProgress
    {
        public string Hypervisor { get; set; }

        public List<BackupState> BackupStates { get; set; }
    }
}
