using System.Globalization;
using System.IO;
using System.Text;
using Nett.Util;

namespace Nett
{
    public static class Toml
    {
        public enum MergeCommentsMode
        {
            Overwrite,
            KeepNonEmpty,
            KeepAll,
        }

        private const MergeCommentsMode DefaultMergeCommentsMode = MergeCommentsMode.KeepNonEmpty;

        public static string WriteString<T>(T obj) => WriteString(obj, TomlConfig.DefaultInstance);

        public static string WriteString<T>(T obj, TomlConfig config)
        {
            TomlTable tt = TomlTable.From(obj, config);

            using (var ms = new MemoryStream(1024))
            {
                var sw = new FormattingStreamWriter(ms, CultureInfo.InvariantCulture);
                var writer = new TomlStreamWriter(sw, config);
                writer.WriteToml(tt);
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }

        public static void WriteFile<T>(T obj, string filePath)
        {
            WriteFile(obj, filePath, DefaultMergeCommentsMode, TomlConfig.DefaultInstance);
        }

        public static void WriteFile<T>(T obj, string filePath, MergeCommentsMode cm)
        {
            WriteFile(obj, filePath, cm, TomlConfig.DefaultInstance);
        }

        public static void WriteFile<T>(T obj, string filePath, TomlConfig config)
        {
            WriteFile(obj, filePath, DefaultMergeCommentsMode, config);
        }

        public static void WriteFile<T>(T obj, string filePath, MergeCommentsMode cm, TomlConfig config)
        {
            var table = TomlTable.From(obj, config);

            if ((cm == MergeCommentsMode.KeepNonEmpty || cm == MergeCommentsMode.KeepAll) && File.Exists(filePath))
            {
                var existing = ReadFile(filePath, config);
                table.OverwriteCommentsWithCommentsFrom(existing, cm == MergeCommentsMode.KeepAll);
            }

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var sw = new StreamWriter(fs))
            {
                var writer = new TomlStreamWriter(sw, config);
                writer.WriteToml(table);
            }
        }

        public static T Read<T>(string toRead) => Read<T>(toRead, TomlConfig.DefaultInstance);

        public static T Read<T>(string toRead, TomlConfig tomlConfig)
        {
            TomlTable tt = Read(toRead);
            T result = tt.Get<T>(tomlConfig);
            return result;
        }

        public static TomlTable Read(string toRead)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(toRead);
            using (var ms = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(ms);
                writer.Write(toRead);
                writer.Flush();
                ms.Position = 0;
                return StreamTomlSerializer.Deserialize(ms);
            }
        }

        public static T ReadFile<T>(string filePath) => ReadFile<T>(filePath, TomlConfig.DefaultInstance);

        public static T ReadFile<T>(string filePath, TomlConfig config)
        {
            var tt = ReadFile(filePath, config);
            return tt.Get<T>(config);
        }

        public static TomlTable ReadFile(string filePath) => ReadFile(filePath, TomlConfig.DefaultInstance);

        public static TomlTable ReadFile(string filePath, TomlConfig config)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return StreamTomlSerializer.Deserialize(fs);
            }
        }
    }
}
