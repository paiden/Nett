using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nett
{
    public class StringTomlSerializer
    {
        public static string Serialize<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public static T Deserialize<T>(string serialized)
        {
            throw new NotImplementedException();
        }

        public static TomlTable Deserialize(string serialized)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(serialized);
            using (var ms = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(ms);
                writer.Write(serialized);
                writer.Flush();
                ms.Position = 0;
                return StreamTomlSerializer.Deserialize(ms);
            }
        }
    }
}
