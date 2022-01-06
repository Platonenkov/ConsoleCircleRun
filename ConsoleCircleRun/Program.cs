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

        public static async Task Main(string[] args)
        {
            var host = Hosting;
            await host.StartAsync().ConfigureAwait(false);
            var logger = host.Services.GetService<ILoggerFactory>();
            logger.AddLog4Net();

            var log = host.Services.GetService<ILogger<ProcessConsole>>();
            ProcessConsole process = new ProcessConsole("CMD.exe", $"/C {Command}",log);
            process.Start();
            process.WaitForExit();


            //var process = new Process { StartInfo = new ProcessStartInfo("CMD.exe",$"/C {Command}") { UseShellExecute = true, RedirectStandardOutput = true } };
            //process.OutputDataReceived += (Sender, Args) =>
            //{
            //    Console.WriteLine(Args.Data);
            //};

            //process.Start();
            //process.BeginOutputReadLine();
            //await process.WaitForExitAsync();


            //Console.WriteLine($"Result: {result}");
            //Console.WriteLine(Command??"No Command");
            //Console.WriteLine(LogFile??$"Log file path: {LogFile}");
            //Console.WriteLine(RepeatWhenExit);
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
        }

    }
}
