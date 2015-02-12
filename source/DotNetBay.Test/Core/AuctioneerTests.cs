using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetBay.Test.Core
{
    [TestClass]
    public class AuctioneerTests
    {
        [TestMethod]
        public void Auction_GetsNewerAndLowerBid_HasNoImpact()
        {
            Assert.Fail();            
        }

        [TestMethod]
        public void Auction_GetsNewerAndHigherBid_PriceIsAffected()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Auction_GetsOlderAndLowerBid_HasNoImpact()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Auction_GetsOlderAndHigherBid_HasNoImpact()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Auction_WhenClosed_EventIsRaised()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Bid_WhenAccepted_EventIsRaised()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Bid_WhenDeclined_EventIsRaised()
        {
            Assert.Fail();
        }
    }
}
