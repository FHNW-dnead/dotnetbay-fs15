using System.Linq;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    public interface IAuctionService
    {
        IQueryable<Auction> GetAllAuctions();
    }
}
