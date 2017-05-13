using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class ValueSegment : KeySegment
        {
            public ValueSegment(TomlObjectType segmentType, string key)
                : base(segmentType, key)
            {
                if (segmentType > TomlObjectType.TimeSpan)
                {
                    throw new ArgumentException(
                        $"The passed TOML type '{segmentType}' is invalid because it is not a TOML value type.");
                }
            }

            public override TomlObject Apply(TomlObject obj)
            {
                return this.ApplyValueSegment(obj, this.ThrowWhenKeyNotFound, this.ThrowWhenIncompatibleType);
            }

            public override TomlObject ApplyOrCreate(TomlObject obj) => this.Apply(obj);

            public override void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public override TomlObject TryApply(TomlObject obj) => this.ApplyValueSegment(obj, _ => null, _ => null);

            public override string ToString() => $"/{this.key}";

            private TomlObject ApplyValueSegment(
                            TomlObject obj,
                            Func<TomlTable, TomlObject> onDoesNotExist,
                            Func<TomlObject, TomlObject> onIncompatibleType)
            {
                var target = this.ApplyKey(obj, onDoesNotExist);
                return this.VerifyType(target, onIncompatibleType);
            }
        }
    }
}
