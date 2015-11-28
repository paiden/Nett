using System.Text;

namespace Nett.TestDecoder
{
    internal static class StringExtensions
    {
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
    }
}
