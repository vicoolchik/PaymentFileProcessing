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


namespace PaymentService
{
    public class PaymentProcessingService
    {
        private readonly IHost _host;
        private readonly IFileWatcher _fileWatcher;

        public PaymentProcessingService()
        {
            _host = CreateHostBuilder(Array.Empty<string>()).Build();
            _fileWatcher = _host.Services.GetService<IFileWatcher>();
        }

        public void Start()
        {
            // Check for InputFolderPath and OutputFolderPath
            var inputFolderPath = _host.Services.GetService<IConfiguration>().GetValue<string>("Serilog:InputFolderPath");
            var outputFolderPath = _host.Services.GetService<IConfiguration>().GetValue<string>("Serilog:OutputFolderPath");

            if (string.IsNullOrEmpty(inputFolderPath) || string.IsNullOrEmpty(outputFolderPath))
            {
                Log.Logger.Error("InputFolderPath or OutputFolderPath is not available or empty. The service cannot start.");
                throw new Exception("InputFolderPath or OutputFolderPath is not available or empty. The service cannot start.");
            }

            _fileWatcher.StartWatching();
            _host.Start();
        }

        public void Stop()
        {
            _fileWatcher.StopWatching();
            _host.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Restart()
        {
            _host.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            _fileWatcher.StopWatching(); // stop watching the file system during restart
            _host.Start();
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
