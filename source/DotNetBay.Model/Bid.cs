using System;

namespace DotNetBay.Model
{
    public class Bid
    {
        public Bid()
        {
            this.TransactionId = Guid.NewGuid();
        }

        public long Id { get; set; }

        public DateTime ReceivedOnUtc { get; set; }

        public Guid TransactionId { get; set; }

        public Auction Auction { get; set; }

        public Member Bidder { get; set; }

        public double Amount { get; set; }

        public bool? Accepted { get; set; }
    }
}
