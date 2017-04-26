using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        public static TomlArray CreateAttachedArray(this TomlObject rootSource)
            => new TomlArray(rootSource.Root);

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<bool> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlBool(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<string> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlString(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<long> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlInt(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<int> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlInt(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<double> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlFloat(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<float> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlFloat(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<TimeSpan> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlTimeSpan(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<DateTime> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlDateTime(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<DateTimeOffset> values)
            => new TomlArray(rootSource.Root, values.Select(v => new TomlDateTime(rootSource.Root, v)).ToArray());

        public static TomlArray CreateAttachedArray(this TomlObject rootSource, IEnumerable<TomlValue> values)
        {
            var valArray = values.ToArray();
            if (valArray.Any(v => v.Root != rootSource.Root))
            {
                throw new InvalidOperationException(
                    "Cannot create array with values belonging to different TOML object graph root.");
            }

            return new TomlArray(rootSource.Root, valArray);
        }

        public static TomlTable CreateAttachedTable(
            this TomlObject rootSource, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            => new TomlTable(rootSource.Root, type);

        public static TomlTable CreateAttachedTableFromClass<T>(
            this TomlObject rootSource, T obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            where T : class
        {
            if (obj is TomlObject)
            {
                throw new InvalidOperationException(
                    $"Can't create TOML table from object of type '{obj.GetType().FullName}'. " +
                    "A TOML table can only be created from non TOML objects.");
            }

            var t = TomlTable.CreateFromClass(rootSource.Root, obj, type);
            return t;
        }

        public static TomlTable CreateAttachedTable(
            this TomlObject rootSource,
            IEnumerable<KeyValuePair<string, TomlObject>> rows,
            TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            var tbl = new TomlTable(rootSource.Root, type);

            foreach (var kvp in rows)
            {
                if (kvp.Value.Root != rootSource.Root)
                {
                    throw new InvalidOperationException(
                        "Cannot create table with rows belonging to a different TOML object graph root.");
                }

                tbl.AddRow(new TomlKey(kvp.Key), kvp.Value);
            }

            return tbl;
        }

        public static TomlBool CreateAttachedValue(this TomlObject rootSource, bool value)
            => new TomlBool(rootSource.Root, value);

        public static TomlString CreateAttachedValue(this TomlObject rootSource, string value)
            => new TomlString(rootSource.Root, value);

        public static TomlInt CreateAttachedValue(this TomlObject rootSource, long value)
            => new TomlInt(rootSource.Root, value);

        public static TomlFloat CreateAttachedValue(this TomlObject rootSource, double value)
            => new TomlFloat(rootSource.Root, value);

        public static TomlTimeSpan CreateAttachedValue(this TomlObject rootSource, TimeSpan value)
            => new TomlTimeSpan(rootSource.Root, value);

        public static TomlDateTime CreateAttachedValue(this TomlObject rootSource, DateTimeOffset value)
            => new TomlDateTime(rootSource.Root, value);

        public static TomlTableArray CreateAttachedTableArray(this TomlObject rootSource)
            => new TomlTableArray(rootSource.Root);

        public static TomlTableArray CreateAttachedTableArray(this TomlObject rootSource, IEnumerable<TomlTable> tables)
            => new TomlTableArray(rootSource.Root, tables);
    }
}
