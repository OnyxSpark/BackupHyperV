using System.Collections.Generic;

namespace Common.Models
{
    public class HttpPostHypervisor
    {
        public string Hypervisor { get; set; }

        public List<string> VirtualMachines { get; set; }

        public string BackupTask { get; set; }
    }
}
