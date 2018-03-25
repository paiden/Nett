using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class ConversionTests
    {
        [Fact(DisplayName = "Custom converter is preferred over default converter (because latest registration wins).")]
        public void CustomConverterIsPrefferedOverDefaultConverters()
        {
            // Arrange
            const int value = 10;
            const int expected = value * 2;
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<int>(ct => ct
                    .WithConversionFor<TomlInt>(conv => conv
                        .FromToml(ts => (int)ts.Value * 2))));

            string toml = @"i = 10";

            // Act
            var tbl = Toml.ReadString(toml, config);

            // Assert
            tbl["i"].Get<int>().Should().Be(expected);
        }

        [Fact(DisplayName = "When multiple custom converter apply, the latest custom converter wins.")]
        public void LatestCustomConverterWins()
        {
            // Arrange
            const int value = 10;
            const int expected = value * 4;
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<int>(ct => ct
                    .WithConversionFor<TomlInt>(conv => conv
                        .FromToml(ts => (int)ts.Value * 2)))
                .ConfigureType<int>(ct => ct
                    .WithConversionFor<TomlInt>(conv => conv
                        .FromToml(ts => (int)ts.Value * 4))));

            string toml = @"i = 10";

            // Act
            var tbl = Toml.ReadString(toml, config);

            // Assert
            tbl["i"].Get<int>().Should().Be(expected);
        }

        [Fact]
        public void ReadToml_WhenConfigHasConverter_ConverterGetsUsed()
        {
            // Arrange
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<TestStruct>(ct => ct
                    .WithConversionFor<TomlInt>(conv => conv
                        .FromToml((m, ti) => new TestStruct() { Value = (int)ti.Value })
                        .ToToml(ts => ts.Value)
                    )
                )
            );

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
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<TestStruct>(ct => ct
                    .WithConversionFor<TomlInt>(conv => conv
                        .FromToml((m, ti) => new TestStruct() { Value = (int)ti.Value })
                        .ToToml(ts => ts.Value)
                    )
                    .CreateInstance(() => new TestStruct())
                    .TreatAsInlineTable()
                )
            );
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
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<IGeneric<string>>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                        .FromToml((m, ts) => new GenericImpl<string>(ts.Value))
                    )
                )
                .ConfigureType<IGeneric<int>>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                        .FromToml((m, ts) => new GenericImpl<int>(int.Parse(ts.Value)))
                    )
                )
            );

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
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<ClassWithTrowingProp>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                        .ToToml(_ => "Yeah converter was used, and property not accessed")
                    )
                )
            );

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
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<GenProp<GenType>>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                        .ToToml(_ => "Yeah converter was used.")
                    )
                )
            );
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
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<IGenProp<GenType>>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                        .ToToml(_ => "Yeah converter was used.")
                    )
                )
            );
            var toWrite = new GenInterfaceHost();

            // Act
            var written = Toml.WriteString(toWrite, config);

            // Assert
            Assert.Equal(@"Props = [""Yeah converter was used."", ""Yeah converter was used.""]", written.Trim());
        }



        private static TomlTable SetupConversionSetTest(TomlSettings.ConversionSets set, string tomlInput)
        {
            var config = TomlSettings.Create(cfg => cfg
                .AllowImplicitConversions(set)
            );

            TomlTable table = Toml.ReadString(tomlInput, config);
            return table;
        }

        public static IEnumerable<object[]> EquivalentConversionTestData
        {
            get
            {
                // TomlFloat
                // +
                yield return new object[] { "v=1.1", new Func<TomlTable, object>(t => t.Get<double>("v")), true };
                yield return new object[] { "v=1.1", new Func<TomlTable, object>(t => t.Get<TomlFloat>("v")), true };
                // -
                yield return new object[] { "v=1.1", new Func<TomlTable, object>(t => t.Get<float>("v")), false };
                yield return new object[] { "v=1.1", new Func<TomlTable, object>(t => t.Get<int>("v")), false };
                yield return new object[] { "v=1.1", new Func<TomlTable, object>(t => t.Get<string>("v")), false };
                yield return new object[] { "v=1.1", new Func<TomlTable, object>(t => t.Get<TomlString>("v")), false };

                // TomlInt
                // +
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<long>("v")), true };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<TomlInt>("v")), true };
                // -
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<int>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<short>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<char>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<TomlString>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<bool>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<TomlBool>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<float>("v")), false };
                yield return new object[] { "v=1", new Func<TomlTable, object>(t => t.Get<TomlFloat>("v")), false };
            }
        }

        [Theory(DisplayName = "Getting value when equivalent implicit conversions are activated only exact conversions will work others will fail")]
        [MemberData(nameof(EquivalentConversionTestData))]
        public void ReadToml_Equivalent_AllowsConversionFromTomlIntToFloat(string s, Func<TomlTable, object> read, bool shouldWork)
        {
            // Arrange
            var tbl = SetupConversionSetTest(TomlSettings.ConversionSets.None, s);

            Action a = () => read(tbl);

            // Assert
            if (shouldWork)
            {
                a.ShouldNotThrow();
            }
            else
            {
                a.ShouldThrow<Exception>();
            }
        }

        [Fact(DisplayName = "Using converters allows strings in TOML file to be read as char array from generic TOML table")]
        public void ConverterAllowsStringInTomlFileToBeReadAsCharArray()
        {
            // Arrange
            const string key = "x";
            const string value = "value";
            string tmlSrc = $"{key} = '{value}'";
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<char[]>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                       .FromToml(s => s.Value.ToCharArray()))));

            // Act
            var tml = Toml.ReadString(tmlSrc, cfg);

            // Assert
            tml.Get<char[]>(key).Should().BeEquivalentTo(value.ToCharArray());
        }

        public static IEnumerable<object[]> DotNetExplicitImplicitConversionsTestData
        {
            get
            {
                // TomlFloat
                // +
                yield return new object[] { "a0", "v=1.1", new Func<TomlTable, object>(t => t.Get<double>("v")), true };
                yield return new object[] { "a1", "v=1.1", new Func<TomlTable, object>(t => t.Get<TomlFloat>("v")), true };
                yield return new object[] { "a2", "v=1.1", new Func<TomlTable, object>(t => t.Get<float>("v")), true };
                yield return new object[] { "a3", "v=1.1", new Func<TomlTable, object>(t => t.Get<int>("v")), true };
                yield return new object[] { "a4", "v=1.1", new Func<TomlTable, object>(t => t.Get<long>("v")), true };
                yield return new object[] { "a5", "v=1.1", new Func<TomlTable, object>(t => t.Get<char>("v")), true };

                yield return new object[] { "b0", "v=1.1", new Func<TomlTable, object>(t => t.Get<short>("v")), true };
                yield return new object[] { "b1", "v=1.1", new Func<TomlTable, object>(t => t.Get<TomlInt>("v")), true };

                // -
                yield return new object[] { "e1", "v=1.1", new Func<TomlTable, object>(t => t.Get<string>("v")), false };
                yield return new object[] { "e2", "v=1.1", new Func<TomlTable, object>(t => t.Get<TomlString>("v")), false };
                yield return new object[] { "e3", "v=1.1", new Func<TomlTable, object>(t => t.Get<TomlBool>("v")), false };
                yield return new object[] { "e4", "v=1.1", new Func<TomlTable, object>(t => t.Get<bool>("v")), false };

                // TomlInt
                yield return new object[] { "c1", "v=1", new Func<TomlTable, object>(t => t.Get<long>("v")), true };
                yield return new object[] { "c2", "v=1", new Func<TomlTable, object>(t => t.Get<TomlInt>("v")), true };
                yield return new object[] { "c3", "v=1", new Func<TomlTable, object>(t => t.Get<int>("v")), true };
                yield return new object[] { "c4", "v=1", new Func<TomlTable, object>(t => t.Get<short>("v")), true };
                yield return new object[] { "c5", "v=1", new Func<TomlTable, object>(t => t.Get<char>("v")), true };
                yield return new object[] { "c6", "v=1", new Func<TomlTable, object>(t => t.Get<float>("v")), true };
                yield return new object[] { "c7", "v=1", new Func<TomlTable, object>(t => t.Get<double>("v")), true };
                yield return new object[] { "c8", "v=1", new Func<TomlTable, object>(t => t.Get<TomlFloat>("v")), true };

                // -
                yield return new object[] { "d0", "v=1", new Func<TomlTable, object>(t => t.Get<TomlString>("v")), false };
                yield return new object[] { "d2", "v=1", new Func<TomlTable, object>(t => t.Get<TomlBool>("v")), false };
                yield return new object[] { "d1", "v=1", new Func<TomlTable, object>(t => t.Get<bool>("v")), false };
            }
        }

        [Theory(DisplayName = "Getting value when explicit .Net implicit conversions are activated only that conversions will work others will fail")]
        [MemberData(nameof(DotNetExplicitImplicitConversionsTestData))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void ReadToml_ExplicitDotNetImplicit_AllowsConversionFromTomlIntToFloat(string id, string s, Func<TomlTable, object> read, bool shouldWork)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            // Arrange
            var tbl = SetupConversionSetTest(TomlSettings.ConversionSets.All, s);

            // Act
            Action a = () => read(tbl);

            // Assert
            if (shouldWork)
            {
                a.ShouldNotThrow();
            }
            else
            {
                a.ShouldThrow<Exception>();
            }
        }

        public class Testy
        {
            public float One { get; set; }
        }
        [Fact]
        public void ReadToml_WithAllConversionEnabled_AllowsConversionFromTomlIntToTomlFloat()
        {
            // Arrange
            var config = TomlSettings.Create(cfg => cfg
                .AllowImplicitConversions(TomlSettings.ConversionSets.All)
            );

            string abc = "SomeFloat = 1" + Environment.NewLine;
            TomlTable table = Toml.ReadString(abc, config);

            // Act
            var val = table.Get<TomlFloat>("SomeFloat");

            // Assert
            val.Value.Should().Be(1.0f);
        }

        [Fact]
        public void ReadToml_WithAllConversionEnabled_AllowsConversionFromTomlIntToDouble()
        {
            // Arrange
            var config = TomlSettings.Create(cfg => cfg
                .AllowImplicitConversions(TomlSettings.ConversionSets.All)
            );

            string abc = "SomeFloat = 1" + Environment.NewLine;
            TomlTable table = Toml.ReadString(abc, config);

            // Act
            var val = table.Get<double>("SomeFloat");

            // Assert
            val.Should().Be(1.0);
        }

        [Fact(DisplayName = "Default config is able to automatically read TOML string as GUID")]
        public void ReadToml_WhenTomlStringIsGuid_CanAutomaticallyConvertToGuid()
        {
            // Arrange
            Guid g = Guid.NewGuid();
            var read = Toml.ReadString($"g = '{g}'");

            // Act
            var rg = read.Get<Guid>("g");

            // Assert
            rg.Should().Be(g);
        }

        [Theory(DisplayName = "Config sets 'Numerical*' cannot handle GUID / TOML string conversion automatically")]
        [InlineData(TomlSettings.ConversionSets.NumericalSize)]
        [InlineData(TomlSettings.ConversionSets.NumericalType)]
        public void ReadToml_WhenConversionLevelBelowConvert_CannotConvertStringToGuidAutomatically(TomlSettings.ConversionSets set)
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c.AllowImplicitConversions(set));
            Guid g = Guid.NewGuid();
            var read = Toml.ReadString($"g = '{g}'", cfg);

            // Act
            Action a = () => read.Get<Guid>("g");

            // Assert
            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "Default conversion set cannot do numeric type conversions (int -> float).")]
        public void ReadToml_WhenDefaultConversionIsUsed_CannotConvertBetweenIntAndFloatTypes()
        {
            // Arrange
            var read = Toml.ReadString($"i = 100");

            // Act
            Action a = () => read.Get<float>("i");

            // Assert
            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "Default conversion set cannot do numeric type conversions (float -> int).")]
        public void ReadToml_WhenDefaultConversionIsUsed_CannotConvertBetweenFloatAndIntTypes()
        {
            // Arrange
            var read = Toml.ReadString($"f = 100.0");

            // Act
            Action a = () => read.Get<int>("f");

            // Assert
            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "When all conversion activated, numeric type conversions are done implicitly (int -> float).")]
        public void ReadToml_WhenAllConversionEnabled_CannotConvertBetweenIntAndFloatTypes()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c.AllowImplicitConversions(TomlSettings.ConversionSets.All));
            var read = Toml.ReadString($"i = 100", cfg);

            // Act
            var r = read.Get<float>("i");

            // Assert
            r.Should().Be(100.0f);
        }

        [Fact(DisplayName = "When all conversion activated, numeric type conversions are done implicitly (float -> int).")]
        public void ReadToml_WhenAllConversionsEnabled_CannotConvertBetweenFloatAndIntTypes()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c.AllowImplicitConversions(TomlSettings.ConversionSets.All));
            var read = Toml.ReadString($"f = 100.0", cfg);

            // Act
            var r = read.Get<int>("f");

            // Assert
            r.Should().Be(100);
        }


        // This test doesn't test anything, it just checks that the conversion specialization extension methods
        // exist for all TOML primitives => compile error, something broke
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void AllTypeConversionSupportedByConverterApi()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            // Arrange
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<ConvertToAnyType>(ct => ct
                    .WithConversionFor<TomlInt>(conv => conv
                        .ToToml(a => 1))
                    .WithConversionFor<TomlString>(conv => conv
                        .ToToml(a => "It worked"))
                    .WithConversionFor<TomlBool>(conv => conv
                        .ToToml(a => true))
                    .WithConversionFor<TomlFloat>(conv => conv
                        .ToToml(a => 1.0))
                    .WithConversionFor<TomlDateTime>(conv => conv
                        .ToToml(a => DateTimeOffset.MaxValue))
                    .WithConversionFor<TomlDuration>(conv => conv
                        .ToToml(a => TimeSpan.MaxValue))));
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

        private class ConvertToAnyType
        {

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
