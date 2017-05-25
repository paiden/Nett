using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        internal abstract class Segment : ITPathSegment
        {
            protected readonly TomlObjectType segmentType;

            public Segment(TomlObjectType segmentType)
            {
                this.segmentType = segmentType;
            }

            public abstract TomlObject Apply(TomlObject obj, PathSettings settings);

            public abstract void SetValue(TomlObject target, TomlObject value, PathSettings settings);

            public abstract TomlObject TryApply(TomlObject target, PathSettings settings);

            protected TomlObject VerifyType(TomlObject obj, Func<string, TomlObject> onError, PathSettings settings)
            {
                if (settings.HasFlag(PathSettings.VerifyType))
                {
                    return obj.TomlType == this.segmentType
                        ? obj
                        : onError($"Cannot apply '{this.ToString()}' with type '{this.segmentType}' because TOML table " +
                            $"row has incompatible type '{obj.ReadableTypeName}'.");
                }

                return obj;
            }
        }
    }
}
