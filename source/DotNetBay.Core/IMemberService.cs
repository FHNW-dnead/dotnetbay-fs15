using System.Collections.Generic;

using DotNetBay.Model;

namespace DotNetBay.Core
{
    public interface IMemberService
    {
        Member GetCurrentMember();

        Member GetByUniqueId(string uniqueId);

        Member Add(string displayName, string mail);

        Member Save(Member member);

        IEnumerable<Member> GetAll();
    }
}