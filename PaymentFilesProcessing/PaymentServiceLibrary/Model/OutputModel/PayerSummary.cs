using System;

namespace PaymentServiceLibrary.Model.OutputModel
{
    public class PayerSummary
    {
        public string Name { get; set; }
        public decimal Payment { get; set; }
        public DateTime Date { get; set; }
        public long AccountNumber { get; set; }
    }
}
