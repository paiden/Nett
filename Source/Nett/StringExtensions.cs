namespace Nett
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Nett.Parser;
    using Nett.Parser.Matchers;

    internal static class StringExtensions
    {
        private static readonly int RegexOffset = @"\u".Length;
        private static readonly Regex RegexUtf8Long = new Regex(@"\\U([0-9A-Fa-f]{8})", RegexOptions.Compiled);
        private static readonly Regex RegexUtf8Short = new Regex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled);

        public static bool EnsureDirectoryExists(this string s)
        {
            var d = Path.GetDirectoryName(s);
            if (string.IsNullOrWhiteSpace(d))
            {
                return false;
            }

            if (Directory.Exists(d))
            {
                return false;
            }

            Directory.CreateDirectory(d);
            return true;
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

        public static bool HasBareKeyCharsOnly(this string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!s[i].IsBareKeyChar())
                {
                    return false;
                }
            }

            return true;
        }

        public static string Unescape(this string src, Token tkn)
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
                        default:
                            throw Parser.Parser.CreateParseError(tkn, $"String '{src}' contains the invalid escape sequence '\\{src[i = 1]}'.");
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
                result = RegexUtf8Long.Replace(result, (m) => char.ConvertFromUtf32(int.Parse(m.Value.Substring(RegexOffset), NumberStyles.HexNumber)));
                result = RegexUtf8Short.Replace(result, (m) => ((char)int.Parse(m.Value.Substring(RegexOffset), NumberStyles.HexNumber)).ToString());
            }

            return result;
        }
    }
}
