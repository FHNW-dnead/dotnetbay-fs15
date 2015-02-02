using System.Linq;
using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    public class AuctionService : IAuctionService
    {
        private readonly IDataStore dataStore;

        public AuctionService(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public IQueryable<Auction> GetAllAuctions()
        {
            return this.dataStore.GetAuctions();
        }

        public Auction AddAuction(Auction auction)
        {
            return this.dataStore.Add(auction);
        }

        public Bid PlaceBid(Member bidder, Auction auction, double amount)
        {
            var bid = new Bid()
            {
                Auction = auction,
                Amount = amount,
                Bidder = bidder
            };

            this.dataStore.Add(bid);

            return bid;
        }
    }
}