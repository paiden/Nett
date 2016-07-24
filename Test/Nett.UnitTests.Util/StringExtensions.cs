using System;
using System.IO;

namespace Nett.UnitTests.Util
{
    public static class StringExtensions
    {
        public static Stream ToStream(this string s)
        {
            var ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(s);
            writer.Flush();
            ms.Position = 0;
            return ms;
        }

        public static string StripWhitespace(this string s) =>
            s.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

        public static string TestRunUniqueName(this string s) => s + Guid.NewGuid();
    }
}
