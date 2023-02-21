using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentServiceLibrary.Concrete
{
    public class PaymentDataValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ErrorMessages { get; set; }

        public PaymentDataValidationResult(bool isValid, List<string> errorMessages)
        {
            IsValid = isValid;
            ErrorMessages = errorMessages;
        }
    }
}
