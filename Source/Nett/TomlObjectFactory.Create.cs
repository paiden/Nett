using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        // Values
        public static TomlBool CreateAttached(this TomlObject rootSource, bool value)
            => new TomlBool(rootSource.Root, value);

        public static TomlString CreateAttached(this TomlObject rootSource, string value)
            => new TomlString(rootSource.Root, value);

        public static TomlInt CreateAttached(this TomlObject rootSource, long value)
            => new TomlInt(rootSource.Root, value);

        public static TomlFloat CreateAttached(this TomlObject rootSource, double value)
            => new TomlFloat(rootSource.Root, value);

        public static TomlTimeSpan CreateAttached(this TomlObject rootSource, TimeSpan value)
            => new TomlTimeSpan(rootSource.Root, value);

        public static TomlDateTime CreateAttached(this TomlObject rootSource, DateTimeOffset value)
            => new TomlDateTime(rootSource.Root, value);

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
            => new TomlArray(rootSource.Root, values.Select(v => new TomlTimeSpan(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<DateTime> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlDateTime(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttached(this TomlObject rootSource, IEnumerable<DateTimeOffset> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlDateTime(rootSource.Root, v)).ToArray());

        // Table
        public static TomlTable CreateAttached(
            this TomlObject rootSource, object obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            if (obj is TomlObject)
            {
                throw new InvalidOperationException("A table can only be created from a non TOML object");
            }

            var t = TomlTable.CreateFromClass(rootSource.Root, obj, type);
            return t;
        }

        // Table Array
        public static TomlTableArray CreateAttached(
            this TomlObject rootSource, IEnumerable<object> obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            foreach (var it in obj.Select((x, i) => new { Value = x, Index = i }))
            {
                if (it.Value is TomlObject to)
                {
                    throw new InvalidOperationException("T table array can only be created from non TOML objects." +
                        $"The object at element location '{it.Index}' is a TOML object of type '{to.ReadableTypeName}'.");
                }
            }

            var tables = obj.Select(o => TomlTable.CreateFromClass(rootSource.Root, o, type));

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
