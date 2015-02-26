using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Nett.Parser
{
    internal sealed partial class Parser
    {
        private static readonly Regex RegexUtf8Short = new Regex(@"\\[uU]([0-9A-Fa-f]{4})", RegexOptions.Compiled);
        private static readonly StringBuilder ssb = new StringBuilder(1024);

        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        private long ParseIntVal(StringBuilder sb, bool neg)
        {
            if(this.errors.count <= 0)
            {
                if(sb.Length > 1 && sb[0] == '0') { throw new Exception("Leading zeros are not allowed."); }

                var iv = long.Parse(sb.ToString());
                return neg ? -iv : iv;
            }
            else
            {
                return 0;
            }
        }

        private double ParseFloatVal(StringBuilder sb, bool neg)
        {
            if (this.errors.count <= 0)
            {
                // Note leading zero check is not required here, as int value gets always parsed no matter if
                // the final value is int or float, so the int parse call will already do the correct value check
                // so we can omit it here (performance).
                var fv = double.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                return neg ? -fv : fv;
            }
            else
            {
                return 0;
            }
        }


        private static string ParseStringVal(string source)
        {
            ssb.Clear();
            ssb.Append(source);
            ssb.Remove(0, 1);
            ssb.Length--;
            return ProcessStringValueInCurrentBuilder();
        }

        private static string ParseMStringVal(string source)
        {
            ssb.Clear();
            ssb.Append(source);
            ssb.Remove(0, 3);
            ssb.Length -= 3;
            
            if(ssb.Length > 0 && ssb[0] == '\r') { ssb.Remove(0, 1); }
            if(ssb.Length > 0 && ssb[0] == '\n') { ssb.Remove(0, 1); }

            string s = ProcessStringValueInCurrentBuilder();
            return ReplaceDelimeterBackslash(s);
        }

        private static string ProcessStringValueInCurrentBuilder()
        {
            ssb.Replace(@"\b", "\b")
               .Replace(@"\t", "\t")
               .Replace(@"\n", "\n")
               .Replace(@"\f", "\f")
               .Replace(@"\r", "\r")
               .Replace(@"\""", "\"")
               .Replace(@"\\", "\\");

            var replaced = ssb.ToString();

            if (replaced.IndexOf(@"\u", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                replaced = ReplaceUnicodeSequences(replaced);
            }

            return replaced;
        }

        private static string ReplaceUnicodeSequences(string source)
        {
            var replaced = RegexUtf8Short.Replace(source, (m) =>
                ((char)int.Parse(m.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            return replaced;
        }

        private static string ReplaceDelimeterBackslash(string source)
        {
            for(int d = DelimeterBackslashPos(source, 0); d >= 0; d = DelimeterBackslashPos(source, d))
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

        private static int NextNonWhitespaceCharacer(string source, int startIndex)
        {
            for(int i = startIndex; i < source.Length; i++)
            {
                if(!WhitspaceCharSet.Contains(source[i]))
                {
                    return i;
                }
            }

            return source.Length;
        }

        private static int MinGreaterThan(int absoluteMin, int defaultValue, params int[] values)
        {
            int currenMin = int.MaxValue;

            for(int i = 0; i < values.Length; i++)
            {
                if(values[i] >= absoluteMin && values[i] < currenMin)
                {
                    currenMin = values[i];
                }
            }

            return currenMin == int.MaxValue ? defaultValue : currenMin;
        }

        private static string ParseLStringVal(string s)
        {
            s = s.Substring(1);
            return s.Substring(0, s.Length - 1);
        }

        private static string ParseMLStringVal(string source)
        {
            ssb.Clear();
            ssb.Append(source);
            ssb.Remove(0, 3);
            ssb.Length -= 3;

            if (ssb.Length > 0 && ssb[0] == '\r') { ssb.Remove(0, 1); }
            if (ssb.Length > 0 && ssb[0] == '\n') { ssb.Remove(0, 1); }

            return ssb.ToString();
        }
    }
}
