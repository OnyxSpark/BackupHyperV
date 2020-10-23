using BackupHyperV.Service.Interfaces;
using Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BackupHyperV.Service.Impl
{
    public class CentralServer : ICentralServer
    {
        public bool PingSuccess { get { return pingSuccess; } }

        private readonly IConfiguration _config;
        private readonly ILogger<CentralServer> _logger;

        private readonly string UrlPing;
        private readonly string UrlUpdateHypervisor;
        private readonly string UrlSendBackupProgress;
        private readonly string UrlGetBackupTask;

        private readonly HttpClient client = new HttpClient();
        private bool pingSuccess;

        public CentralServer(IConfiguration config
                           , ILogger<CentralServer> logger)
        {
            _config = config;
            _logger = logger;

            string centralServer = _config.GetValue<string>("CentralServer");

            if (string.IsNullOrWhiteSpace(centralServer))
            {
                _logger.LogDebug("Config parameter \"CentralServer\" is not set. Will continue in standalone mode.");
                pingSuccess = false;
                return;
            }

            centralServer = centralServer.TrimEnd('/');

            UrlPing = $"{centralServer}/api/ping";
            UrlUpdateHypervisor = $"{centralServer}/api/UpdateHypervisor";
            UrlSendBackupProgress = $"{centralServer}/api/SendBackupProgress";
            UrlGetBackupTask = $"{centralServer}/api/GetBackupTask";

            pingSuccess = ServerPing().Result;
        }

        public async Task<bool> ServerPing()
        {
            var result = await client.GetAsync(UrlPing);
            return result.IsSuccessStatusCode;
        }

        public async Task<ApiResult> SendBackupProgress(HttpPostBackupProgress progress)
        {
            return await DoPostRequest(UrlSendBackupProgress, progress);
        }

        public async Task<ApiResult> UpdateHypervisor(HttpPostHypervisor hyper)
        {
            return await DoPostRequest(UrlUpdateHypervisor, hyper);
        }

        public async Task<ApiResult> GetBackupTask(string hypervisor)
        {
            if (string.IsNullOrWhiteSpace(hypervisor))
                throw new ArgumentException("Parameter hypervisor is null or empty.");

            string url = $"{UrlGetBackupTask}?hypervisor={hypervisor}";
            return await DoGetRequest(url);
        }

        private async Task<ApiResult> DoPostRequest(string url, object content)
        {
            string jsonRequest = JsonConvert.SerializeObject(content);
            HttpContent httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var msg = await client.PostAsync(url, httpContent);

            if (!msg.IsSuccessStatusCode)
            {
                return new ApiResult()
                {
                    Success = false,
                    Message = $"StatusCode: {(int)msg.StatusCode}, ReasonPhrase: {msg.ReasonPhrase}, Url: {url}"
                };
            }

            string jsonResponse = await msg.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResult>(jsonResponse);
        }

        private async Task<ApiResult> DoGetRequest(string url)
        {
            var msg = await client.GetAsync(url);

            if (!msg.IsSuccessStatusCode)
            {
                return new ApiResult()
                {
                    Success = false,
                    Message = $"StatusCode: {(int)msg.StatusCode}, ReasonPhrase: {msg.ReasonPhrase}, Url: {url}"
                };
            }

            string jsonResponse = await msg.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResult>(jsonResponse);
        }
    }
}
