using PaymentServiceLibrary.Concrete.Process;
using PaymentServiceLibrary.Interfaces.Process;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.EventLog;

using Serilog.Events;


namespace PaymentProcessingService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] servicesToRun = new ServiceBase[]
            {
                new PaymentFilesProcessingServise()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
