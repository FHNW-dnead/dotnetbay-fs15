using System;
using System.Linq;
using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    public class AuctionService : IAuctionService
    {
        private readonly IMainRepository mainRepository;

        public AuctionService(IMainRepository mainRepository)
        {
            this.mainRepository = mainRepository;
        }

        public Auction GetAuctionById(long id)
        {
            return this.GetAuctions().FirstOrDefault(a => a.Id == id);
        }

        public IQueryable<Auction> GetAuctions()
        {
            return this.mainRepository.GetAuctions();
        }

        public Auction Save(Auction auction)
        {
            if (this.mainRepository.GetAuctions().Any(a => a.Id == auction.Id))
            {
                return this.mainRepository.Update(auction);
            }

            return this.mainRepository.Add(auction);
        }

        public Bid PlaceBid(Member bidder, Auction auction, double amount)
        {
            var auct = this.mainRepository.GetAuctions().FirstOrDefault(a => a.Id == auction.Id && a == auction);

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

            this.mainRepository.Add(bid);

            return bid;
        }
    }
}