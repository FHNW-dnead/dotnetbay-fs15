using System;
using System.Threading;

using DotNetBay.Core.Execution;
using DotNetBay.Interfaces;

namespace DotNetBay.Core
{
    public class AuctionRunner : IAuctionRunner, IDisposable
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

        public IAuctioneer Auctioneer
        {
            get
            {
                return this.auctioneer;
            }
        }

        public void Start()
        {
            this.timer.Change(this.checkInterval, this.checkInterval);
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }

        private void Callback(object state)
        {
            this.auctioneer.DoAllWork();
        }
    }
}