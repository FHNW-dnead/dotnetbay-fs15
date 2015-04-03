using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using DotNetBay.Core;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;

using NUnit.Framework;

namespace DotNetBay.Test.Core
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This is a testclass")]
    public class AuctionServiceTests
    {
        [TestCase]
        public void GivenAProperService_SavesAValidAuction_ShouldReturnSameFromAuctionList()
        {
            var repo = new InMemoryMainRepository();
            var userService = new SimpleMemberService(repo);
            var service = new AuctionService(repo, userService);

            var auction = CreateGeneratedAuction();

            auction.Seller = userService.Add("Seller", "seller@mail.com");

            service.Save(auction);

            var auctionFromService = service.GetAll().First();
            Assert.AreEqual(auctionFromService, auction);
        }

        [TestCase]
        public void WithExistingAuction_AfterPlacingABid_TheBidShouldBeAssignedToAuctionAndUser()
        {
            var repo = new InMemoryMainRepository();
            var userService = new MockedMemberService(repo);
            var service = new AuctionService(repo, userService);

            var auction = CreateGeneratedAuction();

            auction.Seller = userService.Add("Seller", "seller@mail.com");

            service.Save(auction);

            // Litte hack: Manual change of start time
            auction.StartDateTimeUtc = DateTime.UtcNow.AddDays(-1);

            var bidder = userService.Add("Michael", "michael.schnyder@fhnw.ch");
            userService.SetCurrentMember(bidder);

            service.PlaceBid(auction, 51);

            Assert.AreEqual(1, auction.Bids.Count);
            Assert.AreEqual(1, bidder.Bids.Count);
        }

        [TestCase]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenAddingAnAuction_WithUnknownMember_RaisesException()
        {
            var auction = CreateGeneratedAuction();

            var repo = new InMemoryMainRepository();

            var service = new AuctionService(repo, new SimpleMemberService(repo));
            var memberService = new SimpleMemberService(repo);

            service.Save(auction);
        }

        [TestCase]
        [ExpectedException(typeof(Exception))]
        public void PlacingABid_AuctionHasNotYetStarted_RaisesExecption()
        {
            var repo = new InMemoryMainRepository();
            var simpleMemberService = new SimpleMemberService(repo);
            var auction = CreateGeneratedAuction();
            auction.Seller = simpleMemberService.GetCurrentMember();

            var service = new AuctionService(repo, simpleMemberService);

            auction.StartDateTimeUtc = DateTime.UtcNow.AddDays(1);
            service.Save(auction);

            service.PlaceBid(auction, 100);
        }

        [TestCase]
        [ExpectedException(typeof(Exception))]
        public void PlacingABid_AuctionHasExpired_RaisesExecption()
        {
            var repo = new InMemoryMainRepository();
            var simpleMemberService = new SimpleMemberService(repo);
            var auction = CreateGeneratedAuction();
            auction.Seller = simpleMemberService.GetCurrentMember();

            var service = new AuctionService(repo, simpleMemberService);

            auction.StartDateTimeUtc = DateTime.UtcNow.AddDays(-2);
            auction.EndDateTimeUtc = DateTime.UtcNow.AddDays(-2);
            
            repo.Add(auction);

            service.PlaceBid(auction, 100);
        }

        private static Auction CreateGeneratedAuction()
        {
            return new Auction()
                       {
                           Title = "Generated Auction",
                           StartPrice = 50.5,
                           StartDateTimeUtc = DateTime.UtcNow.AddHours(1),
                           EndDateTimeUtc = DateTime.UtcNow.AddHours(2),
                       };
        }
    }
}
