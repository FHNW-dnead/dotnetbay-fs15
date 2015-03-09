using System;
using System.Collections.Generic;
using System.Linq;

using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Data.FileStorage
{
    public class InMemoryMainRepository : IMainRepository
    {
        private readonly object syncRoot = new object();

        private bool isLoaded;

        private DataRootElement data;

        #region Interface Implementation

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
                    return auction;
                }

                var maxId = this.data.Auctions.Any() ? this.data.Auctions.Max(a => a.Id) : 0;
                auction.Id = maxId + 1;

                this.data.Auctions.Add(auction);

                // Add auction to sellers list of auctions
                if (seller.Auctions.All(a => a.Id != auction.Id))
                {
                    seller.Auctions.Add(auction);
                }

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
                    return member;
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

                // Reference back from bidder
                var bidder = this.data.Members.FirstOrDefault(b => b.UniqueId == bid.Bidder.UniqueId);
                if (bidder.Bids == null)
                {
                    bidder.Bids = new List<Bid>(new[] { bid });
                }
                else if (!bidder.Bids.Contains(bid))
                {
                    bidder.Bids.Add(bid);
                }

                return bid;
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

        public virtual void SaveChanges()
        {
            lock (this.syncRoot)
            {
                this.EnsureCompleteLoaded();

                this.ThrowForInvalidReferences();

                this.Save();
            }
        }

        #endregion

        #region Factory Methods

        internal virtual DataRootElement LoadData()
        {
            return new DataRootElement();
        }

        internal virtual void SaveData(DataRootElement data)
        {
        }

        #endregion

        #region Before / After Save Hooks

        internal virtual void BeforeLoad(DataRootElement data)
        {
        }

        internal virtual void AfterLoad(DataRootElement data)
        {
        }

        internal virtual void BeforeSave(DataRootElement data)
        {
        }

        internal virtual void AfterSave(DataRootElement data)
        {
        }

        #endregion

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
            this.ThrowIfReferenceNotFound(auction, x => x.ActiveBid, this.data.Bids, r => r.Id);
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

            if (referencedElementsToTest.Any(element => !resolvedElementsById.Contains<TNavigationElementType>(element)))
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

        #region Internals

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
                this.BeforeLoad(this.data);

                var restored = this.LoadData();

                this.data = restored ?? new DataRootElement();

                this.isLoaded = true;

                this.AfterLoad(this.data);
            }
        }

        private void Save()
        {
            lock (this.syncRoot)
            {
                this.BeforeSave(this.data);

                this.SaveData(this.data);

                this.AfterSave(this.data);
            }
        }

        #endregion
    }
}