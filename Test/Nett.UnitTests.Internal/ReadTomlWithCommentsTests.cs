using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Internal
{
    public class ReadTomlWithCommentsTests
    {
        [Fact]
        public void WhenTomlOnlyContainsComments_RootTableGetsCommentBlock()
        {
            const string TML = @"# fc
#sc

#nlc ";

            var tt = Toml.ReadString(TML);

            tt.Comments.Count.Should().Be(3);
            tt.Comments[0].Text.Should().Be(" fc");
            tt.Comments[1].Text.Should().Be("sc");
            tt.Comments[2].Text.Should().Be("nlc ");
        }

        [Fact]
        public void WhenTomlContainsAppendComment_CommentIsAddedToAppendedElemenet()
        {
            // Arrange
            const string TML = @"
x = 0 # AC";
            // Act
            var tt = Toml.ReadString(TML);

            // Assert
            tt["x"].Comments.Count.Should().Be(1);
            tt["x"].Comments[0].Text.Should().Be(" AC");
            tt["x"].Comments[0].Location.Should().Be(CommentLocation.Append);
        }

        [Fact]
        public void WhenTomlContainsPrependCommen_CommentIsAddedToPrependElement()
        {
            // Arrange
            const string TML = @"
#PC
x = 0";

            // Act
            var tt = Toml.ReadString(TML);

            // Assert
            tt["x"].Comments.Count.Should().Be(1);
            tt["x"].Comments[0].Text.Should().Be("PC");
            tt["x"].Comments[0].Location.Should().Be(CommentLocation.Prepend);
        }
    }
}
