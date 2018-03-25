using System;
using System.Collections.Generic;
using static System.Diagnostics.Debug;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        // Values
        public static TomlBool Add(this TomlTable table, string key, bool value)
            => AddTomlObjectInternal(table, key, table.CreateAttached(value));

        public static TomlString Add(this TomlTable table, string key, string value)
            => AddTomlObjectInternal(table, key, table.CreateAttached(value));

        public static TomlInt Add(this TomlTable table, string key, long value)
            => AddTomlObjectInternal(table, key, table.CreateAttached(value));

        public static TomlInt Add(this TomlTable table, string key, int value)
            => AddTomlObjectInternal(table, key, table.CreateAttached((long)value));

        public static TomlFloat Add(this TomlTable table, string key, double value)
            => AddTomlObjectInternal(table, key, table.CreateAttached(value));

        public static TomlFloat Add(this TomlTable table, string key, float value)
            => AddTomlObjectInternal(table, key, table.CreateAttached((double)value));

        public static TomlDateTime Add(this TomlTable table, string key, DateTimeOffset value)
            => AddTomlObjectInternal(table, key, table.CreateAttached(value));

        public static TomlDuration Add(this TomlTable table, string key, TimeSpan value)
            => AddTomlObjectInternal(table, key, table.CreateAttached(value));

        // Arrays
        public static TomlArray Add(this TomlTable table, string key, IEnumerable<bool> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<string> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<long> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<int> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<double> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<float> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<DateTimeOffset> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<DateTime> array)
            => AddTomlObjectInternal(table, key, table.CreateAttached(array));

        public static TomlArray Add(this TomlTable table, string key, IEnumerable<TimeSpan> values)
            => AddTomlObjectInternal(table, key, table.CreateAttached(values));

        // Table
        public static TomlTable Add<T>(
            this TomlTable table,
            string key,
            IDictionary<string, T> tableData,
            TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
            => AddTomlObjectInternal(table, key, CreateAttached(table, tableData, tableType));

        public static TomlTable Add(
            this TomlTable table, string key, object obj, TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
        {
            return AddTomlObjectInternal(table, key, TomlTable.CreateFromComplexObject(table.Root, obj, tableType));
        }

        // Table Array
        public static TomlTableArray Add<T>(
            this TomlTable table,
            string key,
            IEnumerable<T> tableArray,
            TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
        {
            return AddTomlObjectInternal(table, key, table.CreateAttached(tableArray, tableType));
        }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        public static T Add<T>(
            this TomlTable table, string key, T obj, RequireTomlObject<T> _ = null)
            where T : TomlObject
            => AddTomlObjectInternal(table, key, obj.Root == table.Root ? obj : (T)obj.CloneFor(table.Root));
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter

        // Internal
        private static T AddTomlObjectInternal<T>(TomlTable table, string key, T o)
            where T : TomlObject
        {
            Assert(o.Root == table.Root);
            table.AddRow(new TomlKey(key), o);
            return o;
        }

        public class RequireTomlObject<T>
            where T : TomlObject
        { }
    }
}
