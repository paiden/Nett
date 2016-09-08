using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett.Coma
{
    internal sealed class KeyChain
    {
        private readonly List<string> segments;

        public KeyChain(IEnumerable<string> keyChain)
        {
            if (keyChain.Count() <= 0) { throw new ArgumentException("KeyChain needs at least one key segment"); }

            this.segments = new List<string>(keyChain);
        }

        public string TargetTableKey => this.segments.Last();

        public TomlTable ResolveTargetTable(TomlTable table)
        {
            TomlTable resolved = table;

            for (int i = 0; i < this.segments.Count - 1; i++)
            {
                resolved = (TomlTable)resolved[this.segments[i]];
            }

            return resolved;
        }
    }
}
