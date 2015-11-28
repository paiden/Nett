using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Nett.TestDecoder
{
    class Program
    {
        static int Main(string[] args)
        {
            TextReader reader = Console.In;
            TextWriter writer = Console.Out;
            string line;
            StringBuilder sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                sb.Append(line);
            }

            try
            {
                var t = Toml.ReadString(sb.ToString());
                var dict = t.ToDictionary();
                var json = JsonConvert.SerializeObject(dict);
                writer.Write(json);
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}
