using System.IO;
using DotNetBay.Data.FileStorage;

namespace DotNetBay.Test.Core
{
    public class InMemoryMainRepository : FileSystemMainRepository
    {
        public InMemoryMainRepository() : base(Path.GetTempFileName())
        {
        }

        protected override void Save()
        {
            // Do nothing
        }

        protected override void Load()
        {
            // Do nothing
        }
    }
}