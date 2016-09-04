using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Functional
{
    public sealed class ConfigSourceTests
    {
        private const string FuncGetSettingSource = "Get Setting Source";

        [FFact(FuncGetSettingSource, "Of default initialized Git config returns the correct source")]
        public void Foo()
        {
            using (var scenario = GitScenario.Setup(nameof(ConfigSourceTests)))
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();

                // Act
                var s0 = config.GetSource(c => c.Core.Symlinks);
                var s1 = config.GetSource(c => c.User.EMail);
                var s2 = config.GetSource(c => c.Core.IgnoreCase);

                // Assert
                s0.Alias.Should().Be(scenario.SystemAlias);
                s1.Alias.Should().Be(scenario.UserAlias);
                s2.Alias.Should().Be(scenario.RepoAlias);
            }
        }
    }
}
