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

            this.ValidateNewAuctionAndThrowOnError(auction);
            return this.mainRepository.Add(auction);
        }

        public Bid PlaceBid(Member bidder, Auction auction, double amount)
        {
            var auct = this.mainRepository.GetAuctions().FirstOrDefault(a => a.Id == auction.Id && a == auction);

            if (auct == null)
            {
                throw new ArgumentException("This auction does not exist in the store");
            }

            if (auct.EndDateTimeUtc <= DateTime.UtcNow)
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

        private void ValidateNewAuctionAndThrowOnError(Auction auction)
        {
            if (auction == null)
            {
                throw new ArgumentException("Auction cannot be null", "auction");
            }

            if (auction.StartDateTimeUtc < DateTime.UtcNow)
            {
                throw new ArgumentException("The start of the auction needs to be in the future", "auction");
            }

            if (auction.EndDateTimeUtc < DateTime.UtcNow)
            {
                throw new ArgumentException("The end of the auction needs to be in the future", "auction");
            }

            if (auction.Bids != null && auction.Bids.Any())
            {
                throw new ArgumentException("A new auction cannot have bids", "auction");
            }

            if (auction.Seller == null)
            {
                throw new ArgumentException("The Seller of an auction cannot be null", "auction");
            }

            if (auction.Winner != null)
            {
                throw new ArgumentException("The Winner of an auction cannot be known at the begin of an auction", "auction");
            }

            if (auction.StartPrice < 0)
            {
                throw new ArgumentException("Negative startprices are not allowed", "auction");
            }

            if (string.IsNullOrEmpty(auction.Title))
            {
                throw new ArgumentException("Every auction needs a title", "auction");
            }
        }
    }
}