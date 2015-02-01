using System.Linq;
using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    public class AuctionService : IAuctionService
    {
        private readonly IStorageProvider storageProvider;

        public AuctionService(IStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        public IQueryable<Auction> GetAllAuctions()
        {
            return this.storageProvider.GetAuctions();
        }

        public Auction AddAuction(Auction auction)
        {
            return this.storageProvider.Add(auction);
        }

        public Bid PlaceBid(Member bidder, Auction auction, double amount)
        {
            var bid = new Bid()
            {
                Auction = auction,
                Amount = amount,
                Bidder = bidder
            };

            this.storageProvider.Add(bid);

            return bid;
        }
    }
}