namespace Nett.Util
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal sealed class FormattingStreamWriter : StreamWriter
    {
        private readonly IFormatProvider formatProvider;

        public FormattingStreamWriter(Stream s, IFormatProvider formatProvider)
            : base(s)
        {
            Debug.Assert(formatProvider != null);

            this.formatProvider = formatProvider;
        }

        public override IFormatProvider FormatProvider => this.formatProvider;
    }
}
