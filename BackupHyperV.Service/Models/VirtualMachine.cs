using Newtonsoft.Json;
using SimpleSchedules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace BackupHyperV.Service.Models
{
    public class VirtualMachine
    {
        [JsonProperty("VmName")]
        public string Name { get; set; }

        [JsonProperty("BackupSchedules")]
        public List<ScheduleConfig> SchedulesConfigs { get; set; }

        public Schedule[] LoadedSchedules { get; internal set; }

        public string ExportPathTemplate { get; set; }

        public string ArchivePathTemplate { get; set; }

        public uint ExportRotateDays { get; set; }

        public uint ArchiveRotateDays { get; set; }

        public bool CreateArchive { get; set; }

        public int ArchiveCompressionLevel { get; set; }

        public BackupJobStatus Status
        {
            get { return (BackupJobStatus)_status; }
            set { Interlocked.Exchange(ref _status, (int)value); }
        }

        public string ExportPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_exportPath))
                    CreateExportPathFromTemplate();

                return _exportPath;
            }
        }

        public string ArchivePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_archivePath))
                    CreateArchivePathFromTemplate();

                return _archivePath;
            }
        }

        public int ExportPercentComplete
        {
            get { return _exportPercentComplete; }
            set { Interlocked.Exchange(ref _exportPercentComplete, value); }
        }

        public int ArchivePercentComplete
        {
            get { return _archivePercentComplete; }
            set { Interlocked.Exchange(ref _archivePercentComplete, value); }
        }

        private volatile int _status;
        private volatile int _exportPercentComplete;
        private volatile int _archivePercentComplete;

        private string _exportPath;
        private string _archivePath;

        /// <summary>
        /// Creates export folder name from template. This name will be used in export, archive and rotate phases.
        /// </summary>
        public void CreateExportPathFromTemplate()
        {
            _exportPath = ReplacePlaceholders(ExportPathTemplate);
        }

        /// <summary>
        /// Creates archive file name from template. This name will be used in archive and rotate phases.
        /// </summary>
        public void CreateArchivePathFromTemplate()
        {
            _archivePath = ReplacePlaceholders(ArchivePathTemplate);
        }

        private string ReplacePlaceholders(string template)
        {
            string s = null;
            DateTime dt = DateTime.Now;

            s = template.Replace("{HV_HOST_FULL}", GetCurrentServerFQDN());
            s = s.Replace("{HV_HOST_SHORT}", Environment.MachineName.ToLower());
            s = s.Replace("{HV_GUEST_FULL}", Name);
            s = s.Replace("{HV_GUEST_SHORT}", GetShortName(Name));
            s = s.Replace("{YEAR}", dt.Year.ToString("D4"));
            s = s.Replace("{MONTH}", dt.Month.ToString("D2"));
            s = s.Replace("{DAY}", dt.Day.ToString("D2"));
            s = s.Replace("{HOUR}", dt.Hour.ToString("D2"));
            s = s.Replace("{MINUTE}", dt.Minute.ToString("D2"));
            s = s.Replace("{SECOND}", dt.Second.ToString("D2"));

            return s.TrimEnd(Path.DirectorySeparatorChar);
        }

        private string GetShortName(string name)
        {
            // name.suffix => name
            return name.Split('.')[0];
        }

        private string GetDomainFQDN()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            return (properties != null && properties.DomainName != null) ? properties.DomainName : null;
        }

        private string GetCurrentServerFQDN()
        {
            string srv = Environment.MachineName.ToLower();
            string domain = GetDomainFQDN();

            if (string.IsNullOrWhiteSpace(domain))
                return srv;

            return $"{srv}.{domain}";
        }
    }
}
