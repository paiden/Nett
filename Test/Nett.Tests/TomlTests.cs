using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Internal
{
    //TODO: Move to API test once API is adapted
    [ExcludeFromCodeCoverage]
    public class TomlTests
    {
        [Fact]
        public void ReadFileT_WhenFileNameArgIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile((string)null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ReadFileT_WhenConfigIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile<SimpleToml>("SimpleToml.tml", null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ReadFileT_WhenArgIsFileStreamAndNoConfigGiven_ReadsFileCorrectly()
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
        public void ReadFile_WhenFileStreamArgIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile((FileStream)null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ReadFileT_WithStreamWhenConfigIsNull_ThrowsArgNull()
        {
            using (var s = GetStream())
            {
                Action a = () => Toml.ReadFile<SimpleToml>(s, null);
                a.ShouldThrow<ArgumentNullException>();
            }
        }

        [Fact]
        public void ReadStringT_WhenStringIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadString(null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ReadStringT_WhenConfigIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadString<SimpleToml>("", null);
            a.ShouldThrow<ArgumentNullException>();
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
        public void ReadStreamOfT_WhenNoConfigGiven_ReadsFileCorrectly()
        {
            using (var s = GetStream())
            {
                var tml = Toml.ReadStream<SimpleToml>(GetStream());
                tml.X.Should().Be(1);
            }
        }

        [Fact]
        public void ReadStreamOfT_WhenConfigIsNull_ThrowsArgNull()
        {
            using (var s = GetStream())
            {
                Action a = () => Toml.ReadStream<SimpleToml>(GetStream(), null);
                a.ShouldThrow<ArgumentNullException>();
            }
        }

        [Fact]
        public void WriteFile_WhenTableIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteFile((TomlTable)null, RndFilePath);

            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteFile_WritesTaleCorrectly()
        {
            var fn = RndFilePath;
            const int expected = 1;
            Toml.WriteFile(CreateSimpleTomlAsTable(expected), fn);

            var read = Toml.ReadFile<SimpleToml>(fn);

            read.X.Should().Be(expected);
        }

        [Fact]
        public void WriteFile_WhenTomlConfigIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteFile(CreateSimpleTomlAsTable(1), RndFilePath, null);

            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteFileT_WhenObjIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteFile((SimpleToml)null, RndFilePath);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteFileT_WhenConfigIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteFile(new SimpleToml(), RndFilePath, null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteFileT_WhenFilePathIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteFile(new SimpleToml(), null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteStringT_WhenObjIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteString((SimpleToml)null);
            a.ShouldThrow<ArgumentNullException>();
        }
        [Fact]
        public void WriteStringT_WhenConfigIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteString(new SimpleToml(), null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteStream_WithoutConfig_WritesToStream()
        {
            using (var s = new MemoryStream())
            {
                Toml.WriteStream(CreateFoo(), s);
                var read = Toml.ReadStream<Foo>(s);

                read.X.Should().Be(1);
            }
        }

        [Fact]
        public void WriteStream_WhenStreamIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteStream(new SimpleToml(), (Stream)null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteStream_WhenObjIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteStream((SimpleToml)null, new MemoryStream());
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteStream_WhenConfigIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.WriteStream(new SimpleToml(), new MemoryStream(), null);
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void WriteStream_SetsStreamPositionTo0AfterWritingSoThatItIsReadoToBeReadAgain()
        {
            using (var s = new MemoryStream())
            {
                Toml.WriteStream(CreateFoo(), s, TomlSettings.Create());

                s.Position.Should().Be(0);
            }
        }

        [Fact]
        public void WriteStream_WithConfig_WritesToStream()
        {
            using (var s = new MemoryStream())
            {
                Toml.WriteStream(CreateFoo(), s, TomlSettings.Create());
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

        private static TomlTable CreateSimpleTomlAsTable(int x)
        {
            var tt = Toml.Create();
            tt.Add("X", (long)1);

            return tt;
        }

        private class SimpleToml
        {
            public int X { get; set; }
        }

        private class Foo
        {
            public int X { get; set; }
        }

        private static string RndFilePath = Guid.NewGuid() + ".tml";
    }
}
