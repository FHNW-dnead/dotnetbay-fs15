using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using DotNetBay.Interfaces;
using DotNetBay.Model;

using NUnit.Framework;

namespace DotNetBay.Test.Storage
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This are tests")]
    public abstract class MainRepositoryTestBase
    {
        [TestCase]
        public void GivenAnEmptyRepo_AddOneAuction_NotEmptyAnymore()
        {
            var myAuction = CreateAnAuction();
            myAuction.Seller = CreateAMember();

            Auction auctionFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myAuction);
                initRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();

                auctionFromRepo = testRepo.GetAuctions().FirstOrDefault();
            }

            Assert.IsNotNull(auctionFromRepo);
        }

        [TestCase]
        public void GivenANewRepository_CanBeSaved_WithNoIssues()
        {
            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.SaveChanges();
            }
        }

        [TestCase]
        public void GivenAnEmptyRepo_AddAuctionWithSeller_AuctionAndMemberAreRepodIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            myAuction.Seller = myMember;

            Member memberFromRepo;
            Auction auctionFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myAuction);
                initRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();

                auctionFromRepo = testRepo.GetAuctions().FirstOrDefault();
                memberFromRepo = testRepo.GetMembers().FirstOrDefault();
            }

            Assert.IsNotNull(auctionFromRepo, "auctionFromRepo != null");
            Assert.IsNotNull(auctionFromRepo.Seller, "auctionFromRepo.Seller != null");

            Assert.IsNotNull(memberFromRepo, "memberForRepo != null");
            Assert.IsNotNull(memberFromRepo.Auctions, "memberForRepo.Auctions != null");
            Assert.AreEqual(1, memberFromRepo.Auctions.Count, "There should be exact one euction for this member");

            Assert.AreEqual(myAuction.Title, auctionFromRepo.Title, "Auction's title is not the same");
            Assert.AreEqual(myMember.UniqueId, memberFromRepo.UniqueId, "Member's uniqueId is not the same");
            Assert.AreEqual(1, memberFromRepo.Auctions.Count, "There should be exact one euction for this member");
        }

        [TestCase]
        public void GivenAnEmptyRepo_AddAMemberWithAuctions_MemberAndAuctionsAreInRepoIndividually()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            Member memberForRepo;
            Auction auctionFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myMember);
                initRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();

                memberForRepo = testRepo.GetMembers().FirstOrDefault();
                auctionFromRepo = testRepo.GetAuctions().FirstOrDefault();
            }

            Assert.IsNotNull(memberForRepo, "memberForRepo != null");
            Assert.IsNotNull(memberForRepo.Auctions, "memberForRepo.Auctions != null");
            Assert.AreEqual(1, memberForRepo.Auctions.Count, "There should be exact one euction for this member");

            Assert.IsNotNull(auctionFromRepo, "auctionFromRepo != null");
            Assert.IsNotNull(auctionFromRepo.Seller, "auctionFromRepo.Seller != null");
            Assert.AreEqual(auctionFromRepo.Seller.UniqueId, myMember.UniqueId);
        }

        [TestCase]
        public void GivenAnExistingMember_AddAuctionWithExistingMemberAsSeller_AuctionIsAttachedToMember()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();

            IQueryable<Member> allMembersFromRepo;
            IQueryable<Auction> allAuctionsFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myMember);
                initRepo.SaveChanges();

                var secondRepo = factory.CreateMainRepository();

                // References
                myAuction.Seller = secondRepo.GetMembers().FirstOrDefault();
                secondRepo.Add(myAuction);
                secondRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();
                allAuctionsFromRepo = testRepo.GetAuctions();
                allMembersFromRepo = testRepo.GetMembers();
            }

            Assert.AreEqual(1, allAuctionsFromRepo.Count(), "There should be exact 1 auction");
            Assert.AreEqual(1, allMembersFromRepo.Count(), "There should be exact 1 member");

            Assert.IsNotNull(allMembersFromRepo, "memberForRepo != null");
            Assert.IsNotNull(allMembersFromRepo.First().Auctions, "memberForRepo.Auctions != null");
            
            Assert.AreEqual(1, allMembersFromRepo.First().Auctions.Count(), "There should be a auction attached to the member");
            Assert.AreEqual(myAuction.Id, allMembersFromRepo.First().Auctions.First().Id);
        }

        [TestCase]
        public void GivenARepoWithAuctionAndMember_AddBid_BidGetsListedInAuction()
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

            List<Auction> allAuctionsFromRepo;

            using (var factory = this.CreateFactory())
            {
                var testRepo = factory.CreateMainRepository();

                testRepo.Add(myAuction);
                testRepo.Add(theBidder);
                testRepo.Add(bid);

                allAuctionsFromRepo = testRepo.GetAuctions().ToList();
            }

            // Sanity check
            Assert.AreEqual(1, allAuctionsFromRepo.Count());
            Assert.IsNotNull(allAuctionsFromRepo[0].Bids);

            Assert.AreEqual(1, allAuctionsFromRepo[0].Bids.Count);
            Assert.AreEqual(bid, allAuctionsFromRepo[0].Bids[0]);
        }

        [TestCase]
        public void GivenARepoWithAuctionAndMember_AddBid_AuctionIsReferencedFromBidder()
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

            List<Member> allMembersFromRepo;

            using (var factory = this.CreateFactory())
            {
                var testRepo = factory.CreateMainRepository();

                testRepo.Add(myAuction);
                testRepo.Add(theBidder);
                testRepo.Add(bid);

                allMembersFromRepo = testRepo.GetMembers().ToList();
            }

            // Sanity check
            Assert.AreEqual(2, allMembersFromRepo.Count());
            Assert.IsNotNull(allMembersFromRepo[1].Bids);

            Assert.AreEqual(1, allMembersFromRepo[1].Bids.Count);
            Assert.AreEqual(bid, allMembersFromRepo[1].Bids[0]);
        }

        [TestCase]
        public void GivenARepoWithAuctionAndMember_AddBid_CanBeRetrievedByTransactionId()
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
                var testRepo = factory.CreateMainRepository();
                testRepo.Add(theBidder);
                testRepo.Add(myAuction);
                testRepo.Add(bid);

                retrievedBid = testRepo.GetBidByTransactionId(bid.TransactionId);
            }

            Assert.IsNotNull(retrievedBid);
            Assert.AreEqual(bid, retrievedBid);
        }

        [TestCase]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenARepoWithMember_AddMemberAgain_ShouldRaiseExecption()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var factory = this.CreateFactory())
            {
                var firstRepo = factory.CreateMainRepository();
                firstRepo.Add(myMember);
                firstRepo.Add(myMember);
            }
        }

        [TestCase]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenARepoWithAuction_AddAuctionAgain_ShouldRaiseExecption()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var factory = this.CreateFactory())
            {
                var testRepo = factory.CreateMainRepository();
                testRepo.Add(myAuction);
                testRepo.Add(myAuction);
            }
        }

        [TestCase]
        [ExpectedException(typeof(Exception))]
        public void GivenEmptyRepo_AddAuctionAndMemberFromOtherInstance_ShouldRaiseException()
        {
            var myAuction = CreateAnAuction();
            var myMember = CreateAMember();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myAuction);
                initRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();
                testRepo.Add(myAuction);
            }
        }

        [TestCase]
        [ExpectedException(typeof(Exception))]
        public void GivenEmptyRepo_AddMemberWithAuctionsFromOtherInstance_ShouldRaiseException()
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
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myMember);
                initRepo.SaveChanges();

                var testSore = factory.CreateMainRepository();
                testSore.Add(otherMember);
            }
        }

        [TestCase]
        public void GivenAnEmptyRepo_AddAuctionAndMember_ReferencesShouldBeEqual()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();

            // References
            myAuction.Seller = myMember;
            myMember.Auctions = new List<Auction>(new[] { myAuction });

            IQueryable<Member> allMembersFromRepo;
            IQueryable<Auction> allAuctionFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myAuction);
                initRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();
                allAuctionFromRepo = testRepo.GetAuctions();
                allMembersFromRepo = testRepo.GetMembers();
            }

            Assert.AreEqual(1, allAuctionFromRepo.Count(), "There should be exact 1 auction");
            Assert.AreEqual(1, allMembersFromRepo.Count(), "There should be exact 1 member");

            Assert.AreEqual(allAuctionFromRepo.FirstOrDefault().Seller, allMembersFromRepo.FirstOrDefault());
            Assert.AreEqual(allMembersFromRepo.FirstOrDefault().Auctions.FirstOrDefault(), allAuctionFromRepo.FirstOrDefault());
        }

        [TestCase]
        public void AuctionWithImage_IsSavedInRepo_CanBeRetrievedAfterwards()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();
            myAuction.Seller = myMember;

            var myImage = Guid.NewGuid().ToByteArray();
            myAuction.Image = myImage;

            byte[] imageFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myAuction);
                initRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();
                imageFromRepo = testRepo.GetAuctions().First().Image;
            }

            Assert.AreEqual(myImage, imageFromRepo);
        }

        [TestCase]
        public void AuctionWithImage_IsUpdatedWithNoImage_ImageIsGone()
        {
            var myMember = CreateAMember();
            var myAuction = CreateAnAuction();
            myAuction.Seller = myMember;

            var myImage = Guid.NewGuid().ToByteArray();
            myAuction.Image = myImage;

            byte[] imageFromRepo;

            using (var factory = this.CreateFactory())
            {
                var initRepo = factory.CreateMainRepository();
                initRepo.Add(myAuction);
                initRepo.SaveChanges();

                var secondRepo = factory.CreateMainRepository();
                var auctionFromRepo = secondRepo.GetAuctions().First();
                auctionFromRepo.Image = null;
                secondRepo.Update(auctionFromRepo);
                secondRepo.SaveChanges();

                var testRepo = factory.CreateMainRepository();
                imageFromRepo = testRepo.GetAuctions().First().Image;
            }

            Assert.IsNull(imageFromRepo);
        }

        protected abstract IRepositoryFactory CreateFactory();

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
                DisplayName = "GeneratedMember",
                UniqueId = "UniqueId" + Guid.NewGuid()
            };
        }

        #endregion
    }
}
