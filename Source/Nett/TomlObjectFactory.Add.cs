using System;
using System.Collections.Generic;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        public static TomlArray AddArray(this TomlTable table, string key, TomlArray a)
        {
            table.AddRow(new TomlKey(key), a);
            return a;
        }

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<bool> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<string> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<long> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<int> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<double> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<float> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<DateTimeOffset> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<DateTime> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlArray AddArray(this TomlTable table, string key, IEnumerable<TimeSpan> values)
            => table.AddArray(key, table.CreateAttachedArray(values));

        public static TomlTable AddTable(this TomlTable table, string key, TomlTable t)
        {
            table.AddRow(new TomlKey(key), t);
            return t;
        }

        public static TomlTable AddTable(
            this TomlTable table, string key, TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
            => table.AddTable(key, table.CreateAttachedTable(tableType));

        public static TomlTable AddTable(
            this TomlTable table,
            string key,
            IEnumerable<KeyValuePair<string, TomlObject>> rows,
            TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            => table.AddTable(key, table.CreateAttachedTable(rows, type));

        public static TomlTable AddTableFromClass<T>(
                    this TomlTable table, string key, T obj, TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
                    where T : class
            => table.AddTable(key, table.CreateAttachedTableFromClass(obj, tableType));

        public static TomlTableArray AddTableArray(this TomlTable table, string key, TomlTableArray array)
        {
            if (array.Root != table.Root)
            {
                throw new InvalidOperationException("Cannot add TOML table array to table because it belongs to a different graph root.");
            }

            table.AddRow(new TomlKey(key), array);
            return array;
        }

        public static TomlTableArray AddTableArray(this TomlTable table, string key)
            => table.AddTableArray(key, table.CreateAttachedTableArray());

        public static TomlTableArray AddTableArray(this TomlTable table, string key, IEnumerable<TomlTable> tables)
            => table.AddTableArray(key, table.CreateAttachedTableArray(tables));

        public static TomlBool AddValue(this TomlTable table, string key, bool value)
        {
            var b = table.CreateAttachedValue(value);
            table.AddRow(new TomlKey(key), b);
            return b;
        }

        public static TomlString AddValue(this TomlTable table, string key, string value)
        {
            var s = table.CreateAttachedValue(value);
            table.AddRow(new TomlKey(key), table.CreateAttachedValue(value));
            return s;
        }

        public static TomlInt AddValue(this TomlTable table, string key, long value)
        {
            var i = table.CreateAttachedValue(value);
            table.AddRow(new TomlKey(key), i);
            return i;
        }

        public static TomlFloat AddValue(this TomlTable table, string key, double value)
        {
            var f = table.CreateAttachedValue(value);
            table.AddRow(new TomlKey(key), f);
            return f;
        }

        public static TomlDateTime AddValue(this TomlTable table, string key, DateTimeOffset dateTime)
        {
            var dt = table.CreateAttachedValue(dateTime);
            table.AddRow(new TomlKey(key), dt);
            return dt;
        }

        public static TomlTimeSpan AddValue(this TomlTable table, string key, TimeSpan timeSpan)
        {
            var ts = table.CreateAttachedValue(timeSpan);
            table.AddRow(new TomlKey(key), ts);
            return ts;
        }
    }
}
