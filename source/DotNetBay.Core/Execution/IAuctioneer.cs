using System;

namespace DotNetBay.Core.Execution
{
    public interface IAuctioneer
    {
        event EventHandler<ProcessedBidEventArgs> BidDeclined;

        event EventHandler<ProcessedBidEventArgs> BidAccepted;

        event EventHandler<AuctionEventArgs> AuctionEnded;
    }
}