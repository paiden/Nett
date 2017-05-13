using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class IndexSegment : ITPathSegment
        {
            private readonly int index;

            public IndexSegment(int index)
            {
                this.index = index;
            }

            public TomlObject Apply(TomlObject obj)
            {
                return this.ApplyIndex(obj, (err) => throw new InvalidOperationException(err));
            }

            public TomlObject ApplyOrCreate(TomlObject obj) => this.Apply(obj);

            public void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public TomlObject TryApply(TomlObject obj) => this.ApplyIndex(obj, _ => null);

            public override string ToString() => $"[{this.index}]";

            private TomlObject ApplyIndex(
                TomlObject obj, Func<string, TomlObject> onError)
            {
                switch (obj)
                {
                    case TomlArray a: return a[this.index];
                    case TomlTableArray ta: return ta[this.index];
                    default:
                        return onError(
                        $"Cannot apply index '[{this.index}]' onto TOML object of type '{obj.ReadableTypeName}'.");
                }
            }
        }
    }
}
