using System.IO;
using FluentAssertions;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Unit
{
    public sealed class FileConfigTests
    {
        [MFact(nameof(FileConfig), nameof(FileConfig.WasChangedExternally), "When file was not loaded will always return true")]
        public void WasChangedExternally_WhenFileWasNotLoadedAtLeastOnce_WillAlwaysReturnTrue()
        {
            var t = nameof(WasChangedExternally_WhenThereWasNoModification_ReturnsFalse);
            using (var file = TestFileName.Create(t, "x", Toml.FileExtension))
            {
                // Arrange
                File.WriteAllText(file, "x=0");
                var cfg = new FileConfig(file);

                // Act
                var r1 = cfg.WasChangedExternally();
                var r2 = cfg.WasChangedExternally();

                // Assert
                r1.Should().Be(true);
                r2.Should().Be(true);
            }
        }

        [MFact(nameof(FileConfig), nameof(FileConfig.WasChangedExternally), "When there was no modification will return false")]
        public void WasChangedExternally_WhenThereWasNoModification_ReturnsFalse()
        {
            var t = nameof(WasChangedExternally_WhenThereWasNoModification_ReturnsFalse);
            using (var file = TestFileName.Create(t, "x", Toml.FileExtension))
            {
                // Arrange
                File.WriteAllText(file, "x=0");
                var cfg = new FileConfig(file);
                cfg.Load();

                // Act
                var r = cfg.WasChangedExternally();

                // Assert
                r.Should().Be(false);
            }
        }

        [MFact(nameof(FileConfig), nameof(FileConfig.WasChangedExternally), "When there was a modification will return true until changes were loaded")]
        public void WasChangedExternally_WhenFileWasModified_ReturnsTrueUntilLoadIsPerformed()
        {
            var t = nameof(WasChangedExternally_WhenThereWasNoModification_ReturnsFalse);
            using (var file = TestFileName.Create(t, "x", Toml.FileExtension))
            {
                // Arrange
                File.WriteAllText(file, "x=0");
                var cfg = new FileConfig(file);
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
