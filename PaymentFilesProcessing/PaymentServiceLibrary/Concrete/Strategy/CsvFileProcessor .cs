using PaymentServiceLibrary.Interfaces;
using PaymentServiceLibrary.Interfaces.Process;
using PaymentServiceLibrary.Model;
using PaymentServiceLibrary.Model.OutputModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaymentServiceLibrary.Concrete.Strategy
{
    public class CsvFileProcessor : IFileProcessorStrategy
    {
        private readonly string _inputPath;
        private readonly string _outputPath;
        private readonly IPaymentFileParser _parser;
        private readonly IPaymentDataValidator _validator;
        private readonly IPaymentDataTransformer _transformer;
        private readonly IPaymentDataWriter _writer;
        private readonly ILogger<CsvFileProcessor> _logger;

        public CsvFileProcessor(string inputPath, 
            string outputPath, 
            ILogger<CsvFileProcessor> logger, 
            IPaymentFileParser parser, 
            IPaymentDataValidator validator, 
            IPaymentDataTransformer transformer, 
            IPaymentDataWriter writer)
        {
            _logger = logger;
            _outputPath = outputPath;
            _inputPath = inputPath;
            _parser = parser;
            _validator = validator;
            _transformer = transformer;
            _writer = writer;
        }
        public void Process(string filePath)
        {
            try
            {
                _logger.LogInformation($"Processing file: {filePath}");

                // Step 1: Parse the TXT file and obtain the payment data
                IEnumerable<PaymentData> paymentData = _parser.Parse(filePath);

                // Step 2: Validate the payment data
                PaymentDataValidationResult validationResult = _validator.Validate(paymentData);

                if (!validationResult.IsValid)
                {
                    foreach (string errorMessage in validationResult.ErrorMessages)
                    {
                        _logger.LogError(errorMessage);
                    }
                }

                // Filter out the invalid payment data and return only the valid ones.
                IEnumerable<PaymentData> validPaymentData = paymentData.Where(pd => !validationResult.ErrorMessages.Contains(pd.Id));

                // Step 3: Transform the payment data into the desired format
                IEnumerable<PaymentSummary> transformedData = _transformer.Transform(validPaymentData);

                // Step 4: Write the transformed payment data to the output folder
                _writer.Write(_outputPath, transformedData);

                _logger.LogInformation($"Processed file: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file: {filePath}");
            }
        }
    }
}
