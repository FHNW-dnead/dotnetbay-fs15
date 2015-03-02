using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using DotNetBay.Core.Execution;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;

using NUnit.Framework;

namespace DotNetBay.Test.Core
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This is a Testclass")]
    public class AuctioneerTests
    {
        [TestCase]
        public void Auction_BidIsBelowStartPrice_HasNoImpact()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            
            auctioneer.DoAllWork();

            var bidder = new Member() { DisplayName = "Bidder1", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder);

            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Auction = auction, Amount = auction.StartPrice - 10, Bidder = bidder });

            auctioneer.DoAllWork();

            Assert.AreEqual(1, auction.Bids.Count);
            Assert.IsNull(auction.ActiveBid);
        }

        [TestCase]
        public void Auction_HasNewerButLowerBid_HasNoImpact()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);
            
            auctioneer.DoAllWork();

            var bidder2 = new Member() { DisplayName = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
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

            var bidder2 = new Member() { DisplayName = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
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

            var bidder2 = new Member() { DisplayName = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow.AddMinutes(-10), Bidder = bidder2, Amount = 51, Auction = auction });

            auctioneer.DoAllWork();

            Assert.AreEqual(2, auction.Bids.Count);
            Assert.AreEqual(60, auction.CurrentPrice);
        }

        [TestCase]
        [ExpectedException(typeof(ApplicationException))]
        public void Auction_GetsOlderButHigherBid_FailsWithException()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);

            auctioneer.DoAllWork();

            var bidder2 = new Member() { DisplayName = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow.AddMinutes(-10), Bidder = bidder2, Amount = 70, Auction = auction });

            auctioneer.DoAllWork();
        }

        [TestCase]
        public void Auction_StartTimeHasArrived_AuctionGetsRunning()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            Assert.IsFalse(auction.IsRunning);

            auctioneer.DoAllWork();

            Assert.IsTrue(auction.IsRunning);

            // Turn back the time
            auction.EndDateTimeUtc = DateTime.UtcNow;

            auctioneer.DoAllWork();

            Assert.IsTrue(auction.IsClosed);
            Assert.IsFalse(auction.IsRunning);
        }

        [TestCase]
        public void Auction_EndTimeHasArrived_AuctionGetsClosed()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            auctioneer.DoAllWork();

            Assert.IsFalse(auction.IsClosed);

            // Turn back the time
            auction.EndDateTimeUtc = DateTime.UtcNow;

            auctioneer.DoAllWork();

            Assert.IsTrue(auction.IsClosed);
            Assert.IsFalse(auction.IsRunning);
        }

        [TestCase]
        public void AuctionHasStartTimeInPast_AuctioneerRuns_AuctionIsRunning()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            auctioneer.DoAllWork();

            Assert.IsFalse(auction.IsClosed);
            Assert.IsTrue(auction.IsRunning);
        }

        [TestCase]
        public void AuctionIsClosed_AuctioneerRuns_AuctionIsNotRunning()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
            auction.IsRunning = true;
            auction.IsClosed = true;

            auctioneer.DoAllWork();

            Assert.IsTrue(auction.IsClosed);
            Assert.IsFalse(auction.IsRunning);
        }

        [TestCase]
        public void AuctionStartedAndEndedInThePast_AuctioneerRuns_DontGetsStarted()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            AuctionEventArgs raisedArgs = null;
            auctioneer.AuctionStarted += (sender, args) => raisedArgs = args;

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
            auction.IsRunning = false;
            auction.IsClosed = true;

            auctioneer.DoAllWork();

            Assert.IsNull(raisedArgs);
            Assert.IsTrue(auction.IsClosed);
            Assert.IsFalse(auction.IsRunning);
        }

        [TestCase]
        public void Auction_HasOneBidAndEnds_TheBidderShouldBeTheWinner()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            auctioneer.DoAllWork();

            var bidder2 = new Member() { DisplayName = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Bidder = bidder2, Amount = 70, Auction = auction });

            // Turn back the time
            auction.EndDateTimeUtc = DateTime.UtcNow;

            auctioneer.DoAllWork();

            Assert.AreEqual(auction.Winner, bidder2);
        }

        [TestCase]
        public void Auction_WhenStarted_EventIsRaised()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            AuctionEventArgs raisedArgs = null;
            auctioneer.AuctionStarted += (sender, args) => raisedArgs = args;

            auctioneer.DoAllWork();

            Assert.NotNull(raisedArgs);
            Assert.NotNull(raisedArgs.Auction);
            Assert.AreEqual(auction, raisedArgs.Auction);
        }

        [TestCase]
        public void Auction_WhenEnded_EventIsRaised()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            AuctionEventArgs raisedArgs = null;
            auctioneer.AuctionEnded += (sender, args) => raisedArgs = args;

            // Turn back the time
            auction.EndDateTimeUtc = DateTime.UtcNow;

            auctioneer.DoAllWork();

            Assert.NotNull(raisedArgs);
            Assert.NotNull(raisedArgs.Auction);
            Assert.NotNull(raisedArgs.IsSuccessful);
        }

        [TestCase]
        public void Bid_WhenAccepted_EventIsRaised()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

            ProcessedBidEventArgs raisedArgs = null;
            auctioneer.BidAccepted += (sender, args) => raisedArgs = args;

            AddInitialBidToAuction(repo, auction);

            auctioneer.DoAllWork();

            Assert.NotNull(raisedArgs);
            Assert.NotNull(raisedArgs.Auction);
            Assert.NotNull(raisedArgs.Bid);
        }

        [TestCase]
        public void Bid_WhenDeclined_EventIsRaised()
        {
            var repo = new InMemoryMainRepository();
            var auctioneer = new Auctioneer(repo);

            var auction = CreateAndStoreAuction(repo, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
            AddInitialBidToAuction(repo, auction);

            auctioneer.DoAllWork();

            ProcessedBidEventArgs raisedArgs = null;
            auctioneer.BidDeclined += (sender, args) => raisedArgs = args;

            var bidder2 = new Member() { DisplayName = "Bidder2", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder2);
            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Bidder = bidder2, Amount = 51, Auction = auction });
            
            auctioneer.DoAllWork();

            Assert.NotNull(raisedArgs);
            Assert.NotNull(raisedArgs.Auction);
            Assert.NotNull(raisedArgs.Bid);
        }

        private static void AddInitialBidToAuction(InMemoryMainRepository repo, Auction auction)
        {
            var bidder = new Member() { DisplayName = "Bidder1", UniqueId = Guid.NewGuid().ToString() };
            repo.Add(bidder);

            repo.Add(new Bid() { ReceivedOnUtc = DateTime.UtcNow, Auction = auction, Amount = auction.StartPrice + 10, Bidder = bidder });
        }

        private static Auction CreateAndStoreAuction(InMemoryMainRepository repo, DateTime startDateTimeUtc, DateTime endDateTimeUtc)
        {
            var seller = new Member() { DisplayName = "Seller", UniqueId = Guid.NewGuid().ToString() };
            var auction = new Auction() { Title = "TestAuction", Seller = seller, StartPrice = 50, StartDateTimeUtc = startDateTimeUtc, EndDateTimeUtc = endDateTimeUtc };

            repo.Add(seller);
            repo.Add(auction);

            Assert.AreEqual(1, repo.GetAuctions().Count());
            Assert.AreEqual(1, repo.GetMembers().Count());

            return auction;
        }
    }
}
