namespace Nett
{
    using System;

    public partial class TomlTable
    {
        internal static RootTable From(TomlSettings settings, object obj)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            if (obj is RootTable rt) { return rt; }

            return ClrToTomlTableConverter.Convert(obj, settings);
        }

        internal sealed class RootTable : TomlTable, ITomlRoot
        {
            private readonly TomlSettings settings;

            public RootTable(TomlSettings settings)
                : base(null)
            {
                if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

                this.settings = settings;
            }

            TomlSettings ITomlRoot.Settings => this.settings;
        }
    }
}
