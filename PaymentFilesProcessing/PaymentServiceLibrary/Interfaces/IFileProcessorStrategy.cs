namespace PaymentServiceLibrary.Interfaces
{
    public interface IFileProcessorStrategy
    {
        void Process(string filePath);
    }
}
