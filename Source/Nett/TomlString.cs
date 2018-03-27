namespace Nett
{
    using System.Diagnostics;
    using System.Linq;
    using Extensions;
    using Nett.Parser;

    [DebuggerDisplay("{Value}")]
    public sealed class TomlString : TomlValue<string>
    {
        private static readonly char[] RT = { '\r' };
        private static readonly char[] NT = { '\n' };

        private readonly TypeOfString type;

        internal TomlString(ITomlRoot root, string value, TypeOfString type = TypeOfString.Default)
            : base(root, MultilineTrim(type, value))
        {
            this.type = type;
        }

        public enum TypeOfString
        {
            Default,
            Normal,
            Literal,
            Multiline,
            MultilineLiteral,
        }

        public override string ReadableTypeName => "string";

        public override TomlObjectType TomlType => TomlObjectType.String;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneStringFor(root);

        internal TomlString CloneStringFor(ITomlRoot root) => CopyComments(new TomlString(root, this.Value), this);

        internal override TomlValue ValueWithRoot(ITomlRoot root) => this.StringWithRoot(root);

        internal override TomlObject WithRoot(ITomlRoot root) => this.StringWithRoot(root);

        internal TomlString StringWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlString(root, this.Value, this.type);
        }

        private static string MultilineTrim(TypeOfString type, string input)
        {
            switch (type)
            {
                case TypeOfString.MultilineLiteral: return TrimFirstNewline(input);
                case TypeOfString.Multiline: return ReplaceDelimeterBackslash(TrimFirstNewline(input));
                default: return input;
            }

            string TrimFirstNewline(string i)
                => i.TrimStart(RT).TrimStart(NT);
        }

        private static string ReplaceDelimeterBackslash(string source)
        {
            for (int d = DelimeterBackslashPos(source, 0); d >= 0; d = DelimeterBackslashPos(source, d))
            {
                var nnw = NextNonWhitespaceCharacer(source, d + 1);
                source = source.Remove(d, nnw - d);
            }

            return source;
        }

        private static int DelimeterBackslashPos(string source, int startIndex)
        {
            int i1 = source.IndexOf("\\\r", startIndex);
            int i2 = source.IndexOf("\\\n", startIndex);

            var minIndex = MinGreaterThan(0, -1, i1, i2);
            return minIndex;
        }

        private static int MinGreaterThan(int absoluteMin, int defaultValue, params int[] values)
        {
            int currenMin = int.MaxValue;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] >= absoluteMin && values[i] < currenMin)
                {
                    currenMin = values[i];
                }
            }

            return currenMin == int.MaxValue ? defaultValue : currenMin;
        }

        private static int NextNonWhitespaceCharacer(string source, int startIndex)
        {
            for (int i = startIndex; i < source.Length; i++)
            {
                if (!CharExtensions.WhitspaceCharSet.Contains(source[i]))
                {
                    return i;
                }
            }

            return source.Length;
        }
    }
}
