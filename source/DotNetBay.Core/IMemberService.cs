using System.Collections.Generic;

using DotNetBay.Model;

namespace DotNetBay.Core
{
    public interface IMemberService
    {
        Member GetCurrentMember();

        Member Add(Member member);

        bool Save(Member member);

        IEnumerable<Member> GetAll();
    }
}