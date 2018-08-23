using System;
using System.Linq;
using System.Reflection;

namespace Nett
{
    public sealed class TargetPropertySelectors
    {
        internal static readonly TargetPropertySelectors Instance = new TargetPropertySelectors();

        private static readonly ITargetPropertySelector ExactSel = new ExactSelector();

        private static readonly ITargetPropertySelector IgnoreCaseSel = new IgnoreCaseSelector();

        private TargetPropertySelectors()
        {
        }

        public ITargetPropertySelector Exact
            => ExactSel;

        public ITargetPropertySelector IgnoreCase
            => IgnoreCaseSel;

        private sealed class ExactSelector : ITargetPropertySelector
        {
            public PropertyInfo TryGetTargetProperty(string key, Type target)
                => target.GetProperty(key, TomlSettings.PropBindingFlags);
        }

        private sealed class IgnoreCaseSelector : ITargetPropertySelector
        {
            public PropertyInfo TryGetTargetProperty(string key, Type target)
                => target.GetProperties(TomlSettings.PropBindingFlags)
                    .SingleOrDefault(pi => pi.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
