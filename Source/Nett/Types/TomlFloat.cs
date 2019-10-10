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

        public override string ToString()
        {
            if (double.IsNaN(this.Value)) { return "nan"; }
            else if (this.Value == double.NegativeInfinity) { return "-inf"; }
            else if (this.Value == double.PositiveInfinity) { return "+inf"; }
            else { return this.Value.ToString("0.0###############", CultureInfo.InvariantCulture); }
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

        internal TomlFloat FloatWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlFloat(root, this.Value);
        }
    }
}
