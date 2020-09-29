using BackupHyperV.Service.Models;

namespace BackupHyperV.Service.Interfaces
{
    public interface IVmArchiver
    {
        bool CreateArchive(VirtualMachine virtualMachine);
    }
}
