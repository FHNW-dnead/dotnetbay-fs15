using System.Collections.Generic;
using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Data.FileStorage
{
    public class FileStorage : IAuctionStore
    {
        public void Add(Auction newOne)
        {
            
        }

        public List<Auction> GetAll()
        {
            return new List<Auction>();
        }
    }
}
