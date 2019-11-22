using System;
using System.Collections.Generic;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class DottedKeyTests
    {
        private readonly ST StringTransformForComparison = ST.Do(ST.Norm, ST.NoSpc, ST.Trim);

        [Fact]
        public void Read_WithSingleDottedKey_ReadsTableCorrectly()
        {
            // Arrange
            const string Input = @"
fruit.id = 1";

            // Act
            var t = Toml.ReadString(Input);

            // Assert
            t.Get<TomlTable>("fruit").Get<int>("id").Should().Be(1);
            t.Get<TomlTable>("fruit").TableType.Should().Be(TomlTable.TableTypes.Dotted);
        }

        [Fact]
        public void Read_WithAlreadyDefinedRow_ThrowsInvalidOp()
        {
            // Arrange
            const string Input = @"
a.b = 1
a.b.c = 2
";
            // Act
            Action a = () => Toml.ReadString(Input);

            // Assert
            a.ShouldThrow<InvalidOperationException>();
        }


        [Fact]
        public void Read_WhereDottekKeyHasSameKeyAsParentTable_DottedIsAddedAsChildToParent()
        {
            // Arrange
            const string Input = @"
[a]
a.b = 1
a.c = 2
";

            // Act
            var read = Toml.ReadString(Input);

            // Assert
            read.Get<TomlTable>("a").Get<TomlTable>("a").Get<int>("b").Should().Be(1);
            read.Get<TomlTable>("a").Get<TomlTable>("a").Get<int>("c").Should().Be(2);
        }


        [Fact]
        public void Read_WithSutTableAlreadyDefinedByDottedKey_ThrowsInvalidOp()
        {
            // Arrange
            const string Input = @"
a.b = 1
a.b.c = 2
[b]
";

            // Act
            Action a = () => Toml.ReadString(Input);

            // Assert
            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Write_WhenTableWasReadAsDotted_WritesTableAsDottedTableAgain()
        {
            // Arrange
            var root = Toml.Create(TomlTable.TableTypes.Dotted);
            var adat = new Dictionary<string, object>()
            {
                {"b", 1 },
                {"c", 2 },
            };
            root.Add("a", adat, TomlTable.TableTypes.Dotted);


            // Act
            var written = Toml.WriteString(root);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("a.b=1 a.c=2");
        }

        [Fact]
        public void Write_WhenDottedTablesAreNested_WritesTablesAsDottedTables()
        {
            // Arrange
            var root = Toml.Create(TomlTable.TableTypes.Dotted);
            var adat = new Dictionary<string, object>()
            {
                {"b", 1 },
                {"c", 2 },
            };
            var a = root.Add("a", adat, TomlTable.TableTypes.Dotted);

            var ndat = new Dictionary<string, object>()
            {
                { "x", 3 },
            };

            a.ConfigureAdded(n => n.Add("n", ndat, TomlTable.TableTypes.Dotted));

            // Act
            var written = Toml.WriteString(root);

            // Assert
            written.Should().BeAfterTransforms(StringTransformForComparison, @"
a.b=1
a.c=2
a.n.x=3");
        }

        [Fact]
        public void Write_WhenNestedTableIsAStandardTable_NestedTableIsStillWrittenAsDottedTable()
        {
            // Arrange
            var root = Toml.Create(TomlTable.TableTypes.Dotted);
            var adat = new Dictionary<string, object>()
            {
                {"b", 1 },
                {"c", 2 },
            };
            var a = root.Add("a", adat, TomlTable.TableTypes.Dotted).Added;

            var ndat = new Dictionary<string, object>()
            {
                { "x", 3 },
            };

            a.Add("n", ndat, TomlTable.TableTypes.Default);

            // Act
            var written = Toml.WriteString(root);

            // Assert
            written.Should().BeAfterTransforms(StringTransformForComparison, @"
a.b=1
a.c=2
a.n.x=3");
        }

        [Fact]
        public void Write_WhenNestedTableIsInlineTable_NestedTableIsWrittenAsInlineTable()
        {
            // Arrange
            var root = Toml.Create(TomlTable.TableTypes.Dotted);
            var adat = new Dictionary<string, object>()
            {
                {"b", 1 },
                {"c", 2 },
            };
            var a = root.Add("a", adat, TomlTable.TableTypes.Dotted).Added;

            var ndat = new Dictionary<string, object>()
            {
                { "x", 3 },
            };

            a.Add("n", ndat, TomlTable.TableTypes.Inline);

            // Act
            var written = Toml.WriteString(root);

            // Assert

            written.Should().BeAfterTransforms(StringTransformForComparison, @"
a.b=1
a.c=2
a.n={x=3}");
        }

        [Fact]
        public void GivenDottedKeyValueWithPreComment_WhenWrittenToFile_WritesCommentBeforeLine()
        {
            // Arrange
            var tbl = Toml.Create();
            var obj = tbl.CreateEmptyAttachedTable(TomlTable.TableTypes.Dotted);
            tbl.Add(nameof(obj), obj);

            obj.Add("x", 1).Added.AddComment("The Comment", CommentLocation.Prepend);

            // Act
            var tml = Toml.WriteString(tbl);

            // Assert
            tml.Should().BeAfterTransforms(StringTransformForComparison, @"
[obj]
#TheComment
x=1");
        }

        [Fact]
        public void GivenDottedKeyValueWithAppendComment_WhenWrittenToFile_WritesCommentBeforeLine()
        {
            // Arrange
            var tbl = Toml.Create();
            var obj = tbl.CreateEmptyAttachedTable(TomlTable.TableTypes.Dotted);
            tbl.Add(nameof(obj), obj);

            obj.Add("x", 1).Added.AddComment("The Comment", CommentLocation.Append);

            // Act
            var tml = Toml.WriteString(tbl);

            // Assert
            tml.Should().BeAfterTransforms(StringTransformForComparison, @"
[obj]
x=1#TheComment");
        }
    }
}
