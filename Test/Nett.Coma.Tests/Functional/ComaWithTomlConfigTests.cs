using FluentAssertions;
using Nett.Tests.Util;
using Nett.Tests.Util.TestData;
using Xunit;

namespace Nett.Coma.Tests.Functional
{
    public sealed class ComaWithTomlConfigTests
    {
        [Fact]
        public void WriteConfigInitially_UsesGivenTomlConfigToWriteContent()
        {
            using (var scenario = MoneyScenario.Setup(nameof(WriteConfigInitially_UsesGivenTomlConfigToWriteContent)))
            {
                // Act
                var cfg = scenario.NewConfig();

                // Assert
                scenario.AssertInitiallyCreatedFileContainsCorrectMoney();
            }
        }

        [Fact]
        public void SetConfigProperty_OfTypeHandledByConverter_WritesThePropertyCorrectly()
        {
            using (var scenario = MoneyScenario.Setup(nameof(WriteConfigInitially_UsesGivenTomlConfigToWriteContent)))
            {
                // Arrange
                var cfg = scenario.NewConfig();
                const string newCurrency = "USD";

                // Act
                cfg.Set(c => c.Money.Currency, newCurrency);

                // Assert
                scenario.AssertFileCurrencyIs(newCurrency);
            }
        }

        [Fact]
        public void SetConfigProperty_WhenMultiLevelTypeHasConverter_WritesTheSubPropertyCorrectly()
        {
            using (var fn = TestFileName.Create("file", Toml.FileExtension))
            {
                // Arrange
                var tmlCfg = TomlConfig.Create(c => c
                    .ConfigureType<MultiTableConverterObject>(tc => tc
                        .WithConversionFor<TomlString>(conv => conv
                            .ToToml(o => o.ToString())
                            .FromToml(s => MultiTableConverterObject.Parse(s.Value)))));

                var cfg = Config.CreateAs()
                    .MappedToType(() => new Root())
                    .UseTomlConfiguration(tmlCfg)
                    .StoredAs(s => s.File(fn))
                    .Initialize();

                // Act
                cfg.Set(c => c.ConvertedItem.UnitItem.Unit, "B");

                // Assert
                var tbl = Toml.ReadFile<Root>(fn, tmlCfg);
                tbl.ConvertedItem.UnitItem.Unit.Should().Be("B");
            }
        }

        public class Root
        {
            public MultiTableConverterObject ConvertedItem { get; set; } = new MultiTableConverterObject();
        }

        public class MultiTableConverterObject
        {
            public double Value { get; set; } = 1.0;
            public SubItem UnitItem { get; set; } = new SubItem();

            public class SubItem
            {
                public string Unit { get; set; } = "A";
            }

            public static MultiTableConverterObject Parse(string s)
            {
                var sp = s.Split(new char[] { ' ' });
                return new MultiTableConverterObject()
                {
                    Value = double.Parse(sp[0]),
                    UnitItem = new SubItem() { Unit = sp[1] },
                };
            }

            public override string ToString() => $"{this.Value} {this.UnitItem.Unit}";
        }
    }
}
