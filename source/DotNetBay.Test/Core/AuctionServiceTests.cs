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
            var service = new AuctionService(new InMemoryMainRepository());

            var auction = CreateGeneratedAuction();
            service.Save(auction);

            var auctionFromService = service.GetAll().First();
            Assert.AreEqual(auctionFromService, auction);
        }

        [TestCase]
        public void WithExistingAuction_AfterPlacingABid_TheBidShouldBeAssignedToAuctionAndUser()
        {
            var auction = CreateGeneratedAuction();
            var bidder = new Member() { Name = "Michael", UniqueId = Guid.NewGuid().ToString() };

            var inMemoryMainRepository = new InMemoryMainRepository();

            var service = new AuctionService(inMemoryMainRepository);
            var memberService = new MemberService(inMemoryMainRepository);

            service.Save(auction);

            memberService.Add(bidder);
            
            service.PlaceBid(bidder, auction, 51);

            Assert.AreEqual(1, auction.Bids.Count);
            Assert.AreEqual(1, bidder.Bids.Count);
        }

        private static Auction CreateGeneratedAuction()
        {
            return new Auction()
                       {
                           Seller =
                               new Member()
                                   {
                                       UniqueId = Guid.NewGuid().ToString(),
                                       Name = "AGeneratedMember"
                                   },
                           Title = "Generated Auction",
                           StartPrice = 50.5,
                           StartDateTimeUtc = DateTime.UtcNow.AddHours(1),
                           EndDateTimeUtc = DateTime.UtcNow.AddHours(2),
                       };
        }
    }
}
