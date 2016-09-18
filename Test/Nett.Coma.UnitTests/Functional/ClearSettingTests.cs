using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Functional
{
    public sealed class ClearSettingTests
    {
        private const string FuncClearSetting = "Clear Setting";

        [FFact(FuncClearSetting, "When no target is specified, the current originator source is cleared of the setting (clear removes it from repo source).")]
        public void Foo()
        {
            using (var scenario = GitScenario.Setup(nameof(Foo))
                .UseSystemDefaultContentForeachSource())
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();

                // Act
                bool r = config.Clear(c => c.Core.Symlinks);

                r.Should().Be(true);
                config.GetSource(c => c.Core.Symlinks).Should().Be(scenario.UserFileSource);
            }
        }

        [FFact(FuncClearSetting, "When no target is specified, the current originator source is cleared of the setting (clear removes it from user source).")]
        public void FooX()
        {
            using (var scenario = GitScenario.Setup(nameof(Foo))
                .UseSystemDefaultContentForeachSource())
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();
                config.Clear(c => c.Core.Symlinks); // removes it from repo source

                // Act
                bool r = config.Clear(c => c.Core.Symlinks); // removes it from user source

                r.Should().Be(true);
                config.GetSource(c => c.Core.Symlinks).Should().Be(scenario.SystemFileSource);
            }
        }

        [FFact(FuncClearSetting, "When no target is specified, the current originator source is cleared of the setting (clear removes it from user system source and so completely form the config -> CLR default is used).")]
        public void FooXX()
        {
            using (var scenario = GitScenario.Setup(nameof(Foo))
                .UseSystemDefaultContentForeachSource())
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();
                config.Clear(c => c.Core.Symlinks); // removes it from repo source
                config.Clear(c => c.Core.Symlinks); // removes it from user source

                // Act
                var r = config.Clear(c => c.Core.Symlinks); // removes it completely

                r.Should().Be(true);
                config.GetSource(c => c.Core.Symlinks).Should().Be(null);
            }
        }

        [FFact(FuncClearSetting, "When no target is specified, the current originator source is cleared of the setting (clear does nothing as there is no source with the setting left).")]
        public void FooXXX()
        {
            using (var scenario = GitScenario.Setup(nameof(Foo))
                .UseSystemDefaultContentForeachSource())
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();
                config.Clear(c => c.Core.Symlinks); // removes it from repo source
                config.Clear(c => c.Core.Symlinks); // removes it from user source
                config.Clear(c => c.Core.Symlinks); // removes it completely

                // Act
                var r = config.Clear(c => c.Core.Symlinks); // nothing happens

                r.Should().Be(false);
                config.GetSource(c => c.Core.Symlinks).Should().Be(null);
            }
        }
    }
}
