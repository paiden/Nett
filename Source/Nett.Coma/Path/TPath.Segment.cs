using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        internal abstract class Segment : ITPathSegment
        {
            protected readonly Func<TomlObject, TomlObject> ThrowWhenIncompatibleType;

            protected readonly TomlObjectType segmentType;

            public Segment(TomlObjectType segmentType)
            {
                this.segmentType = segmentType;

                this.ThrowWhenIncompatibleType = (to)
                    => throw new InvalidOperationException(
                        $"Cannot apply '{this.ToString()}' because TOML table row has incompatible type '{to.ReadableTypeName}'.");
            }

            public abstract TomlObject Apply(TomlObject obj);

            public abstract TomlObject ApplyOrCreate(TomlObject obj);

            public abstract void SetValue(TomlObject value);

            public abstract TomlObject TryApply(TomlObject obj);

            protected TomlObject VerifyType(TomlObject obj, Func<TomlObject, TomlObject> onVerifyFailed)
            {
                return obj.TomlType == this.segmentType
                    ? obj
                    : onVerifyFailed(obj);
            }
        }
    }
}
