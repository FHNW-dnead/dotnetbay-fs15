using System.Collections.Generic;

namespace DotNetBay.Model
{
    public class Member
    {
        public string UniqueId { get; set; }

        public string Name { get; set; }

        public List<Auction> Auctions { get; set; }

        public List<Bid> Bids { get; set; } 

        public Member()
        {
            this.Auctions = new List<Auction>();
        }
    }
}