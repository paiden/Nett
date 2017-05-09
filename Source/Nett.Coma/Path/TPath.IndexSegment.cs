using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class IndexSegment : Segment
        {
            private readonly int index;

            public IndexSegment(TomlObjectType segmentType, int index)
                : base(segmentType)
            {
                this.index = index;
            }

            public override TomlObject Apply(TomlObject obj)
            {
                return this.ApplyIndex(obj, (err) => throw new InvalidOperationException(err), this.ThrowWhenIncompatibleType);
            }

            public override TomlObject ApplyOrCreate(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public override TomlObject TryApply(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            private TomlObject ApplyIndex(
                TomlObject obj, Func<string, TomlObject> onError, Func<TomlObject, TomlObject> onIncompatibleType)
            {
                switch (obj)
                {
                    case TomlArray a: return this.VerifyType(a[this.index], onIncompatibleType);
                    case TomlTableArray ta: return this.VerifyType(ta[this.index], onIncompatibleType);
                    default:
                        return onError(
                        $"Cannot apply index '[{this.index}]' onto TOML object of type '{obj.ReadableTypeName}'.");
                }
            }
        }
    }
}
