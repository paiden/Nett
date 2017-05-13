using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private class ArraySegment : KeySegment
        {
            public ArraySegment(string key)
                : base(TomlObjectType.Array, key)
            {
            }

            public override TomlObject Apply(TomlObject obj)
            {
                return this.ApplyArraySegment(obj, this.ThrowWhenKeyNotFound, this.ThrowWhenIncompatibleType);
            }

            public override TomlObject ApplyOrCreate(TomlObject obj) => this.Apply(obj);

            public override void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public override TomlObject TryApply(TomlObject obj) => this.ApplyArraySegment(obj, _ => null, _ => null);

            public override string ToString() => $"/[{this.key}]";

            private TomlObject ApplyArraySegment(
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
