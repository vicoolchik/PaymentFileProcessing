using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using PaymentServiceLibrary.Concrete.Process;
using PaymentServiceLibrary.Interfaces.Process;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using PaymentServiceLibrary.Interfaces;
using PaymentServiceLibrary.Concrete;

namespace PaymentProcessingService
{
    public partial class PaymentFilesProcessingServise : ServiceBase
    {

        public PaymentFilesProcessingServise()
        {
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }

    }
}
