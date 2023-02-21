using System.Collections.Generic;

namespace PaymentServiceLibrary.Model.OutputModel
{
    public class ServiceSummary
    {
        public string Name { get; set; }
        public List<PayerSummary> Payers { get; set; }
        public decimal Total { get; set; }
    }
}
