using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetBay.Core.Test
{
    [TestClass]
    public class FileStorageStoreTests
    {
        #region Create Helpers

        private static Auction CreateAnAuction()
        {
            var myAuction = new Auction()
            {
                Title = "TitleOfTheAuction",
                StartPrice = 50.5,
                StartDateTimeUtc = DateTime.UtcNow.AddDays(10),
            };

            return myAuction;
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

        [TestMethod]
        public void GivenAnEmptyStore_AddOneAuction_NotEmptyAnymore()
        {
            var myAuction = CreateAnAuction();
            myAuction.Seller = CreateAMember();

            Auction auctionFromStore = null;

            using (var tempFile = new TempFile())
            {
                var initStore = new FileStorageProvider(tempFile.FullPath);
                initStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);

                auctionFromStore = testStore.GetAuctions().FirstOrDefault();
            }

            Assert.IsNotNull(auctionFromStore);
        }

        [TestMethod]
        public void GivenEmptyStore_AddAuctionWithSeller_AuctionAndMemberAreStoredIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            myAuction.Seller = myMember;

            Member memberFromStore = null;
            Auction auctionFromStore = null;

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

        [TestMethod]
        public void GivenEmptyStore_AddAMemberWithAuctions_MemberAndAuctionsAreStoredIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            Member memberForStore = null;
            Auction auctionFromStore = null;

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

        [TestMethod]
        public void GivenAnExistingMember_AddAuctionWithExistingMemberAsSeller_AuctionIsAttachedToMember()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = myMember;

            IQueryable<Member> allMembersFromStore = null;
            IQueryable<Auction> allAuctionFromStore = null;
            
            using (var tempFile = new TempFile())
            {
                var firstStore = new FileStorageProvider(tempFile.FullPath);
                firstStore.Add(myMember);

                var secondStore = new FileStorageProvider(tempFile.FullPath);
                secondStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);
                allAuctionFromStore = testStore.GetAuctions();
                allMembersFromStore = testStore.GetMembers();
            }

            Assert.AreEqual(1, allAuctionFromStore.Count(), "There should be exact 1 auction");
            Assert.AreEqual(1, allMembersFromStore.Count(), "There should be exact 1 member");

            Assert.IsNotNull(allMembersFromStore, "memberForStore != null");
            Assert.IsNotNull(allMembersFromStore.First().Auctions, "memberForStore.Auctions != null");
            
            Assert.AreEqual(1, allMembersFromStore.First().Auctions.Count(), "There should be a auction attached to the member");
            Assert.AreEqual(myAuction.Id, allMembersFromStore.First().Auctions.First().Id);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
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

        [TestMethod]
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

        [TestMethod]
        public void GivenAnEmptyStore_AddAuctionAndMember_ReferencesShouldBeEqual()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            IQueryable<Member> allMembersFromStore = null;
            IQueryable<Auction> allAuctionFromStore = null;

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
