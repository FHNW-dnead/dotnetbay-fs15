using System.Linq;

using DotNetBay.Model;

namespace DotNetBay.Core
{
    public interface IAuctionService
    {
        IQueryable<Auction> GetAuctions();

        Auction Save(Auction auction);

        Bid PlaceBid(Member bidder, Auction auction, double amount);
    }
}
