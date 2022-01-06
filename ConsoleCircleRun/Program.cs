using System;
using System.Threading.Tasks;

using ConsoleCircleRun.Logger;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleCircleRun
{
    internal class Program
    {
        private static string Command;
        private static string LogFile;
        private static bool RepeatWhenExit;
        private static bool UseConsole;

        public static async Task Main(string[] args)
        {
            var host = Hosting;
            await host.StartAsync().ConfigureAwait(false);
            var logger = host.Services.GetService<ILoggerFactory>();

            logger.AddLog4Net();

            var printer = new Printer();
            if (!string.IsNullOrWhiteSpace(LogFile))
                printer._Log = host.Services.GetService<ILogger<ProcessConsole>>();
            printer.UseConsole = UseConsole;

            printer.Print($"Start worker: {Command}");
            if(RepeatWhenExit)
                while (true)
                {
                    ProcessConsole process = new ProcessConsole("CMD.exe", $"/C {Command}", printer);
                    process.Start();
                    process.WaitForExit();
                    if(process.WasCloseByUser)
                        break;
                }
            else
            {
                ProcessConsole process = new ProcessConsole("CMD.exe", $"/C {Command}", printer);
                process.Start();
                process.WaitForExit();
            }

            await host.StopAsync();
        }

        private static IHost __Hosting;

        public static IHost Hosting => __Hosting
            ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .ConfigureServices(ConfigureServices); // Добавляем дополнительные сервисы вручную в методе ниже

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddLogging(sp => sp.SetMinimumLevel(LogLevel.Information));

            var settings = host.Configuration.GetSection("Properties");
            Command = settings.GetValue<string>("Command");
            LogFile = settings.GetValue<string>("LogFile");
            RepeatWhenExit = settings.GetValue<bool>("RepeatWhenExit");
            UseConsole = settings.GetValue<bool>("UseConsole");
        }

    }
}
