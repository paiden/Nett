using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nett.Coma
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KeyChain
    {
        private readonly List<string> segments;

        public KeyChain(IEnumerable<string> keyChain)
        {
            if (keyChain.Count() <= 0) { throw new ArgumentException("KeyChain needs at least one key segment"); }

            this.segments = new List<string>(keyChain);
        }

        public string TargetTableKey => this.segments.Last();

        private string DebuggerDisplay => string.Join(".", this.segments);

        public TomlTable ResolveChildTableOf(TomlTable table)
        {
            TomlTable resolved = table;

            for (int i = 0; i < this.segments.Count - 1; i++)
            {
                resolved = (TomlTable)resolved[this.segments[i]];
            }

            return resolved;
        }

        public TomlObject ResolveTargetRow(TomlTable table)
            => this.ResolveChildTableOf(table)[this.TargetTableKey];
    }
}
