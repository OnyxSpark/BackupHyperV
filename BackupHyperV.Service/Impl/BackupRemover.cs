using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BackupHyperV.Service.Impl
{
    public class BackupRemover : IBackupRemover
    {
        private readonly ILogger<BackupRemover> _logger;

        public BackupRemover(ILogger<BackupRemover> logger)
        {
            _logger = logger;
        }

        public void RemoveBackups(VirtualMachine virtualMachine,
                        out int removedExportDirs, out int removedArchiveFiles)
        {
            removedExportDirs = 0;
            removedArchiveFiles = 0;

            var timeMark = DateTime.Now;

            try
            {
                removedExportDirs = RemoveExportDirs(timeMark, virtualMachine);
                removedArchiveFiles = RemoveArchiveFiles(timeMark, virtualMachine);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while rotating backups for virtual machine '{name}'.",
                            virtualMachine.Name);
            }
        }

        private int RemoveExportDirs(DateTime timeMark, VirtualMachine virtualMachine)
        {
            int cntr = 0;

            var parentExportDir = new DirectoryInfo(Directory.GetParent(virtualMachine.ExportPath).FullName);
            var dirList = parentExportDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);

            foreach (var dir in dirList)
            {
                var span = timeMark - dir.LastWriteTime;

                if (virtualMachine.ExportRotateDays > 0 && span.TotalDays >= virtualMachine.ExportRotateDays)
                {
                    _logger.LogInformation("Deleting folder: '{folder}'", dir.FullName);
                    dir.Delete(true);
                    cntr++;
                }
            }

            return cntr;
        }

        private int RemoveArchiveFiles(DateTime timeMark, VirtualMachine virtualMachine)
        {
            int cntr = 0;

            var parentArchiveDir = new DirectoryInfo(Directory.GetParent(virtualMachine.ArchivePath).FullName);
            var fileList = parentArchiveDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly);

            foreach (var file in fileList)
            {
                var span = timeMark - file.LastWriteTime;

                if (virtualMachine.ArchiveRotateDays > 0 && span.TotalDays >= virtualMachine.ArchiveRotateDays)
                {
                    _logger.LogInformation("Deleting file: '{file}'", file.FullName);
                    file.Delete();
                    cntr++;
                }
            }

            return cntr;
        }
    }
}
