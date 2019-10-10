namespace Nett
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Nett.Extensions;
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
        public static TomlTable Create() => Create(TomlSettings.DefaultInstance);

        /// <summary>
        /// Creates a empty TOML table.
        /// </summary>
        /// <param name="settings">The config to use, to create the table.</param>
        /// <returns>A new empty table created with the given config.</returns>
        /// <exception cref="ArgumentNullException">*config* is **null**.</exception>
        /// <remarks>[!include[_](../specs/toml-create-remarks.md)]</remarks>
        public static TomlTable Create(TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            return new TomlTable.RootTable(settings);
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
        public static TomlTable Create<T>(T obj) => Create(obj, TomlSettings.DefaultInstance);

        /// <summary>
        /// Creates a TOML table from a given CLR object.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object.</typeparam>
        /// <param name="obj">The CLR object instance for that the TOML table will be created.</param>
        /// <param name="settings">The config to use for the creation.</param>
        /// <returns>A new table representing the CLR object. The table is associated with the given config.</returns>
        /// <remarks>[!include[_](../specs/toml-create-remarks.md)]</remarks>
        public static TomlTable Create<T>(T obj, TomlSettings settings) => TomlTable.RootTable.From(settings, obj);

        /// <summary>
        /// Reads the TOML contents from some file and converts it to a CLR object.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object.</typeparam>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <returns>A CLR object representing the TOML contents of the file.</returns>
        /// <exception cref="ArgumentNullException">If *filePath* is **null**.</exception>
        /// <remarks>Uses the default <see cref="TomlSettings"/></remarks>
        public static T ReadFile<T>(string filePath) => ReadFile<T>(filePath, TomlSettings.DefaultInstance);

        /// <summary>
        /// Reads the TOML contents from some file and converts it to a CLR object.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object.</typeparam>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <param name="settings">The settings used to process the TOML content.</param>
        /// <returns>A CLR object representing the TOML contents of the file.</returns>
        /// <exception cref="ArgumentNullException">If *filePath* is **null**.</exception>
        /// <exception cref="ArgumentNullException">If *settings* is **null**</exception>
        public static T ReadFile<T>(string filePath, TomlSettings settings)
        {
            filePath.CheckNotNull(nameof(filePath));
            settings.CheckNotNull(nameof(settings));

            var tt = ReadFile(filePath, settings);
            return tt.Get<T>();
        }

        /// <summary>
        /// Reads the TOML contents from some file and maps it into a TomlTable structure.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <returns>A <see cref="TomlTable"/> corresponding to the file content.</returns>
        /// <remarks>Uses the default TOML settings while processing the file.</remarks>
        public static TomlTable ReadFile(string filePath) => ReadFile(filePath, TomlSettings.DefaultInstance);

        /// <summary>
        /// Reads the TOML contents from some file and maps it into a TomlTable structure.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <param name="settings">The settings used to process the TOML content.</param>
        /// <returns>A <see cref="TomlTable"/>corresponding to the file content.</returns>
        public static TomlTable ReadFile(string filePath, TomlSettings settings)
        {
            filePath.CheckNotNull(nameof(filePath));
            settings.CheckNotNull(nameof(settings));

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return StreamTomlSerializer.Deserialize(fs, settings);
            }
        }

        public static T ReadFile<T>(FileStream stream) => ReadFile<T>(stream, TomlSettings.DefaultInstance);

        public static T ReadFile<T>(FileStream stream, TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            var tt = StreamTomlSerializer.Deserialize(stream, settings);
            return tt.Get<T>();
        }

        public static TomlTable ReadFile(FileStream stream) => ReadFile(stream, TomlSettings.DefaultInstance);

        public static TomlTable ReadFile(FileStream stream, TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            return StreamTomlSerializer.Deserialize(stream, settings);
        }

        public static T ReadStream<T>(Stream stream) => ReadStream<T>(stream, TomlSettings.DefaultInstance);

        public static T ReadStream<T>(Stream stream, TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            var tt = StreamTomlSerializer.Deserialize(stream, settings);
            return tt.Get<T>();
        }

        public static TomlTable ReadStream(Stream stream) => ReadStream(stream, TomlSettings.DefaultInstance);

        public static TomlTable ReadStream(Stream stream, TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            return StreamTomlSerializer.Deserialize(stream, settings);
        }

        public static T ReadString<T>(string toRead) => ReadString<T>(toRead, TomlSettings.DefaultInstance);

        public static T ReadString<T>(string toRead, TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            TomlTable tt = ReadString(toRead, settings);
            T result = tt.Get<T>();
            return result;
        }

        public static TomlTable ReadString(string toRead) => ReadString(toRead, TomlSettings.DefaultInstance);

        public static TomlTable ReadString(string toRead, TomlSettings settings)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(toRead)))
            {
                return StreamTomlSerializer.Deserialize(ms, settings);
            }
        }

        public static void WriteFile<T>(T obj, string filePath) =>
            WriteFile(obj, filePath, TomlSettings.DefaultInstance);

        public static void WriteFile<T>(T obj, string filePath, TomlSettings settings)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            var table = TomlTable.RootTable.From(settings, obj);

            WriteFileInternal(table, filePath, settings);
        }

        public static void WriteFile(TomlTable table, string filePath) =>
            WriteFileInternal(table, filePath, TomlSettings.DefaultInstance);

        public static void WriteFile(TomlTable table, string filePath, TomlSettings settings) =>
            WriteFileInternal(table, filePath, settings);

        public static void WriteStream<T>(T obj, Stream output) =>
            WriteStreamInternal(TomlTable.RootTable.From(TomlSettings.DefaultInstance, obj), output);

        public static void WriteStream<T>(T obj, Stream outStream, TomlSettings settings) =>
            WriteStreamInternal(TomlTable.RootTable.From(settings, obj), outStream);

        public static void WriteStream(TomlTable table, Stream outStream) =>
            WriteStreamInternal(table, outStream);

        public static string WriteString<T>(T obj) =>
            WriteStringInternal(TomlTable.RootTable.From(TomlSettings.DefaultInstance, obj));

        public static string WriteString<T>(T obj, TomlSettings settings) =>
            WriteStringInternal(TomlTable.RootTable.From(settings, obj));

        public static string WriteString(TomlTable table)
            => WriteStringInternal(table);

        private static void WriteFileInternal(TomlTable table, string filePath, TomlSettings settings)
        {
            if (table == null) { throw new ArgumentNullException(nameof(table)); }
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            filePath.EnsureDirectoryExists();

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var sw = new FormattingStreamWriter(fs, CultureInfo.InvariantCulture))
            {
                var writer = new TomlTableWriter(sw, settings);
                writer.WriteToml(table);
            }
        }

        private static void WriteStreamInternal(TomlTable table, Stream outStream)
        {
            if (table == null) { throw new ArgumentNullException(nameof(table)); }
            if (outStream == null) { throw new ArgumentNullException(nameof(outStream)); }

            var sw = new FormattingStreamWriter(outStream, CultureInfo.InvariantCulture);
            var tw = new TomlTableWriter(sw, table.Root.Settings);
            tw.WriteToml(table);
            outStream.Position = 0;
        }

        private static string WriteStringInternal(TomlTable table)
        {
            using (var ms = new MemoryStream(1024))
            {
                var sw = new FormattingStreamWriter(ms, CultureInfo.InvariantCulture);
                var writer = new TomlTableWriter(sw, table.Root.Settings);
                writer.WriteToml(table);
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }
    }
}
