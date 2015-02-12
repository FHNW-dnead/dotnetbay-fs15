using System.Linq;

using DotNetBay.Interfaces;

namespace DotNetBay.Core
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
            }

        }

        public void CloseFinishedAuctions()
        {
            
        }
    }
}
