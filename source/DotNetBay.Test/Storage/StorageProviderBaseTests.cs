using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using DotNetBay.Interfaces;
using DotNetBay.Model;

using NUnit.Framework;

namespace DotNetBay.Test.Storage
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This are testclasses")]
    public abstract class StorageProviderBaseTests
    {
        [TestCase]
        public void GivenAnEmptyStore_AddOneAuction_NotEmptyAnymore()
        {
            var myAuction = CreateAnAuction();
            myAuction.Seller = CreateAMember();

            Auction auctionFromStore;

            using (var factory = this.CreateFactory())
            {
                var initStore = factory.CreateStore();
                initStore.Add(myAuction);

                var testStore = factory.CreateStore();

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

            using (var factory = this.CreateFactory())
            {
                var initStore = factory.CreateStore();
                initStore.Add(myAuction);

                var testStore = factory.CreateStore();

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

            using (var factory = this.CreateFactory())
            {
                var initStore = factory.CreateStore();
                initStore.Add(myMember);

                var testStore = factory.CreateStore();

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

            IQueryable<Member> allMembersFromStore;
            IQueryable<Auction> allAuctionsFromStore;

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myMember);

                var secondStore = factory.CreateStore();

                // References
                myAuction.Seller = secondStore.GetMembers().FirstOrDefault();
                secondStore.Add(myAuction);

                var testStore = factory.CreateStore();
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
            var bid = new Bid()
            {
                Auction = myAuction,
                Bidder = theBidder,
                Amount = 12
            };

            List<Auction> allAuctionsFromStore;

            using (var factory = this.CreateFactory())
            {
                var testStore = factory.CreateStore();
                testStore.Add(myAuction);
                testStore.Add(theBidder);
                testStore.Add(bid);

                allAuctionsFromStore = testStore.GetAuctions().ToList();
            }

            // Sanity check
            Assert.AreEqual(1, allAuctionsFromStore.Count());
            Assert.IsNotNull(allAuctionsFromStore[0].Bids);

            Assert.AreEqual(1, allAuctionsFromStore[0].Bids.Count);
            Assert.AreEqual(bid, allAuctionsFromStore[0].Bids[0]);
        }

        [TestCase]
        public void GivenAStoreWithAuctionAndMember_AddBid_RetrievedByTransactionId()
        {
            var theSeller = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = theSeller;

            var theBidder = CreateAMember();
            var bid = new Bid()
            {
                Auction = myAuction,
                Bidder = theBidder,
                Amount = 12
            };

            Bid retrievedBid;

            using (var factory = this.CreateFactory())
            {
                var testStore = factory.CreateStore();
                testStore.Add(theBidder);
                testStore.Add(myAuction);
                testStore.Add(bid);

                retrievedBid = testStore.GetBidByTransactionId(bid.TransactionId);
            }

            Assert.IsNotNull(retrievedBid);
            Assert.AreEqual(bid, retrievedBid);
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

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myMember);
                firstStore.Add(myMember);
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

            using (var factory = this.CreateFactory())
            {
                var testStore = factory.CreateStore();
                testStore.Add(myAuction);
                testStore.Add(myAuction);
            }
        }

        [TestCase]
        [ExpectedException(typeof(Exception))]
        public void GivenEmptyStore_AddAuctionAndMemberFromOtherInstance_ShouldRaiseException()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myAuction);

                var testStore = factory.CreateStore();
                testStore.Add(myAuction);
            }
        }

        [TestCase]
        [ExpectedException(typeof(Exception))]
        public void GivenEmptyStore_AddMemberWithAuctionsFromOtherInstance_ShouldRaiseException()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();
            var otherMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });
            
            otherMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myMember);

                var testSore = factory.CreateStore();
                testSore.Add(otherMember);
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

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myAuction);

                var testStore = factory.CreateStore();
                allAuctionFromStore = testStore.GetAuctions();
                allMembersFromStore = testStore.GetMembers();
            }

            Assert.AreEqual(1, allAuctionFromStore.Count(), "There should be exact 1 auction");
            Assert.AreEqual(1, allMembersFromStore.Count(), "There should be exact 1 member");

            Assert.AreEqual(allAuctionFromStore.FirstOrDefault().Seller, allMembersFromStore.FirstOrDefault());
            Assert.AreEqual(allMembersFromStore.FirstOrDefault().Auctions.FirstOrDefault(), allAuctionFromStore.FirstOrDefault());
        }

        [TestCase]
        public void AuctionWithImage_IsSavedInStore_CanBeRetrievedAfterwards()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();
            myAuction.Seller = myMember;

            var myImage = Guid.NewGuid().ToByteArray();
            myAuction.Image = myImage;

            byte[] imageFromStore;

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myAuction);

                var testStore = factory.CreateStore();
                imageFromStore = testStore.GetAuctions().First().Image;
            }

            Assert.AreEqual(myImage, imageFromStore);
        }

        [TestCase]
        public void AuctionWithImage_IsUpdatedWithNoImage_ImageIsGone()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();
            myAuction.Seller = myMember;

            var myImage = Guid.NewGuid().ToByteArray();
            myAuction.Image = myImage;

            byte[] imageFromStore;

            using (var factory = this.CreateFactory())
            {
                var firstStore = factory.CreateStore();
                firstStore.Add(myAuction);

                var secondStore = factory.CreateStore();
                var auctionFromStore = secondStore.GetAuctions().First();
                auctionFromStore.Image = null;
                secondStore.Update(auctionFromStore);

                var testStore = factory.CreateStore();
                imageFromStore = testStore.GetAuctions().First().Image;
            }

            Assert.IsNull(imageFromStore);
        }

        protected abstract IDataStoreFactory CreateFactory();

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

        #endregion
    }
}
