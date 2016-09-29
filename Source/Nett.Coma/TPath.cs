using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Nett.Extensions;

namespace Nett.Coma
{
    internal interface ITPathSegment : IEquatable<ITPathSegment>
    {
        TomlObject Apply(TomlObject obj);

        void ApplyValue(TomlObject target, TomlObject value);

        TomlObject TryApply(TomlObject obj);
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    internal class TPath : IEnumerable<ITPathSegment>, IEquatable<TPath>
    {
        public const char PathSeperatorChar = '/';

        private readonly TPath prefixPath;
        private readonly ITPathSegment segment;

        public TPath()
        {
            this.prefixPath = null;
            this.segment = null;
        }

        private TPath(TPath prefixPath, ITPathSegment segment)
        {
            this.prefixPath = prefixPath;
            this.segment = segment;
        }

        public TPath Prefix => this.prefixPath;

        private string DebuggerDisplay => this.ToString();

        private bool IsRootPrefixPath => this.segment == null;

        public static TPath Parse(string src)
        {
            src.CheckNotNull(nameof(src));

            var path = new TPath();

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == '/')
                {
                    i++;
                    var key = ParseKey(ref i, src);
                    path = path.WithKeyAdded(key);
                }
                else if (src[i] == '[')
                {
                    i++;
                    var index = ParseIndex(ref i, src);
                    path = path.WithIndexAdded(index);
                }
                else
                {
                    throw new ArgumentException($"Input '{src}' is no valid TPath");
                }
            }

            return path;
        }

        public TomlObject Apply(TomlObject obj)
        {
            var ar = this.TryApply(obj);
            if (ar == null)
            {
                throw new InvalidOperationException("Failed to apply TPath");
            }
            else
            {
                return ar;
            }
        }

        public void ApplyValue(TomlObject applyTo, TomlObject value)
        {
            var target = this.prefixPath.Apply(applyTo);
            this.segment.ApplyValue(target, value);
        }

        public bool ClearFrom(TomlTable from)
        {
            from.CheckNotNull(nameof(from));

            var keySegment = this.segment as KeySegment;
            if (keySegment == null)
            {
                throw new InvalidOperationException("Final segment of path needs to be a key");
            }

            var targetTable = (TomlTable)this.prefixPath.Apply(from);
            return keySegment.ClearFrom(targetTable);
        }

        public bool Equals(TPath other)
        {
            if (other == null) { return false; }
            if (other.segment != this.segment) { return false; }

            return this.prefixPath == other.prefixPath;
        }

        public IEnumerator<ITPathSegment> GetEnumerator() => this.Segments().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public override string ToString()
        {
            if (this.IsRootPrefixPath) { return string.Empty; }

            return this.prefixPath.ToString() + this.segment.ToString();
        }

        public TomlObject TryApply(TomlObject obj)
        {
            if (this.IsRootPrefixPath)
            {
                return obj;
            }

            var po = this.prefixPath.TryApply(obj);
            return this.segment.TryApply(po);
        }

        public TPath WithIndexAdded(int index) => new TPath(this, new IndexSegment(index));

        public TPath WithKeyAdded(string key) => new TPath(this, new KeySegment(key));

        internal TPath WithSegmentAdded(ITPathSegment segment) => new TPath(this, segment);

        private static string GetKey(string segment)
        {
            var endIndex = segment.IndexOf("[");
            endIndex = endIndex > 0 ? endIndex : segment.Length;
            return segment.Substring(0, endIndex);
        }

        private static int ParseIndex(ref int parseIndex, string input) => int.Parse(ParseSegment(ref parseIndex, input, 8));

        private static string ParseKey(ref int parseIndex, string input) => ParseSegment(ref parseIndex, input, 32);

        private static string ParseSegment(ref int parseIndex, string input, int capacity)
        {
            var sb = new StringBuilder(capacity);
            for (; parseIndex < input.Length; parseIndex++)
            {
                var ic = input[parseIndex];

                if (ic == '/' || ic == ']' || ic == '[')
                {
                    if (ic != ']') { parseIndex--; } // No closing tag -> got back one char to compensate the final parseIndex++
                    break;
                }
                else
                {
                    sb.Append(ic);
                }
            }

            var s = sb.ToString();

            if (s.Trim().Length <= 0)
            {
                throw new ArgumentException($"Input '{input}' is no valid TPath");
            }

            return s;
        }

        private static int? TryGetIndex(string segment)
        {
            int index = segment.IndexOf("[");
            if (index >= 0)
            {
                var indexString = segment.Substring(index + 1);
                indexString = indexString.Substring(0, indexString.Length - 1);
                indexString.Replace("]", string.Empty);
                return int.Parse(indexString);
            }

            return null;
        }

        private IEnumerable<ITPathSegment> Segments()
        {
            if (this.IsRootPrefixPath)
            {
                Debug.WriteLine("emtpy");
                yield break;
            }

            foreach (var seg in this.prefixPath)
            {
                Debug.WriteLine($"Seg: {seg}");
                yield return seg;
            }

            Debug.WriteLine($"Seg: {this.segment}");
            yield return this.segment;
        }

        private sealed class IndexSegment : ITPathSegment
        {
            private readonly int index;

            public IndexSegment(int index)
            {
                this.index = index;
            }

            public TomlObject Apply(TomlObject obj)
            {
                var ar = this.TryApply(obj);

                if (ar == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot apply index path segment '{this.index}' on TOML object of type '{obj.ReadableTypeName}'.");
                }
                else
                {
                    return ar;
                }
            }

            public void ApplyValue(TomlObject target, TomlObject value)
            {
                throw new NotSupportedException("Applying values on (table-) arrays is not yet supported");
            }

            public bool Equals(ITPathSegment other)
            {
                var otherIndexSeg = other as IndexSegment;
                return otherIndexSeg != null && this.index == otherIndexSeg.index;
            }

            public override bool Equals(object obj) => this.Equals(obj as ITPathSegment);

            public override string ToString() => $"[{this.index.ToString()}]";

            public TomlObject TryApply(TomlObject obj)
            {
                var ta = obj as TomlArray;
                if (ta != null) { return ta[this.index]; }

                var tta = obj as TomlTableArray;
                if (tta != null) { return tta[this.index]; }

                return null;
            }
        }

        private sealed class KeySegment : ITPathSegment
        {
            private readonly string key;

            public KeySegment(string key)
            {
                this.key = key;
            }

            public TomlObject Apply(TomlObject obj)
            {
                var ar = this.TryApply(obj);

                if (ar == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot apply key path segment '{this.key}' on TOML object of type '{obj.ReadableTypeName}.");
                }
                else
                {
                    return ar;
                }
            }

            public void ApplyValue(TomlObject target, TomlObject value)
            {
                ((TomlTable)target)[this.key] = value;
            }

            public bool ClearFrom(TomlTable table) => table.Remove(this.key);

            public int CompareTo(ITPathSegment other)
            {
                throw new NotImplementedException();
            }

            public bool Equals(ITPathSegment other)
            {
                var otherKeySegment = other as KeySegment;

                if (other == null) { return false; }

                return this.key == otherKeySegment.key;
            }

            public override bool Equals(object obj) => this.Equals(obj as ITPathSegment);

            public override string ToString() => $"/{this.key}";

            public TomlObject TryApply(TomlObject obj)
            {
                var table = obj as TomlTable;
                if (table != null && table.ContainsKey(this.key)) { return table[this.key]; }

                return null;
            }
        }
    }
}
