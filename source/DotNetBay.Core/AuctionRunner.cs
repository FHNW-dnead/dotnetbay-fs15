using System;
using System.Threading;

using DotNetBay.Interfaces;

namespace DotNetBay.Core
{
    public class AuctionRunner : IAuctionRunner
    {
        private readonly IMainRepository mainRepository;

        private readonly TimeSpan checkInterval;

        private readonly Timer timer;

        private readonly Auctioneer auctioneer;

        public AuctionRunner(IMainRepository mainRepository)
            : this(mainRepository, TimeSpan.FromSeconds(5))
        {
        }

        public AuctionRunner(IMainRepository mainRepository, TimeSpan checkInterval)
        {
            this.mainRepository = mainRepository;
            this.checkInterval = checkInterval;
            this.timer = new Timer(this.Callback, null, Timeout.Infinite, Timeout.Infinite);

            this.auctioneer = new Auctioneer(mainRepository);
        }

        public void Start()
        {
            this.timer.Change(this.checkInterval, this.checkInterval);
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Callback(object state)
        {
            this.auctioneer.ProcessOpenBids();

            this.auctioneer.CloseFinishedAuctions();
        }
    }
}