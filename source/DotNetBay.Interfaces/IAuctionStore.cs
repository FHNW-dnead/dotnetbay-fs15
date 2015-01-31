using System.Collections.Generic;
using DotNetBay.Model;

namespace DotNetBay.Interfaces
{
    public interface IAuctionStore
    {
        List<Auction> GetAll();
    }
}