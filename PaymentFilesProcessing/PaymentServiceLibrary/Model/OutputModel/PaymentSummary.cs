using System.Collections.Generic;

namespace PaymentServiceLibrary.Model.OutputModel
{
    public class PaymentSummary
    {
        public string City { get; set; }
        public List<ServiceSummary> Services { get; set; }
        public decimal Total { get; set; }
    }
}
