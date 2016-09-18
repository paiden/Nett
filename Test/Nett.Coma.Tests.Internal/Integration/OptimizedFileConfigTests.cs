using System.ComponentModel;
using System.IO;
using FluentAssertions;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Internal.Integration
{
    public sealed class OptimizedFileConfigTests
    {
        [MFact(nameof(OptimizedFileConfig), nameof(OptimizedFileConfig.Save), "Updates in memory table")]
        [Description("There was a bug, where optimized file config didn't update the in memory table and so the next load delivered old stuff")]
        public void Save_UpdatesInMemoryConfigSoNextLoadWillDeliverThatConfig()
        {
            using (var fn = TestFileName.Create(nameof(Save_UpdatesInMemoryConfigSoNextLoadWillDeliverThatConfig), "file", Toml.FileExtension))
            {
                // Arrange
                var fc = new FileConfig(new FileConfigSource(fn));
                var oc = new OptimizedFileConfig(fc);
                File.WriteAllText(fn, "X = 1");
                oc.Load(); // At least one load needs to be done, otherwise a load will be done because not any data was loaded yet

                // Act
                oc.Save(Toml.ReadString("X = 2"));

                // Assert
                var readBack = oc.Load();
                ((TomlInt)readBack["X"]).Value.Should().Be(2);
            }
        }
    }
}
