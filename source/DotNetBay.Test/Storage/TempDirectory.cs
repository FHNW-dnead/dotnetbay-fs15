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
