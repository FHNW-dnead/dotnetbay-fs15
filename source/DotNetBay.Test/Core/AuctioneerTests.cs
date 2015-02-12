using System;
using System.Linq;

using DotNetBay.Core.Execution;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;

using NUnit.Framework;

namespace DotNetBay.Test.Core
{
    public class AuctioneerTests
    {
        [TestCase]
        public void Auction_HasNewerButLowerBid_HasNoImpact()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);
            
            auctioneer.DoAllWork();

            var bidder2 = new Member() { Name = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Bidder = bidder2, Amount = 51, Auction = auction });

            auctioneer.DoAllWork();

            Assert.AreEqual(2, auction.Bids.Count);
            Assert.AreEqual(60, auction.CurrentPrice);
        }

        [TestCase]
        public void Auction_GetsNewerButHigherBid_PriceIsAffected()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);

            auctioneer.DoAllWork();

            var bidder2 = new Member() { Name = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Bidder = bidder2, Amount = 70, Auction = auction });

            auctioneer.DoAllWork();

            Assert.AreEqual(2, auction.Bids.Count);
            Assert.AreEqual(70, auction.CurrentPrice);
        }

        [TestCase]
        public void Auction_GetsOlderButLowerBid_HasNoImpact()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);

            auctioneer.DoAllWork();

            var bidder2 = new Member() { Name = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow.AddMinutes(-10), Bidder = bidder2, Amount = 51, Auction = auction });

            auctioneer.DoAllWork();

            Assert.AreEqual(2, auction.Bids.Count);
            Assert.AreEqual(60, auction.CurrentPrice);
        }

        [TestCase]
        public void Auction_GetsOlderButHigherBid_HasNoImpact()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);

            auctioneer.DoAllWork();

            var bidder2 = new Member() { Name = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow.AddMinutes(-10), Bidder = bidder2, Amount = 70, Auction = auction });

            auctioneer.DoAllWork();

            Assert.AreEqual(2, auction.Bids.Count);
            Assert.AreEqual(60, auction.CurrentPrice);
        }

        [TestCase]
        public void Auction_EndTimeHasArrived_AuctionGetsClosed()
        {
            
        }

        [TestCase]
        public void Auction_WhenClosed_EventIsRaised()
        {
            Assert.Fail();
        }

        [TestCase]
        public void Bid_WhenAccepted_EventIsRaised()
        {
            Assert.Fail();
        }

        [TestCase]
        public void Bid_WhenDeclined_EventIsRaised()
        {
            Assert.Fail();
        }

        private static void AddInitialBidToAuction(InMemoryMainRepository repo, Auction auction)
        {
            var bidder = new Member() { Name = "Bidder1", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder);

            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Auction = auction, Amount = auction.StartPrice + 10, Bidder = bidder });
        }

        private static Auction CreateAndStoreAuction(InMemoryMainRepository repo, DateTime startDateTimeUtc, DateTime endDateTimeUtc)
        {
            var seller = new Member() { Name = "Seller", UniqueId = Guid.NewGuid().ToString() };
            var auction = new Auction() { Title = "TestAuction", Seller = seller, StartPrice = 50, StartDateTimeUtc = startDateTimeUtc, EndDateTimeUtc = endDateTimeUtc };

            repo.Add(seller);
            repo.Add(auction);

            Assert.AreEqual(1, repo.GetAuctions().Count());
            Assert.AreEqual(1, repo.GetMembers().Count());

            return auction;
        }
    }
}
