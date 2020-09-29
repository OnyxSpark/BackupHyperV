using BackupHyperV.Service.Interfaces;
using BackupHyperV.Service.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace BackupHyperV.Service.Impl
{
    public class VmArchiver : IVmArchiver
    {
        private readonly ILogger<VmArchiver> _logger;

        public VmArchiver(ILogger<VmArchiver> logger)
        {
            _logger = logger;
        }

        public bool CreateArchive(VirtualMachine virtualMachine)
        {
            bool success = true;

            try
            {
                virtualMachine.ArchivePercentComplete = 0;

                CreateArchiveInternal(virtualMachine);

                virtualMachine.ArchivePercentComplete = 100;
            }
            catch (Exception e)
            {
                success = false;
                _logger.LogError(e, "Error occurred when creating archive of VM '{name}'", virtualMachine.Name);
            }

            return success;
        }

        private long GetFilesSize(IEnumerable<string> files)
        {
            long size = 0;

            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                size += fi.Length;
            }

            return size;
        }

        private void CreateArchiveInternal(VirtualMachine vm)
        {
            var files = Directory.EnumerateFiles(vm.ExportPath, "*", SearchOption.AllDirectories);

            long dirSize = GetFilesSize(files);
            long zipSize = 0;

            // retreive root folder name
            // S:\1\2\...\VmName\folder  ->  folder
            string topLevelDir = new DirectoryInfo(vm.ExportPath).Name;

            int bufSize = 1024 * 1024;  // read and write by 1 mb

            using (FileStream zipStream = new FileStream(vm.ArchivePath, FileMode.Create, FileAccess.Write))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (string file in files)
                {
                    // S:\1\2\...\VmName\file.txt -> VmName\file.txt
                    string currFile = file.Replace(vm.ExportPath, topLevelDir);
                    ZipArchiveEntry entry = archive.CreateEntry(currFile,
                                        (CompressionLevel)vm.ArchiveCompressionLevel);

                    using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                    using (BinaryWriter writer = new BinaryWriter(entry.Open()))
                    {
                        long currFilePosition = 0;
                        while (currFilePosition < reader.BaseStream.Length)
                        {
                            byte[] buf = reader.ReadBytes(bufSize);
                            writer.Write(buf);
                            writer.Flush();

                            currFilePosition += bufSize;
                            zipSize += bufSize;

                            vm.ArchivePercentComplete =
                                dirSize == 0
                                ? 0
                                : Convert.ToInt32(Math.Round(Convert.ToDouble((zipSize * 100) / dirSize)));
                        }
                    }
                }
            }
        }
    }
}
