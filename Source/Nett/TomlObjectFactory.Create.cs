using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        private const string TomlObjectCreateNotAllowed2A = "'{0}' is a TOML object of type '{1}'. Passing TOML objects to " +
            "a create method is not allowed.";

        // Values
        public static TomlBool CreateAttached(this TomlObject rootSource, bool value)
            => new TomlBool(rootSource.Root, value);

        public static TomlString CreateAttached(this TomlObject rootSource, string value)
            => new TomlString(rootSource.Root, value);

        public static TomlInt CreateAttached(this TomlObject rootSource, long value)
            => new TomlInt(rootSource.Root, value);

        public static TomlFloat CreateAttached(this TomlObject rootSource, double value)
            => new TomlFloat(rootSource.Root, value);

        public static TomlDuration CreateAttached(this TomlObject rootSource, TimeSpan value)
            => new TomlDuration(rootSource.Root, value);

        public static TomlOffsetDateTime CreateAttached(this TomlObject rootSource, DateTimeOffset value)
            => new TomlOffsetDateTime(rootSource.Root, value);

        // Arrays
        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<bool> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlBool(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<string> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlString(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<long> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlInt(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<int> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlInt(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<double> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlFloat(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<float> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlFloat(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<TimeSpan> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlDuration(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<DateTime> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlOffsetDateTime(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<DateTimeOffset> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlOffsetDateTime(rootSource.Root, v)).ToArray());

        // Table
        public static TomlTable CreateAttached(
            this TomlObject rootSource, object obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            if (obj is TomlObject to)
            {
                throw new ArgumentException(string.Format(TomlObjectCreateNotAllowed2A, nameof(obj), to.ReadableTypeName));
            }

            if (obj is IResult fr)
            {
                throw new ArgumentException($"Cannot add factory result object of type '{fr.GetType()}'.");
            }

            var t = TomlTable.CreateFromComplexObject(rootSource.Root, obj, type);
            return t;
        }

        public static TomlTable CreateAttached<TValue>(
            this TomlObject rootSource,
            IDictionary<string, TValue> tableData,
            TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            if (tableData is TomlObject)
            {
                throw new ArgumentException(string.Format(TomlObjectCreateNotAllowed2A, nameof(tableData), tableData.GetType()));
            }

            var tbl = TomlTable.CreateFromDictionary(rootSource.Root, tableData, type);
            return tbl;
        }

        // Table Array
        public static TomlTableArray CreateAttached<T>(
            this TomlObject rootSource, IEnumerable<T> obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            if (obj is TomlObject tobj)
            {
                throw new ArgumentException(string.Format(TomlObjectCreateNotAllowed2A, nameof(obj), tobj.ReadableTypeName));
            }

            foreach (var it in obj.Select((x, i) => new { Value = x, Index = i }))
            {
                if ((object)it.Value is TomlObject to)
                {
                    throw new InvalidOperationException(
                        $"Element location '{it.Index}' is a TOML object of type '{to.ReadableTypeName}'. " +
                        $"Passing TOML objects to a create method is not allowed.");
                }
            }

            var tables = obj.Select(o => TomlTable.CreateFromComplexObject(rootSource.Root, o, type));

            return new TomlTableArray(rootSource.Root, tables);
        }

        // Empty Structures
        public static TomlArray CreateEmptyAttachedArray(this TomlObject rootSource)
            => CreateAttached(rootSource, new bool[] { });

        public static TomlTableArray CreateEmptyAttachedTableArray(this TomlObject rootSource)
            => CreateAttached(rootSource, new object[] { });

        public static TomlTable CreateEmptyAttachedTable(
            this TomlObject rootSource, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            => CreateAttached(rootSource, new object());
    }
}
