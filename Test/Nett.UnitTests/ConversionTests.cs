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

        [Fact]
        public void RadToml_WithGenricConverters_CanFindCorrectConverter()
        {
            // Arrange
            var config = TomlConfig.Default()
                .AddConversion().From<string>().To<IGeneric<int>>().As((s) => new GenericImpl<int>(int.Parse(s)))
                .AddConversion().From<string>().To<IGeneric<string>>().As((s) => new GenericImpl<string>(s));

            string toml = @"
Foo = ""Hello""
Foo2 = ""10""
Foo3 = [""A""]";

            // Act
            var co = Toml.Read<GenericHost>(toml, config);

            // Assert
            Assert.NotNull(co);
            Assert.NotNull(co.Foo);
            Assert.Equal("Hello", co.Foo.Value);
            Assert.NotNull(co.Foo2);
            Assert.Equal(10, co.Foo2.Value);
        }

        private class ConfigObject
        {
            public TestStruct S { get; set; }
        }

        private struct TestStruct
        {
            public int Value;
        }

        private class GenericHost
        {
            public IGeneric<string> Foo { get; set; }
            public IGeneric<int> Foo2 { get; set; }

            public List<IGeneric<string>> Foo3 { get; set; }
        }

        private interface IGeneric<T>
        {
            T Value { get; set;}
        }

        private class GenericImpl<T> : IGeneric<T>
        {
            public T Value { get; set; }

            public GenericImpl(T val)
            {
                this.Value = val;
            }
        }
    }
}
