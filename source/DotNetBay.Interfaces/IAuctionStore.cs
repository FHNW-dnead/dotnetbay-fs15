using System;
using System.Linq;
using DotNetBay.Model;

namespace DotNetBay.Interfaces
{
    public interface IAuctionStore
    {
        IQueryable<Auction> GetAuctions();
        
        Auction Add(Auction auction);
        Auction Update(Auction auction);
        
        Bid Add(Bid bid);

        Bid GetBid(Guid transactionId);
    }
}