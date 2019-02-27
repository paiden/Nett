using System;
using System.Globalization;
using FluentAssertions;
using Nett.Parser;
using Xunit;

namespace Nett.Exp.Tests
{
    public sealed class ValueWithUnitTests
    {
        [Fact]
        public void ReadValuewWithUnit_WithSpecialUnitChar_ReadsItAsValidTomlValueWithUnit()
        {
            // Arrange

            // Act
            var t = Toml.ReadString("X = 11.4 ($)", CreateSettings());

            // Assert
            var obj = t.Get<TomlFloat>("X");

            obj.GetUnit().Should().Be("$");
            obj.Value.Should().Be(11.4);
        }

        [Fact]
        public void ReadValueWithUnit_WithSpecialUnitChar_CanBeReadAsNormalTomlValueAlso()
        {
            // Act
            var t = Toml.ReadString("X = 11.4 ($)", CreateSettings());

            // Assert
            var obj = t.Get<TomlFloat>("X");
            obj.Value.Should().Be(11.4);
        }

        [Fact]
        public void GivenUnitWithSpacesAround_WhenUnitWasRead_UnitIsTrimmmed()
        {
            // Act
            var t = Toml.ReadString<Root>("X = 11.4 ( USD )", CreateSettings());

            // Assert
            t.X.Value.Should().Be(11.4);
            t.X.Currency.Should().Be("USD");
        }

        [Fact]
        public void GivenUnitWithSpacesInUnit_WhenUnitWasRead_KeptThatInnerSpacesInUnit()
        {
            // Act
            var t = Toml.ReadString<Root>("X = 11.4 ( U S \\ \\D  \t\t )", CreateSettings());

            // Assert
            t.X.Value.Should().Be(11.4);
            t.X.Currency.Should().Be(@"U S \ \D");
        }

        [Fact]
        public void WriteValueWithUnit_ProducesCorrectTomlFragment()
        {
            // Act
            var t = Toml.WriteString(new Root(), CreateSettings());

            // Assert
            t.Trim().Should().Be("X = 10.2 (EUR)");
        }

        [Fact]
        public void WriteValueWithUnit_WithArrayOfSuchValues_WritesCorrectArray()
        {
            // Act
            var t = Toml.WriteString(new Root2(), CreateSettings());

            // Assert
            t.Trim().Should().Be("X = [1.5 ($), -1.6 (€)]");
        }

        [Fact]
        public void ReadValueWithUnit_WithArrayOfSuchValues_WritesCorrectArray()
        {
            // Act
            var t = Toml.ReadString<Root2>("X = [1.5 ($), -1.6 (€)]", CreateSettings());

            // Assert
            t.X.Should().BeEquivalentTo(new Root2().X);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("A", "A")]
        [InlineData("3", "3")]
        [InlineData("3 A", "3 A")]
        [InlineData(" 3 A ", "3 A")]
        [InlineData("m \\ s", "m \\ s")]
        [InlineData("m\\s", "m\\s")]
        [InlineData("\"B\"", "\"B\"")]
        public void GivenValidTomlWithUnit_WhenRead_ThenProducesCorrectStringUnit(string unit, string expected)
        {
            // Act
            var t = Toml.ReadString($"X = 2.3 ({unit})", CreateSettings());

            // Assert
            t.Get<TomlFloat>("X").GetUnit().Should().Be(expected);
        }

        [Theory]
        [InlineData("(()")]
        [InlineData("())")]
        [InlineData("(u))")]
        [InlineData("(u)(u)")]
        public void GivenInvalidTomlWithUnit_WhenRead_ThenProducesParseError(string unit)
        {
            // Act
            Action a = () => Toml.ReadString($"X = 2.3 ({unit})", CreateSettings());

            // Assert
            a.Should().Throw<ParseException>();
        }

        [Fact]
        public void GivenConverterForTomlBoolRegistered_WhenTomlTypeIsWritten_ThenConverterIsUsed()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
               .EnableExperimentalFeatures(f => f
                   .ValuesWithUnit())
               .ConfigureType<Money>(ct => ct
                   .WithConversionFor<TomlBool>(conv => conv
                       .ToToml(m => true, m => m.Currency))));

            // Act
            var tml = Toml.WriteString(new Root(), settings);

            // Assert
            tml.Trim().Should().Be("X = true (EUR)");
        }

        [Fact]
        public void GivenConverterForTomlIntRegistered_WhenTomlTypeIsWritten_ThenConverterIsUsed()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
               .EnableExperimentalFeatures(f => f
                   .ValuesWithUnit())
               .ConfigureType<Money>(ct => ct
                   .WithConversionFor<TomlInt>(conv => conv
                       .ToToml(m => (long)m.Value, m => m.Currency))));

            // Act
            var tml = Toml.WriteString(new Root(), settings);

            // Assert
            tml.Trim().Should().Be("X = 10 (EUR)");
        }


        [Fact]
        public void GivenConverterForTomlFloatRegistered_WhenTomlTypeIsWritten_ThenConverterIsUsed()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
               .EnableExperimentalFeatures(f => f
                   .ValuesWithUnit())
               .ConfigureType<Money>(ct => ct
                   .WithConversionFor<TomlFloat>(conv => conv
                       .ToToml(m => m.Value, m => m.Currency))));

            // Act
            var tml = Toml.WriteString(new Root(), settings);

            // Assert
            tml.Trim().Should().Be("X = 10.2 (EUR)");
        }

        [Fact]
        public void GivenConverterForTomlStringRegistered_WhenTomlTypeIsWritten_ThenConverterIsUsed()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
               .EnableExperimentalFeatures(f => f
                   .ValuesWithUnit())
               .ConfigureType<Money>(ct => ct
                   .WithConversionFor<TomlString>(conv => conv
                       .ToToml(m => m.Value.ToString(CultureInfo.InvariantCulture), m => m.Currency))));

            // Act
            var tml = Toml.WriteString(new Root(), settings);

            // Assert
            tml.Trim().Should().Be("X = \"10.2\" (EUR)");
        }

        private static TomlSettings CreateSettings()
        {
            var cfg = TomlSettings.Create(s => s
                .EnableExperimentalFeatures(f => f
                    .ValuesWithUnit())
                .ConfigureType<Money>(ct => ct
                    .WithConversionFor<TomlFloat>(conv => conv
                        .ToToml(m => m.Value, m => m.Currency)
                        .FromToml(uv => new Money() { Currency = uv.GetUnit(), Value = uv.Value }))));
            return cfg;
        }

        public class Root
        {
            public Money X { get; set; } = new Money();
        }

        public class Root2
        {
            public Money[] X { get; set; } = new Money[]
            {
                new Money() { Value = 1.5, Currency = "$" },
                new Money() { Value = -1.6, Currency = "€" },
            };
        }

        public class Money
        {
            public double Value { get; set; } = 10.2;

            public string Currency { get; set; } = "EUR";

            public override bool Equals(object obj)
            {
                return obj is Money m
                    ? this.Value == m.Value && this.Currency == m.Currency
                    : false;
            }

            public override int GetHashCode()
                => HashCode.Combine(this.Value, this.Currency);
        }
    }
}
