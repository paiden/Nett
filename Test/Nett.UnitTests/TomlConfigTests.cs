using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class TomlConfigTests
    {


        [Fact]
        public void WhenConfigHasActivator_ActivatorGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Default().AddActivator<IFoo>(() => new Foo());
            string toml = @"[Foo]";

            // Act
            var co = Toml.Read<ConfigOjectWithInterface>(toml, config);

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
