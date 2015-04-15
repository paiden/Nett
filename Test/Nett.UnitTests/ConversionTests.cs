using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class ConversionTests
    {
        [Fact]
        public void ReadToml_WhenConfigHasConverter_ConverterGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Default()
                .AddConversion().From<int>().To<TestStruct>().As((i) => new TestStruct() { Value = i })
                .AddConversion().From<TestStruct>().To<TomlValue>().As((s) => TomlValue.From(s.Value));

            string toml = @"S = 10";

            // Act
            var co = Toml.Read<ConfigObject>(toml, config);

            // Assert
            Assert.Equal(10, co.S.Value);
        }

        [Fact]
        public void WriteToml_WhenConfigHasConverter_ConverterGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Default()
                .AddConversion().From<int>().To<TestStruct>().As((i) => new TestStruct() { Value = i })
                .AddConversion().From<TestStruct>().To<TomlValue>().As((s) => TomlValue.From(s.Value));
            var obj = new ConfigObject() { S = new TestStruct() { Value = 222 } };

            // Act
            var ser = Toml.WriteString(obj, config);

            // Assert
            Assert.Equal("S = 222\r\n", ser);
        }

        private class ConfigObject
        {
            public TestStruct S { get; set; }
        }

        private struct TestStruct
        {
            public int Value;
        }
    }
}
