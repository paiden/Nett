using System;
using Nett.Util;

namespace Nett.Parser
{
    public struct SourceLocation : IEquatable<SourceLocation>
    {
        public static readonly SourceLocation None = new SourceLocation(0, 0);

        private readonly int line;
        private readonly int col;

        public SourceLocation(int line, int colum)
        {
            this.line = line;
            this.col = colum;
        }

        public override int GetHashCode()
            => HashUtil.GetHashCode(this.line, this.col);

        public override bool Equals(object obj)
            => obj is SourceLocation l && this.Equals(l);

        public bool Equals(SourceLocation other)
            => this.line == other.line && this.col == other.col;

        public override string ToString()
            => $"Line {this.line}, column {this.col}";
    }
}
