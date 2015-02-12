using System;
using System.Diagnostics.CodeAnalysis;

using DotNetBay.Core.Service;
using DotNetBay.Interfaces;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetBay.Test.Core
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This is a testclass")]
    public class AuctionServiceTests
    {
        [TestMethod]
        public void GivenAProperService_GetsAnAcution_ShouldReturnSameFromAuctionList()
        {
            var service = new AuctionService(new InMemoryMainRepository());
        }
    }
}
