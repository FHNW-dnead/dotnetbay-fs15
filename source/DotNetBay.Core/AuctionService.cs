using System;
using System.Linq;

using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core
{
    public class AuctionService : IAuctionService
    {
        private readonly IMainRepository mainRepository;

        private readonly IMemberService memberService;

        public AuctionService(IMainRepository mainRepository, IMemberService memberService)
        {
            this.mainRepository = mainRepository;
            this.memberService = memberService;
        }

        public Auction GetById(long id)
        {
            return this.GetAll().FirstOrDefault(a => a.Id == id);
        }

        public IQueryable<Auction> GetAll()
        {
            return this.mainRepository.GetAuctions();
        }

        public Auction Save(Auction auction)
        {
            if (this.mainRepository.GetAuctions().Any(a => a.Id == auction.Id))
            {
                var updatedAuction = this.mainRepository.Update(auction);
                this.mainRepository.SaveChanges();
                
                return updatedAuction;
            }

            this.ValidateNewAuctionAndThrowOnError(auction);
            
            var newAuction = this.mainRepository.Add(auction);
            this.mainRepository.SaveChanges();
            
            return newAuction;
        }

        public Bid PlaceBid(Auction auction, double amount)
        {
            var auct = this.mainRepository.GetAuctions().ToList().FirstOrDefault(a => a.Id == auction.Id && a == auction);

            var bidder = this.memberService.GetCurrentMember();

            if (auct == null)
            {
                throw new ArgumentException("This auction does not exist in the store");
            }

            if (auct.StartDateTimeUtc > DateTime.UtcNow)
            {
                throw new Exception("The requested auction has not started yet");
            }

            if (auct.EndDateTimeUtc <= DateTime.UtcNow)
            {
                throw new Exception("The requested auction has already closed");
            }

            var bid = new Bid()
            {
                ReceivedOnUtc = DateTime.UtcNow,
                Accepted = null,
                Auction = auct,
                Amount = amount,
                Bidder = bidder
            };

            this.mainRepository.Add(bid);
            this.mainRepository.SaveChanges();

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

            if (this.memberService.GetByUniqueId(auction.Seller.UniqueId) == null)
            {
                throw new ArgumentException("The seller cannot be cound and has to be created before using it in a auction", "auction");
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