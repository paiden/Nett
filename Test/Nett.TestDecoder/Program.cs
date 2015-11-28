using System;
using System.Diagnostics;
using System.IO;

namespace Nett.TestDecoder
{
    class Program
    {
        static int Main(string[] args)
        {

            TextReader reader = Console.In;
            TextWriter writer = Console.Out;
            var content = reader.ReadToEnd();

            try
            {
                //Debugger.Launch();
                var t = Toml.ReadString(content);
                var dict = t.ToDictionary();
                var conv = new ToJsonConverter();
                var json = conv.Convert(t);
                writer.Write(json);

                Console.WriteLine(json);

                return 0;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                return 1;
            }
        }
    }
}
