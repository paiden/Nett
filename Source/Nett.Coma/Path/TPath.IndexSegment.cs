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

            public TomlObject Apply(TomlObject obj, PathSettings settings = PathSettings.None)
                => this.ApplyIndex(obj, (err) => throw new InvalidOperationException(err));

            public TomlObject TryApply(TomlObject obj, PathSettings settings = PathSettings.None)
                => this.ApplyIndex(obj, _ => null);

            public void SetValue(TomlObject target, TomlObject value, PathSettings settings)
            {
                if (value is TomlArray a)
                {
                    a.Items[this.index] = (TomlValue)value;
                }
                else if (value is TomlTableArray ta)
                {
                    ta.Items[this.index] = (TomlTable)value;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot apply index '[{this.index}]' onto TOML object of type '{target.ReadableTypeName}'.");
                }
            }

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
