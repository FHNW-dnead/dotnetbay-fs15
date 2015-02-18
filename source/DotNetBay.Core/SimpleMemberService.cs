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

        public Member Add(string displayName, string mail)
        {
            var uniqueId = mail;

            if (this.repository.GetMembers().All(m => m.UniqueId != uniqueId))
            {
                var member = new Member()
                                    {
                                        UniqueId = uniqueId,
                                        DisplayName = displayName,
                                        EMail = mail
                                    };

                this.repository.Add(member);
                this.repository.SaveChanges();

                return member;
            }

            return null;
        }

        public Member Save(Member member)
        {
            var foundMember = this.repository.GetMembers().FirstOrDefault(m => m.UniqueId == member.UniqueId);

            if (foundMember == null)
            {
                throw new ArgumentException("Cannot save an unknown member");
            }
            
            foundMember.DisplayName = member.DisplayName;
            foundMember.EMail = member.DisplayName;

            this.repository.SaveChanges();

            return foundMember;
        }

        public IEnumerable<Member> GetAll()
        {
            return this.repository.GetMembers();
        }

        public Member GetByUniqueId(string uniqueId)
        {
            return this.repository.GetMembers().FirstOrDefault(m => m.UniqueId == uniqueId);
        }
    }
}
