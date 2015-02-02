using System;

namespace DotNetBay.Interfaces
{
    public interface IDataStoreFactory: IDisposable
    {
        IDataStore CreateStore();
    }
}
