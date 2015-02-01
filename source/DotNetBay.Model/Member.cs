using System;
using System.Collections.Generic;

namespace DotNetBay.Model
{
    public class Member : IEquatable<Member>
    {
        #region Equality overrides

        public bool Equals(Member other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(this.UniqueId, other.UniqueId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Member) obj);
        }

        public override int GetHashCode()
        {
            return (this.UniqueId != null ? this.UniqueId.GetHashCode() : 0);
        }

        public static bool operator ==(Member left, Member right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Member left, Member right)
        {
            return !Equals(left, right);
        }


        #endregion

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