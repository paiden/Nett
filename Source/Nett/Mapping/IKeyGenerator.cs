using System.Reflection;

namespace Nett
{
    public interface IKeyGenerator
    {
        string GetKey(PropertyInfo property);
    }
}
