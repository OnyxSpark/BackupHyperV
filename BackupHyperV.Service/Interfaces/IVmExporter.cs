using BackupHyperV.Service.Models;

namespace BackupHyperV.Service.Interfaces
{
    public interface IVmExporter
    {
        bool ExportVirtualSystem(VirtualMachine virtualMachine, SnapshotExport snapshotExport);
    }
}
