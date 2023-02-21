using System;

namespace PaymentServiceLibrary.Interfaces
{
    public interface IFileWatcher
    {
        event EventHandler<FileEventArgs> FileCreated;

        void StartWatching();
        void StopWatching();
    }

    public class FileEventArgs : EventArgs
    {
        public string FilePath { get; set; }
    }
}
