using System;
using System.Threading;

using DotNetBay.Interfaces;

namespace DotNetBay.Core.Execution
{
    public class AuctionRunner : IAuctionRunner, IDisposable
    {
        private readonly IMainRepository repository;

        private readonly TimeSpan checkInterval;

        private readonly Timer timer;

        private readonly Auctioneer auctioneer;

        public AuctionRunner(IMainRepository repository)
            : this(repository, TimeSpan.FromSeconds(5))
        {
        }

        public AuctionRunner(IMainRepository repository, TimeSpan checkInterval)
        {
            this.repository = repository;
            this.checkInterval = checkInterval;
            this.timer = new Timer(this.Callback, null, Timeout.Infinite, Timeout.Infinite);

            this.auctioneer = new Auctioneer(repository);
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