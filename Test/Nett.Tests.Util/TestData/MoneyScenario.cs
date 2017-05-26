using System;
using FluentAssertions;
using Nett.Coma;

namespace Nett.Tests.Util.TestData
{
    public sealed class MoneyScenario : IDisposable
    {
        public TestFileName File { get; }

        private MoneyScenario(string test)
        {
            this.File = TestFileName.Create(test, "money", Toml.FileExtension);
        }

        public static MoneyScenario Setup(string test)
        {
            var scenario = new MoneyScenario(test);
            scenario.CreateFileContents();
            return scenario;
        }

        public Config<Root> NewConfig()
        {
            return Config.Create(() => new Root(), this.File);
        }

        private MoneyScenario CreateFileContents()
        {
            Toml.WriteFile(new Root(), this.File);

            return this;
        }

        public class Root
        {
            public Money Money { get; set; } = new Money() { Ammount = 1.0, Currency = "EUR" };
        }

        public class Money
        {
            public double Ammount { get; set; }

            public string Currency { get; set; }
        }

        public void Dispose()
        {
            this.File.Dispose();
        }

        public void AssertFileCurrencyIs(string c)
        {
            var tml = Toml.ReadFile<Root>(this.File);

            tml.Money.Currency.Should().Be(c);
        }
    }
}
