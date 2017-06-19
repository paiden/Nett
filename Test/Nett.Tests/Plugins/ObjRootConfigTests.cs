using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Plugins
{
    [ExcludeFromCodeCoverage]
    public sealed class ObjRootConfigTests
    {
        [Fact(DisplayName = "When mapping from table key to type exists, property gets set to that type")]
        public void ReadWhenMappingToTypeExistsPropertyGetSetToThatType()
        {
            // Arrange
            var config = TomlSettings.Create(cfg => cfg
                .MapTableKey(ObjRootConfig.PluginConfigKey).To<SimplePluginConfig>());

            // Act
            var c = Toml.ReadString<ObjRootConfig>(ObjRootConfig.DefaultSimpleToml, config);

            // Assert
            c.PluginConfig.Should().Be(SimplePluginConfig.CreateDefault());
        }

        [Fact(DisplayName = "When registered plugin contains dictionary, that dictionary is read correctly")]
        public void ReadReadsPuginDictionaryCorrectly()
        {
            // Arrange
            const string SrcToml = @"
RootSetting = 1

[PluginConfig]
[PluginConfig.Ports]
PortA = 1
PortB = 2";
            var config = TomlSettings.Create(cfg => cfg
                .MapTableKey(ObjRootConfig.PluginConfigKey)
                .To<PluginConfigWithIntDict>());

            // Act
            var c = Toml.ReadString<ObjRootConfig>(ObjRootConfig.DefaultSimpleToml, config);

            // Assert
            c.PluginConfig.Should().Be(new PluginConfigWithIntDict());
        }
    }
}
