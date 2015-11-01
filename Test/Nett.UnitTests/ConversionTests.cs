using System;
using System.Collections.Generic;
using Xunit;

namespace Nett.UnitTests
{
    public class ConversionTests
    {
        [Fact]
        public void ReadToml_WhenConfigHasConverter_ConverterGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Create()
                .ConfigureType<TestStruct>()
                    .As.ConvertFrom<TomlInt>().As(ti => new TestStruct() { Value = (int)ti.Value })
                    .And.ConvertTo<TomlInt>().As(ts => new TomlInt(ts.Value))
                    .Apply();

            string toml = @"S = 10";

            // Act
            var co = Toml.ReadString<ConfigObject>(toml, config);

            // Assert
            Assert.Equal(10, co.S.Value);
        }

        [Fact]
        public void WriteToml_WhenConfigHasConverter_ConverterGetsUsed()
        {
            // Arrange
            var config = TomlConfig.Create()
                .ConfigureType<TestStruct>().As
                    .ConvertFrom<TomlInt>().As(ti => new TestStruct() { Value = (int)ti.Value })
                    .And.ConvertTo<TomlInt>().As(ts => new TomlInt(ts.Value))
                    .And.CreateWith(() => new TestStruct())
                    .And.TreatAsInlineTable()
                    .Apply();
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
            var config = TomlConfig.Create()
                .ConfigureType<IGeneric<string>>()
                    .As.ConvertFrom<TomlString>().As((ts) => new GenericImpl<string>(ts.Value))
                    .Apply()
                .ConfigureType<IGeneric<int>>()
                    .As.ConvertFrom<TomlString>().As((ts) => new GenericImpl<int>(int.Parse(ts.Value)))
                    .Apply();

            string toml = @"
Foo = ""Hello""
Foo2 = ""10""
Foo3 = [""A""]";

            // Act
            var co = Toml.ReadString<GenericHost>(toml, config);

            // Assert
            Assert.NotNull(co);
            Assert.NotNull(co.Foo);
            Assert.Equal("Hello", co.Foo.Value);
            Assert.NotNull(co.Foo2);
            Assert.Equal(10, co.Foo2.Value);
        }

        [Fact]
        public void WriteToml_ConverterIsUsedAndConvertedPropertiesAreNotEvaluated()
        {
            // Arrange
            var config = TomlConfig.Create()
                .ConfigureType<ClassWithTrowingProp>()
                    .As.ConvertTo<TomlValue>().As((_) => new TomlString("Yeah converter was used, and property not accessed"))
                    .Apply();

            var toWrite = new Foo();

            // Act
            var written = Toml.WriteString(toWrite, config);

            // Assert
            Assert.Equal(@"Prop = ""Yeah converter was used, and property not accessed""", written.Trim());
        }

        [Fact]
        public void WriteToml_WithListItemConverter_UsesConverter()
        {
            // Arrange
            var config = TomlConfig.Create()
                .ConfigureType<GenProp<GenType>>()
                    .As.ConvertTo<TomlValue>().As((_) => new TomlString("Yeah converter was used."))
                    .Apply();
            var toWrite = new GenHost();

            // Act
            var written = Toml.WriteString(toWrite, config);

            // Assert
            Assert.Equal(@"Props = [""Yeah converter was used."", ""Yeah converter was used.""]", written.Trim());
        }

        [Fact]
        public void WriteToml_WithListItemConverterAndPropertyUsesInterface_UsesConverter()
        {
            // Arrange
            var config = TomlConfig.Create()
                .ConfigureType<IGenProp<GenType>>()
                    .As.ConvertTo<TomlValue>().As((_) => new TomlString("Yeah converter was used."))
                    .Apply();
            var toWrite = new GenInterfaceHost();

            // Act
            var written = Toml.WriteString(toWrite, config);

            // Assert
            Assert.Equal(@"Props = [""Yeah converter was used."", ""Yeah converter was used.""]", written.Trim());
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
            T Value { get; set; }
        }

        private class GenericImpl<T> : IGeneric<T>
        {
            public T Value { get; set; }

            public GenericImpl(T val)
            {
                this.Value = val;
            }
        }

        private class Foo
        {
            public IClassWithTrowingProp Prop { get; set; }

            public Foo()
            {
                this.Prop = new ClassWithTrowingProp();
            }
        }

        private interface IClassWithTrowingProp
        {
            object Value { get; }
        }

        private class ClassWithTrowingProp : IClassWithTrowingProp
        {
            public object Value { get { throw new NotImplementedException(); } }
        }

        private class GenHost
        {
            public List<GenProp<GenType>> Props { get; set; }

            public GenHost()
            {

                this.Props = new List<GenProp<GenType>>()
                {
                    new GenProp<GenType>() { Value = new GenType() },
                    new GenProp<GenType>() { Value = new GenType() },
                };
            }
        }

        private class GenInterfaceHost
        {
            public List<IGenProp<GenType>> Props { get; set; }

            public GenInterfaceHost()
            {

                this.Props = new List<IGenProp<GenType>>()
                {
                    new GenProp<GenType>() { Value = new GenType() },
                    new GenProp<GenType>() { Value = new GenType() },
                };
            }
        }

        private interface IGenProp<T>
        {
            T Value { get; set; }
        }

        private class GenProp<T> : IGenProp<T>
        {
            public T Value { get; set; }
        }

        private class GenType
        {
            public string Value { get { throw new NotImplementedException(); } }
        }
    }
}
