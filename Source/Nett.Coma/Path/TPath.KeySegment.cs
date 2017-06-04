#pragma warning disable SA1313

using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private abstract class KeySegment : Segment
        {
            protected static readonly Func<string, TomlObject> ThrowOnError =
                err => throw new InvalidOperationException(err);

            protected static readonly Func<string, TomlObject> ReturnDefaultOnError = _ => null;

            protected readonly Func<TomlTable, TomlObject> ThrowWhenKeyNotFound;

            protected readonly string key;

            public KeySegment(TomlObjectType segmentType, string key)
                : base(segmentType)
            {
                if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentException(nameof(key)); }
                this.key = key;

                this.ThrowWhenKeyNotFound = _ => throw new InvalidOperationException(this.KeyNotFoundMessage);
            }

            protected string KeyNotFoundMessage => $"TOML table does not contain a row with key '{this.key}'.";

            public bool ClearFrom(TomlTable tbl)
            {
                return tbl.Remove(this.key);
            }

            public override TomlObject Apply(TomlObject obj, Func<TomlObject> _, PathSettings settings)
                => this.ApplyKey(obj, this.ThrowWhenKeyNotFound, ThrowOnError, settings);

            public override TomlObject TryApply(TomlObject obj, Func<TomlObject> __, PathSettings settings)
                => this.ApplyKey(obj, _ => null, ReturnDefaultOnError, settings);

            public override void SetValue(TomlObject target, TomlObject value, PathSettings settings)
            {
                if (target is TomlTable tbl)
                {
                    tbl[this.key] = value;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot set key '{this.key}' value for path segment '{this.ToString()}' because target " +
                        $"object is of type '{target.ReadableTypeName}' instead of 'table'.");
                }
            }

            protected TomlObject ApplyKey(
                TomlObject obj,
                Func<TomlTable, TomlObject> onDoesNotExist,
                Func<string, TomlObject> onError,
                PathSettings settings)
            {
                switch (obj)
                {
                    case TomlTable tbl:
                        return tbl.TryGetValue(this.key, out var val)
                            ? this.VerifyType(val, onError, settings)
                            : onDoesNotExist(tbl);
                    default:
                        return onError($"Key '{this.key}' cannot be applied. " +
                           $"Keys can only be applied on TOML tables but object has type " +
                           $"'{obj?.ReadableTypeName ?? "object was null"}'.");
                }
            }
        }
    }
}
