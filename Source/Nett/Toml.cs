namespace Nett
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Nett.Util;
    using Writer;

    /// <summary>
    /// Main Nett API access.
    /// </summary>
    public static class Toml
    {
        /// <summary>
        /// The TOML standard file extension '.toml'.
        /// </summary>
        public const string FileExtension = ".toml";

        /// <summary>
        /// Creates a empty TOML table.
        /// </summary>
        /// <returns>A empty table instance created with the default config.</returns>
        /// <remarks>[!include[_](../specs/toml-create-remarks.md)]</remarks>
        public static TomlTable Create() => Create(TomlConfig.DefaultInstance);

        /// <summary>
        /// Creates a empty TOML table.
        /// </summary>
        /// <param name="config">The config to use, to create the table.</param>
        /// <returns>A new empty table created with the given config.</returns>
        /// <exception cref="ArgumentNullException">*config* is **null**.</exception>
        /// <remarks>[!include[_](../specs/toml-create-remarks.md)]</remarks>
        public static TomlTable Create(TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            return new TomlTable.RootTable(config);
        }

        /// <summary>
        /// Creates a new TOML table from a given CLR object.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object.</typeparam>
        /// <param name="obj">The CLR object instance for that the TOML table will be created.</param>
        /// <returns>
        /// A new TomlTable created with the default config, equivalent to the passed CLR object.
        /// </returns>
        /// <exception cref="ArgumentNullException">*obj* is **null**.</exception>
        /// <remarks>
        /// [!include[_](../specs/toml-create-from-remarks.md)]
        /// [!include[_](../specs/toml-create-remarks.md)]
        /// </remarks>
        public static TomlTable Create<T>(T obj) => Create(obj, TomlConfig.DefaultInstance);

        /// <summary>
        /// Creates a TOML table from a given CLR object.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object.</typeparam>
        /// <param name="obj">The CLR object instance for that the TOML table will be created.</param>
        /// <param name="config">The config to use for the creation.</param>
        /// <returns>A new table representing the CLR object. The table is associated with the given config.</returns>
        /// <remarks>[!include[_](../specs/toml-create-remarks.md)]</remarks>
        public static TomlTable Create<T>(T obj, TomlConfig config) => TomlTable.RootTable.From(config, obj);

        public static T ReadFile<T>(string filePath) => ReadFile<T>(filePath, TomlConfig.DefaultInstance);

        public static T ReadFile<T>(string filePath, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var tt = ReadFile(filePath, config);
            return tt.Get<T>();
        }

        public static TomlTable ReadFile(string filePath) => ReadFile(filePath, TomlConfig.DefaultInstance);

        public static TomlTable ReadFile(string filePath, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return StreamTomlSerializer.Deserialize(fs, config);
            }
        }

        public static T ReadFile<T>(FileStream stream) => ReadFile<T>(stream, TomlConfig.DefaultInstance);

        public static T ReadFile<T>(FileStream stream, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var tt = StreamTomlSerializer.Deserialize(stream, config);
            return tt.Get<T>();
        }

        public static TomlTable ReadFile(FileStream stream) => ReadFile(stream, TomlConfig.DefaultInstance);

        public static TomlTable ReadFile(FileStream stream, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            return StreamTomlSerializer.Deserialize(stream, config);
        }

        public static T ReadStream<T>(Stream stream) => ReadStream<T>(stream, TomlConfig.DefaultInstance);

        public static T ReadStream<T>(Stream stream, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var tt = StreamTomlSerializer.Deserialize(stream, config);
            return tt.Get<T>();
        }

        public static TomlTable ReadStream(Stream stream) => ReadStream(stream, TomlConfig.DefaultInstance);

        public static TomlTable ReadStream(Stream stream, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            return StreamTomlSerializer.Deserialize(stream, config);
        }

        public static T ReadString<T>(string toRead) => ReadString<T>(toRead, TomlConfig.DefaultInstance);

        public static T ReadString<T>(string toRead, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            TomlTable tt = ReadString(toRead, config);
            T result = tt.Get<T>();
            return result;
        }

        public static TomlTable ReadString(string toRead) => ReadString(toRead, TomlConfig.DefaultInstance);

        public static TomlTable ReadString(string toRead, TomlConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(toRead)))
            {
                return StreamTomlSerializer.Deserialize(ms, config);
            }
        }

        public static void WriteFile<T>(T obj, string filePath) =>
            WriteFile(obj, filePath, TomlConfig.DefaultInstance);

        public static void WriteFile<T>(T obj, string filePath, TomlConfig config)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var table = TomlTable.RootTable.From(config, obj);

            WriteFileInternal(table, filePath, config);
        }

        public static void WriteFile(TomlTable table, string filePath) =>
            WriteFileInternal(table, filePath, TomlConfig.DefaultInstance);

        public static void WriteFile(TomlTable table, string filePath, TomlConfig config) =>
            WriteFileInternal(table, filePath, config);

        public static void WriteStream<T>(T obj, Stream output) =>
            WriteStreamInternal(TomlTable.RootTable.From(TomlConfig.DefaultInstance, obj), output);

        public static void WriteStream<T>(T obj, Stream outStream, TomlConfig config) =>
            WriteStreamInternal(TomlTable.RootTable.From(config, obj), outStream);

        public static void WriteStream(TomlTable table, Stream outStream) =>
            WriteStreamInternal(table, outStream);

        public static string WriteString<T>(T obj) =>
            WriteStringInternal(TomlTable.RootTable.From(TomlConfig.DefaultInstance, obj));

        public static string WriteString<T>(T obj, TomlConfig config) =>
            WriteStringInternal(TomlTable.RootTable.From(config, obj));

        private static void WriteFileInternal(TomlTable table, string filePath, TomlConfig config)
        {
            if (table == null) { throw new ArgumentNullException(nameof(table)); }
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            filePath.EnsureDirectoryExists();

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var sw = new FormattingStreamWriter(fs, CultureInfo.InvariantCulture))
            {
                var writer = new TomlTableWriter(sw, config);
                writer.WriteToml(table);
            }
        }

        private static void WriteStreamInternal(TomlTable table, Stream outStream)
        {
            if (table == null) { throw new ArgumentNullException(nameof(table)); }
            if (outStream == null) { throw new ArgumentNullException(nameof(outStream)); }

            var sw = new FormattingStreamWriter(outStream, CultureInfo.InvariantCulture);
            var tw = new TomlTableWriter(sw, table.Root.Config);
            tw.WriteToml(table);
            outStream.Position = 0;
        }

        private static string WriteStringInternal(TomlTable table)
        {
            using (var ms = new MemoryStream(1024))
            {
                var sw = new FormattingStreamWriter(ms, CultureInfo.InvariantCulture);
                var writer = new TomlTableWriter(sw, table.Root.Config);
                writer.WriteToml(table);
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }
    }
}
