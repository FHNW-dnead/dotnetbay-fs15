using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetBay.Interfaces;
using DotNetBay.Model;
using Newtonsoft.Json;

namespace DotNetBay.Data.FileStorage
{
    public class FileStorage : IAuctionStore
    {
        // Poor-Mans locking mechanism
        readonly object syncRoot = new object();

        bool isLoaded = false;
        private string fullPath;

        private DataRootElement data;

        public FileStorage(string fileName)
        {
            // It's good practice to expect either absolute or relative paths and handle both the same
            this.fullPath = Path.GetFullPath(fileName);
        }

        public void Load()
        {
            if (!File.Exists(this.fullPath))
            {
                var file = File.Create(this.fullPath);
                file.Close();
            }

            var content = File.ReadAllText(this.fullPath);

            var restored = JsonConvert.DeserializeObject<DataRootElement>(content);

            if (restored != null)
            {
                this.data = restored;
            }
            else
            {
                this.data = new DataRootElement();
            }
        }

        public void Save()
        {
            var content = JsonConvert.SerializeObject(this.data);
            
            File.WriteAllText(this.fullPath, content);
        }

        private void EnsureCompleteLoaded()
        {
            if (!this.isLoaded || this.data == null || this.data.Auctions == null || this.data.Bids == null || this.data.Members == null)
            {
                this.Load();
            }
        }

        public Auction Add(Auction newOne)
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                this.data.Auctions.Add(newOne);
                this.Save();

                return newOne;
            }
        }

        public Auction Update(Auction auction)
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                // TODO Update

                return null;
            }
        }

        public Bid Add(Bid bid)
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                // TODO: Add

                return null;
            }
        }

        public Bid GetBid(Guid transactionId)
        {
            this.EnsureCompleteLoaded();

            return null;
        }

        public IQueryable<Auction> GetAuctions()
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                return new List<Auction>().AsQueryable();
            }
        }
    }
}
