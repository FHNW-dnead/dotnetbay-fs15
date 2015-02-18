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
            var userService = new SimpleMemberService(repo);
            var service = new AuctionService(repo, userService);

            var auction = CreateGeneratedAuction();

            auction.Seller = userService.Add("Seller", "seller@mail.com");

            var memberService = userService;

            service.Save(auction);

            var bidder = memberService.Add("Michael", "michael.schnyder@fhnw.ch");
            
            service.PlaceBid(bidder, auction, 51);

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
