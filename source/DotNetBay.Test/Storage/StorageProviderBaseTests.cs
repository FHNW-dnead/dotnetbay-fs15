using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;
using NUnit.Framework;

namespace DotNetBay.Test.Storage
{
    public abstract class StorageProviderBaseTests
    {
        #region Create Helpers

        private static Auction CreateAnAuction()
        {
            return new Auction()
            {
                Title = "TitleOfTheAuction",
                StartPrice = 50.5,
                StartDateTimeUtc = DateTime.UtcNow.AddDays(10),
            };
        }

        private static Member CreateAMember()
        {
            return new Member()
            {
                Name = "GeneratedMember",
                UniqueId = "UniqueId" + Guid.NewGuid()
            };
        }

        private static Bid CreateABid()
        {
            return new Bid
            {
                Amount = 99.9,
            };
        }

        #endregion

        [TestCase]
        public void GivenAnEmptyStore_AddOneAuction_NotEmptyAnymore()
        {
            var myAuction = CreateAnAuction();
            myAuction.Seller = CreateAMember();

            Auction auctionFromStore;

            using (var tempFile = new TempFile())
            {
                var initStore = new FileStorageProvider(tempFile.FullPath);
                initStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);

                auctionFromStore = testStore.GetAuctions().FirstOrDefault();
            }

            Assert.IsNotNull(auctionFromStore);
        }

        [TestCase]
        public void GivenEmptyStore_AddAuctionWithSeller_AuctionAndMemberAreStoredIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            myAuction.Seller = myMember;

            Member memberFromStore;
            Auction auctionFromStore;

            using (var tempFile = new TempFile())
            {
                var initStore = new FileStorageProvider(tempFile.FullPath);
                initStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);

                auctionFromStore = testStore.GetAuctions().FirstOrDefault();
                memberFromStore = testStore.GetMembers().FirstOrDefault();
            }

            Assert.IsNotNull(auctionFromStore, "auctionFromStore != null");
            Assert.IsNotNull(auctionFromStore.Seller, "auctionFromStore.Seller != null");

            Assert.IsNotNull(memberFromStore, "memberForStore != null");
            Assert.IsNotNull(memberFromStore.Auctions, "memberForStore.Auctions != null");
            Assert.AreEqual(1, memberFromStore.Auctions.Count, "There should be exact one euction for this member");

            Assert.AreEqual(myAuction.Title, auctionFromStore.Title, "Auction's title is not the same");
            Assert.AreEqual(myMember.UniqueId, memberFromStore.UniqueId, "Member's uniqueId is not the same");
            Assert.AreEqual(1, memberFromStore.Auctions.Count, "There should be exact one euction for this member");
        }

        [TestCase]
        public void GivenEmptyStore_AddAMemberWithAuctions_MemberAndAuctionsAreStoredIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            Member memberForStore;
            Auction auctionFromStore;

            using (var tempFile = new TempFile())
            {
                var initStore = new FileStorageProvider(tempFile.FullPath);
                initStore.Add(myMember);

                var testStore = new FileStorageProvider(tempFile.FullPath);

                memberForStore = testStore.GetMembers().FirstOrDefault();
                auctionFromStore = testStore.GetAuctions().FirstOrDefault();
            }

            Assert.IsNotNull(memberForStore, "memberForStore != null");
            Assert.IsNotNull(memberForStore.Auctions, "memberForStore.Auctions != null");
            Assert.AreEqual(1, memberForStore.Auctions.Count, "There should be exact one euction for this member");

            Assert.IsNotNull(auctionFromStore, "auctionFromStore != null");
            Assert.IsNotNull(auctionFromStore.Seller, "auctionFromStore.Seller != null");
            Assert.AreEqual(auctionFromStore.Seller.UniqueId, myMember.UniqueId);
        }

        [TestCase]
        public void GivenAnExistingMember_AddAuctionWithExistingMemberAsSeller_AuctionIsAttachedToMember()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = myMember;

            IQueryable<Member> allMembersFromStore;
            IQueryable<Auction> allAuctionsFromStore;
            
            using (var tempFile = new TempFile())
            {
                var firstStore = new FileStorageProvider(tempFile.FullPath);
                firstStore.Add(myMember);

                var secondStore = new FileStorageProvider(tempFile.FullPath);
                secondStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);
                allAuctionsFromStore = testStore.GetAuctions();
                allMembersFromStore = testStore.GetMembers();
            }

            Assert.AreEqual(1, allAuctionsFromStore.Count(), "There should be exact 1 auction");
            Assert.AreEqual(1, allMembersFromStore.Count(), "There should be exact 1 member");

            Assert.IsNotNull(allMembersFromStore, "memberForStore != null");
            Assert.IsNotNull(allMembersFromStore.First().Auctions, "memberForStore.Auctions != null");
            
            Assert.AreEqual(1, allMembersFromStore.First().Auctions.Count(), "There should be a auction attached to the member");
            Assert.AreEqual(myAuction.Id, allMembersFromStore.First().Auctions.First().Id);
        }

        [TestCase]
        public void GivenAStoreWithAuctionAndMember_AddBid_BidGetsListedInAuction()
        {
            var theSeller = CreateAMember();
            var myAuction = CreateAnAuction();
            
            // References
            myAuction.Seller = theSeller;

            var theBidder = CreateAMember();
            var aBid = new Bid()
            {
                Auction = myAuction,
                Bidder = theBidder,
                Amount = 12
            };

            List<Auction> allAuctionsFromStore;

            using (var tempFile = new TempFile())
            {
                var testStore = new FileStorageProvider(tempFile.FullPath);
                testStore.Add(myAuction);
                testStore.Add(theBidder);
                testStore.Add(aBid);

                allAuctionsFromStore = testStore.GetAuctions().ToList();
            }

            // Sanity check
            Assert.AreEqual(1, allAuctionsFromStore.Count());
            Assert.IsNotNull(allAuctionsFromStore[0].Bids);

            Assert.AreEqual(1, allAuctionsFromStore[0].Bids.Count);
            Assert.AreEqual(aBid, allAuctionsFromStore[0].Bids[0]);
        }

        [TestCase]
        public void GivenAStoreWithAuctionAndMember_AddBid_RetrievedByTransactionId()
        {
            var theSeller = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = theSeller;

            var theBidder = CreateAMember();
            var aBid = new Bid()
            {
                Auction = myAuction,
                Bidder = theBidder,
                Amount = 12
            };

            Bid retrievedBid;

            using (var tempFile = new TempFile())
            {
                var testStore = new FileStorageProvider(tempFile.FullPath);
                testStore.Add(theBidder);
                testStore.Add(myAuction);
                testStore.Add(aBid);

                retrievedBid = testStore.GetBidByTransactionId(aBid.TransactionId);
            }

            Assert.IsNotNull(retrievedBid);
            Assert.AreEqual(aBid, retrievedBid);
        }

        [TestCase]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenAStoreWithMember_AddMemberAgain_ShouldRaiseExecption()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var tempFile = new TempFile())
            {
                var firstStore = new FileStorageProvider(tempFile.FullPath);
                firstStore.Add(myMember);

                var testSore = new FileStorageProvider(tempFile.FullPath);
                testSore.Add(myMember);
            }
        }

        [TestCase]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenAStoreWithAuction_AddAuctionAgain_ShouldRaiseExecption()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var tempFile = new TempFile())
            {
                var firstStore = new FileStorageProvider(tempFile.FullPath);
                firstStore.Add(myAuction);

                var testSore = new FileStorageProvider(tempFile.FullPath);
                testSore.Add(myAuction);
            }
        }

        [TestCase]
        public void GivenAnEmptyStore_AddAuctionAndMember_ReferencesShouldBeEqual()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            IQueryable<Member> allMembersFromStore;
            IQueryable<Auction> allAuctionFromStore;

            using (var tempFile = new TempFile())
            {
                var firstStore = new FileStorageProvider(tempFile.FullPath);
                firstStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);
                allAuctionFromStore = testStore.GetAuctions();
                allMembersFromStore = testStore.GetMembers();
            }

            Assert.AreEqual(1, allAuctionFromStore.Count(), "There should be exact 1 auction");
            Assert.AreEqual(1, allMembersFromStore.Count(), "There should be exact 1 member");

            Assert.AreEqual(allAuctionFromStore.FirstOrDefault().Seller, allMembersFromStore.FirstOrDefault());
            Assert.AreEqual(allMembersFromStore.FirstOrDefault().Auctions.FirstOrDefault(), allAuctionFromStore.FirstOrDefault());
        }
    }
}
