using Asn.FileProcessor;
using ASN.Infrastructure.Data;
using AsnWatcher.Configuration;

namespace AsnWatcher
{
    public class FileWatcher : IDisposable
    {
        private FileSystemWatcher _fileSystemWatcher;
        
        private readonly SupplierConfig _suppliersConfig;       
        public readonly UnitOfWork _unitOfWork;

        public FileWatcher(SupplierConfig specificSuppliersConfig, UnitOfWork unitOfWork)
        {
            _suppliersConfig = specificSuppliersConfig;
            _unitOfWork = unitOfWork;
        }

        public void Watch()
        {
            if (string.IsNullOrWhiteSpace(_suppliersConfig.MonitoringFilePath))
            {
                throw new ArgumentException("Monitoring file path is required");
            }

            _fileSystemWatcher = new FileSystemWatcher(_suppliersConfig.MonitoringFilePath);    

            if (!string.IsNullOrWhiteSpace(_suppliersConfig.Filter))
            {
                _fileSystemWatcher.Filter = _suppliersConfig.Filter;
            }
            else
            {
                _fileSystemWatcher.Filter = "*.*";
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Created += OnCreated;
        }

        // This should set send notification to some queue or some other service to process the file
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            var fullPath = e.FullPath;
            WaitForFile(fullPath);
            new FileProcessor(_unitOfWork).Process(fullPath);
        }

        private void WaitForFile(string fullPath)
        {
            bool fileIsReady = false;
            while (!fileIsReady)
            {
                try
                {
                    // Try to open the file with exclusive access.
                    using (FileStream stream = File.Open(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        fileIsReady = true;
                    }
                }
                catch (IOException)
                {
                    // If an IOException is caught, it means the file is still locked, wait for a bit before retrying.
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public void Dispose()
        {
            _fileSystemWatcher.Created -= OnCreated;
            _fileSystemWatcher.Dispose();
        }
    }
}
