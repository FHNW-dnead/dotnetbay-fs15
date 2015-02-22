using System;
using System.Collections.Generic;

namespace DotNetBay.Model
{
    public class Auction
    {
        public Auction()
        {
            this.Bids = new List<Bid>();
        }

        public long Id { get; set; }

        public double StartPrice { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }

        public byte[] Image { get; set; }

        public double CurrentPrice { get; set; }

        /// <summary>
        /// Gets or sets the UTC DateTime values to avoid wrong data when serializing the values
        /// </summary>
        public DateTime StartDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the UTC DateTime values to avoid wrong data when serializing the values
        /// </summary>
        public DateTime EndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the UTC DateTime values to avoid wrong data when serializing the values
        /// </summary>
        public DateTime CloseDateTimeUtc { get; set; }

        public Member Seller { get; set; }

        public Member Winner { get; set; }

        public List<Bid> Bids { get; set; }

        public Bid ActiveBid { get; set; }

        public bool IsClosed { get; set; }

        public bool IsRunning { get; set; }
    }
}