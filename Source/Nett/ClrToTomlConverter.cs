using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nett.Exceptions;
using Nett.LinqExtensions;

namespace Nett
{
    internal static class ClrToTomlTableConverter
    {
        public static TomlTable.RootTable Convert(object obj, TomlSettings settings)
            => (TomlTable.RootTable)ConvertInternal(obj, settings, null, new CyclicReferenceDetector());

        public static TomlObject Convert(object obj, ITomlRoot root)
            => ConvertInternal(obj, root.Settings, root, new CyclicReferenceDetector());

        private static TomlObject ConvertInternal(
            object obj, TomlSettings settings, ITomlRoot root, CyclicReferenceDetector cycleDetector)
        {
            var objType = obj.GetType();

            using (cycleDetector.CheckedPush(obj))
            {
                var converter = settings.TryGetToTomlConverter(objType);
                if (root != null && converter != null)
                {
                    return (TomlObject)converter.Convert(root, obj, Types.TomlObjectType);
                }
                else if (obj is IEnumerable enumerable && IsTomlArrayEnumerable())
                {
                    return ConvertArrayType(root, enumerable, cycleDetector);
                }
                else
                {
                    return ConvertComplexType(obj, root, settings, cycleDetector);
                }
            }

            bool IsTomlArrayEnumerable() =>
                objType != Types.StringType && !Types.DictType.IsAssignableFrom(objType);
        }

        private static TomlTable ConvertComplexType(
            object obj, ITomlRoot root, TomlSettings settings, CyclicReferenceDetector cycleDetector)
        {
            var tbl = InitTable(ref root, settings, obj.GetType());

            List<Tuple<string, TomlObject>> rows;

            if (obj is IDictionary dict)
            {
                rows = ConvertDictionary(dict, root, cycleDetector);
            }
            else
            {
                rows = ConvertCustomClass(obj, root, cycleDetector);
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

        private static List<Tuple<string, TomlObject>> ConvertDictionary(
            IDictionary dict, ITomlRoot root, CyclicReferenceDetector cycleDetector)
        {
            var rows = new List<Tuple<string, TomlObject>>();

            foreach (DictionaryEntry de in dict)
            {
                try
                {
                    rows.Add(Tuple.Create((string)de.Key, ConvertInternal(de.Value, root.Settings, root, cycleDetector)));
                }
                catch (CircularReferenceException)
                {
                    throw new InvalidOperationException($"A circular reference was detected for key '{de.Key}'.");
                }
            }

            return rows;
        }

        private static List<Tuple<string, TomlObject>> ConvertCustomClass(
            object obj, ITomlRoot root, CyclicReferenceDetector cycleDetector)
        {
            var rows = new List<Tuple<string, TomlObject>>();
            var props = root.Settings.GetSerializationProperties(obj.GetType());

            foreach (var p in props)
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    try
                    {
                        TomlObject to = ConvertInternal(val, root.Settings, root, cycleDetector);
                        AddComments(to, p);
                        rows.Add(Tuple.Create(p.Name, to));
                    }
                    catch (CircularReferenceException)
                    {
                        throw new InvalidOperationException(
                            $"A circular reference was detected for property " +
                            $"'{p.Name}' of Type '{obj.GetType().FullName}'.");
                    }
                }
            }

            return rows;
        }

        private static TomlObject ConvertArrayType(ITomlRoot root, IEnumerable e, CyclicReferenceDetector cycleDetector)
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
                        (TomlTable)ConvertInternal(o, root.Settings, root, cycleDetector)));
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

        private sealed class CyclicReferenceDetector
        {
            private readonly Stack<object> activeParents = new Stack<object>();

            public IDisposable CheckedPush(object obj)
            {
                if (this.activeParents.Any(p => p == obj))
                {
                    throw new CircularReferenceException("Cyclic reference detected.");
                }

                this.activeParents.Push(obj);

                return new PopOnDispose(this);
            }

            private sealed class PopOnDispose : IDisposable
            {
                private readonly CyclicReferenceDetector detector;

                public PopOnDispose(CyclicReferenceDetector detector) => this.detector = detector;

                public void Dispose() => this.detector.activeParents.Pop();
            }
        }
    }
}
