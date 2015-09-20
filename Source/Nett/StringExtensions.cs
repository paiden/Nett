using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nett
{
    internal static class StringExtensions
    {
        private static readonly Regex RegexUtf8Short = new Regex(@"\\[uU]([0-9A-Fa-f]{4})", RegexOptions.Compiled);

        public static string Unescape(this string src)
        {
            bool hasUnicodeSequences = false;
            if (string.IsNullOrEmpty(src)) { return src; }

            StringBuilder sb = new StringBuilder(src.Length);

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == '\\' && (i + 1 < src.Length))
                {
                    switch (src[i + 1])
                    {
                        case '\\': sb.Append(@"\"); break;
                        case '"': sb.Append("\""); break;
                        case 'b': sb.Append("\b"); break;
                        case 'f': sb.Append("\f"); break;
                        case 't': sb.Append("\t"); break;
                        case 'n': sb.Append("\n"); break;
                        case 'r': sb.Append("\r"); break;
                        case 'u': goto case 'U';
                        case 'U': hasUnicodeSequences = true; sb.Append(src[i]); sb.Append(src[i + 1]); break;
                    }

                    i++;
                }
                else
                {
                    sb.Append(src[i]);
                }
            }

            string result = sb.ToString();
            if (hasUnicodeSequences)
            {
                result = RegexUtf8Short.Replace(result, (m) => ((char)int.Parse(m.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            }

            return result;
        }

        public static string Escape(this string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }

            StringBuilder sb = new StringBuilder(s.Length * 2);
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '\\': sb.Append(@"\\"); break;
                    case '"': sb.Append(@"\"""); break;
                    case '\b': sb.Append(@"\b"); break;
                    case '\f': sb.Append(@"\f"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    default: sb.Append(s[i]); break;
                }
            }

            return sb.ToString();
        }

        public static string TrimNChars(this string s, int n)
        {
            return s.Substring(0, s.Length - n).Substring(n);
        }
    }
}
