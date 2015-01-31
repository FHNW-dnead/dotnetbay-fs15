using System.Collections.Generic;
using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core.Service
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionStore auctionStore;

        public AuctionService(IAuctionStore auctionStore)
        {
            this.auctionStore = auctionStore;
        }

        public List<Auction> GetAllAuctions()
        {
            return this.auctionStore.GetAll();
        }
    }
}