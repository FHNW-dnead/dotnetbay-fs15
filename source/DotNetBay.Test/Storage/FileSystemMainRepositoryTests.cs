using System;
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
            private TempDirectory tempDirectory;

            public TempFileMainRepositoryFactory()
            {
                this.tempDirectory = new TempDirectory();
            }

            public IMainRepository CreateMainRepository()
            {
                return new FileSystemMainRepository(Path.Combine(this.tempDirectory.Root, "data.json"));
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            // The bulk of the clean-up code is implemented in Dispose(bool)
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // free managed resources
                    if (this.tempDirectory != null)
                    {
                        this.tempDirectory.Dispose();
                        this.tempDirectory = null;
                    }
                }

                // free native resources if there are any.
            }
        }
    }
}