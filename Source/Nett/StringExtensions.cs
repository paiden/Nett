using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Nett.Parser;

namespace Nett
{
    internal static class StringExtensions
    {
        private static readonly Regex EscapedSequence = new Regex(@"\\(u[\da-fA-F]{4}|U[\da-fA-F]{8}|.)", RegexOptions.Compiled);

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
            return EscapedSequence.Replace(
                src,
                (m) =>
                {
                    string value = m.Value;
                    if (value.Length == 2)
                    {
                        switch (value[1])
                        {
                            case '\\': return @"\";
                            case '"': return "\"";
                            case 'b': return "\b";
                            case 'f': return "\f";
                            case 't': return "\t";
                            case 'n': return "\n";
                            case 'r': return "\r";
                            default:
                                throw Parser.Parser.CreateParseError(tkn, $"String '{src}' contains the invalid escape sequence '\\{value[1]}'.");
                        }
                    }
                    else
                    {
                        return char.ConvertFromUtf32(int.Parse(value.Substring(2), NumberStyles.HexNumber));
                    }
                });
        }
    }
}
