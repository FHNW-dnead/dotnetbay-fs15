using System.Linq;
using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionStore auctionStore;

        public AuctionService(IAuctionStore auctionStore)
        {
            this.auctionStore = auctionStore;
        }

        public IQueryable<Auction> GetAllAuctions()
        {
            return this.auctionStore.GetAuctions();
        }

        public Auction AddAuction(Auction auction)
        {
            return this.auctionStore.Add(auction);
        }

        public Bid PlaceBid(Member bidder, Auction auction, double amount)
        {
            var bid = new Bid()
            {
                Auction = auction,
                Amount = amount,
                Bidder = bidder
            };

            this.auctionStore.Add(bid);

            return bid;
        }
    }
}