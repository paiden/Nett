using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    public class ReadTomlWithCommentsTests
    {
        [Fact]
        public void WhenTomlOnlyContainsComments_RootTableGetsAppendCommentBlock()
        {
            const string TML = @"# fc
#sc

#nlc";

            var tt = Toml.Read(TML);

            tt.Comments.Count.Should().Be(3);
            tt.Comments[1].Should().Be("# fc\r\n#sc\r\n\r\n#nlc");
        }

        [Fact]
        public void WhenTomlContainsAppendComment_CommentIsAddedToAppendedElemenet()
        {
            // Arrange
            const string TML = @"
x = 0 # AC";
            // Act
            var tt = Toml.Read(TML);

            // Assert
            tt["x"].Comments.Count.Should().Be(1);
            tt["x"].Comments[0].CommentText.Should().Be(" AC");
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
            var tt = Toml.Read(TML);

            // Assert
            tt["x"].Comments.Count.Should().Be(1);
            tt["x"].Comments[0].CommentText.Should().Be("PC");
            tt["x"].Comments[0].Location.Should().Be(CommentLocation.Prepend);
        }
    }
}
