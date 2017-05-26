using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class TomlEnumTests
    {
        private enum TestEnum
        {
            DefaultValue = 0,
            SomeValue = 10,
            AnotherValue = 50
        }

        [Flags]
        private enum TestFlags
        {
            NoFlags = 0,
            FlagA = 1,
            FlagB = 2,
            FlagC = 4,
            FlagD = 8
        }

        private class Foo
        {
            public TestEnum NormalEnum { get; set; } = TestEnum.SomeValue;
            public TestFlags FlagsEnum { get; set; } = TestFlags.FlagB | TestFlags.FlagC;
        }

        [Fact(DisplayName = "Show that a object that contains enums can be serialized and deserialized correctly")]
        public void SerializeAndDeserialize_ForObjectWithEnums_Works()
        {
            // Arrange
            var f = new Foo();

            // Act
            var s = Toml.WriteString(f);
            var readBack = Toml.ReadString<Foo>(s);

            // Assert
            readBack.FlagsEnum.Should().Be(f.FlagsEnum);
            readBack.NormalEnum.Should().Be(f.NormalEnum);
        }

        [Fact]
        public void DeserializeEnum_WorksCorrectly()
        {
            //Arrange
            string tomlStringTable =
                  "SomeKey = \"SomeValue\"" + Environment.NewLine
                + "AnotherKey = \"AnotherValue\"" + Environment.NewLine
                + "DefaultKey = \"DefaultValue\"" + Environment.NewLine;

            // Act
            TomlTable table = Toml.ReadString(tomlStringTable);

            //Assert
            TestEnum defaultValue = table.Get<TestEnum>("DefaultKey");
            Assert.Equal(TestEnum.DefaultValue, defaultValue);

            TestEnum anotherValue = table.Get<TestEnum>("AnotherKey");
            Assert.Equal(TestEnum.AnotherValue, anotherValue);

            TestEnum someValue = table.Get<TestEnum>("SomeKey");
            Assert.Equal(TestEnum.SomeValue, someValue);
        }

        [Fact]
        public void DeserializeFlagEnum_WorksCorrectly()
        {
            // Arrange
            string tomlStringTable =
                  "FlagAKey = \"FlagA\"" + Environment.NewLine
                + "FlagABKey = \"FlagA, FlagB\"" + Environment.NewLine
                + "FlagABCDKey = \"FlagA, FlagB, FlagC, FlagD\"" + Environment.NewLine;

            // Act
            TomlTable table = Toml.ReadString(tomlStringTable);

            // Assert
            TestFlags flagA = table.Get<TestFlags>("FlagAKey");
            Assert.Equal(TestFlags.FlagA, flagA);

            TestFlags flagAB = table.Get<TestFlags>("FlagABKey");
            Assert.Equal(TestFlags.FlagA | TestFlags.FlagB, flagAB);

            TestFlags flagABCD = table.Get<TestFlags>("FlagABCDKey");
            Assert.Equal(TestFlags.FlagA | TestFlags.FlagB | TestFlags.FlagC | TestFlags.FlagD, flagABCD);
        }
    }
}

