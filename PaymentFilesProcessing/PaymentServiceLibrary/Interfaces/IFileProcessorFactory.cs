namespace PaymentServiceLibrary.Interfaces
{
    public interface IFileProcessorFactory
    {
        IFileProcessorStrategy CreateFileProcessor(string intputfolderPath, string outputfolderPath);
    }

}
