using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using Nett.Coma.Path;
using Xunit;

namespace Nett.Coma.Tests.Internal.Unit
{
    public class Tpo
    {
        public string X { get; set; }

        public Dictionary<string, string> D { get; set; }

        public Dictionary<string, Dictionary<string, string>> DD { get; set; }

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> DDD { get; set; }
    }

    public class TPathBuilderTests
    {
        [Fact]
        public void BuildTPathOnProperty()
        {
            // Arrange

            // Act
            var path = Build(x => x.X);

            // Assert
            path.ToString().Should().Be("/X");
        }

        [Fact]
        public void BuildTPathOnDictionary()
        {
            // Arrange

            // Act
            var path = Build(x => x.D["S"]);

            // Assert
            path.ToString().Should().Be("/D/S");
        }

        [Fact]
        public void BuildTPathOn2LevelDictionary()
        {
            // Arrange

            // Act
            var path = Build(x => x.DD["A"]["B"]);

            // Assert
            path.ToString().Should().Be("/DD/A/B");
        }

        [Fact]
        public void BuildTPathOn3LevelDictionary()
        {
            // Arrange

            // Act
            var path = Build(x => x.DDD["A"]["B"]["C"]);

            // Assert
            path.ToString().Should().Be("/DDD/A/B/C");
        }

        private static TPath Build<TR>(Expression<Func<Tpo, TR>> expression)
            => TPath.Build(expression);
    }
}
