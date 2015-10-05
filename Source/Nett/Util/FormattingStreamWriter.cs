using System;
using System.Diagnostics;
using System.IO;

namespace Nett.Util
{
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
