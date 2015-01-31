using System.Collections.Generic;
using System.Linq;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    interface IAuctionService
    {
        IQueryable<Auction> GetAllAuctions();
    }
}
