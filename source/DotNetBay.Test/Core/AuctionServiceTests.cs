using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using DotNetBay.Core;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetBay.Test.Core
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This is a testclass")]
    public class AuctionServiceTests
    {
        [TestMethod]
        public void GivenAProperService_GetsAValidAuction_ShouldReturnSameFromAuctionList()
        {
            var service = new AuctionService(new InMemoryMainRepository());

            var auction = CreateGeneratedAuction();
            service.Save(auction);

            var auctionFromService = service.GetAuctions().First();
            Assert.AreEqual(auctionFromService, auction);
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
