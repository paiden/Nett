using Nett.Parser;
using Xunit;

namespace Nett.Tests.Util.TestData
{
    internal static class TomlValues
    {
        public static TheoryData<string, int> AllValidTomlValueTokens
        {
            get
            {
                int it = (int)TokenType.Integer;
                int ht = (int)TokenType.HexInteger;
                int ot = (int)TokenType.OctalInteger;
                int bt = (int)TokenType.BinaryInteger;
                int lt = (int)TokenType.Bool;
                int ft = (int)TokenType.Float;

                int st = (int)TokenType.String;
                int mlt = (int)TokenType.MultilineString;
                int lst = (int)TokenType.LiteralString;
                int mllt = (int)TokenType.MultilineLiteralString;

                int dt = (int)TokenType.Date;
                int tt = (int)TokenType.Time;
                int dtt = (int)TokenType.DateTime;
                int tst = (int)TokenType.Timespan;

                return new TheoryData<string, int>
                {
                    // Decimal
                    { "+99", it },
                    { "42", it },
                    { "0", it },
                    { "-17", it },
                    { "+0", it },
                    { "-0", it },
                    { "1_1", it },
                    { "1_000", it },
                    { "5_349_221", it },
                    {  "1_2_3_4_5", it },

                    // Hex
                    { "0xDEADBEEF", ht },
                    { "0xdeadbeef", ht },
                    { "0xdead_beef", ht },

                    // Octal
                    { "0o0123_4567", ot },
                    { "0o755", ot },

                    // Binary
                    { "0b11010110", bt },

                    // Bool
                    { "true", lt },
                    { "false", lt },

                    // Float
                    { "inf", ft },
                    { "+inf", ft },
                    { "-inf", ft },
                    { "nan", ft },
                    { "+nan", ft },
                    { "-nan", ft },
                    { "+1.0", ft },
                    { "3.1415", ft },
                    { "-0.01", ft },
                    { "5e+22", ft },
                    { "1e6", ft },
                    { "-2E-2", ft},
                    { "6.626e-34", ft },
                    { "1e1_00", ft },
                    { "0.99", ft },

                    // String (basic)
                    { "\"\"", st },
                    { "\"X\"", st},
                    { "\"I'm a string. \\\"You can quote me\\\". Name\tJos\u00E9\nLocation\tSF.\"", st },
                    // String (multiline basic)
                    { "\"\"\"\"\"\"", mlt },
                    { "\"\"\"\r\nRoses are red\r\nViolets are blue\"\"\"", mlt },
                    // Literal strings
                    { @"''", lst },
                    { @"'C:\Users\nodejs\templates'", lst },
                    { @"'<\i\c*\s*>'", lst},
                    { @"'Tom ""Dubs"" Preston-Werner'", lst },
                    { @"'\\ServerX\admin$\system32\'", lst },
                    // String (multiline literal)
                    { @"''''''", mllt },
                    { @"'''I [dw]on't need \d{{2}} apples'''", mllt },
                    { "'''First\r\nSecond\r\nThird'''", mllt },
                    // DateTime
                    // Offset Datetime
                    { "1979-05-27T07:32:00Z", dtt },
                    { "1979-05-27T00:32:00-07:00", dtt },
                    { "1979-05-27T00:32:00.999999-07:00", dtt },
                    { "1979-05-27 07:32:00Z", dtt },
                    { "1979-05-27 00:32:00-07:00", dtt },
                    { "1979-05-27 00:32:00.999999-07:00", dtt },
                    // LocalDateTime
                    { "1979-05-27T07:32:00", dtt },
                    { "1979-05-27T00:32:00.999999", dtt },
                    { "1979-05-27 07:32:00", dtt },
                    { "1979-05-27 00:32:00.999999", dtt },
                    // Time (currently span for backwards compat with old impl. Toml Standard changed since than.
                    // Will be updated once everything else works again.
                    { "07:32:00", tst },
                    { "00:32:00.999999", tst },
                    //// Date
                    { "1979-05-27", dtt }, // Currently datetime for backwards compatibility, change after refactoring
                };
            }
        }
    }
}
