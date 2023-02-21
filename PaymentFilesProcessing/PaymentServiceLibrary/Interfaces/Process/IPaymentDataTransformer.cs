using PaymentServiceLibrary.Model;
using PaymentServiceLibrary.Model.OutputModel;
using System.Collections.Generic;

namespace PaymentServiceLibrary.Interfaces.Process
{
    public interface IPaymentDataTransformer
    {
        IEnumerable<PaymentSummary> Transform(IEnumerable<PaymentData> paymentData);
    }
}
