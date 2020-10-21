using Common.Models;
using System.Threading.Tasks;

namespace BackupHyperV.Service.Interfaces
{
    public interface ICentralServer
    {
        bool PingSuccess { get; }

        Task<bool> ServerPing();

        Task<ApiResult> UpdateHypervisor(HttpPostHypervisor hyper);

        Task<ApiResult> SendBackupProgress(HttpPostBackupProgress progress);

        Task<ApiResult> GetBackupTask(string hypervisor);
    }
}
