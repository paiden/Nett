namespace Nett.Tests.Util.TestData
{
    /// <summary>
    /// Contains all test cases from: https://github.com/BurntSushi/toml-test/tree/master/tests, except the completely trivial
    /// ones that area already handled by other cases.
    /// </summary>
    public static class TomlStrings
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

emptyArr1 = [ ] # End

emptyArr2 = [ # Comment

    #Comment

] # End

questions = [ # Comment
 # Comment

    { question = '*', answer = 42 }, # Comments within inline table arrays are also fun.
    # Comment

    # Comment
] # End
# Comment
";

            public const string DateTime = "bestdayever = 1987-07-05T17:45:00Z";
            public const string Empty = "";
            public const string Example = @"
best-day-ever = 1987-07-05T17:45:00Z

[numtheory]
boring = false
perfection = [6, 28, 496]
";
            public const string Floats = @"
pi = 3.14
negpi = -3.14
";
            public const string ImplicitAndExplicitAfter = @"
[a.b.c]
answer = 42

[a]
better = 43";
            public const string ImplicitAndExplicitBefore = @"
[a]
better = 43

[a.b.c]
answer = 42";
            public const string ImplicitGroups = @"
[a.b.c]
answer = 42";
            public const string Integer = @"
answer = 42
neganswer = -42";
            public const string KeyEqualsNoSpace = @"answer=42";
            public const string KeyEqualsSpace = @"""a b"" = 42";
            public const string KeySpecialChars = @"""~!@$^&*()_+-`1234567890[]|/?><.,;:'"" = 42";
            public const string LongFloats = @"longpi = 3.141592653589793
neglongpi = -3.141592653589793";
            public const string LongInts = @"answer = 9223372036854775806
neganswer = -9223372036854775807";
            public const string MultiLineStrings = @"
multiline_empty_one = """"""""""""
multiline_empty_two = """"""
""""""
multiline_empty_three = """"""\
    """"""
multiline_empty_four = """"""\
   \
   \
   """"""

equivalent_one = ""The quick brown fox jumps over the lazy dog.""
equivalent_two = """"""
The quick brown \


  fox jumps over \
    the lazy dog.""""""

equivalent_three = """"""\
       The quick brown \
       fox jumps over \
       the lazy dog.\
       """"""";
            public const string RawMultilineStrings = @"
oneline = '''This string has a ' quote character.'''
firstnl = '''
This string has a ' quote character.'''
multiline = '''
This string
 has a ' quote character
 and more than
 one newline
 in it.'''";
            public const string RawStrings = @"backspace = 'This string has a \b backspace character.'
tab = 'This string has a \t tab character.'
newline = 'This string has a \n new line character.'
formfeed = 'This string has a \f form feed character.'
carriage = 'This string has a \r carriage return character.'
slash = 'This string has a \/ slash character.'
backslash = 'This string has a \\ backslash character.'";
            public const string StringEmpty = "answer = \"\"";
            public const string StringEscapes = "backspace = \"This string has a \\b backspace character.\"" +
"\r\ntab = \"This string has a \\t tab character.\"" +
"\r\nnewline = \"This string has a \\n new line character.\"" +
"\r\nformfeed = \"This string has a \\f form feed character.\"" +
"\r\ncarriage = \"This string has a \\r carriage return character.\"" +
"\r\nquote = \"This string has a \\\" quote character.\"" +
"\r\nbackslash = \"This string has a \\\\ backslash character.\"" +
"\r\nnotunicode1 = \"This string does not have a unicode \\\\u escape.\"" +
"\r\nnotunicode2 = \"This string does not have a unicode \\u005Cu escape.\"" +
"\r\nnotunicode3 = \"This string does not have a unicode \\\\u0075 escape.\"" +
"\r\nnotunicode4 = \"This string does not have a unicode \\\\\\u0075 escape.\"";
            public const string StringWithPound = @"pound = ""We see no # comments here.""
poundcomment = ""But there are # some comments here."" # Did I # mess you up?";
            public const string TableArrayImplicit = @"[[albums.songs]]
name = ""Glory Days""";
            public const string TableArrayMany = @"[[people]]
first_name = ""Bruce""
last_name = ""Springsteen""

[[people]]
first_name = ""Eric""
last_name = ""Clapton""

[[people]]
first_name = ""Bob""
last_name = ""Seger""";

            public const string NestedArrayOfTables = @"
[[fruits]]
  name = ""apple""

  [fruits.physical]
            color = ""red""
    shape = ""round""

  [[fruits.variety]]
    name = ""red delicious""

  [[fruits.variety]]
    name = ""granny smith""

[[fruits]]
  name = ""banana""

  [[fruits.variety]]
    name = ""plantain""";

            public const string InlineTableNoSpaces = @"
[Test]
InlineTable = {""test"" = 1}";
        }
    }
}
