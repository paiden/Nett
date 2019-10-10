using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class TomlTableTests
    {
        [Fact]
        public void Overwrite_WhenSourceTableHasNoComments_SourceCommentStayIntact()
        {
            var from = this.CreateEmpty();
            var to = this.CreateSingleProp("x");

            to.OverwriteCommentsWithCommentsFrom(from, overwriteWithEmpty: false);

            to.Rows.Count().Should().Be(1);
            to.Get("r1").As<TomlInt>().Value.Should().Be(1);
            to.Get("r1").Comments.Single().Text.Should().Be("x");
        }

        [Fact]
        public void OverwriteComments_WhenSourceTableHasCommentsButTargetNot_SourceTableCommentsAreUsed()
        {
            var from = CreateSingleProp("expected");
            var to = this.CreateSingleProp("willbeoverwritten");

            to.OverwriteCommentsWithCommentsFrom(from, overwriteWithEmpty: false);

            to.Rows.Count().Should().Be(1);
            to.Get("r1").As<TomlInt>().Value.Should().Be(1);
            to.Get("r1").Comments.Single().Text.Should().Be("expected");
        }

        [Fact]
        public void OverwriteComment_WithComplexTables_ProcudesCorrectResult()
        {
            var from = this.CreateComplexFrom();
            var to = this.CreateComplexTo();

            to.OverwriteCommentsWithCommentsFrom(from, overwriteWithEmpty: false);

            to.Get("I").Comments.Single().Text.Should().Be("to", "Because the source comment was empty");
            to.Get("F").Comments.Single().Text.Should().Be("from", "Because only source comment existed");
            to.Get("SubTable").Comments.Single().Text.Should().Be("from", "Because when both comments exist, source comment should overwrite target comment");
            to.Get<TomlTable>("SubTable").Get("A").Comments.Count().Should().Be(2);
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(0).Text.Should().Be("fa1");
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(0).Location.Should().Be(CommentLocation.Prepend);
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(1).Text.Should().Be("fa2");
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(1).Location.Should().Be(CommentLocation.Append);
        }

        [Fact]
        public void OverwriteComment_WithComplexTablesAndOverwriteAlwaysEnabled_ProcudesCorrectResult()
        {
            var from = this.CreateComplexFrom();
            var to = this.CreateComplexTo();

            to.OverwriteCommentsWithCommentsFrom(from, overwriteWithEmpty: true);

            to.Get("I").Comments.Count().Should().Be(0, "Because comment is always overwritten, event when empty");
            to.Get("F").Comments.Single().Text.Should().Be("from", "Because only source comment existed");
            to.Get("SubTable").Comments.Single().Text.Should().Be("from", "Because when both comments exist, source comment should overwrite target comment");
            to.Get<TomlTable>("SubTable").Get("A").Comments.Count().Should().Be(2);
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(0).Text.Should().Be("fa1");
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(0).Location.Should().Be(CommentLocation.Prepend);
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(1).Text.Should().Be("fa2");
            to.Get<TomlTable>("SubTable").Get("A").Comments.ElementAt(1).Location.Should().Be(CommentLocation.Append);
        }

        [Fact]
        public void CloneAlsoCreatesARootTable()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            var clone = tbl.CloneFor(tbl.Root);

            // Assert
            clone.Should().BeAssignableTo<ITomlRoot>();
        }

        private TomlTable CreateEmpty()
        {
            return Toml.Create();
        }

        private TomlTable CreateSingleProp()
        {
            var tt = Toml.Create();
            tt.Add("r1", (long)1);
            return tt;
        }

        private TomlTable CreateSingleProp(string comment)
        {
            var tt = Toml.Create();
            tt.Add("r1", (long)1).ConfigureAdded(ti => ti.AddComment(new TomlComment(comment, CommentLocation.Prepend)));
            return tt;
        }

        private TomlTable CreateComplex()
        {
            var t = Toml.Create();
            var tt = t.Add("SubTable", t.CreateEmptyAttachedTable());
            t.Add("I", (long)1);
            t.Add("F", 1.0);

            tt.Added.Add("A", (System.Collections.Generic.IEnumerable<int>)(new int[] { 1, 2, 3 }));

            return t;
        }

        private TomlTable CreateComplexFrom()
        {
            var t = this.CreateComplex();
            //t.Get("I").Comments.Add(new TomlComment("from", CommentLocation.Prepend));
            t.Get("F").AddComment(new TomlComment("from", CommentLocation.Prepend));
            t.Get("SubTable").AddComment(new TomlComment("from", CommentLocation.Prepend));

            t.Get<TomlTable>("SubTable").Get("A").AddComment(new TomlComment("fa1", CommentLocation.Prepend));
            t.Get<TomlTable>("SubTable").Get("A").AddComment(new TomlComment("fa2", CommentLocation.Append));

            return t;
        }

        private TomlTable CreateComplexTo()
        {
            var t = this.CreateComplex();
            t.Get("I").AddComment(new TomlComment("to", CommentLocation.Prepend));
            //t.Get("F").Comments.Add(new TomlComment("to", CommentLocation.Prepend));
            t.Get("SubTable").AddComment(new TomlComment("to", CommentLocation.Prepend));
            return t;
        }
    }
}
