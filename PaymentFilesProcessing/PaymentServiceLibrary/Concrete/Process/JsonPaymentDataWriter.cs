using PaymentServiceLibrary.Interfaces.Process;
using PaymentServiceLibrary.Model.OutputModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace PaymentServiceLibrary.Concrete.Process
{
    public class JsonPaymentDataWriter : IPaymentDataWriter
    {
        private readonly string _outputDirectory;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly ILogger<JsonPaymentDataWriter> _logger;

        public JsonPaymentDataWriter(string outputDirectory, ILogger<JsonPaymentDataWriter> logger)
        {
            _outputDirectory = outputDirectory;
            _logger = logger;
        }

        public void Write(string outputPath, IEnumerable<PaymentSummary> cityData)
        {
            try
            {
                _semaphore.Wait();

                // Create output directory if it doesn't exist
                Directory.CreateDirectory(_outputDirectory);

                // Create subdirectory with current date
                string subDirectory = Path.Combine(_outputDirectory, DateTime.Today.ToString("MM-dd-yyyy"));
                Directory.CreateDirectory(subDirectory);

                // Get current file number
                int currentFileNumber = GetCurrentFileNumber(subDirectory);

                // Increment current file number
                currentFileNumber++;

                // Save output to file
                string outputFileName = $"output{currentFileNumber}.json";
                string outputFile = Path.Combine(subDirectory, outputFileName);
                var json = JsonConvert.SerializeObject(cityData);
                File.WriteAllText(outputFile, json);

                _logger.LogInformation("Data written to file: {OutputFile}", outputFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing data to file");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private int GetCurrentFileNumber(string subDirectory)
        {
            int currentFileNumber = 0;

            // Get all files in subdirectory with name starting with "output"
            var outputFiles = Directory.GetFiles(subDirectory, "output*.json");

            // If there are any files, get the highest file number
            if (outputFiles.Any())
            {
                currentFileNumber = outputFiles
                    .Select(file => int.Parse(Path.GetFileNameWithoutExtension(file).Substring(6)))
                    .Max();
            }

            return currentFileNumber;
        }
    }
}
