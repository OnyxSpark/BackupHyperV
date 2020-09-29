using BackupHyperV.Service.Impl;
using BackupHyperV.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SimpleSchedules;
using System;

namespace BackupHyperV.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                             .ReadFrom.Configuration(config)
                             .CreateLogger();

            try
            {
                CreateHostBuilder(args, config).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config)
        {
            return new HostBuilder()
                       .UseSerilog()
                       .UseWindowsService()
                       .ConfigureServices((hostContext, services) =>
                       {
                           services.AddSingleton(typeof(IConfiguration), config);
                           services.AddSingleton<ISchedulesManager, SchedulesManager>();
                           services.AddSingleton<IVmExporter, VmExporter>();
                           services.AddSingleton<IVmArchiver, VmArchiver>();
                           services.AddSingleton<IBackupRemover, BackupRemover>();
                           services.AddSingleton<IProgressReporter, ProgressReporter>();
                           services.AddSingleton<MainLogic>();

                           services.AddHostedService<Worker>();
                       });
        }
    }
}
