using System.IO;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests.Issues
{


    public class Issue84Tests
    {
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

        [Fact(Skip = "Disabled as followup commits are needed to make it work.")]
        public void CommentsNotLostWhenPorpertyValueIsSet()
        {
            var expectedUnchanged = @"#Some comment
b = true

#Comment on nested table
[a]
#Comment on C
c = """"
";
            var expectedChanged = @"#Some comment
b = false

#Comment on nested
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
            readValue.ShouldBeNormalizedEqualTo(expectedUnchanged);

            config.Set(c => c.b, false);

            readValue = File.ReadAllText(filename);
            readValue.ShouldBeNormalizedEqualTo(expectedChanged);
        }
    }
}
