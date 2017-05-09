using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Nett.Extensions;

namespace Nett.Coma.Path
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal sealed partial class TPath : IEnumerable<TPath.ITPathSegment>, IEquatable<TPath>
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

        public static TPath operator /(TPath path, ITPathSegment segment)
        {
            return new TPath(path, segment);
        }

        public TomlObject Apply(TomlObject obj)
        {
            if (this.IsRootPrefixPath) { return obj; }

            var po = this.prefixPath.Apply(obj);
            return this.segment.Apply(po);
        }

        public TomlObject TryApply(TomlObject obj)
        {
            try
            {
                if (this.IsRootPrefixPath) { return obj; }

                var po = this.prefixPath.TryApply(obj);
                return this.segment.TryApply(po);
            }
            catch
            {
                Debug.Assert(false, "Should never happen if the try methods above are implemented correctly.");
                return null;
            }
        }

        public void SetValue(TomlObject applyTo, TomlObject value)
        {
            var target = this.prefixPath.Apply(applyTo);
            this.segment.SetValue(target);
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

        internal TPath WithSegmentAdded(ITPathSegment segment) => new TPath(this, segment);

        private IEnumerable<ITPathSegment> Segments()
        {
            if (this.IsRootPrefixPath)
            {
                yield break;
            }

            foreach (var seg in this.prefixPath)
            {
                yield return seg;
            }

            yield return this.segment;
        }
    }
}
