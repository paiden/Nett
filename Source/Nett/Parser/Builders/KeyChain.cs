using System.Collections.Generic;
using System.Linq;
using Nett.Parser.Nodes;

namespace Nett.Parser.Builders
{
    internal sealed class KeyChain
    {
        private readonly IEnumerable<TomlKey> segments;

        private KeyChain(IEnumerable<TomlKey> segments)
        {
            this.segments = segments;
        }

        public bool IsLastSegment
            => this.segments.Count() <= 1;

        public bool IsEmpty
            => !this.segments.Any();

        public TomlKey Key
            => this.segments.First();

        public KeyChain Next
            => new KeyChain(this.segments.Skip(1));

        public static KeyChain FromSegments(IEnumerable<TerminalNode> segments)
        {
            return new KeyChain(segments.Select(s => new TomlKey(s.Terminal.Value)));
        }
    }
}
