using BackupHyperV.Service.Models;
using System;
using System.Collections.Generic;

namespace BackupHyperV.Service.Interfaces
{
    public interface IProgressReporter : IDisposable
    {
        void SetReportFrequency(int frequencyMilliseconds);

        void SendReportsFor(IList<VirtualMachine> virtualMachines);

        void StartReporting();

        void StopReporting();
    }
}
