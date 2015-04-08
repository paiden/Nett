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
        public void WhenConfigHasConverter_ConverterGetsUsed()
        {
            // Arrange
            var conv = TomlConverter<TestStruct, int>
                .FromlToml((i) => new TestStruct() { Value = i })
                .ToToml((s) => new TomlValue<int>(s.Value));
            var config = TomlConfig.Default().AddConverter(conv);

            string toml = @"S = 10";

            // Act
            var co = Toml.Read<ConfigObject>(toml, config);

            // Assert
            Assert.Equal(10, co.S.Value);
        }


        class ConfigObject
        {
            public TestStruct S { get; set; }
        }

        struct TestStruct
        {
            public int Value;
        }
    }
}
