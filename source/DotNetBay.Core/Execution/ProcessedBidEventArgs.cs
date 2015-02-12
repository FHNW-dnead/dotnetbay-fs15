using System;

using DotNetBay.Model;

namespace DotNetBay.Core.Execution
{
    public class ProcessedBidEventArgs : EventArgs
    {
        public Bid Bid { get; set; }

        public Auction Auction { get; set; }
    }
}