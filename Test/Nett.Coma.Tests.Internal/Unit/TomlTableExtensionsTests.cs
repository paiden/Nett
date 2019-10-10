using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Nett.Coma.Test.Util;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class TomlTableExtensionsTests
    {
        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.OverwriteWithValuesForSaveFrom),
            "When source table doesn't contain any rows, all target table rows will get deleted.")]
        public void OverwriteForSaveFrom_WhenTargetTableDoesntContainRow_AllTargetTableRowsGetDeleted()
        {
            // Arrange
            var tgtTable = Toml.Create();
            tgtTable.Add("x", (long)0);
            var fromTable = Toml.Create();

            // Act
            tgtTable.OverwriteWithValuesForSaveFrom(fromTable, addNewRows: false);

            // Assert
            tgtTable.Count.Should().Be(0);
        }

        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.OverwriteWithValuesForSaveFrom),
                    "When target table doesn't contain any row, rows of source table will not be added to target.")]
        public void OverwriteForSaveFrom_WhenTargetTableDoesntContainRow_RowWillNotBeAddedToTargetTable()
        {
            // Arrange
            var targetTable = Toml.Create();
            var fromTable = Toml.Create();
            fromTable.Add("x", (long)0);

            // Act
            targetTable.OverwriteWithValuesForSaveFrom(fromTable, addNewRows: false);

            // Assert
            targetTable.Count.Should().Be(0);
        }

        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.TransformToSourceTable), "When table is null, throws ArgumentNull exception")]
        public void TransformToSourceTable_WhenTableIsNull_ThrowsArgNull()
        {
            // Arrange
            TomlTable table = null;

            // Act
            Action a = () => table.TransformToSourceTable(new ConfigSource("x"));

            // Assert
            a.ShouldThrow<ArgumentNullException>().WithMessage("*table*");
        }

        [MFact(nameof(TomlTableExtensions), nameof(TomlTableExtensions.TransformToSourceTable), "Produces correct source table")]
        public void TransformToSourceTable_ProducesCorrectTable()
        {
            using (var scenario = MultiLevelTableScenario.Setup())
            {
                // Act
                var src = new ConfigSource("test");
                var sourceTable = scenario.Table.TransformToSourceTable(new ConfigSource("test"));

                // Assert
                sourceTable.Get<IConfigSource>(nameof(scenario.Clr.X)).Should().Be(src);
                var sub = sourceTable.Get<TomlTable>(nameof(scenario.Clr.Sub));
                sub.Get<IConfigSource>(nameof(scenario.Clr.Sub.Y)).Should().Be(src);
            }
        }

        [Fact]
        public void Clone_WhenSourceTableHasTypeInline_CloneTableHasSameTableType()
        {
            // Arrange
            var tbl = Toml.Create();
            tbl.TableType = TomlTable.TableTypes.Inline;

            // Act
            var cloned = tbl.Clone();

            // Assert
            cloned.TableType.Should().Be(tbl.TableType);
        }

        [Fact]
        public void CloneAlsoClonesSubtableComments()
        {
            const string Tml = @"#Some comment
b = true

#Comment on nested table
[a]
#Comment on C
c = """"
";

            var tbl = Toml.ReadString(Tml);

            var cloned = tbl.Clone();

            Toml.WriteString(cloned).ShouldBeNormalizedEqualTo(Tml);
        }
    }
}
