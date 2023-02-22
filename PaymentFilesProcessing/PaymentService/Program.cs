using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using Topshelf;

namespace PaymentService
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<PaymentProcessingService>(s =>
                {
                    s.ConstructUsing(name => new PaymentProcessingService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
                x.RunAsLocalSystem();

                x.SetServiceName("PaymentProcessingService");
                x.SetDisplayName("Payment Processing Service");
                x.SetDescription("A service that processes payment files.");

                x.OnException((exception) =>
                {
                    Log.Logger.Error(exception, "Service encountered an exception and will be stopped.");
                });

                x.EnableServiceRecovery(recoveryOptions =>
                {
                    recoveryOptions.RestartService(1); //restart service after 1 minute
                    recoveryOptions.OnCrashOnly(); //only automatically recover on crash
                });
            });

            var exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetType());
            Environment.ExitCode = exitCodeValue;
        }
     }
}

