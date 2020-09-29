using BackupHyperV.Service.Models;

namespace BackupHyperV.Service.Interfaces
{
    public interface IBackupRemover
    {
        void RemoveBackups(VirtualMachine virtualMachine,
                    out int removedExportDirs, out int removedArchiveFiles);
    }
}
