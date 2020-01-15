using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Xunit;

namespace Nett.Coma.Tests.Issues
{
    /// <summary>
    /// Tests clearing of values from merged configuration objects
    /// </summary>
    public class Issue87Tests
    {
        private static readonly ST StringTransformForComparison = ST.Do(ST.Norm, ST.NoSpc, ST.Trim);

        [Fact]
        public void WhenRootTableClearsD_DDoesNotExistInFileOrInMemoryTable()
        {
            // Arrange
            using var scen = MergeThreeFilesScenario.Setup(nameof(WhenRootTableClearsD_DDoesNotExistInFileOrInMemoryTable));
            var cfg = scen.Load();

            // Act
            bool r = cfg.Clear(c => c.D);

            // Assert
            r.Should().BeTrue();
            var expected = MergeThreeFilesScenario.GetToml(('C', "local"));
            File.ReadAllText(scen.LocalFile).Should().BeAfterTransforms(StringTransformForComparison, expected);
            Action a = () => cfg.Get(c => c.D);
            a.ShouldThrow<KeyNotFoundException>();
        }

        [Fact]
        public void GivenConfigWithoutD_ClearingDReturnsFalse()
        {
            // Arrange
            using var scen = MergeThreeFilesScenario.Setup(nameof(WhenRootTableClearsD_DDoesNotExistInFileOrInMemoryTable));
            var cfg = scen.Load();
            cfg.Clear(c => c.D);

            // Act
            bool r = cfg.Clear(c => c.D);

            // Assert
            r.Should().BeFalse();
        }

        [Fact]
        public void GivenConfigWithAInMachineAndUser_WhenClearingOnce_MachineAIsFetchedAfterwards()
        {
            // Arrange
            using var scen = MergeThreeFilesScenario.Setup(nameof(WhenRootTableClearsD_DDoesNotExistInFileOrInMemoryTable));
            var cfg = scen.Load();
            cfg.Get(c => c.A).ItemVal.Should().Be("user");

            // Act
            var r = cfg.Clear(c => c.A);

            // Assert
            r.Should().BeTrue();
            cfg.Get(c => c.A).ItemVal.Should().Be("machine");
        }

        [Fact]
        public void GivenConfigWithAInMachineAndUser_WhenClearingOnceFromAllSources_AIsCompletelyGone()
        {
            // Arrange
            using var scen = MergeThreeFilesScenario.Setup(nameof(WhenRootTableClearsD_DDoesNotExistInFileOrInMemoryTable));
            var cfg = scen.Load();
            cfg.Get(c => c.A).ItemVal.Should().Be("user");

            // Act
            var r = cfg.Clear(c => c.A, fromAllSources: true);

            // Assert
            Action a = () => cfg.Get(c => c.A);
            a.ShouldThrow<KeyNotFoundException>();
        }
    }
}
