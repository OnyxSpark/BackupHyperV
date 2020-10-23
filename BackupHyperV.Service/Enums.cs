namespace BackupHyperV.Service
{
    public enum BackupTaskStatus
    {
        Idle,
        Active
    };

    public enum BackupJobStatus
    {
        Idle,
        Exporting,
        Archiving,
        Rotating,
        Completed,
        Canceled
    };

    public enum JobState
    {
        New = 2,
        Starting = 3,
        Running = 4,
        Suspended = 5,
        ShuttingDown = 6,
        Completed = 7,
        Terminated = 8,
        Killed = 9,
        Exception = 10,
        CompletedWithWarnings = 32768
    }

    // https://docs.microsoft.com/en-us/windows/win32/hyperv_v2/msvm-virtualsystemexportsettingdata

    public enum SnapshotExport
    {
        /// <summary>
        /// All snapshots will be exported with the virtual machine.
        /// </summary>
        AllSnapshots,

        /// <summary>
        /// No snapshots will be exported with the virtual machine.
        /// </summary>
        NoSnapshots,

        /// <summary>
        /// The snapshots identified by the SnapshotVirtualSystem property will be exported with the virtual machine. The CopyVmStorage and CopyVmRuntimeInformation properties are ignored, storage and run-time information is exported with the virtual machine, and any VHD differencing disks will be merged into a new VHD.
        /// </summary>
        OneSnapshot,

        /// <summary>
        /// Added in Windows 10 and Windows Server 2016. The snapshot identified by the SnapshotVirtualSystem property will be exported for the purpose of backing up the VM. The exported configuration will use ID of the VM.
        /// </summary>
        OneSnapshotForBackup
    }
}
