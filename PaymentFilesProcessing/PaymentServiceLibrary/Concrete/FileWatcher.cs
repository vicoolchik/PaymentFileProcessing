using System;
using System.IO;
using PaymentServiceLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using PaymentServiceLibrary.Model.MetaModel;

namespace PaymentServiceLibrary.Concrete
{
    public partial class FileWatcher: IFileWatcher
    {
        private readonly IFileProcessorFactory _fileProcessorFactory;
        private readonly FileSystemWatcher _fileWatcher;
        private readonly string _inputfolderPath;
        private readonly string _outputfolderPath;
        private readonly ILogger<FileWatcher> _logger;

        private int _parsedFilesCount;
        private int _parsedLinesCount;
        private int _foundErrorsCount;
        private List<string> _invalidFiles = new List<string>();

        public FileWatcher(string outputfolderPath, string inputfolderPath, ILogger<FileWatcher> logger, IFileProcessorFactory fileProcessorFactory)
        {
            _outputfolderPath = outputfolderPath;
            _fileProcessorFactory = fileProcessorFactory;
            _inputfolderPath = inputfolderPath;
            _fileWatcher = new FileSystemWatcher(_inputfolderPath);
            _fileWatcher.Created += OnCreated;
            _fileWatcher.Error += OnError;
            _logger = logger;
            _parsedFilesCount = 0;
            _parsedLinesCount = 0;
            _foundErrorsCount = 0;
            _invalidFiles = new List<string>();
        }

        public void StartWatching()
        {
            _logger.LogInformation("Starting to watch folder {FolderPath}", _inputfolderPath);
            _fileWatcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            _fileWatcher.EnableRaisingEvents = false;
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "Error occurred while watching folder {FolderPath}", _inputfolderPath);
        }


        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("New file created: {FilePath}", e.FullPath);

            // Create a file processor for the new file

            var processor = _fileProcessorFactory.CreateFileProcessor(e.FullPath, _outputfolderPath);

            try
            {
                // Process the file
                ProcessResult result = processor.Process(e.FullPath);
                _parsedFilesCount++;
                _parsedLinesCount += result.ParsedLinesCount;
                _foundErrorsCount += result.FoundErrorsCount;
                _invalidFiles.AddRange(result.InvalidFiles);

                _logger.LogInformation("File processed successfully: {FilePath}", e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", e.FullPath);
            }
        }

        public int ParsedFilesCount => _parsedFilesCount;

        public int ParsedLinesCount => _parsedLinesCount;

        public int FoundErrorsCount => _foundErrorsCount;

        public string[] InvalidFiles => _invalidFiles.ToArray();
    }
}
