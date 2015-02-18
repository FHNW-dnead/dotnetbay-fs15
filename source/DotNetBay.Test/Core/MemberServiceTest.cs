using DotNetBay.Core;
using DotNetBay.Data.FileStorage;

using NUnit.Framework;

namespace DotNetBay.Test.Core
{
    public class MemberServiceTest
    {
        [TestCase]
        public void WhenMemberIsNotInRepo_GetCurrentUser_AlwaysGetAMember()
        {
            var repo = new InMemoryMainRepository();
            var service = new SimpleMemberService(repo);

            var currentMember = service.GetCurrentMember();

            Assert.NotNull(currentMember);
            Assert.IsNotNullOrEmpty(currentMember.DisplayName);
            Assert.IsNotNullOrEmpty(currentMember.EMail);
        }

        [TestCase]
        public void GettingCurrentMemberTwice_IsSame()
        {
            var repo = new InMemoryMainRepository();
            var service = new SimpleMemberService(repo);

            var currentMember1 = service.GetCurrentMember();
            var currentMember2 = service.GetCurrentMember();
            
            Assert.AreEqual(currentMember1, currentMember2);
        }
    }
}
