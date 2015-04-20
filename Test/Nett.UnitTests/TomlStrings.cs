using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nett.UnitTests
{
    internal static class TomlStrings
    {
        public static class Valid
        {
            public const string EmptyArray = "thevoid = [[]]";
            public const string ArrayNoSpaces = "ints = [1,2,3]";
            public const string HetArray = "mixed = [[1, 2], [\"a\", \"b\"], [1.1, 2.1]]";
            public const string ArraysNested = "nest = [[\"a\"], [\"b\"]]";
            public const string Arrays = @"
ints = [1, 2, 3]
floats = [1.1, 2.1, 3.1,]
strings = [""a"", ""b"", ""c""]
dates = [
  1987-07-05T17:45:00Z,
  1979-05-27T07:32:00Z,
  2006-06-01T11:00:00Z,
]";
            public const string TableArrayNested = @"
[[albums]]
name = ""Born to Run""

  [[albums.songs]]
  name = ""Jungleland""

  [[albums.songs]]
  name = ""Meeting Across the River""

[[albums]]
name = ""Born in the USA""
  
  [[albums.songs]]
  name = ""Glory Days""

  [[albums.songs]]
  name = ""Dancing in the Dark""
";
        }
    }
}
