using System.Collections.Generic;

namespace PaymentServiceLibrary.Model.MetaModel
{
    public class ProcessResult
    {
        public int ParsedLinesCount { get; set; }
        public int FoundErrorsCount { get; set; }
        public IEnumerable<string> InvalidFiles { get; set; }
    }
}
