using PaymentServiceLibrary.Concrete;
using PaymentServiceLibrary.Model;
using System.Collections.Generic;

namespace PaymentServiceLibrary.Interfaces.Process
{
    public interface IPaymentDataValidator
    {
        PaymentDataValidationResult Validate(IEnumerable<PaymentData> paymentData);
    }
}
