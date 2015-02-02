using System;
using System.IO;

namespace DotNetBay.Test.Storage
{
    public class TempDirectory : IDisposable
    {
        private readonly string root;

        public TempDirectory()
        {
            this.root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        public string Root
        {
            get { return this.root; }
        }

        public override string ToString()
        {
            return this.root;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(this.root, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
