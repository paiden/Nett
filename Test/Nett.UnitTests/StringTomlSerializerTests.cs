using System;
using Xunit;
using Xunit.Extensions;

namespace Nett.UnitTests
{
    public class StringTomlSerializerTests
    {
        [Theory]
        [InlineData("Key = 0", "Key", 0)]
        [InlineData("Key = +0", "Key", 0)]
        [InlineData("Key = -0", "Key", 0)]
        [InlineData("Key = 1", "Key", 1)]
        [InlineData("Key = +1", "Key", 1)]
        [InlineData("Key = -1", "Key", -1)]
        public void Deserialize_SimpleyKeyValuePair_ProducesCorrectObject(string toParse, string key, object expectedValue)
        {
            var parsed = StringTomlSerializer.Deserialize(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
            Assert.Equal((long)(int)expectedValue, (long)parsed[key]);
        }
    }
}
