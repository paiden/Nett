using System;
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

        public static T ReadFile<T>(string filePath) => ReadFile<T>(filePath, TomlConfig.DefaultInstance);

        public static T ReadFile<T>(string filePath, TomlConfig config)
        {
            var tt = ReadFile(filePath, config);
            return tt.Get<T>(config);
        }

        public static TomlTable ReadFile(string filePath) => ReadFile(filePath, TomlConfig.DefaultInstance);

        public static TomlTable ReadFile(string filePath, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return StreamTomlSerializer.Deserialize(fs);
            }
        }

        public static T ReadFile<T>(FileStream stream) => ReadFile<T>(stream, TomlConfig.DefaultInstance);

        public static T ReadFile<T>(FileStream stream, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var tt = StreamTomlSerializer.Deserialize(stream);
            return tt.Get<T>(config);
        }

        public static TomlTable ReadFile(FileStream stream) => ReadFile(stream, TomlConfig.DefaultInstance);

        // Make public when config will get used for something in this case in the future.
        private static TomlTable ReadFile(FileStream stream, TomlConfig config)
        {
            return StreamTomlSerializer.Deserialize(stream);
        }

        public static T ReadStream<T>(Stream stream) => ReadStream<T>(stream, TomlConfig.DefaultInstance);

        public static T ReadStream<T>(Stream stream, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var tt = StreamTomlSerializer.Deserialize(stream);
            return tt.Get<T>(config);
        }

        public static TomlTable ReadStream(Stream stream) => ReadStream(stream, TomlConfig.DefaultInstance);

        // Keep private as long as the config parameter isn't used in the method body
        private static TomlTable ReadStream(Stream stream, TomlConfig config)
        {
            return StreamTomlSerializer.Deserialize(stream);
        }

        public static T ReadString<T>(string toRead) => ReadString<T>(toRead, TomlConfig.DefaultInstance);

        public static T ReadString<T>(string toRead, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            TomlTable tt = ReadString(toRead);
            T result = tt.Get<T>(config);
            return result;
        }

        public static TomlTable ReadString(string toRead) => ReadString(toRead, TomlConfig.DefaultInstance);

        public static TomlTable ReadString(string toRead, TomlConfig config)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(toRead)))
            {
                return StreamTomlSerializer.Deserialize(ms);
            }
        }

        public static void WriteFile<T>(T obj, string filePath) =>
            WriteFile(obj, filePath, DefaultMergeCommentsMode, TomlConfig.DefaultInstance);

        public static void WriteFile<T>(T obj, string filePath, MergeCommentsMode cm) =>
            WriteFile(obj, filePath, cm, TomlConfig.DefaultInstance);

        public static void WriteFile<T>(T obj, string filePath, TomlConfig config) =>
            WriteFile(obj, filePath, DefaultMergeCommentsMode, config);

        public static void WriteFile<T>(T obj, string filePath, MergeCommentsMode cm, TomlConfig config)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var table = TomlTable.From(obj, config);

            WriteFileInternal(table, filePath, cm, config);
        }

        public static void WriteFile(TomlTable table, string filePath) =>
            WriteFileInternal(table, filePath, DefaultMergeCommentsMode, TomlConfig.DefaultInstance);

        public static void WriteFile(TomlTable table, string filePath, TomlConfig config) =>
            WriteFileInternal(table, filePath, DefaultMergeCommentsMode, config);

        public static void WriteFile(TomlTable table, string filePath, MergeCommentsMode cm) =>
            WriteFileInternal(table, filePath, cm, TomlConfig.DefaultInstance);
        public static void WriteFile(TomlTable table, string filePath, MergeCommentsMode cm, TomlConfig config)
        {
            WriteFileInternal(table, filePath, cm, config);
        }

        private static void WriteFileInternal(TomlTable table, string filePath, MergeCommentsMode cm, TomlConfig config)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            if ((cm == MergeCommentsMode.KeepNonEmpty || cm == MergeCommentsMode.KeepAll) && File.Exists(filePath))
            {
                var existing = ReadFile(filePath, config);
                table.OverwriteCommentsWithCommentsFrom(existing, cm == MergeCommentsMode.KeepAll);
            }

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var sw = new FormattingStreamWriter(fs, CultureInfo.InvariantCulture))
            {
                var writer = new TomlStreamWriter(sw, config);
                writer.WriteToml(table);
            }
        }

        public static void WriteStream<T>(T obj, Stream output) =>
            WriteStreamInternal(TomlTable.From(obj, TomlConfig.DefaultInstance), output, TomlConfig.DefaultInstance);

        public static void WriteStream<T>(T obj, Stream outStream, TomlConfig config) =>
            WriteStreamInternal(TomlTable.From(obj, config), outStream, config);

        public static void WriteStream(TomlTable table, Stream outStream) =>
            WriteStreamInternal(table, outStream, TomlConfig.DefaultInstance);

        public static void WriteStream(TomlTable table, Stream outStream, TomlConfig config) =>
            WriteStreamInternal(table, outStream, config);

        private static void WriteStreamInternal(TomlTable table, Stream outStream, TomlConfig config)
        {
            if (table == null) { throw new ArgumentNullException(nameof(table)); }
            if (outStream == null) { throw new ArgumentNullException(nameof(outStream)); }
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var sw = new FormattingStreamWriter(outStream, CultureInfo.InvariantCulture);
            var tw = new TomlStreamWriter(sw, config);
            tw.WriteToml(table);
            outStream.Position = 0;
        }

        public static string WriteString<T>(T obj) =>
            WriteStringInternal(TomlTable.From(obj, TomlConfig.DefaultInstance), TomlConfig.DefaultInstance);

        public static string WriteString<T>(T obj, TomlConfig config) =>
            WriteStringInternal(TomlTable.From(obj, config), config);

        public static string WriteString(TomlTable table) =>
            WriteStringInternal(table, TomlConfig.DefaultInstance);

        public static string WriteString(TomlTable table, TomlConfig config) =>
            WriteStringInternal(table, config);

        private static string WriteStringInternal(TomlTable table, TomlConfig config)
        {
            using (var ms = new MemoryStream(1024))
            {
                var sw = new FormattingStreamWriter(ms, CultureInfo.InvariantCulture);
                var writer = new TomlStreamWriter(sw, config);
                writer.WriteToml(table);
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }
    }
}
