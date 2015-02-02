using System;
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

        public Auction GetAuctionById(long id)
        {
            return this.GetAuctions().FirstOrDefault(a => a.Id == id);
        }

        public IQueryable<Auction> GetAuctions()
        {
            return this.dataStore.GetAuctions();
        }

        public Auction Save(Auction auction)
        {
            if (this.dataStore.GetAuctions().Any(a => a.Id == auction.Id))
            {
                return this.dataStore.Update(auction);
            }

            return this.dataStore.Add(auction);
        }

        public Bid PlaceBid(Member bidder, Auction auction, double amount)
        {
            var auct = this.dataStore.GetAuctions().FirstOrDefault(a => a.Id == auction.Id && a == auction);

            if (auct == null)
            {
                throw new ArgumentException("This auction does not exist in the store");
            }

            if (auct.EndDateTimeUtc >= DateTime.UtcNow)
            {
                throw new Exception("The requested auction has already closed");
            }

            var bid = new Bid()
            {
                Auction = auct,
                Amount = amount,
                Bidder = bidder
            };

            this.dataStore.Add(bid);

            return bid;
        }
    }
}