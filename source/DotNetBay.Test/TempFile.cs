using System;
using System.IO;

namespace DotNetBay.Core.Test
{
    public class TempFile : IDisposable
    {
        private string fullPath;

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
            catch (Exception e)
            {
                
            }
        }
    }
}