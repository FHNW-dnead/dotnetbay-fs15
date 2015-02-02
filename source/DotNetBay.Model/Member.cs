using System.Collections.Generic;

namespace DotNetBay.Model
{
    public class Member
    {
        public Member()
        {
            this.Auctions = new List<Auction>();
        }

        public string UniqueId { get; set; }

        public string Name { get; set; }

        public List<Auction> Auctions { get; set; }

        public List<Bid> Bids { get; set; } 
    }
}