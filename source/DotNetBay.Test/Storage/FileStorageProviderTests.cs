using System.IO;
using DotNetBay.Data.FileStorage;
using DotNetBay.Interfaces;

namespace DotNetBay.Test.Storage
{
    public class FileStorageProviderTests : StorageProviderBaseTests
    {
        protected override IDataStoreFactory CreateFactory()
        {
            return new TempFileDataStoreFactory();
        }

        public class TempFileDataStoreFactory : IDataStoreFactory
        {
            private readonly TempDirectory tempDirectory;

            public TempFileDataStoreFactory()
            {
                this.tempDirectory = new TempDirectory();
            }

            public void Dispose()
            {
                this.tempDirectory.Dispose();
            }

            public IDataStore CreateStore()
            {
                return new FileDataStore(Path.Combine(this.tempDirectory.Root, "data.json"));
            }
        }
    }
}