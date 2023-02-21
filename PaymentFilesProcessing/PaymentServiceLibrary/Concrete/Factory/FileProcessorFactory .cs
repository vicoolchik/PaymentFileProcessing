using PaymentServiceLibrary.Concrete.Process;
using PaymentServiceLibrary.Concrete.Strategy;
using PaymentServiceLibrary.Interfaces;
using PaymentServiceLibrary.Interfaces.Process;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace PaymentServiceLibrary.Concrete.Factory
{
    public class FileProcessorFactory : IFileProcessorFactory
    {
        private readonly ILogger<FileProcessorFactory> _logger;
        private readonly IPaymentDataTransformer _transformer;
        private readonly IPaymentDataWriter _writer;
        private readonly IPaymentDataValidator _validator;
        private readonly ILogger<CsvPaymentFileParser> _csvParserLogger;
        private readonly ILogger<TxtPaymentFileParser> _txtParserLogger;
        private readonly ILogger<TxtFileProcessor> _txtProcessorLogger;
        private readonly ILogger<CsvFileProcessor> _csvProcessorLogger;

        public FileProcessorFactory(
            ILogger<FileProcessorFactory> logger,
            IPaymentDataTransformer transformer,
            IPaymentDataWriter writer,
            IPaymentDataValidator validator,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _transformer = transformer;
            _writer = writer;
            _validator = validator;
            _csvParserLogger = loggerFactory.CreateLogger<CsvPaymentFileParser>();
            _txtParserLogger = loggerFactory.CreateLogger<TxtPaymentFileParser>();
            _txtProcessorLogger = loggerFactory.CreateLogger<TxtFileProcessor>();
            _csvProcessorLogger = loggerFactory.CreateLogger<CsvFileProcessor>();
        }
    
        public IFileProcessorStrategy CreateFileProcessor(string _inputfolderPath, string _outputfolderPath)
        {
            var extension = Path.GetExtension(_inputfolderPath);
            switch (extension.ToLowerInvariant())
            {
                case ".csv":
                    var csvParser = new CsvPaymentFileParser(_csvParserLogger);
                    return new CsvFileProcessor(_inputfolderPath, _outputfolderPath, _csvProcessorLogger, csvParser, _validator, _transformer, _writer);
                case ".txt":
                    var txtParser = new TxtPaymentFileParser(_txtParserLogger);
                    return new TxtFileProcessor(_inputfolderPath, _outputfolderPath, _txtProcessorLogger, txtParser, _validator, _transformer, _writer);
                // Add more cases for other file extensions or content types
                default:
                    throw new NotSupportedException($"File extension '{extension}' is not supported.");
            }
        }
    }
}

