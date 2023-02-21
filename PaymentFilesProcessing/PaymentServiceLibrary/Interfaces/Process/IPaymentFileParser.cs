using PaymentServiceLibrary.Model;
using System.Collections.Generic;

namespace PaymentServiceLibrary.Interfaces.Process
{
    public interface IPaymentFileParser
    {
        IEnumerable<PaymentData> Parse(string intputfolderPath);
    }
}
