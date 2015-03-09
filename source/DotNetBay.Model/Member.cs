using System.Collections.Generic;

namespace DotNetBay.Model
{
    public class Member
    {
        public Member()
        {
            this.Auctions = new List<Auction>();
        }

        public long Id { get; set; }

        public string UniqueId { get; set; }

        public string DisplayName { get; set; }

        public string EMail { get; set; }

        public List<Auction> Auctions { get; set; }

        public List<Bid> Bids { get; set; } 
    }
}