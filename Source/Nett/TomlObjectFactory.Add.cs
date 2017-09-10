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

        public static TomlTimeSpan Add(this TomlTable table, string key, TimeSpan value)
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
        public static TomlTable Add(
            this TomlTable table, string key, object obj, TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
        {
            if (obj is TomlObject)
            {
                throw new InvalidOperationException($"TOML objects must be added using method '{nameof(AddTomlObject)}'.");
            }

            return AddTomlObjectInternal(table, key, TomlTable.CreateFromClass(table.Root, obj, tableType));
        }

        // Table Array
        public static TomlTableArray Add(
            this TomlTable table,
            string key,
            IEnumerable<object> tableArray,
            TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
        {
            return AddTomlObjectInternal(table, key, table.CreateAttached(tableArray));
        }

        // The generic method needs a different name from the add methods, so that overload resolution with type casts
        // chooses the correct method. Otherwise the overload resolution will pick this method instead of one of the
        // Add methods above with the correct implicit cast. Instead the compiler will whine, that the type of the call
        // does not conform to the type constraint T:TomlObject. Yes a stupid little compiler you are, choosing the wrong
        // overload... tztz. E.g. if this method would also be called Add, the call tbl.Add("x", 1) would cause the compiler
        // to chose this Add instead of the correct one - Add(string,long).
        public static T AddTomlObject<T>(
            this TomlTable table, string key, T obj)
            where T : TomlObject
            => AddTomlObjectInternal(table, key, obj.Root == table.Root ? obj : (T)obj.CloneFor(table.Root));

        // Internal
        private static T AddTomlObjectInternal<T>(TomlTable table, string key, T o)
            where T : TomlObject
        {
            Assert(o.Root == table.Root);
            table.AddRow(new TomlKey(key), o);
            return o;
        }
    }
}
