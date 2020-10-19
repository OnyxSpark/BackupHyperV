using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DB
{
    public class Hypervisor
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        public string BackupTask { get; set; }

        public List<VirtualMachine> VirtualMachines { get; set; }
    }
}
