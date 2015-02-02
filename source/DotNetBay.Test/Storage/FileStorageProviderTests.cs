using DotNetBay.Data.FileStorage;
using DotNetBay.Interfaces;

namespace DotNetBay.Test.Storage
{
    public class FileStorageProviderTests : StorageProviderBaseTests
    {
        public class TempFileDataStoreFactory : IDataStoreFactory
        {
            private readonly TempFile tempFile;

            public TempFileDataStoreFactory()
            {
                this.tempFile = new TempFile();
            }

            public void Dispose()
            {
                this.tempFile.Dispose();
            }

            public IDataStore CreateStore()
            {
                return new FileDataStore(this.tempFile.FullPath);
            }
        }

        protected override IDataStoreFactory CreateFactory()
        {
            return new TempFileDataStoreFactory();
        }
    }
}