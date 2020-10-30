using System;

namespace WebApi.Models
{
    public class VmState
    {
        public int VmId { get; set; }

        public string Hypervisor { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public int? PercentComplete { get; set; }

        public DateTime? StatusUpdated { get; set; }

        public DateTime? LastBackup { get; set; }
    }
}
