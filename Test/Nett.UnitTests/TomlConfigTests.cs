using Xunit;

namespace Nett.UnitTests
{
    public class TomlConfigTests
    {
        [Fact]
        public void WhenConfigHasActivator_ActivatorGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Create(cfg => cfg
                .ConfigureType<IFoo>(ct => ct
                    .CreateInstance(() => new Foo())
                )
            );
            string toml = @"[Foo]";

            // Act
            var co = Toml.ReadString<ConfigOjectWithInterface>(toml, config);

            // Assert
            Assert.IsType<Foo>(co.Foo);
        }

        [Fact(DisplayName = "External configuration batch code is executed when configuration is done")]
        public void WhenConfigShouldRunExtenralConfig_ThisConfigGetsExecuted()
        {
            // Arrange
            var config = TomlConfig.Create(cfg => cfg
                .Apply(ExternalConfigurationBatch)
            );
            string toml = @"[Foo]";

            // Act
            var co = Toml.ReadString<ConfigOjectWithInterface>(toml, config);

            // Assert
            Assert.IsType<Foo>(co.Foo);
        }

        private static void ExternalConfigurationBatch(TomlConfig.ITomlConfigBuilder builder)
        {
            builder
                .ConfigureType<IFoo>(ct => ct
                    .CreateInstance(() => new Foo())
                );
        }

        class ConfigObject
        {
            public TestStruct S { get; set; }
        }

        struct TestStruct
        {
            public int Value;
        }

        interface IFoo
        {

        }

        class Foo : IFoo
        {
        }

        class ConfigOjectWithInterface
        {
            public IFoo Foo { get; set; }
        }
    }
}
