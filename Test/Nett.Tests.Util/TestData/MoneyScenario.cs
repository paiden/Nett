using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Nett.Coma;

using static System.FormattableString;

namespace Nett.Tests.Util.TestData
{
    public sealed class MoneyScenario : IDisposable
    {
        public TestFileName FilePath { get; }
        public TomlConfig TmlConfig { get; }

        private MoneyScenario(string test)
        {
            this.FilePath = TestFileName.Create("money", Toml.FileExtension, test);

            this.TmlConfig = TomlConfig.Create(cfg => cfg
                .ConfigureType<Money>(typeConfig => typeConfig
                    .WithConversionFor<TomlString>(conversion => conversion
                        .ToToml(m => m.ToString())
                        .FromToml(s => Money.Parse(s.Value)))));
        }

        public static MoneyScenario Setup(string test)
        {
            var scenario = new MoneyScenario(test);
            return scenario;
        }

        public Config<Root> NewConfig()
        {
            return Config.CreateAs()
                .MappedToType(() => new Root())
                .StoredAs(store => store.File(this.FilePath))
                .UseTomlConfiguration(this.TmlConfig)
                .Initialize();
        }

        public class Root
        {
            public Money Money { get; set; } = new Money() { Ammount = 1.0, Currency = "EUR" };
        }

        public class Money
        {
            public double Ammount { get; set; }

            public string Currency { get; set; }

            public override string ToString() => Invariant($"{this.Ammount:0.00} {this.Currency}");

            public static Money Parse(string s)
            {
                var split = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new Money() { Ammount = double.Parse(split[0], CultureInfo.InvariantCulture), Currency = split[1] };

            }
        }

        public void Dispose()
        {
            this.FilePath.Dispose();
        }

        public void AssertFileCurrencyIs(string c)
        {
            var tml = Toml.ReadFile<Root>(this.FilePath, this.TmlConfig);

            tml.Money.Currency.Should().Be(c);
        }

        public void AssertInitiallyCreatedFileContainsCorrectMoney()
        {
            var content = File.ReadAllText(this.FilePath);
            content.Should().Be("Money = \"1.00 EUR\"\r\n");
        }
    }
}
