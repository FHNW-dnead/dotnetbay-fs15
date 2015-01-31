using System.Collections.Generic;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    interface IAuctionService
    {
        List<Auction> GetAllAuctions();
    }
}
