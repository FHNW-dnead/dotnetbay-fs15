using System;
using System.IO;

namespace DotNetBay.Test.Storage
{
    public class TempFile : IDisposable
    {
        private readonly string fullPath;

        public string FullPath {
            get { return this.fullPath; }
        }

        public TempFile()
        {
            this.fullPath = Path.GetTempFileName();
        }

        public override string ToString()
        {
            return this.fullPath;
        }

        public void Dispose()
        {
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