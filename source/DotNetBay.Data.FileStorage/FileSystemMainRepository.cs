using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotNetBay.Interfaces;
using DotNetBay.Model;

using Newtonsoft.Json;

namespace DotNetBay.Data.FileStorage
{
    public class FileSystemMainRepository : IMainRepository
    {
        // Poor-Mans locking mechanism
        private readonly object syncRoot = new object();
        private readonly string fullPath;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly string rootDirectory;

        private bool isLoaded;

        private DataRootElement data;
        private string dataPath;

        public FileSystemMainRepository(string fileName)
        {
            // It's good practice to expect either absolute or relative paths and handle both the same
            this.fullPath = Path.GetFullPath(fileName);
            this.rootDirectory = Path.GetDirectoryName(this.fullPath);
            this.dataPath = Path.Combine(this.rootDirectory, "fileContent");

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        public Auction Add(Auction auction)
        {
            if (auction.Seller == null)
            {
                throw new ArgumentException("Its required to set a seller");
            }

            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                // Add Member (from Seller) if not yet exists
                var seller = this.data.Members.FirstOrDefault(m => m.UniqueId == auction.Seller.UniqueId);

                // Create member as seller if not exists
                if (seller == null)
                {
                    // The seller does not yet exist in store
                    seller = auction.Seller;
                    seller.Auctions = new List<Auction>(new[] { auction });
                    this.data.Members.Add(seller);
                }

                this.ThrowForInvalidReferences(auction);

                if (this.data.Auctions.Any(a => a.Id == auction.Id))
                {
                    throw new ArgumentException("The auction is already stored");
                }

                var maxId = this.data.Auctions.Any() ? this.data.Auctions.Max(a => a.Id) : 0;
                auction.Id = maxId + 1;

                this.data.Auctions.Add(auction);

                // Add auction to sellers list of auctions
                if (seller.Auctions.All(a => a.Id != auction.Id))
                {
                    seller.Auctions.Add(auction);
                }

                this.Save();

                return auction;
            }
        }

        public Member Add(Member member)
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                if (this.data.Members.Any(m => m.UniqueId == member.UniqueId))
                {
                    throw new ArgumentException("A member with the same uniqueId already exists!");
                }

                this.ThrowForInvalidReferences(member);

                this.data.Members.Add(member);

                if (member.Auctions != null && member.Auctions.Any())
                {
                    foreach (var auction in member.Auctions)
                    {
                        this.Add(auction);
                    }
                }

                this.Save();

                return member;
            }
        }

        public Auction Update(Auction auction)
        {
            if (auction.Seller == null)
            {
                throw new ArgumentException("Its required to set a seller");
            }

            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                if (this.data.Auctions.All(a => a.Id != auction.Id))
                {
                    throw new ApplicationException("This auction does not exist and cannot be updated!");
                }

                this.ThrowForInvalidReferences(auction);

                foreach (var bid in auction.Bids)
                {
                    bid.Auction = auction;

                    if (!this.data.Bids.Contains(bid))
                    {
                        this.data.Bids.Add(bid);
                    }
                }

                this.Save();

                return auction;
            }
        }

        public Bid Add(Bid bid)
        {
            if (bid.Bidder == null)
            {
                throw new ArgumentException("Its required to set a bidder");
            }

            if (bid.Auction == null)
            {
                throw new ArgumentException("Its required to set an auction");
            }

            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                // Does the auction exist?
                if (this.data.Auctions.All(a => a.Id != bid.Auction.Id))
                {
                    throw new ApplicationException("This auction does not exist an cannot be added this way!");
                }

                // Does the member exist?
                if (this.data.Members.All(a => a.UniqueId != bid.Bidder.UniqueId))
                {
                    throw new ApplicationException("the bidder does not exist and cannot be added this way!");
                }

                this.ThrowForInvalidReferences(bid);

                var maxId = this.data.Bids.Any() ? this.data.Bids.Max(a => a.Id) : 0;
                bid.Id = maxId + 1;
                bid.Accepted = null;
                bid.TransactionId = Guid.NewGuid();
                
                this.data.Bids.Add(bid);

                // Reference back from auction
                var auction = this.data.Auctions.FirstOrDefault(a => a.Id == bid.Auction.Id);
                auction.Bids.Add(bid);

                return null;
            }
        }

        public Bid GetBidByTransactionId(Guid transactionId)
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                return this.data.Bids.FirstOrDefault(b => b.TransactionId == transactionId);
            }
        }

        public IQueryable<Auction> GetAuctions()
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                return this.data.Auctions.AsQueryable();
            }
        }

        public IQueryable<Member> GetMembers()
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                return this.data.Members.AsQueryable();
            }
        }

        protected virtual void AfterLoad()
        {
            // Reload Images from FS
            foreach (var auction in this.data.Auctions)
            {
                auction.Image = this.LoadBinary(string.Format("auction-{0}-image1.jpg", auction.Id));
            }
        }

        protected void BeforeLoad()
        {
            // Factory method to override
        }

        protected void AfterSave()
        {
            // Reload Images from FS
            foreach (var auction in this.data.Auctions)
            {
                auction.Image = this.LoadBinary(string.Format("auction-{0}-image1.jpg", auction.Id));
            }
        }

        protected void BeforeSave()
        {
            // Remove byte values from images and save individually
            foreach (var auction in this.data.Auctions)
            {
                this.SaveBinary(string.Format("auction-{0}-image1.jpg", auction.Id), auction.Image);
                auction.Image = null;
            }
        }

        #region Reference Checks

        private void ThrowForInvalidReferences()
        {
            foreach (var auction in this.data.Auctions)
            {
                this.ThrowForInvalidReferences(auction);
            }

            foreach (var member in this.data.Members)
            {
                this.ThrowForInvalidReferences(member);
            }

            foreach (var bid in this.data.Bids)
            {
                this.ThrowForInvalidReferences(bid);
            }
        }

        private void ThrowForInvalidReferences(Auction auction)
        {
            // Check References
            this.ThrowIfReferenceNotFound(auction, x => x.Bids, this.data.Bids, r => r.Id);
            this.ThrowIfReferenceNotFound(auction, x => x.Seller, this.data.Members, r => r.UniqueId);
            this.ThrowIfReferenceNotFound(auction, x => x.Winner, this.data.Members, r => r.UniqueId);
        }

        private void ThrowForInvalidReferences(Bid bid)
        {
            this.ThrowIfReferenceNotFound(bid, x => x.Auction, this.data.Auctions, r => r.Id);
            this.ThrowIfReferenceNotFound(bid, x => x.Bidder, this.data.Members, r => r.UniqueId);
        }

        private void ThrowForInvalidReferences(Member member)
        {
            this.ThrowIfReferenceNotFound(member, x => x.Auctions, this.data.Auctions, r => r.Id);
            this.ThrowIfReferenceNotFound(member, x => x.Bids, this.data.Bids, r => r.Id);
        }

        private void ThrowIfReferenceNotFound<TRootElementType, TNavigationElementType>(
            TRootElementType obj,
            Func<TRootElementType, List<TNavigationElementType>> navigationAccessor,
            IEnumerable<TNavigationElementType> validInstances,
            Func<TNavigationElementType, object> identificationAccessor)
        {
            var value = navigationAccessor(obj);

            if (value == null)
            {
                return;
            }

            var referencedElementsToTest = value.Where(x => validInstances.Any(r => identificationAccessor(r) == identificationAccessor(x)));
            var resolvedElementsById = validInstances.Where(x => referencedElementsToTest.Any(r => identificationAccessor(r).Equals(identificationAccessor(x))));

            if (referencedElementsToTest.Any(element => !resolvedElementsById.Contains(element)))
            {
                throw new Exception("Unable to process objects across contexts!");
            }
        }

        private void ThrowIfReferenceNotFound<TRootElementType, TNavigationElementType>(
            TRootElementType obj,
            Func<TRootElementType, TNavigationElementType> navigationAccessor,
            IEnumerable<TNavigationElementType> validInstances,
            Func<TNavigationElementType, object> identificationAccessor) where TNavigationElementType : class
        {
            var referencedElement = navigationAccessor(obj);

            if (referencedElement == null)
            {
                return;
            }

            var resolvedElementById = validInstances.FirstOrDefault(x => identificationAccessor(x).Equals(identificationAccessor(referencedElement)));

            if (referencedElement != resolvedElementById)
            {
                throw new Exception("Unable to process objects across contexts!");
            }
        }

        #endregion

        #region Binary-Fields (Save/Loading)

        private void SaveBinary(string fileName, byte[] fileContent)
        {
            var fullFileName = Path.Combine(this.rootDirectory, fileName);

            if (fileContent == null)
            {
                try
                {
                    File.Delete(fullFileName);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                File.WriteAllBytes(fullFileName, fileContent);
            }
        }

        private byte[] LoadBinary(string fileName)
        {
            var fullFileName = Path.Combine(this.rootDirectory, fileName);

            if (File.Exists(fullFileName))
            {
                return File.ReadAllBytes(fullFileName);
            }

            return null;
        }


        #endregion

        #region Persistence

        private void EnsureCompleteLoaded()
        {
            if (!this.isLoaded || this.data == null || this.data.Auctions == null || this.data.Bids == null
                || this.data.Members == null)
            {
                this.Load();
            }
        }

        private void Load()
        {
            lock (this.syncRoot)
            {
                // Ensure existence of directory
                Directory.CreateDirectory(this.rootDirectory);

                this.BeforeLoad();

                if (!File.Exists(this.fullPath))
                {
                    var file = File.Create(this.fullPath);
                    file.Close();
                }

                var content = File.ReadAllText(this.fullPath);

                var restored = JsonConvert.DeserializeObject<DataRootElement>(content, this.jsonSerializerSettings);

                this.data = restored ?? new DataRootElement();

                this.isLoaded = true;

                this.AfterLoad();
            }
        }

        private void Save()
        {
            lock (this.syncRoot)
            {
                // Ensure existence of directory
                Directory.CreateDirectory(this.rootDirectory);

                this.BeforeSave();

                var content = JsonConvert.SerializeObject(this.data, this.jsonSerializerSettings);

                File.WriteAllText(this.fullPath, content);

                this.AfterSave();
            }
        }


        #endregion
    }
}
