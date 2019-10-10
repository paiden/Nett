using System.IO;
using Nett.Tests.Util;
using Nett.Coma;
using Xunit;

namespace Nett.Coma.Tests.Issues
{


    public class Issue84Tests
    {
        private static readonly string InitialToml = @"#Some comment
b = true

#Comment on nested table
[a]
#Comment on C
c = """"
";

        public class Configuration
        {

            [TomlComment("Comment on nested table")]
            public A a { get; set; } = new A();

            [TomlComment("Some comment")]
            public bool b { get; set; } = true;

            public class A
            {
                [TomlComment("Comment on C")]
                public string c { get; set; } = "";
            }
        }

        [Fact]
        public void CommentsNotLostWhenPorpertyValueIsSet()
        {
            // Arrange
            var expectedChanged = @"#Some comment
b = false

#Comment on nested table
[a]
#Comment on C
c = """"
";

            using var filename = TestFileName.Create($"settings", Toml.FileExtension);
            var config = Config.CreateAs()
                .MappedToType(() => new Configuration())
                .StoredAs(builder => builder.File(filename))
                .Initialize();

            var readValue = File.ReadAllText(filename);
            readValue.ShouldBeNormalizedEqualTo(InitialToml);

            // Act
            config.Set(c => c.b, false);

            // Assert
            readValue = File.ReadAllText(filename);
            readValue.ShouldBeNormalizedEqualTo(expectedChanged);
        }
    }
}
