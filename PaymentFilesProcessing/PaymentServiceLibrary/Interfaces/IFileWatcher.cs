using System;

namespace PaymentServiceLibrary.Interfaces
{
    public interface IFileWatcher
    {
        void StartWatching();
        void StopWatching();
        int ParsedFilesCount { get; }
        int ParsedLinesCount { get; }
        int FoundErrorsCount { get; }
        string[] InvalidFiles { get; }
    }

    public class FileEventArgs : EventArgs
    {
        public string FilePath { get; set; }
    }

}
