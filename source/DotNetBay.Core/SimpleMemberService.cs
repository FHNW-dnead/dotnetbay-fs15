using System;
using System.Collections.Generic;
using System.Linq;

using DotNetBay.Interfaces;
using DotNetBay.Model;

namespace DotNetBay.Core
{
    public class SimpleMemberService : IMemberService
    {
        private readonly IMainRepository repository;

        public SimpleMemberService(IMainRepository repository)
        {
            this.repository = repository;
        }

        public Member GetCurrentMember()
        {
            var uniqueId = string.Format("{0}@{1}.local", Environment.UserName, Environment.UserDomainName);
            
            var member = this.repository.GetMembers().FirstOrDefault(m => m.UniqueId == uniqueId);

            if (member == null)
            {
                member = new Member
                    {
                        UniqueId = uniqueId, 
                        DisplayName = Environment.UserName, 
                        EMail = string.Format("{0}@{1}.local", Environment.UserName, Environment.UserDomainName)
                    };

                this.repository.Add(member);
                this.repository.SaveChanges();
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
