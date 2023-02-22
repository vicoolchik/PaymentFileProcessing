using PaymentServiceLibrary.Interfaces;
using PaymentServiceLibrary.Interfaces.Process;
using PaymentServiceLibrary.Model;
using PaymentServiceLibrary.Model.OutputModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using PaymentServiceLibrary.Model.MetaModel;

namespace PaymentServiceLibrary.Concrete.Strategy
{
    public class TxtFileProcessor : IFileProcessorStrategy
    {
        private readonly string _inputPath;
        private readonly string _outputPath;
        private readonly IPaymentFileParser _parser;
        private readonly IPaymentDataValidator _validator;
        private readonly IPaymentDataTransformer _transformer;
        private readonly IPaymentDataWriter _writer;
        private readonly ILogger<TxtFileProcessor> _logger;

        public TxtFileProcessor(string inputPath,
            string outputPath,
            ILogger<TxtFileProcessor> logger,
            IPaymentFileParser parser,
            IPaymentDataValidator validator,
            IPaymentDataTransformer transformer,
            IPaymentDataWriter writer)
        {
            _outputPath = outputPath;
            _logger = logger;
            _inputPath = inputPath;
            _parser = parser;
            _validator = validator;
            _transformer = transformer;
            _writer = writer;
        }

        public ProcessResult Process(string filePath)
        {
            ProcessResult result = new ProcessResult();
            try
            {
                _logger.LogInformation($"Processing file: {filePath}");

                // Step 1: Parse the TXT file and obtain the payment data
                IEnumerable<PaymentData> paymentData = _parser.Parse(filePath);
                result.ParsedLinesCount = paymentData.Count();

                // Step 2: Validate the payment data
                PaymentDataValidationResult validationResult = _validator.Validate(paymentData);

                if (!validationResult.IsValid)
                {
                    foreach (string errorMessage in validationResult.ErrorMessages)
                    {
                        _logger.LogError(errorMessage);
                        result.FoundErrorsCount++;
                    }
                }

                // Filter out the invalid payment data and return only the valid ones.
                IEnumerable<PaymentData> validPaymentData = paymentData.Where(pd => !validationResult.ErrorMessages.Contains(pd.Id));
                IEnumerable<PaymentData> invalidPaymentData = paymentData.Except(validPaymentData);

                result.InvalidFiles = invalidPaymentData.Any() ? new string[] { filePath } : new string[0];

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

            return result;
        }
    }
}
