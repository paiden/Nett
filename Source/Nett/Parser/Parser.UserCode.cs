using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Nett.Parser
{
    internal sealed partial class Parser
    {
        private static readonly Regex RegexUtf8Short = new Regex(@"\\[uU]([0-9A-Fa-f]{4})", RegexOptions.Compiled);
        private static readonly StringBuilder ssb = new StringBuilder(1024);
        private static readonly TomlValue DefaultTimespanValue = new TomlValue<TimeSpan>(TimeSpan.Zero);
        private static readonly string[] PathSplit = new string[] { "." };

        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        private TomlValue<long> ParseIntVal(StringBuilder sb, bool neg)
        {
            if(this.errors.count <= 0)
            {
                if(sb.Length > 1 && sb[0] == '0') { throw new Exception("Leading zeros are not allowed."); }

                var iv = long.Parse(sb.ToString());
                return new TomlValue<long>(neg ? -iv : iv);
            }
            else
            {
                return new TomlValue<long>(0);
            }
        }

        private TomlValue<double> ParseFloatVal(StringBuilder sb, bool neg)
        {
            if (this.errors.count <= 0)
            {
                // Note leading zero check is not required here, as int value gets always parsed no matter if
                // the final value is int or float, so the int parse call will already do the correct value check
                // so we can omit it here (performance).
                var fv = double.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                return new TomlValue<double>(neg ? -fv : fv);
            }
            else
            {
                return new TomlValue<double>(0);
            }
        }


        private static TomlValue ParseStringVal(string source)
        {
            ssb.Clear();
            ssb.Append(source);
            ssb.Remove(0, 1);
            ssb.Length--;
            return new TomlValue<string>(ProcessStringValueInCurrentBuilder());
        }

        private TomlValue ParseTimespanVal(string src)
        {
            if(this.errors.count <= 0)
            {
                return new TomlValue<TimeSpan>(TimeSpan.Parse(src));
            }
            else
            {
                return DefaultTimespanValue;
            }
        }

        private static TomlObject ParseMStringVal(string source)
        {
            ssb.Clear();
            ssb.Append(source);
            ssb.Remove(0, 3);
            ssb.Length -= 3;
            
            if(ssb.Length > 0 && ssb[0] == '\r') { ssb.Remove(0, 1); }
            if(ssb.Length > 0 && ssb[0] == '\n') { ssb.Remove(0, 1); }

            string s = ProcessStringValueInCurrentBuilder();
            return new TomlValue<string>(ReplaceDelimeterBackslash(s));
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

        private static TomlValue<string> ParseLStringVal(string s)
        {
            s = s.Substring(1);
            return new TomlValue<string>(s.Substring(0, s.Length - 1));
        }

        private static TomlValue<string> ParseMLStringVal(string source)
        {
            ssb.Clear();
            ssb.Append(source);
            ssb.Remove(0, 3);
            ssb.Length -= 3;

            if (ssb.Length > 0 && ssb[0] == '\r') { ssb.Remove(0, 1); }
            if (ssb.Length > 0 && ssb[0] == '\n') { ssb.Remove(0, 1); }

            return new TomlValue<string>(ssb.ToString());
        }

        private bool NotADateTime()
        {
            Token t = la;
            return t.val.Length != 4 && scanner.Peek().val != "-";
        }

        private bool IsTimespan()
        {
            Token t = la;
            if(t.val.Length != 2 || t.kind != _number) { return false; } else { t = scanner.Peek(); }
            if(t.val != ":") { return false; } else { t = scanner.Peek(); }
            if (t.val.Length != 2 || t.kind != _number) { return false; } else { t = scanner.Peek(); }
            if (t.val != ":") { return false; } else { t = scanner.Peek(); }
            if (t.val.Length != 2 || t.kind != _number) { return false; }

            return true;
        }

        private bool CommaWithAppendedValueInArray()
        {
            return la.val == "," && scanner.Peek().val != "]";
        }

        private bool IsArray()
        {
            return la.val == "[" && scanner.Peek().val == "[";
        }

        private bool NotAnArray()
        {
            Token t = la;
            return t.val == "[" && scanner.Peek().val != "[";
        }

        private void CreateTable(IEnumerable<string> tableNames)
        {
            this.current = this.parsed;

            foreach(var p in tableNames)
            {
                if(!this.current.Rows.ContainsKey(p))
                {
                    var c = new TomlTable(p);
                    this.current.Add(p, c);
                    this.current = c;
                }
                else
                {
                    this.current = (TomlTable)current.Rows[p];
                }
            }

            if (current.IsDefined)
            {
                this.SemErr(
                    string.Format("Table '{0}' was already defined. Defining a table multiple times is not allowed.", GetTableName(tableNames)));
            }
            else
            {
                this.current.IsDefined = true;
            }
        }

        private TomlArray CreateOrGetTomlArray(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                this.SemErr("Array has a invalid name.");
            }
            
            TomlTable target = GetTarget(name, this.parsed, ref name);
            TomlObject value;
            if(target.Rows.TryGetValue(name, out value))
            {
                var existing = target[name];
                var arr = existing as TomlArray;

                if (arr == null && existing != null)
                {
                    this.SemErr($"Cannot create array '{name}' because there is already a member of type '{existing.GetType().Name}' with the same name");
                }

                return arr;
            }
            else
            {
                var na = new TomlArray();
                target.Rows.Add(name, na);
                return na;
            }
        }

        private static TomlTable GetTarget(string key, TomlTable current, ref string propertyName)
        {
            if(!key.Contains(".")) { return current; }

            string[] path = key.Split(PathSplit, StringSplitOptions.None);

            for (int i = 0; i < path.Length - 1; i++)
            {
                var obj = current[path[i]];
                current = (obj as TomlTable) ?? ((TomlArray)obj).Last() as TomlTable;
            }

            propertyName = path[path.Length - 1];

            return current;
        }

        private string GetTableName(IEnumerable<string> tableNames)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var tn in tableNames)
            {
                sb.Append(tn);
                sb.Append(".");
            }

            return sb.Remove(sb.Length - 1, 1).Insert(0, '[').Append(']').ToString();
        }

        private void AddKeyValue(string key, TomlObject value)
        {
            this.current.Add(key, value);
        }
    }
}
