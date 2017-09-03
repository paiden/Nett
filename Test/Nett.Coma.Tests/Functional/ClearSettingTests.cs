using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Nett.Tests.Util;

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
                config.GetSource(c => c.Core.Symlinks).Should().Be(scenario.UserSource);
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
                config.GetSource(c => c.Core.Symlinks).Should().Be(scenario.SystemSource);
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

                // Assert
                r.Should().Be(false);
                config.GetSource(c => c.Core.Symlinks).Should().Be(null);
            }
        }

        [FFact(FuncClearSetting, "When source is specified and setting exists in source, setting is cleared from that source")]
        public void ClearSetting_WhenSourceSpecifiedAndSettingExists_SettingIsClearedFromThatSource()
        {
            using (var scenario = GitScenario.Setup(nameof(ClearSetting_WhenSourceSpecifiedAndSettingExists_SettingIsClearedFromThatSource))
                .UseSystemDefaultContentForeachSource())
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();
                var tbl = (TomlTable)Toml.ReadFile(scenario.UserFile)["Core"];
                tbl.ContainsKey(nameof(GitScenario.GitConfig.CoreConfig.Symlinks)).Should().Be(true);

                // Act
                var r = config.Clear(c => c.Core.Symlinks, scenario.UserSource);

                // Assert
                r.Should().Be(true);
                tbl = (TomlTable)Toml.ReadFile(scenario.UserFile)["Core"];
                tbl.ContainsKey(nameof(GitScenario.GitConfig.CoreConfig.Symlinks)).Should().Be(false);
            }
        }

        [FFact(FuncClearSetting, "When source is specified and setting exists in source, setting is cleared from that source")]
        public void ClearSetting_WhenSourceSpecifiedButSettingDoesnExist_NothingHappens()
        {
            using (var scenario = GitScenario.Setup(nameof(ClearSetting_WhenSourceSpecifiedButSettingDoesnExist_NothingHappens))
                .UseSystemDefaultContentForeachSource())
            {
                // Arrange
                var config = scenario.CreateMergedFromDefaults();
                config.Clear(c => c.Core.Symlinks, scenario.UserSource);

                // Act
                var r = config.Clear(c => c.Core.Symlinks, scenario.UserSource);

                // Assert
                r.Should().Be(false);
            }
        }
    }
}
