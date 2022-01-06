using System;
using System.Diagnostics;
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
        private static string Command2;
        private static bool RepeatWhenExit;
        private static bool UseLogFile;
        private static bool UseConsole;
        private static int MaxErrorCount;
        private static int MinSecondsWorkTime;

        public static async Task Main(string[] args)
        {
            var host = Hosting;
            await host.StartAsync().ConfigureAwait(false);
            var logger = host.Services.GetService<ILoggerFactory>();

            logger.AddLog4Net();

            var printer = new Printer();
            if (UseLogFile)
                printer._Log = host.Services.GetService<ILogger<ProcessConsole>>();
            printer.UseConsole = UseConsole;

            var timer = Stopwatch.StartNew();
            var counter = 0;

            printer.Print($"Start worker: {Command}");
            if(RepeatWhenExit)
                while (true)
                {
                    timer.Restart();

                    var process = new ProcessConsole("CMD.exe", $"/C {Command}", printer);
                    process.Start();
                    process.WaitForExit();
                    if(process.WasCloseByUser)
                        break;

                    //счётчик числа коротких циклов
                    counter++;
                    var work_time = timer.Elapsed.TotalSeconds;
                    if (work_time > MinSecondsWorkTime)
                    {
                        counter = 0;
                        continue;
                    } 
                    timer.Stop();

                    // если число коротких циклов больше заданных и время работы команды было меньше заданного - запускаем резервную команду
                    if (process.ExitCode == 1 && !string.IsNullOrWhiteSpace(Command2) && work_time < MinSecondsWorkTime && counter > MaxErrorCount)
                    {
                        ProcessConsole process2 = new ProcessConsole("CMD.exe", $"/C {Command2}", printer);
                        process2.Start();
                        process2.WaitForExit();
                        if (process2.WasCloseByUser)
                            break;

                    }
                }
            else
            {
                var process = new ProcessConsole("CMD.exe", $"/C {Command}", printer);
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
            Command = settings.GetValue<string>("Command2");
            RepeatWhenExit = settings.GetValue<bool>("RepeatWhenExit");
            UseLogFile = settings.GetValue<bool>("UseLogFile");
            UseConsole = settings.GetValue<bool>("UseConsole");
            MaxErrorCount = settings.GetValue<int>("MaxErrorCount");
            MinSecondsWorkTime = settings.GetValue<int>("MinSecondsWorkTime");
        }

    }
}
