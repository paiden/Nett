using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Plugins
{
    public sealed class ObjRootConfigTests
    {
        [Fact(DisplayName = "When mapping from table key to type exists, property gets set to that type")]
        public void ReadWhenMappingToTypeExistsPropertyGetSetToThatType()
        {
            // Arrange
            var config = TomlConfig.Create(cfg => cfg
                .MapTableKey(ObjRootConfig.PluginConfigKey).To<SimplePluginConfig>());

            // Act
            var c = Toml.ReadString<ObjRootConfig>(ObjRootConfig.DefaultSimpleToml, config);

            // Assert
            c.PluginConfig.Should().Be(SimplePluginConfig.CreateDefault());
        }
    }
}
