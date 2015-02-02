using DotNetBay.Interfaces;

namespace DotNetBay.Data.FileStorage
{
    public class FileDataStoreFactory : IDataStoreFactory
    {
        private readonly string fileName;

        public FileDataStoreFactory(string fileName)
        {
            this.fileName = fileName;
        }

        public IDataStore CreateStore()
        {
            return new FileDataStore(this.fileName);
        }

        public void Dispose()
        {
        }
    }
}