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

        public static string Escape(this string s, TomlStringType type)
        {
            if (string.IsNullOrEmpty(s) || type == TomlStringType.Literal) { return s; }

            bool isLiteral = (type & TomlStringType.Literal) == TomlStringType.Literal;
            bool isMultiline = (type & TomlStringType.Multiline) == TomlStringType.Multiline;

            StringBuilder sb = new StringBuilder(s.Length * 2);
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '\\': sb.Append(isLiteral ? "\\" : @"\\"); break;
                    case '"': sb.Append(isLiteral ? "\"" : @"\"""); break;
                    case '\b': sb.Append(isLiteral ? "\b" : @"\b"); break;
                    case '\f': sb.Append(isLiteral ? "\f" : @"\f"); break;
                    case '\t': sb.Append(isLiteral ? "\t" : @"\t"); break;
                    case '\n': sb.Append(isMultiline ? "\n" : @"\n"); break;
                    case '\r': sb.Append(isMultiline ? "\r" : @"\r"); break;
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
