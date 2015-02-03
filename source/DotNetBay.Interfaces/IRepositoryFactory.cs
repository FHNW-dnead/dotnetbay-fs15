using System;

namespace DotNetBay.Interfaces
{
    public interface IRepositoryFactory : IDisposable
    {
        IMainRepository CreateMainRepository();
    }
}
