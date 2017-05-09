using System;
using System.Diagnostics;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class TableSegment : KeySegment
        {
            public TableSegment(string key)
                : base(TomlObjectType.Table, key)
            {
            }

            public override TomlObject Apply(TomlObject obj)
            {
                return this.ApplyTableSegment(obj, this.ThrowWhenKeyNotFound, this.ThrowWhenIncompatibleType);
            }

            public override TomlObject ApplyOrCreate(TomlObject obj)
            {
                return this.ApplyTableSegment(obj, tbl => tbl.AddTable(this.key), this.ThrowWhenIncompatibleType);
            }

            public override void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public override TomlObject TryApply(TomlObject obj)
            {
                try
                {
                    return obj == null
                        ? null
                        : this.ApplyTableSegment(obj, _ => null, _ => null);
                }
                catch
                {
                    Debug.Assert(false, "This should never happen if ApplyTableSegment was implemented correctly");
                    return null;
                }
            }

            public override string ToString() => $"/{{{this.key}}}";

            private TomlObject ApplyTableSegment(
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
