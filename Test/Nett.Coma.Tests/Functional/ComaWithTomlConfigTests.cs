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
    }
}
