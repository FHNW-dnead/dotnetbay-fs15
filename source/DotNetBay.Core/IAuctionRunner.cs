using DotNetBay.Core.Execution;

namespace DotNetBay.Core
{
    public interface IAuctionRunner
    {
        IAuctioneer Auctioneer { get; }

        void Start();

        void Stop();
    }
}
