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
                var tmlCfg = TomlSettings.Create(c => c
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

        [Fact]
        public void ReadConfigPropertyAfterInit_WhenMultiLevelTypeHasConverterInSpecializedScope_ReadsItemFromSpecScope()
        {
            using (var main = TestFileName.Create("main", Toml.FileExtension))
            using (var spec = TestFileName.Create("spec", Toml.FileExtension))
            {
                // Arrange
                var tmlCfg = TomlSettings.Create(c => c
                    .ConfigureType<MultiTableConverterObject>(tc => tc
                        .WithConversionFor<TomlString>(conv => conv
                            .ToToml(o => o.ToString())
                            .FromToml(s => MultiTableConverterObject.Parse(s.Value)))));

                Toml.WriteFile(Toml.Create(), main, tmlCfg); ;
                Toml.WriteFile(Toml.Create(CreateWithUnitItem(2.0, "X")), spec, tmlCfg);

                var cfg = Config.CreateAs()
                    .MappedToType(() => new Root())
                    .UseTomlConfiguration(tmlCfg)
                    .StoredAs(s =>
                        s.File(main).AccessedBySource("main", out var _).MergeWith(
                            s.File(spec).AccessedBySource("spec", out var _)))
                    .Initialize();

                // Act
                var val = cfg.Get(c => c.ConvertedItem.UnitItem.Unit);
                var src = cfg.GetSource(c => c.ConvertedItem.UnitItem.Unit);

                // Assert
                val.Should().Be("X");
                src.Name.Should().Be("spec");
            }
        }

        [Fact]
        public void SetProperty_InSpecializedScopeForConvertedMultiLevelType_WritesItBackToSpecScope()
        {
            using (var main = TestFileName.Create("main", Toml.FileExtension))
            using (var spec = TestFileName.Create("spec", Toml.FileExtension))
            {
                // Arrange
                var tmlCfg = TomlSettings.Create(c => c
                    .ConfigureType<MultiTableConverterObject>(tc => tc
                        .WithConversionFor<TomlString>(conv => conv
                            .ToToml(o => o.ToString())
                            .FromToml(s => MultiTableConverterObject.Parse(s.Value)))));

                Toml.WriteFile(Toml.Create(), main, tmlCfg); ;
                Toml.WriteFile(Toml.Create(CreateWithUnitItem(2.0, "X")), spec, tmlCfg);

                var cfg = Config.CreateAs()
                    .MappedToType(() => new Root())
                    .UseTomlConfiguration(tmlCfg)
                    .StoredAs(s => s
                        .File(main).MergeWith(
                            s.File(spec)))
                    .Initialize();

                // Act
                cfg.Set(c => c.ConvertedItem.UnitItem.Unit, "EX");


                // Assert
                var root = Toml.ReadFile<Root>(spec, tmlCfg);
                root.ConvertedItem.UnitItem.Unit.Should().Be("EX");
            }
        }

        private Root CreateWithUnitItem(double value, string unit)
        {
            var r = new Root()
            {
                ConvertedItem = new MultiTableConverterObject()
                {
                    Value = value,
                    UnitItem = new MultiTableConverterObject.SubItem()
                    {
                        Unit = unit,
                    }
                }
            };

            return r;
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
