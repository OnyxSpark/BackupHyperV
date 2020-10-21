using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace BackupHyperV.Service
{
    public class Worker : IHostedService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private readonly MainLogic _mainLogic;

        public Worker(ILogger<Worker> logger, MainLogic mainLogic)
        {
            if (!IsLocalAdmin())
                throw new Exception("Must have Administrator rights to run this program.");

            if (!WmiRoutines.IsFeatureInstalled("Hyper-V"))
                throw new Exception("Server role \"Hyper-V\" is not installed. Cannot continue.");

            _logger = logger;
            _mainLogic = mainLogic;
        }

        private bool IsLocalAdmin()
        {
            bool isElevated = false;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return isElevated;
        }

        public void Dispose()
        {
            _mainLogic.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _mainLogic.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var task = _mainLogic.StopAsync();
            _logger.LogInformation("Program stopped.");
            return task;
        }
    }
}
