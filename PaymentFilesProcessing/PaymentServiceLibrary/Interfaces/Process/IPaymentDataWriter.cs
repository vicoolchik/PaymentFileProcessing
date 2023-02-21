using PaymentServiceLibrary.Model.OutputModel;
using System.Collections.Generic;

namespace PaymentServiceLibrary.Interfaces.Process
{
    public interface IPaymentDataWriter
    {
        void Write(string outputPath, IEnumerable<PaymentSummary> cityData);
    }
}
