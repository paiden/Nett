using System;
using System.Reflection;

namespace Nett
{
    public interface ITargetPropertySelector
    {
        PropertyInfo TryGetTargetProperty(string key, Type target);
    }
}
