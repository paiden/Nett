using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nett.LinqExtensions;

namespace Nett
{
    internal static class ClrToTomlTableConverter
    {
        public static TomlTable.RootTable Convert(object obj, TomlSettings settings)
            => (TomlTable.RootTable)ConvertInternal(obj, settings, null);

        public static TomlObject Convert(object obj, ITomlRoot root)
            => ConvertInternal(obj, root.Settings, root);

        private static TomlObject ConvertInternal(object obj, TomlSettings settings, ITomlRoot root)
        {
            var objType = obj.GetType();

            var converter = settings.TryGetToTomlConverter(objType);
            if (root != null && converter != null)
            {
                return (TomlObject)converter.Convert(root, obj, Types.TomlObjectType);
            }
            else if (obj is IEnumerable enumerable && IsTomlArrayEnumerable())
            {
                return ConvertArrayType(root, enumerable);
            }
            else
            {
                return ConvertComplexType(obj, root, settings);
            }

            bool IsTomlArrayEnumerable() =>
                objType != Types.StringType && !Types.DictType.IsAssignableFrom(objType);
        }

        private static TomlTable ConvertComplexType(object obj, ITomlRoot root, TomlSettings settings)
        {
            var tbl = InitTable(ref root, settings, obj.GetType());

            List<Tuple<string, TomlObject>> rows;

            if (obj is IDictionary dict)
            {
                rows = ConvertDictionary(dict, root);
            }
            else
            {
                rows = ConvertCustomClass(obj, root);
            }

            AddScopeTypesLast(rows, tbl);
            return tbl;
        }

        private static TomlTable InitTable(ref ITomlRoot root, TomlSettings settings, Type objType)
        {
            var table = root == null ? new TomlTable.RootTable(settings) : new TomlTable(root, settings.GetTableType(objType));
            root = root ?? (TomlTable.RootTable)table;

            return table;
        }

        private static List<Tuple<string, TomlObject>> ConvertDictionary(IDictionary dict, ITomlRoot root)
        {
            var rows = new List<Tuple<string, TomlObject>>();

            foreach (DictionaryEntry de in dict)
            {
                rows.Add(Tuple.Create((string)de.Key, ConvertInternal(de.Value, root.Settings, root)));
            }

            return rows;
        }

        private static List<Tuple<string, TomlObject>> ConvertCustomClass(object obj, ITomlRoot root)
        {
            var rows = new List<Tuple<string, TomlObject>>();
            var props = root.Settings.GetSerializationProperties(obj.GetType());

            foreach (var p in props)
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    TomlObject to = ConvertInternal(val, root.Settings, root);
                    AddComments(to, p);
                    rows.Add(Tuple.Create(p.Name, to));
                }
            }

            return rows;
        }

        private static TomlObject ConvertArrayType(ITomlRoot root, IEnumerable e)
        {
            var et = e.GetElementType();

            if (et != null)
            {
                var conv = root.Settings.TryGetToTomlConverter(et);
                if (conv != null)
                {
                    if (conv.CanConvertTo(typeof(TomlValue)))
                    {
                        var values = e.Select((o) => (TomlValue)conv.Convert(root, o, Types.TomlValueType));
                        return new TomlArray(root, values.ToArray());
                    }
                    else if (conv.CanConvertTo(typeof(TomlTable)))
                    {
                        return new TomlTableArray(root, e.Select((o) => (TomlTable)conv.Convert(root, o, Types.TomlTableType)));
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"Cannot create array type from enumerable with element type {et.FullName}");
                    }
                }
                else
                {
                    return new TomlTableArray(root, e.Select((o) =>
                        TomlTable.CreateFromClass(root, o, root.Settings.GetTableType(et))));
                }
            }

            return new TomlArray(root);
        }

        private static void AddScopeTypesLast(List<Tuple<string, TomlObject>> allObjects, TomlTable target)
        {
            foreach (var a in allObjects.Where(o => !ScopeCreatingType(o.Item2)))
            {
                target.AddRow(new TomlKey(a.Item1), a.Item2);
            }

            foreach (var a in allObjects.Where(o => ScopeCreatingType(o.Item2)))
            {
                target.AddRow(new TomlKey(a.Item1), a.Item2);
            }
        }

        private static void AddComments(TomlObject obj, PropertyInfo pi)
        {
            var comments = pi.GetCustomAttributes(typeof(TomlCommentAttribute), false).Cast<TomlCommentAttribute>();
            foreach (var c in comments)
            {
                obj.AddComment(new TomlComment(c.Comment, c.Location));
            }
        }

        private static bool ScopeCreatingType(TomlObject obj) =>
                   obj.TomlType == TomlObjectType.Table || obj.TomlType == TomlObjectType.ArrayOfTables;
    }
}
