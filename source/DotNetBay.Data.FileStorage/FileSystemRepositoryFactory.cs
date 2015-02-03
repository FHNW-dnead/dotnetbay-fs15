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
        }
    }
}