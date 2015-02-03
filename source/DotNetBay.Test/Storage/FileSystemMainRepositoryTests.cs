using System.IO;
using DotNetBay.Data.FileStorage;
using DotNetBay.Interfaces;

namespace DotNetBay.Test.Storage
{
    public class FileSystemMainRepositoryTests : MainRepositoryTestBase
    {
        protected override IRepositoryFactory CreateFactory()
        {
            return new TempFileMainRepositoryFactory();
        }

        public class TempFileMainRepositoryFactory : IRepositoryFactory
        {
            private readonly TempDirectory tempDirectory;

            public TempFileMainRepositoryFactory()
            {
                this.tempDirectory = new TempDirectory();
            }

            public void Dispose()
            {
                this.tempDirectory.Dispose();
            }

            public IMainRepository CreateMainRepository()
            {
                return new FileSystemMainRepository(Path.Combine(this.tempDirectory.Root, "data.json"));
            }
        }
    }
}