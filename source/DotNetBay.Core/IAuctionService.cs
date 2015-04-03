using System.Linq;

using DotNetBay.Model;

namespace DotNetBay.Core
{
    public interface IAuctionService
    {
        IQueryable<Auction> GetAll();

        Auction Save(Auction auction);

        Bid PlaceBid(Auction auction, double amount);
    }
}
