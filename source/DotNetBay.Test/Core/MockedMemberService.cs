using System;
using System.Collections.Generic;

using DotNetBay.Core;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;

namespace DotNetBay.Test.Core
{
    public class MockedMemberService : IMemberService
    {
        private readonly InMemoryMainRepository repo;
        private SimpleMemberService userService;

        private Member currentMember;

        public MockedMemberService(InMemoryMainRepository repo)
        {
            this.repo = repo;
            this.userService = new SimpleMemberService(this.repo);
            this.currentMember = this.userService.GetCurrentMember();
        }

        public Member GetCurrentMember()
        {
            return this.currentMember;
        }

        public void SetCurrentMember(Member member)
        {
            this.currentMember = member;
        }

        public Member GetByUniqueId(string uniqueId)
        {
            return this.userService.GetByUniqueId(uniqueId);
        }

        public Member Add(string displayName, string mail)
        {
            return this.userService.Add(displayName, mail);
        }

        public Member Save(Member member)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Member> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}