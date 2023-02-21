using PaymentServiceLibrary.Concrete;
using PaymentServiceLibrary.Concrete.Factory;
using PaymentServiceLibrary.Concrete.Process;
using PaymentServiceLibrary.Interfaces;
using PaymentServiceLibrary.Interfaces.Process;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var fileWatcher = host.Services.GetService<IFileWatcher>();
            fileWatcher.StartWatching();

            host.Run();
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

