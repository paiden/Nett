using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Functional
{
    [ExcludeFromCodeCoverage]
    public sealed class ClearSettingTests
    {
        private const string FuncClearSetting = "Clear Setting";

        [FFact(FuncClearSetting, "When no target is specified and current source is repo, new source will be user file")]
        public void ClearSetting_WhenNoTargetSpecifiedAndRepoIsSource_NewSourceWillBeUserFile()
        {
            using (var scenario = GitScenario.Setup(nameof(ClearSetting_WhenNoTargetSpecifiedAndRepoIsSource_NewSourceWillBeUserFile))
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

        [FFact(FuncClearSetting, "When no target is specified and current source is user, new source will be system file")]
        public void ClearSetting_WhenNoTargetSpecifiedAndUserIsSource_NewSourceWillBeSystemFile()
        {
            using (var scenario = GitScenario.Setup(nameof(ClearSetting_WhenNoTargetSpecifiedAndUserIsSource_NewSourceWillBeSystemFile))
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

        [FFact(FuncClearSetting, "When no target is specified and current source is system, new source will be null as setting is not in any source anymore")]
        public void ClearSetting_WhenNoTargetSpecifiedAndRepoIsSystem_NewSourceWillBeNull()
        {
            using (var scenario = GitScenario.Setup(nameof(ClearSetting_WhenNoTargetSpecifiedAndRepoIsSystem_NewSourceWillBeNull))
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

        [FFact(FuncClearSetting, "When no target is specified and setting is not in any file, nothing will happen")]
        public void ClearSetting_WhenNoTargetSpecifiedSourceIsNull_NothingWillHappen()
        {
            using (var scenario = GitScenario.Setup(nameof(ClearSetting_WhenNoTargetSpecifiedSourceIsNull_NothingWillHappen))
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
