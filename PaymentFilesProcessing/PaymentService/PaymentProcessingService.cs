using Microsoft.Extensions.Hosting;
using System;
using PaymentServiceLibrary.Concrete;
using PaymentServiceLibrary.Concrete.Factory;
using PaymentServiceLibrary.Concrete.Process;
using PaymentServiceLibrary.Interfaces;
using PaymentServiceLibrary.Interfaces.Process;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System.IO;
//using System.Threading;
using System.Timers;

namespace PaymentService
{
    public class PaymentProcessingService
    {
        private readonly IHost _host;
        private readonly IFileWatcher _fileWatcher;
        private Timer _timer;
        private readonly string _inputFolderPath;
        private readonly string _outputFolderPath;

        public PaymentProcessingService()
        {
            _host = CreateHostBuilder(Array.Empty<string>()).Build();
            _inputFolderPath = _host.Services.GetService<IConfiguration>().GetValue<string>("Serilog:InputFolderPath");
            _outputFolderPath = _host.Services.GetService<IConfiguration>().GetValue<string>("Serilog:OutputFolderPath");
            _fileWatcher = _host.Services.GetService<IFileWatcher>();
        }

        public void Start()
        {
            // Check for InputFolderPath and OutputFolderPath

            if (string.IsNullOrEmpty(_inputFolderPath) || string.IsNullOrEmpty(_outputFolderPath))
            {
                Log.Logger.Error("InputFolderPath or OutputFolderPath is not available or empty. The service cannot start.");
                throw new Exception("InputFolderPath or OutputFolderPath is not available or empty. The service cannot start.");
            }

            var now = DateTime.Now;
            var midnight = now.AddDays(1).Date;
            var timeToMidnight = midnight - now;
            _timer = new Timer();
            _timer.Elapsed += OnTimer;
            _timer.Interval = timeToMidnight.TotalMilliseconds;
            _timer.AutoReset = true;
            _timer.Start();

            _fileWatcher.StartWatching();
            _host.Start();
        }

        public void Stop()
        {
            _timer.Dispose();
            _fileWatcher.StopWatching();
            _host.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Restart()
        {
            _timer.Stop(); // stop the timer during restart
            _host.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            _fileWatcher.StopWatching(); // stop watching the file system during restart
            _host.Start();
            _fileWatcher.StartWatching(); // start watching the file system again after the restart
            _timer.Start(); // start the timer again after the restart
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            var parsedFiles = _fileWatcher.ParsedFilesCount;
            var parsedLines = _fileWatcher.ParsedLinesCount;
            var foundErrors = _fileWatcher.FoundErrorsCount;
            var invalidFiles = _fileWatcher.InvalidFiles;

            WriteMetaLog(parsedFiles, parsedLines, foundErrors, invalidFiles);
        }

        private void WriteMetaLog(int parsedFiles, int parsedLines, int foundErrors, string[] invalidFiles)
        {
            var currentDateTime = DateTime.Now;
            var subFolderPath = Path.Combine(_outputFolderPath, currentDateTime.ToString("MM-dd-yyyy"));

            if (!Directory.Exists(subFolderPath))
            {
                Directory.CreateDirectory(subFolderPath);
            }

            var metaLogPath = Path.Combine(subFolderPath, "meta.log");

            using (var writer = new StreamWriter(metaLogPath))
            {
                writer.WriteLine($"parsed_files: {parsedFiles}");
                writer.WriteLine($"parsed_lines: {parsedLines}");
                writer.WriteLine($"found_errors: {foundErrors}");
                writer.WriteLine($"invalid_files: [{string.Join(", ", invalidFiles)}]");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<IPaymentDataTransformer, PaymentDataTransformer>();
                    services.AddSingleton<IFileWatcher>(provider =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        var logger = provider.GetRequiredService<ILogger<FileWatcher>>();
                        var fileProcessorFactory = provider.GetRequiredService<IFileProcessorFactory>();
                        var inputFolderPath = configuration.GetValue<string>("Serilog:InputFolderPath");
                        var outputFolderPath = configuration.GetValue<string>("Serilog:OutputFolderPath");
                        return new FileWatcher(outputFolderPath, inputFolderPath, logger, fileProcessorFactory);
                    });
                    services.AddSingleton<ILoggerFactory>(provider =>
                        new SerilogLoggerFactory(Log.Logger, true));
                    services.AddScoped<IPaymentDataWriter>(provider =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        var logger = provider.GetRequiredService<ILogger<JsonPaymentDataWriter>>();
                        var outputDirectory = configuration.GetValue<string>("Serilog:OutputFolderPath");
                        return new JsonPaymentDataWriter(outputDirectory, logger);
                    });
                    services.AddScoped<IPaymentDataValidator, PaymentDataValidator>();
                    services.AddScoped<IFileProcessorFactory>(provider =>
                        new FileProcessorFactory(
                            provider.GetRequiredService<ILoggerFactory>().CreateLogger<FileProcessorFactory>(),
                            provider.GetService<IPaymentDataTransformer>(),
                            provider.GetService<IPaymentDataWriter>(),
                            provider.GetService<IPaymentDataValidator>(),
                            provider.GetService<ILoggerFactory>()
                        )
                    );
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration.GetSection("Serilog"))
                                        .Enrich.FromLogContext()
                                        .WriteTo.Console();
                    //.WriteTo.File(hostingContext.Configuration.GetValue<string>("Serilog:File:Path"), rollingInterval: RollingInterval.Day);
                });
    }
}
