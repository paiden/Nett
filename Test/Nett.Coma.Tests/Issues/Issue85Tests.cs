using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests.Issues
{
    public class Issue85Tests
    {
        private readonly ST StringTransformForComparison = ST.Do(ST.Norm, ST.NoSpc, ST.Trim);

        public class DictConfig
        {
            public Dictionary<string, object> Tbl { get; set; } = new Dictionary<string, object>();
        }

        [Fact]
        public void GivenTomlWithTable_ComaCanMapItIntoAGenericDictionaryOfObjectValues()
        {
            using var machine = TestFileName.Create("machine", ".toml");

            // Arrange
            const string machineText = @"
[Tbl]
a = 1
d = ""D""
e = [""b"", ""c""]";

            File.WriteAllText(machine, machineText);

            var cfg = Config.CreateAs()
                .MappedToType(() => new DictConfig())
                .StoredAs(store => store.File(machine))
                .Initialize();

            // Act
            var r = cfg.GetAs<string[]>(c => c.Tbl["e"]);

            // Assert
            r.Should().Equal("b", "c");
        }

        [Fact]
        public void WhenUpdatingRowWithNewValues_WrittenTomlHasTheseNewRowValues()
        {
            using var machine = TestFileName.Create("machine", ".toml");

            // Arrange
            const string machineText = @"
[Tbl]
a = 1
d = ""D""
e = [""b"", ""c""]";

            File.WriteAllText(machine, machineText);

            var cfg = Config.CreateAs()
                .MappedToType(() => new DictConfig())
                .StoredAs(store => store.File(machine))
                .Initialize();

            // Act
            cfg.Set(c => c.Tbl["e"], new string[] { "x", "y" });

            // Assert
            File.ReadAllText(machine).Should().BeAfterTransforms(StringTransformForComparison, @"
[Tbl]
a = 1
d = ""D""
e = [""x"", ""y""]");
        }

        [Fact]
        public void WhenUpdatingValueOfLoadedGenericDict_TheWrittentomlContainsTheUpdatedValue()
        {
            using var machine = TestFileName.Create("machine", ".toml");
            using var user = TestFileName.Create("user", ".toml");

            // Arrange
            const string machineText = @"[Tbl]
x = 1";
            const string userText = @"[Tbl]
x = 2";
            File.WriteAllText(machine, machineText);
            File.WriteAllText(user, userText);

            // Act
            IConfigSource src = null;
            var cfg = Config.CreateAs()
                .MappedToType(() => new DictConfig())
                .StoredAs(store => store.File(machine)
                    .MergeWith(store.File(user).AccessedBySource("user", out src)))
                .Initialize();

            var tbl = cfg.Get(c => c.Tbl);
            tbl["x"] = 4;
            cfg.Set(c => c.Tbl, tbl, src);

            // Assert
            File.ReadAllText(user).ShouldBeSemanticallyEquivalentTo(@"[Tbl]
x = 4");
        }
    }
}
