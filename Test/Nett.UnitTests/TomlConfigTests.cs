using Xunit;

namespace Nett.UnitTests
{
    public class TomlConfigTests
    {


        [Fact]
        public void WhenConfigHasActivator_ActivatorGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Create()
                .ConfigureType<IFoo>()
                    .As.CreateWith(() => new Foo())
                    .Apply();
            string toml = @"[Foo]";

            // Act
            var co = Toml.ReadString<ConfigOjectWithInterface>(toml, config);

            // Assert
            Assert.IsType<Foo>(co.Foo);
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
