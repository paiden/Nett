using System.Collections.Generic;
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

        [Fact(DisplayName = "When plugin is registered as a object, that plugin config gets written correctly.")]
        public void WhenPluginConfigRegisteredAsObjectItGetsWritten()
        {
            // Arrange
            var cfg = ConfigWithPluginARegisteredAsObject();

            // Act
            var s = Toml.WriteString(cfg);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo(ExpectedConfigWrittenWhenARegistered);
        }

        [Fact(DisplayName = "When plugin is registered as a dictionary that plugin config gets written correctly.")]
        public void WhenPluginConfigRegisteredAsDictionaryItGetsWritten()
        {
            // Arrange
            var cfg = ConfigWithPluginARegisteredAsDict();

            // Act
            var s = Toml.WriteString(cfg);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo(ExpectedConfigWrittenWhenARegistered);
        }

        [Fact(DisplayName = "No matter how plugin is registered (object/dict) the written result is the same.")]
        public void WrittenResultsIsTheSameIndependentOfSourceType()
        {
            // Arrange
            var cfgObj = ConfigWithPluginARegisteredAsObject();
            var cfgDict = ConfigWithPluginARegisteredAsDict();

            // Act
            var sObj = Toml.WriteString(cfgObj);
            var sDict = Toml.WriteString(cfgDict);

            // Assert
            sObj.ShouldBeSemanticallyEquivalentTo(sDict);
        }

        /// <summary>
        /// As we do not know the correct CLR type during read, it has to be mapped to a equivalent generic structure. As the root object
        /// of the config is a dictionary, that means that the table (whole table graph) will be converted to a dictionary graph.
        /// </summary>
        [Fact(DisplayName = "When PluginA config section is in TOML file that section is read correctly into data structure (as a dictionary).")]
        public void WhenPluginAInTomlFileThatSectionIsReadCorrectlyIntoDataStructure()
        {
            // Act
            var c = Toml.ReadString<MainConfig>(ExpectedConfigWrittenWhenARegistered);

            // Assert
            c.PluginConfigs.Keys.Single().Should().Be(PluginAConfig.Key);
            c.PluginConfigs[PluginAConfig.Key].GetType().Should().Be(typeof(Dictionary<string, object>));
            ((long)((Dictionary<string, object>)c.PluginConfigs[PluginAConfig.Key])[PluginAConfig.SettingKey]).Should().Be(0);

        }

        private static MainConfig ConfigWithNoPluginRegisteredCreated() => new MainConfig();

        private MainConfig ConfigWithPluginARegisteredAsObject()
        {
            var cfg = ConfigWithNoPluginRegisteredCreated();
            cfg.PluginConfigs[PluginAConfig.Key] = new PluginAConfig();
            return cfg;
        }


        private MainConfig ConfigWithPluginARegisteredAsDict()
        {
            var cfg = ConfigWithNoPluginRegisteredCreated();
            cfg.PluginConfigs[PluginAConfig.Key] = (new PluginAConfig()).ToDictionary();
            return cfg;
        }
    }
}
