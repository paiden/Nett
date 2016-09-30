using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    [ExcludeFromCodeCoverage]
    public sealed class VerifyIssuesTests
    {
        [Fact(DisplayName = "Verify that issue #8 was fixed")]
        public void ReadAndWriteFloat_Issue8_IsFixed()
        {
            // Arrange
            MyObject obj = new MyObject();
            obj.MyFloat = 123;
            string output = Toml.WriteString<MyObject>(obj);

            // Act
            MyObject parsed = Toml.ReadString<MyObject>(output);

            // Assert
            parsed.MyFloat.Should().Be(123.0f);
        }

        public class MyObject
        {
            public float MyFloat { get; set; }
        }

    }
}
