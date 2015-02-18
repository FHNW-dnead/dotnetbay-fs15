using System;

using DotNetBay.Interfaces;

namespace DotNetBay.Data.FileStorage
{
    public class FileSystemRepositoryFactory : IRepositoryFactory
    {
        private readonly string fileName;

        public FileSystemRepositoryFactory(string fileName)
        {
            this.fileName = fileName;
        }

        public IMainRepository CreateMainRepository()
        {
            return new FileSystemMainRepository(this.fileName);
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
            }

            // free native resources if there are any.
        }
    }
}