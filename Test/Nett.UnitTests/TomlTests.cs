using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class TomlTests
    {

        [Fact]
        public void ReadFileOfT_WhenArgIsFileStreamAndNoConfigGiven_ReadsFileCorrectly()
        {
            using (var s = GetStream())
            {
                var tml = Toml.ReadFile<SimpleToml>(GetStream());
                tml.X.Should().Be(1);
            }
        }

        [Fact]
        public void ReadFile_WhenArgIsFileStreamAndNoConfigGiven_ReadsFileCorrectly()
        {
            using (var s = GetStream())
            {
                var tml = Toml.ReadFile(GetStream());
                tml.Get<int>("X").Should().Be(1);
            }
        }

        [Fact]
        public void ReadStreamOfT_WhenArgIsFileStreamAndNoConfigGiven_ReadsFileCorrectly()
        {
            using (var s = GetStream())
            {
                var tml = Toml.ReadStream<SimpleToml>(GetStream());
                tml.X.Should().Be(1);
            }
        }

        [Fact]
        public void ReadStream_WhenArgIsFileStreamAndNoConfigGiven_ReadsFileCorrectly()
        {
            using (var s = GetStream())
            {
                var tml = Toml.ReadStream(GetStream());
                tml.Get<int>("X").Should().Be(1);
            }
        }

        [Fact]
        public void WriteStream_WithoutConfig_WritesToStream()
        {
            using (var s = new MemoryStream())
            {
                Toml.WriteStream(s, CreateFoo());
                var read = Toml.ReadStream<Foo>(s);

                read.X.Should().Be(1);
            }
        }

        [Fact]
        public void WriteStream_SetsStreamPositionTo0AfterWritingSoThatItIsReadoToBeReadAgain()
        {
            using (var s = new MemoryStream())
            {
                Toml.WriteStream(s, CreateFoo(), TomlConfig.DefaultInstance);

                s.Position.Should().Be(0);
            }
        }

        [Fact]
        public void WriteStream_WithConfig_WritesToStream()
        {
            using (var s = new MemoryStream())
            {
                Toml.WriteStream(s, CreateFoo(), TomlConfig.DefaultInstance);
                var read = Toml.ReadStream<Foo>(s);

                read.X.Should().Be(1);
            }
        }

        private static FileStream GetStream()
        {
            return new FileStream("SimpleToml.tml", FileMode.Open, FileAccess.Read);
        }

        private static Foo CreateFoo()
        {
            return new Foo() { X = 1 };
        }

        private class SimpleToml
        {
            public int X { get; set; }
        }

        private class Foo
        {
            public int X { get; set; }
        }
    }
}
