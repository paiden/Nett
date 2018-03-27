using System.Collections.Generic;

namespace Nett.Collections
{
    internal interface IGetChildren<out T>
    {
        IEnumerable<T> GetChildren();
    }
}
