using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackupHyperV.Service.Models
{
    public class BackupTask : IEquatable<BackupTask>
    {
        [JsonIgnore]
        public DateTime? LastExecuted { get; set; }

        [JsonIgnore]
        public BackupTaskStatus Status { get; set; }

        public int ParallelBackups { get; set; }

        public List<VirtualMachine> VirtualMachines { get; set; }

        public bool Equals(BackupTask other)
        {
            if (VirtualMachines == null && other.VirtualMachines != null)
                return false;

            if (VirtualMachines != null && other.VirtualMachines == null)
                return false;

            if (VirtualMachines != null && other.VirtualMachines != null
                && !Enumerable.SequenceEqual(VirtualMachines, other.VirtualMachines))
                return false;

            if (ParallelBackups == other.ParallelBackups)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ParallelBackups, VirtualMachines);
        }

        public static bool operator ==(BackupTask left, BackupTask right)
        {
            return EqualityComparer<BackupTask>.Default.Equals(left, right);
        }

        public static bool operator !=(BackupTask left, BackupTask right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            var bt = obj as BackupTask;

            if (bt == null)
                return false;

            return Equals(bt);
        }
    }
}
