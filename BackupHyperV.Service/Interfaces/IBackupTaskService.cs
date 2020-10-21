using BackupHyperV.Service.Models;
using System;

namespace BackupHyperV.Service.Interfaces
{
    public interface IBackupTaskService : IDisposable
    {
        event EventHandler OnBackupTaskChange;

        BackupTask CurrentBackupTask { get; }

        void SetUpdateFrequency(int frequencyMilliseconds);

        void StartUpdating();

        void StopUpdating();
    }
}
