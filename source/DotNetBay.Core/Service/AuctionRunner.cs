using System;
using System.Linq;
using System.Threading;

using DotNetBay.Interfaces;

namespace DotNetBay.Core.Service
{
    public class AuctionRunner : IAuctionRunner
    {
        private readonly IMainRepository mainRepository;

        private readonly Timer timer;

        public AuctionRunner(IMainRepository mainRepository)
        {
            this.mainRepository = mainRepository;
            this.timer = new Timer(this.Callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            this.timer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Callback(object state)
        {
            // Process all auctions with open bids
            var openAuctions = this.mainRepository.GetAuctions().Where(a => a.Bids.Any(b => b.Accepted == null));

            foreach (var VARIABLE in openAuctions)
            {
                
            }
        }
    }
}