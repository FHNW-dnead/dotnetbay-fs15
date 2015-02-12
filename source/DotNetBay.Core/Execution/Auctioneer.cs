using System;
using System.Linq;

using DotNetBay.Interfaces;

namespace DotNetBay.Core.Execution
{
    public class Auctioneer
    {
        private readonly IMainRepository repository;

        public Auctioneer(IMainRepository repository)
        {
            this.repository = repository;
        }

        public void ProcessOpenBids()
        {
            // Process all auctions with open bids
            var openAuctions = this.repository.GetAuctions().Where(a => a.Bids.Any(b => b.Accepted == null));

            foreach (var auction in openAuctions)
            {
                var openBids = auction.Bids.Where(b => b.Accepted == null).OrderBy(b => b.ReceivedOnUtc).ToList();

                foreach (var bid in openBids)
                {
                    if (bid.Amount > auction.CurrentPrice)
                    {
                        bid.Accepted = true;
                        auction.CurrentPrice = bid.Amount;
                        auction.LastBid = bid;
                    }
                    else
                    {
                        bid.Accepted = false;
                    } 
                }
            }

            this.repository.SaveChanges();
        }

        public void CloseFinishedAuctions()
        {
            // Process all auctions with open bids
            var auctionsToClose = this.repository.GetAuctions().Where(a => !a.IsClosed && a.EndDateTimeUtc <= DateTime.UtcNow).ToList();

            foreach (var auction in auctionsToClose)
            {
                // Skip any auctions with not processed bids
                if (auction.Bids.Any(b => b.Accepted == null))
                {
                    continue;
                }

                if (auction.Bids.Any())
                {
                    auction.Winner = auction.LastBid.Bidder;
                }

                auction.IsClosed = true;
            }
            
            this.repository.SaveChanges();
        }
    }
}
