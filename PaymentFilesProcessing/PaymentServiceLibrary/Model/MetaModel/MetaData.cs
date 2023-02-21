using System.Collections.Generic;

namespace PaymentServiceLibrary.Model.MetaModel
{
    public class MetaData
    {
        public int ParsedFiles { get; set; }
        public int ParsedLines { get; set; }
        public int FoundErrors { get; set; }
        public List<string> InvalidFiles { get; set; }
    }
}
