using System;

namespace DotNetBay.Model
{
    public class Bid
    {
        public long Id { get; set; }

        public DateTime ReceivedOnUtc { get; private set; }

        public Guid TransactionId { get; private set; }

        public Auction Auction { get; set; }

        public Member Bidder { get; set; }

        public double Amount { get; set; }

        public bool? Accepted { get; set; }

        public Bid()
        {
            this.ReceivedOnUtc = DateTime.UtcNow;
            this.TransactionId = new Guid();
        }
    }
}
