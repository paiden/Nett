using System;
using System.Collections.Generic;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        // Values
        public static Result<TomlBool> Update(this TomlTable table, string key, bool value)
            => Update(table, key, table.CreateAttached(value));

        public static Result<TomlString> Update(this TomlTable table, string key, string value)
            => Update(table, key, table.CreateAttached(value));

        public static Result<TomlInt> Update(this TomlTable table, string key, long value)
            => Update(table, key, table.CreateAttached(value));

        public static Result<TomlFloat> Update(this TomlTable table, string key, double value)
            => Update(table, key, table.CreateAttached(value));

        public static Result<TomlOffsetDateTime> Update(this TomlTable table, string key, DateTimeOffset value)
            => Update(table, key, table.CreateAttached(value));

        public static Result<TomlDuration> Update(this TomlTable table, string key, TimeSpan value)
            => Update(table, key, table.CreateAttached(value));

        // Table
        public static Result<TomlTable> Update<T>(
            this TomlTable table,
            string key,
            IDictionary<string, T> tableData,
            TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            => Update(table, key, CreateAttached(table, tableData, type));

        public static Result<TomlTable> Update(
            this TomlTable table, string key, object obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            => Update(table, key, CreateAttached(table, obj, type));

        // Table Array
        public static Result<TomlTableArray> Update<T>(
            this TomlTable table, string key, IEnumerable<T> items, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
        {
            TomlTableArray tblArray = CreateAttached(table, items, type);
            table[key] = tblArray;
            return new Result<TomlTableArray>(table, tblArray);
        }

        // Arrays
        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<bool> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<string> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<long> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<int> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<double> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<float> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<DateTimeOffset> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<DateTime> array)
            => Update(table, key, table.CreateAttached(array));

        public static Result<TomlArray> Update(this TomlTable table, string key, IEnumerable<TimeSpan> array)
            => Update(table, key, table.CreateAttached(array));

        private static Result<T> Update<T>(TomlTable table, string key, T obj)
            where T : TomlObject
        {
            if (table.TryGetValue(key, out var _))
            {
                table.SetRow(new TomlKey(key), obj);
                return new Result<T>(table, obj);
            }
            else
            {
                throw new InvalidOperationException($"Cannot update because row with key '{key}' does not exist.");
            }
        }
    }
}
