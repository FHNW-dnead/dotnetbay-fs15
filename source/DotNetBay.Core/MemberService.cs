using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core
{
    public class MemberService : IMemberService
    {
        private readonly IMainRepository repository;

        public MemberService(IMainRepository repository)
        {
            this.repository = repository;
        }

        public Member GetCurrentMember()
        {
            var principal = Thread.CurrentPrincipal;

            var member = this.repository.GetMembers().FirstOrDefault(m => m.Name == principal.Identity.Name);

            if (member == null)
            {
                member = new Member() { Name = principal.Identity.Name, UniqueId = Guid.NewGuid().ToString() };
                this.repository.Add(member);
            }

            return member;
        }

        public Member Add(Member member)
        {
            if (this.repository.GetMembers().All(m => m.UniqueId != member.UniqueId))
            {
                this.repository.Add(member);
            }

            return member;
        }

        public bool Save(Member member)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Member> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
