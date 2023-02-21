using PaymentServiceLibrary.Interfaces.Process;
using PaymentServiceLibrary.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PaymentServiceLibrary.Concrete.Process
{
    public class CsvPaymentFileParser : IPaymentFileParser
    {
        private const int BUFFER_SIZE = 1000; // Buffer size for reading file in chunks

        private readonly char[] DELIMITERS = { ',' };

        private readonly ILogger<CsvPaymentFileParser> _logger;

        public CsvPaymentFileParser(ILogger<CsvPaymentFileParser> logger)
        {
            _logger = logger;
        }

        public IEnumerable<PaymentData> Parse(string filePath)
        {
            var paymentDataList = new List<PaymentData>();

            try
            {
                using (var streamReader = new StreamReader(filePath))
                {
                    // Skip the first line with headers
                    streamReader.ReadLine();

                    var buffer = new char[BUFFER_SIZE];
                    var chunk = new StringBuilder();

                    while (!streamReader.EndOfStream)
                    {
                        int readCount = streamReader.Read(buffer, 0, BUFFER_SIZE);

                        for (int i = 0; i < readCount; i++)
                        {
                            char c = buffer[i];

                            if (c == '\n')
                            {
                                string line = chunk.ToString().TrimEnd('\r');
                                PaymentData paymentData = ParseLine(line);

                                if (paymentData != null)
                                {
                                    paymentDataList.Add(paymentData);
                                }

                                chunk.Clear();
                            }
                            else
                            {
                                chunk.Append(c);
                            }
                        }
                    }

                    // Process any remaining data in the buffer
                    if (chunk.Length > 0)
                    {
                        PaymentData paymentData = ParseLine(chunk.ToString().TrimEnd('\r'));

                        if (paymentData != null)
                        {
                            paymentDataList.Add(paymentData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing payment file {FilePath}", filePath);
                throw;
            }

            return paymentDataList;
        }

        private PaymentData ParseLine(string line)
        {
            var parts = line.Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 9)
            {
                _logger.LogWarning("Invalid number of fields in line: {0}", line);
                return null;
            }

            var regex = new Regex("[^a-zA-Z0-9]");
            parts = parts.Select((p, i) => i == 5 || i == 6 ? p.Trim() : regex.Replace(p, "").Replace(" ", "")).ToArray();
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "yyyy-dd-MM" };

            var paymentData = new PaymentData
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = parts[0].Trim(),
                LastName = parts[1].Trim(),
                City = parts[2].Trim(),
                Payment = decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) ? amount : 0,
                Date = DateTime.TryParseExact(parts[6], formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : DateTime.MinValue,
                AccountNumber = long.TryParse(parts[7], out var accountNumber) ? accountNumber : 0,
                Service = parts[8].Trim()
            };

            if (paymentData.Payment <= 0 || paymentData.AccountNumber <= 0 || paymentData.Date == DateTime.MinValue)
            {
                _logger.LogWarning("Invalid payment data in line: {0}", line);
                return null;
            }

            return paymentData;
        }
    }
}
