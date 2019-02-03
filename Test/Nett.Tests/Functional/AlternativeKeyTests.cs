using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class AlternativeKeyTests
    {
        [Fact]
        public void UseCustomKey_WithLamdaForPropSelector_SerializationUsesThatKey()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Map(fo => fo.X).ToKey("TheKey")));

            // Act
            var tml = Toml.WriteString(new FObj(), cfg);

            // Assert
            tml.ShouldBeSemanticallyEquivalentTo(@"
TheKey=1");
        }

        [Fact]
        public void UseCustomKey_WithLamdaForPropSelector_DeserializationMapsBackToProperty()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Map(fo => fo.X).ToKey("TheKey")));

            // Act
            var obj = Toml.ReadString<FObj>("TheKey = 3", cfg);

            // Assert
            obj.X.Should().Be(3);
        }

        [Fact]
        public void UseCustomKey_WithStringPrivateFieldSelector_SerializationUsesThatKey()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Map("ThatsMine").ToKey("TheKey")));

            // Act
            var tml = Toml.WriteString(new FObj(), cfg);

            // Assert
            tml.ShouldBeSemanticallyEquivalentTo(@"
X=1
TheKey=""Youfoundme""");
        }

        [Fact]
        public void UseCustomKey_WithStringPrivateFieldSelector_DeserializationUsesThatKey()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Map("ThatsMine").ToKey("TheKey")));

            // Act
            var obj = Toml.ReadString<FObj>(@"
X=2
TheKey=""tmlcontent""", cfg);

            // Assert
            obj.X.Should().Be(2);
            obj.ThatsMineAccessor.Should().Be("tmlcontent");
        }

        [Fact]
        public void UseCustomKey_WithStringPropSelector_SerializationUsesThatKey()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Map(nameof(FObj.X)).ToKey("TheKey")));

            // Act
            var tml = Toml.WriteString(new FObj(), cfg);

            // Assert
            tml.ShouldBeSemanticallyEquivalentTo("TheKey=1");
        }


        [Fact]
        public void Include_WithMemberSelector_CanBeUsedToIncludeOtherwiseIgnoredMembers()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Include(fo => fo.PubField)));

            // Act
            var tml = Toml.WriteString(new FObj(), cfg);

            // Assert
            tml.ShouldBeSemanticallyEquivalentTo(@"
X = 1
PubField = ""Serialize all the things""
");
        }

        [Fact]
        public void Include_WithMemberSelector_CanBeUsedToDeserializeOtherwiseIgnoredMembers()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Include(fo => fo.PubField)));

            // Act
            var obj = Toml.ReadString<FObj>(@"
X = 2
PubField = ""thats different""
", cfg);

            // Assert
            obj.X.Should().Be(2);
            obj.PubField.Should().Be("thats different");
        }

        [Fact]
        public void Include_WithStringSelector_CanBeUsedToIncludePrivateMembers()
        {
            // Arrange
            var cfg = TomlSettings.Create(c => c
                .ConfigureType<FObj>(tc => tc
                    .Include("ThatsMine")));

            // Act
            var tml = Toml.WriteString(new FObj(), cfg);

            // Assert
            tml.ShouldBeSemanticallyEquivalentTo(@"
X = 1
ThatsMine = ""You found me""");
        }

        [Fact]
        public void WriteAttributedClass_WritesAllThePropertiesWithCorrectKeys()
        {
            // Arrange
            var instance = new AObj();

            // Act
            var tml = Toml.WriteString(instance);

            // Assert
            tml.ShouldNormalized().Be(@"
'The Key' = 1
PubField = ""Serialize all the things""
ThatsMine = ""You found me""");
        }

        [Fact]
        public void ReadAttributedClass_ReadsAllThePropertiesCorrectly()
        {
            // Arrange
            // Act
            var obj = Toml.ReadString<AObj>(@"
'The Key' = 2
PubField =""pubfield was read correctly""
ThatsMine=""thatsmine read correctly""");

            // Assert
            obj.X.Should().Be(2);
            obj.PubField.Should().Be("pubfield was read correctly");
            obj.ThatsMineAccessor.Should().Be("thatsmine read correctly");
        }

        [Fact]
        public void WriteNestedAttributedClass_WritesAllThePropertiesWithCorrectKeys()
        {
            // Arrange
            var instance = new AObjWrappper();

            // Act
            var tml = Toml.WriteString(instance);

            // Assert
            tml.ShouldNormalized().Be(@"
Y = 22

[AObj]
'The Key' = 1
PubField = ""Serialize all the things""
ThatsMine = ""You found me""");
        }

        [Fact]
        public void ReadNestedAttributedClass_ReadsAllThePropertiesCorrectly()
        {
            // Arrange
            // Act
            var obj = Toml.ReadString<AObjWrappper>(@"
Y = 44

[AObj]
'The Key' = 6
PubField = ""pubfield was read correctly""
ThatsMine = 'thatsmine read correctly'");

            // Assert
            obj.Y.Should().Be(44);
            obj.AObj.X.Should().Be(6);
            obj.AObj.PubField.Should().Be("pubfield was read correctly");
            obj.AObj.ThatsMineAccessor.Should().Be("thatsmine read correctly");
        }


        public class FObj
        {
            public int X { get; set; } = 1;

            public string PubField = "Serialize all the things";

            private string ThatsMine = "You found me";

            [TomlIgnore]
            public string ThatsMineAccessor => this.ThatsMine;
        }

        public class AObj
        {
            [TomlMember(Key = "The Key")]
            public int X { get; set; } = 1;

            [TomlMember]
            public string PubField = "Serialize all the things";

            [TomlMember]
            private string ThatsMine = "You found me";

            [TomlIgnore]
            public string ThatsMineAccessor => this.ThatsMine;
        }

        public class AObjWrappper
        {
            public int Y { get; set; } = 22;

            public AObj AObj { get; set; } = new AObj();
        }
    }
}
