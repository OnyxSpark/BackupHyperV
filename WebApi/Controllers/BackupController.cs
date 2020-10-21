using Common.Models;
using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public partial class BackupController : ControllerBase
    {
        private readonly BackupDbContext _context;
        private readonly ILogger<BackupController> _logger;

        public BackupController(BackupDbContext context
                              , ILogger<BackupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<ApiResult> UpdateHypervisor([FromBody] HttpPostHypervisor hyper)
        {
            try
            {
                await UpdateHypervisorAsync(hyper.Hypervisor, hyper.VirtualMachines, hyper.BackupTask);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ApiResult() { Success = false, Message = e.Message };
            }

            return new ApiResult() { Success = true };
        }

        [HttpPost]
        public async Task<ApiResult> SendBackupProgress([FromBody] HttpPostBackupProgress progress)
        {
            try
            {
                await SendBackupProgressAsync(progress);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ApiResult() { Success = false, Message = e.Message };
            }

            return new ApiResult() { Success = true };
        }

        [HttpGet]
        public async Task<ApiResult> GetBackupTask(string hypervisor)
        {
            string json = null;

            try
            {
                var hyperv = await FindHypervisorByName(hypervisor);
                if (hyperv != null)
                    json = hyperv.BackupTask;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ApiResult() { Success = false, Message = e.Message };
            }

            return new ApiResult() { Success = true, Data = json };
        }
    }
}
