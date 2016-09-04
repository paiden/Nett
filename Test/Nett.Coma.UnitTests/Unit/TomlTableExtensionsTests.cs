using System;
using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Unit
{
    public sealed class TomlTableExtensionsTests
    {
        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.OverwriteWithValuesForSaveFrom),
            "When source table doesn't contain any rows, all target table rows will get deleted.")]
        public void OverwriteForSaveFrom_WhenTargetTableDoesntContainRow_AllTargetTableRowsGetDeleted()
        {
            // Arrange
            var tgtTable = Toml.Create();
            tgtTable.Add("x", 0);
            var fromTable = Toml.Create();

            // Act
            tgtTable.OverwriteWithValuesForSaveFrom(fromTable);

            // Assert
            tgtTable.Rows.Count.Should().Be(0);
        }

        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.OverwriteWithValuesForSaveFrom),
                    "When target table doesn't contain any row, rows of source table will not be added to target.")]
        public void OverwriteForSaveFrom_WhenTargetTableDoesntContainRow_RowWillNotBeAddedToTargetTable()
        {
            // Arrange
            var targetTable = Toml.Create();
            var fromTable = Toml.Create();
            fromTable.Add("x", 0);

            // Act
            targetTable.OverwriteWithValuesForSaveFrom(fromTable);

            // Assert
            targetTable.Rows.Count.Should().Be(0);
        }

        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.TransformToSourceTable), "When table is null, throws ArgumentNull exception")]
        public void TransformToSourceTable_WhenTableIsNull_ThrowsArgNull()
        {
            // Arrange
            TomlTable table = null;

            // Act
            Action a = () => table.TransformToSourceTable(ConfigSource.CreateFileSource("x"));

            // Assert
            a.ShouldThrow<ArgumentNullException>().WithMessage("*table*");
        }

        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.TransformToSourceTable), "Produces correct source table")]
        public void TransformToSourceTable_ProducesCorrectTable()
        {
            using (var scenario = MultiLevelTableScenario.Setup())
            {
                // Act
                var src = ConfigSource.CreateFileSource("test");
                var sourceTable = scenario.Table.TransformToSourceTable(src);

                // Assert
                sourceTable.Get<IConfigSource>(nameof(scenario.Clr.X)).Should().BeSameAs(src);
                var sub = sourceTable.Get<TomlTable>(nameof(scenario.Clr.Sub));
                sub.Get<IConfigSource>(nameof(scenario.Clr.Sub.Y)).Should().BeSameAs(src);
            }
        }

    }
}
