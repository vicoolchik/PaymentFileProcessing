using PaymentServiceLibrary.Model.MetaModel;

namespace PaymentServiceLibrary.Interfaces
{
    public interface IFileProcessorStrategy
    {
        ProcessResult Process(string filePath);
    }
}
