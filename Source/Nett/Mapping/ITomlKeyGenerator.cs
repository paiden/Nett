using System.Reflection;

namespace Nett
{
    public interface ITomlKeyGenerator
    {
        string GetKey(PropertyInfo property);
    }
}
