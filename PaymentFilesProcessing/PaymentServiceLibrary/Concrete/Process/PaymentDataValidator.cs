using PaymentServiceLibrary.Interfaces.Process;
using PaymentServiceLibrary.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentServiceLibrary.Concrete.Process
{
    public class PaymentDataValidator : IPaymentDataValidator
    {
        private readonly ILogger<PaymentDataValidator> _logger;

        public PaymentDataValidator(ILogger<PaymentDataValidator> logger)
        {
            _logger = logger;
        }

        public PaymentDataValidationResult Validate(IEnumerable<PaymentData> paymentData)
        {
            var validationErrors = new List<string>();

            foreach (var data in paymentData)
            {
                if (string.IsNullOrEmpty(data.FirstName))
                {
                    validationErrors.Add($"Missing first name for payment with ID {data.Id}");
                }

                if (string.IsNullOrEmpty(data.LastName))
                {
                    validationErrors.Add($"Missing last name for payment with ID {data.Id}");
                }

                if (data.AccountNumber <= 0)
                {
                    validationErrors.Add($"Invalid account number for payment with ID {data.Id}");
                }

                if (string.IsNullOrEmpty(data.City))
                {
                    validationErrors.Add($"Missing address for payment with ID {data.Id}");
                }

                if (data.Payment <= 0)
                {
                    validationErrors.Add($"Invalid payment amount for payment with ID {data.Id}");
                }

                if (data.Date == DateTime.MinValue)
                {
                    validationErrors.Add($"Invalid payment date for payment with ID {data.Id}");
                }

                if (string.IsNullOrEmpty(data.Service))
                {
                    validationErrors.Add($"Missing service for payment with ID {data.Id}");
                }
            }

            if (validationErrors.Any())
            {
                _logger.LogError("Payment data validation failed: {Errors}", string.Join("; ", validationErrors));
                return new PaymentDataValidationResult(false, validationErrors);
            }

            _logger.LogInformation("Payment data validation succeeded.");
            return new PaymentDataValidationResult(true, validationErrors);
        }
    }
}
