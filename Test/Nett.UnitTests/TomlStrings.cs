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
            public const string ArrayEmpty = "thevoid = [[]]";
            public const string ArrayNoSpaces = "ints = [1,2,3]";
            public const string ArrayHeterogenous = "mixed = [[1, 2], [\"a\", \"b\"], [1.1, 2.1]]";
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
            public const string Boolean = @"
t = true
f = false
";
            public const string CommentsEverywhere = @"
# Top comment.
  # Top comment.
# Top comment.

# [no-extraneous-groups-please]

[group] # Comment
answer = 42 # Comment
# no-extraneous-keys-please = 999
# Inbetween comment.
more = [ # Comment
  # What about multiple # comments?
  # Can you handle it?
  #
          # Evil.
# Evil.
  42, 42, # Comments within arrays are fun.
  # What about multiple # comments?
  # Can you handle it?
  #
          # Evil.
# Evil.
# ] Did I fool you?
] # Hopefully not.
";
        }
    }
}
