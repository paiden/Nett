using System.Reflection;

namespace Nett
{
    public interface IKeyGenerator
    {
        string GetKey(MemberInfo property);
    }
}
