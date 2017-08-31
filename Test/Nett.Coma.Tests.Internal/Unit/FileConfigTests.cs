using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Nett.Tests.Util;

namespace Nett.Coma.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class FileConfigTests
    {
        [MFact(nameof(FileConfigStore), nameof(FileConfigStore.WasChangedExternally), "When file was not loaded will always return true")]
        public void WasChangedExternally_WhenFileWasNotLoadedAtLeastOnce_WillAlwaysReturnTrue()
        {
            using (var file = TestFileName.Create("x", Toml.FileExtension))
            {
                // Arrange
                File.WriteAllText(file, "x=0");
                var cfg = new FileConfigStore(TomlSettings.DefaultInstance, file);

                // Act
                var r1 = cfg.WasChangedExternally();
                var r2 = cfg.WasChangedExternally();

                // Assert
                r1.Should().Be(true);
                r2.Should().Be(true);
            }
        }

        [MFact(nameof(FileConfigStore), nameof(FileConfigStore.WasChangedExternally), "When there was no modification will return false")]
        public void WasChangedExternally_WhenThereWasNoModification_ReturnsFalse()
        {
            using (var file = TestFileName.Create("x", Toml.FileExtension))
            {
                // Arrange
                File.WriteAllText(file, "x=0");
                var cfg = new FileConfigStore(TomlSettings.DefaultInstance, file);
                cfg.Load();

                // Act
                var r = cfg.WasChangedExternally();

                // Assert
                r.Should().Be(false);
            }
        }

        [MFact(nameof(FileConfigStore), nameof(FileConfigStore.WasChangedExternally), "When there was a modification will return true until changes were loaded")]
        public void WasChangedExternally_WhenFileWasModified_ReturnsTrueUntilLoadIsPerformed()
        {
            using (var file = TestFileName.Create("x", Toml.FileExtension))
            {
                // Arrange
                File.WriteAllText(file, "x=0");
                var cfg = new FileConfigStore(TomlSettings.DefaultInstance, file);
                cfg.Load();
                using (var sw = File.AppendText(file))
                {
                    sw.WriteLine();
                    sw.WriteLine("y=1");
                }

                // Act
                var r1 = cfg.WasChangedExternally();
                var r2 = cfg.WasChangedExternally();
                cfg.Load();
                var r3 = cfg.WasChangedExternally();

                // Assert
                r1.Should().Be(true);
                r2.Should().Be(true);
                r3.Should().Be(false);
            }
        }
    }
}
