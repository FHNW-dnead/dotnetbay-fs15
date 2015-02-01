using System;
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
        public void GivenEmptyStore_AddAuctionWithSeller_AuctionAndMemberAreStoreIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            myAuction.Seller = myMember;

            Member memberForStore = null;
            Auction auctionFromStore = null;

            using (var tempFile = new TempFile())
            {
                var initStore = new FileStorageProvider(tempFile.FullPath);
                initStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);

                auctionFromStore = testStore.GetAuctions().FirstOrDefault();
                memberForStore = testStore.GetMembers().FirstOrDefault();
            }

            Assert.IsNotNull(auctionFromStore, "auctionFromStore != null");
            Assert.IsNotNull(auctionFromStore.Seller, "auctionFromStore.Seller != null");

            Assert.IsNotNull(memberForStore, "memberForStore != null");
            Assert.IsNotNull(memberForStore.Auctions, "memberForStore.Auctions != null");
            Assert.AreEqual(1, memberForStore.Auctions.Count, "There should be exact one euction for this member");
        }

        [TestMethod]
        public void GivenAnExistingMember_AddAuctionWithExistingMemberAsSeller_AuctionIsAttachedToMember()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();
            myAuction.Seller = myMember;

            Member memberFromStore = null;
            IQueryable<Auction> allAuctionFromStore = null;
            
            using (var tempFile = new TempFile())
            {
                var firstStore = new FileStorageProvider(tempFile.FullPath);
                firstStore.Add(myMember);

                var secondStore = new FileStorageProvider(tempFile.FullPath);
                secondStore.Add(myAuction);

                var testStore = new FileStorageProvider(tempFile.FullPath);
                allAuctionFromStore = testStore.GetAuctions();
                memberFromStore = testStore.GetMembers().FirstOrDefault();
            }

            Assert.IsNotNull(memberFromStore, "memberForStore != null");
            Assert.IsNotNull(memberFromStore.Auctions, "memberForStore.Auctions != null");
            
            Assert.AreEqual(1, allAuctionFromStore.Count());
            Assert.AreEqual(myAuction, memberFromStore.Auctions.First());
        }
    }
}
