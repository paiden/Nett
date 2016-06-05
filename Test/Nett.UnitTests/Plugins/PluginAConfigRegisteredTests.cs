using System.Linq;
using FluentAssertions;
using Nett.UnitTests.Util;
using Xunit;

namespace Nett.UnitTests.Plugins
{
    public sealed class PluginAConfigRegisteredTests
    {
        private const string ExpectedConfigWrittenWhenARegistered = @"
Setting = """"

[PluginConfigs]
[PluginConfigs.PluginAConfig]
Setting = 0
";

        [Fact(DisplayName = "When plugin is registered, that plugin config gets written correctly.")]
        public void WhenPluginConfigRegisteredItGetsWritten()
        {
            // Arrange
            var cfg = ConfigWithPluginARegisteredCreated();

            // Act
            var s = Toml.WriteString(cfg);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo(ExpectedConfigWrittenWhenARegistered);
        }

        [Fact(DisplayName = "When PluginA config section is in TOML file that section is read correctly into data structure.",
            Skip = "Functionality not implemented yet.")]
        public void WhenPluginAInTomlFileThatSectionIsReadCorrectlyIntoDataStructure()
        {
            // Act
            var c = Toml.ReadString<MainConfig>(ExpectedConfigWrittenWhenARegistered);

            // Assert
            c.PluginConfigs.Keys.Single().Should().Be(PluginAConfig.Key);
        }

        private static MainConfig ConfigWithNoPluginRegisteredCreated() => new MainConfig();

        private MainConfig ConfigWithPluginARegisteredCreated()
        {
            var cfg = ConfigWithNoPluginRegisteredCreated();
            cfg.PluginConfigs[PluginAConfig.Key] = new PluginAConfig();
            return cfg;
        }
    }
}
