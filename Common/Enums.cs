using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackupJobStatus
    {
        Idle,
        Exporting,
        Archiving,
        Rotating,
        Completed,
        Canceled
    };
}
