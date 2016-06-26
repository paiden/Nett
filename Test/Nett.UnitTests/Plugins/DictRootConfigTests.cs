using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Nett.UnitTests.Util;
using Xunit;

namespace Nett.UnitTests.Plugins
{
    public sealed class DictRootConfigTests
    {
        private const string ExpectedConfigWrittenWhenARegistered = @"
Setting = """"

[PluginConfigs]
[PluginConfigs.SimplePluginConfig]
Setting = 1
";
        private const string TomlWhenARegistered = ExpectedConfigWrittenWhenARegistered;

        private static SimplePluginConfig simplePluginConfig;

        public DictRootConfigTests()
        {
            simplePluginConfig = new SimplePluginConfig();
        }

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
            var c = Toml.ReadString<DictRootConfig>(ExpectedConfigWrittenWhenARegistered);

            // Assert
            c.PluginConfigs.Keys.Single().Should().Be(SimplePluginConfig.Key);
            c.PluginConfigs[SimplePluginConfig.Key].GetType().Should().Be(typeof(Dictionary<string, object>));
            ((long)((Dictionary<string, object>)c.PluginConfigs[SimplePluginConfig.Key])[SimplePluginConfig.SettingKey])
                .Should().Be(SimplePluginConfig.SettingDefaultValue);

        }

        [Fact(DisplayName = "When mapping from table key to .Net type exists, that .Net type gets read instead of a generic dictionary")]
        public void WhenTableKeyToClrMappingExistsThisTypeInsteadOfGenericStructureGetsRead()
        {
            // Arrange
            var config = TomlConfig.Create(cfg => cfg
                .MapTableKey(SimplePluginConfig.Key).To<SimplePluginConfig>());

            // Act
            var c = Toml.ReadString<DictRootConfig>(TomlWhenARegistered, config);

            // Assert
            c.PluginConfigs.Keys.Single().Should().Be(SimplePluginConfig.Key);
            c.PluginConfigs[SimplePluginConfig.Key].GetType().Should().Be<SimplePluginConfig>();
            c.PluginConfigs[SimplePluginConfig.Key].Should().Be(simplePluginConfig);
        }

        [Fact(DisplayName = "When registered plugin contains dictionary and is registered in DictRootConfig, that dictionary is read correctly")]
        public void ReadReadsPuginDictionaryCorrectly()
        {
            // Arrange
            const string SrcToml = @"
Setting = """"

[PluginConfigs]
[PluginConfigs.PluginConfigWithIntDict]
[PluginConfigs.PluginConfigWithIntDict.Ports]
PortA = 1
PortB = 2";
            var config = TomlConfig.Create(cfg => cfg
                .MapTableKey(PluginConfigWithIntDict.Key)
                .To<PluginConfigWithIntDict>());

            // Act
            var c = Toml.ReadString<DictRootConfig>(SrcToml, config);

            // Assert
            c.PluginConfigs[PluginConfigWithIntDict.Key].Should().Be(new PluginConfigWithIntDict());
        }

        private static DictRootConfig CreateMainConfig() => new DictRootConfig();

        private DictRootConfig ConfigWithPluginARegisteredAsObject()
        {
            var cfg = CreateMainConfig();
            cfg.PluginConfigs[SimplePluginConfig.Key] = new SimplePluginConfig();
            return cfg;
        }

        private DictRootConfig ConfigWithPluginARegisteredAsDict()
        {
            var cfg = CreateMainConfig();
            cfg.PluginConfigs[SimplePluginConfig.Key] = (new SimplePluginConfig()).ToDictionary();
            return cfg;
        }
    }
}
