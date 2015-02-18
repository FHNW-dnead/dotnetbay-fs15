using System;
using System.Threading;

using DotNetBay.Interfaces;

namespace DotNetBay.Core.Execution
{
    public class AuctionRunner : IAuctionRunner, IDisposable
    {
        private readonly IMainRepository repository;

        private readonly TimeSpan checkInterval;

        private readonly Auctioneer auctioneer;
        
        private Timer timer;

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
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.timer != null)
                {
                    this.timer.Dispose();
                    this.timer = null;
                }
            }

            // free native resources if there are any.
        }

        private void Callback(object state)
        {
            this.auctioneer.DoAllWork();
        }
    }
}