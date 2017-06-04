using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        [Flags]
        public enum PathSettings
        {
            None = 0b0000_0000_0000_0000,
            VerifyType = 0b0000_0000_0000_0001,
            CreateTables = 0b0000_0000_0000_00010,
        }

        public interface ITPathSegment
        {
            TomlObject Apply(TomlObject obj, Func<TomlObject> resolveParent, PathSettings settings);

            TomlObject TryApply(TomlObject obj, Func<TomlObject> resolveParent, PathSettings settings);

            void SetValue(TomlObject obj, TomlObject value, PathSettings settings);
        }
    }
}
