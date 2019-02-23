using System;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests.Issues
{
    public class Issue25Tests
    {
        public class TheCfg
        {
            public DateTime Dt { get; set; }
        }

        [Fact]
        public void VerifyIssue25_UnitializedDateTime_DoesNotCrasComaInit()
        {
            using (var fn = TestFileName.Create("thecfg", ".toml"))
            {
                // Act
                Action a = () => Config.CreateAs()
                    .MappedToType(() => new TheCfg())
                    .StoredAs(store => store.File(fn))
                    .Initialize();

                // Assert
                a.ShouldNotThrow<ArgumentOutOfRangeException>(because: "Nett's converters should be able to handle the " +
                    "following case internally in any time zone: var off = new DateTimeOfffset(DateTime.MinValue) which happens " +
                    "when the DateTime field of a class is not initialized with an explicit value.");
            }
        }
    }
}
