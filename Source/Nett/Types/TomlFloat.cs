using System;
using System.Diagnostics;
using System.Globalization;
using Nett.Extensions;
using Nett.Parser;

namespace Nett
{
    public sealed class TomlFloat : TomlValue<double>
    {
        internal TomlFloat(ITomlRoot root, double value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "float";

        public override TomlObjectType TomlType => TomlObjectType.Float;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal static TomlFloat FromTerminal(ITomlRoot root, Token token)
        {
            Debug.Assert(token.Type == TokenType.Float);
            var s = CleanupNumberInputString(token.Value);

            switch (s)
            {
                case "-inf": return new TomlFloat(root, double.NegativeInfinity);
                case "inf":
                case "+inf": return new TomlFloat(root, double.PositiveInfinity);
                case "-nan":
                case "nan":
                case "+nan": return new TomlFloat(root, double.NaN);
                default: return new TomlFloat(root, Convert.ToDouble(s, CultureInfo.InvariantCulture));
            }
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneFloatFor(root);

        internal TomlFloat CloneFloatFor(ITomlRoot root) => CopyComments(new TomlFloat(root, this.Value), this);

        internal override TomlValue ValueWithRoot(ITomlRoot root) => this.FloatWithRoot(root);

        internal override TomlObject WithRoot(ITomlRoot root) => this.FloatWithRoot(root);

        internal TomlFloat FloatWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlFloat(root, this.Value);
        }
    }
}
