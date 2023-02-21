using PaymentServiceLibrary.Interfaces.Process;
using PaymentServiceLibrary.Model;
using PaymentServiceLibrary.Model.OutputModel;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentServiceLibrary.Concrete.Process
{
    public class PaymentDataTransformer : IPaymentDataTransformer
    {
        private readonly ILogger<PaymentDataTransformer> _logger;

        public PaymentDataTransformer(ILogger<PaymentDataTransformer> logger)
        {
            _logger = logger;
        }

        public IEnumerable<PaymentSummary> Transform(IEnumerable<PaymentData> paymentData)
        {
            _logger.LogInformation("Starting payment data transformation...");

            var paymentSummaries = new ConcurrentBag<PaymentSummary>();

            // Group payment data by city and service
            var groupedPaymentData = paymentData
                .GroupBy(p => new { p.City, p.Service })
                .ToList();

            Parallel.ForEach(groupedPaymentData, group =>
            {
                _logger.LogDebug("Processing payment data for city {City} and service {Service}", group.Key.City, group.Key.Service);

                var city = group.Key.City;
                var service = group.Key.Service;

                // Group payment data by payer
                var groupedPayers = group
                    .GroupBy(p => new { p.FirstName, p.LastName, p.AccountNumber })
                    .ToList();

                var serviceSummaries = new List<ServiceSummary>();
                decimal serviceTotal = 0;

                foreach (var payerGroup in groupedPayers)
                {
                    var payer = payerGroup.Key;
                    var payerName = $"{payer.FirstName} {payer.LastName}";

                    var payerSummaries = payerGroup
                        .Select(p => new PayerSummary
                        {
                            Name = payerName,
                            Payment = p.Payment,
                            Date = p.Date,
                            AccountNumber = p.AccountNumber
                        })
                        .ToList();

                    var serviceTotalForPayer = payerSummaries.Sum(p => p.Payment);
                    serviceTotal += serviceTotalForPayer;

                    var serviceSummary = new ServiceSummary
                    {
                        Name = service,
                        Payers = payerSummaries,
                        Total = serviceTotalForPayer
                    };

                    serviceSummaries.Add(serviceSummary);
                }

                var paymentSummary = new PaymentSummary
                {
                    City = city,
                    Services = serviceSummaries,
                    Total = serviceTotal
                };

                paymentSummaries.Add(paymentSummary);
            });

            _logger.LogInformation("Payment data transformation complete.");

            return paymentSummaries.ToList();
        }
    }
}
