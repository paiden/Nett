using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private abstract class KeySegment : Segment
        {
            protected readonly Func<TomlTable, TomlObject> ThrowWhenKeyNotFound;

            protected readonly string key;

            public KeySegment(TomlObjectType segmentType, string key)
                : base(segmentType)
            {
                if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentException(nameof(key)); }

                this.key = key;
                this.ThrowWhenKeyNotFound = _
                    => throw new InvalidOperationException($"TOML table does not contain a row with key '{this.key}'.");
            }

            public bool ClearFrom(TomlTable tbl)
            {
                return tbl.Remove(this.key);
            }

            protected TomlObject ApplyKey(TomlObject obj, Func<TomlTable, TomlObject> onDoesNotExist)
            {
                switch (obj)
                {
                    case TomlTable tbl:
                        return tbl.TryGetValue(this.key, out var val)
                            ? val
                            : onDoesNotExist(tbl);
                    default:
                        throw new InvalidOperationException(
                           $"Key cannot be applied. " +
                           $"Keys can only be applied on TOML tables but object has type '{obj.ReadableTypeName}'.");
                }
            }
        }
    }
}
