using System;
using System.IO;

namespace DotNetBay.Test.Storage
{
    public class TempFile : IDisposable
    {
        private readonly string fullPath;

        public TempFile()
        {
            this.fullPath = Path.GetTempFileName();
        }

        public string FullPath 
        {
            get { return this.fullPath; }
        }

        public override string ToString()
        {
            return this.fullPath;
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
                File.Delete(this.fullPath);
            }
            catch (Exception)
            {
            }
        }
    }
}