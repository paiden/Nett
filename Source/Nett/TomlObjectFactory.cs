using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett
{
    public static class TomlObjectFactory
    {
        public static TomlBool Create(this TomlObject rootSource, bool value)
        {
            return new TomlBool(rootSource.Root, value);
        }

        public static TomlInt Create(this TomlObject rootSource, long value)
        {
            return new TomlInt(rootSource.Root, value);
        }

        public static TomlFloat Create(this TomlObject rootSource, double value)
        {
            return new TomlFloat(rootSource.Root, value);
        }

        public static TomlString Create(this TomlObject rootSource, string value)
        {
            return new TomlString(rootSource.Root, value);
        }

        public static TomlDateTime Create(this TomlObject rootSource, DateTimeOffset value)
        {
            return new TomlDateTime(rootSource.Root, value);
        }

        public static TomlTimeSpan Create(this TomlObject rootSource, TimeSpan value)
        {
            return new TomlTimeSpan(rootSource.Root, value);
        }

        public static TomlArray CreateArray(this TomlObject rootSource, IEnumerable<TomlObject> values)
        {
            return new TomlArray(rootSource.Root, values.Cast<TomlValue>().ToArray());
        }

        public static TomlTable CreateTable(this TomlObject rootSource, IEnumerable<KeyValuePair<string, TomlObject>> values)
        {
            return new TomlTable(rootSource.Root, TomlTable.TableTypes.Default);
        }

        public static TomlTableArray CreateTableArray(this TomlObject rootSource, IEnumerable<TomlTable> values)
        {
            return new TomlTableArray(rootSource.Root, values);
        }
    }
}
