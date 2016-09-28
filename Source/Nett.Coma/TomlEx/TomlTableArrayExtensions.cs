using System.Linq;
using Nett.Extensions;

namespace Nett.Coma.TomlEx
{
    internal static class TomlTableArrayExtensions
    {
        public static TomlTableArray Clone(this TomlTableArray source)
        {
            source.CheckNotNull(nameof(source));

            var cloned = new TomlTableArray(source.Root, source.Items.Select(i => i.Clone()));
            return cloned;
        }
    }
}
