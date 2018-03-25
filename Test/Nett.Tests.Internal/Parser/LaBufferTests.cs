using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FluentAssertions;
using Nett.Parser;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Parser
{
    [ExcludeFromCodeCoverage] 
    public class LaBufferTests
    {
        [Fact]
        public void CanReadCompleContent()
        {
            // Arrange
            var content = "This is";
            var sb = new StringBuilder();
            var sr = new StreamReader(content.ToStream());
            var lab = new CharBuffer(() =>
            {
                int read = sr.Read();
                return read != -1 ? new char?((char)read) : new char?();
            }, 3);

            // Act

            while (lab.HasNext())
            {
                sb.Append(lab.PeekAt(0));
                lab.Consume();
            }

            sb.Append(lab.PeekAt(0));

            // Assert
            sb.ToString().Should().Be(content);
        }

        [Fact]
        public void CanExtendBuffer()
        {
            // Arrange
            var content = "This is hello world!";

            var sb = new StringBuilder();
            var sr = new StringReader(content);
            var lab = new CharBuffer(() => {
                int read = sr.Read();
                return read != -1 ? new char?((char)read) : new char?();
            }, 3);

            // Act
            int i = 0;
            while (i < content.Length) {
                sb.Append(lab.PeekAt(i++));
            }

            // Assert
            sb.ToString().Should().Be(content);
            new Action(() => lab.PeekAt(i++)).ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
