using System;
using System.Threading;

using DotNetBay.Interfaces;

namespace DotNetBay.Core
{
    public class AuctionRunner : IAuctionRunner
    {
        private readonly IMainRepository mainRepository;

        private readonly Timer timer;

        private Auctioneer auctioneer;

        public AuctionRunner(IMainRepository mainRepository)
        {
            this.mainRepository = mainRepository;
            this.timer = new Timer(this.Callback, null, Timeout.Infinite, Timeout.Infinite);

            this.auctioneer = new Auctioneer(mainRepository);
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
            this.auctioneer.ProcessOpenBids();

            this.auctioneer.CloseFinishedAuctions();
        }
    }
}