using FluentAssertions;
using Nett.Parser;
using Nett.Parser.Nodes;
using Xunit;

namespace Nett.Tests.Internal.Parser
{
    public sealed class AstParserTests
    {
        public class Parsing
        {
            public class General
            {
                [Fact]
                public void EmptyFile_CreatesNullAst()
                {
                    // Act
                    var parsed = Parse("");

                    // Assert
                    parsed.Should().BeNull();
                }
            }

            public class Expressions
            {
                [Theory]
                [InlineData("true", "true")]
                [InlineData("false", "false")]
                [InlineData("100", "100")]
                [InlineData("100.0", "100.0")]
                [InlineData("\"val\"", "val")]
                [InlineData("'val'", "val")]
                [InlineData("\"\"\"val\"\"\"", "val")]
                [InlineData("'''val'''", "val")]
                [InlineData("1979-05-27T00:32:00.999999-07:00", "1979-05-27T00:32:00.999999-07:00")]
                [InlineData("1979-05-27T07:32:00", "1979-05-27T07:32:00")]
                [InlineData("1d2h3m4s5ms", "1d2h3m4s5ms")]
                public void TerminalKeyValueExpression_CreatesCorrectAst(string input, string treeValue)
                {
                    // Act
                    var parsed = Parse($"x = {input}");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be($@"
S
 E
  K
   x
  =
  V
   {treeValue}
".Trim());
                }

                [Fact]
                public void FileWithNewlinesInBeginning_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse(@"

x = 100");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   100
".Trim());
                }

                [Fact]
                public void MultipleExpressions_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse(@"x = 1
y = 2");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   1
 NE
  E
   K
    y
   =
   V
    2
".Trim());
                }

                [Fact]
                public void KeyOnly_CreatesSyntaxErrorNode()
                {
                    // Act
                    var parsed = Parse("x");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 X
".Trim());
                }

                [Fact]
                public void NoValue_CreatesAstWithSyntaxErrorNode()
                {
                    // Act
                    var parsed = Parse("x = ");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  X
".Trim());
                }
            }

            public class Comments
            {
                [Fact]
                public void CommentBeforeKVE_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse(@"#Cmnt
x = 100
");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   100
".Trim());
                }


                [Fact]
                public void SingleKeyValueWithComment_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = 100#Cmnt");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   100
".Trim());
                }
            }

            public sealed class Tables
            {

                [Fact]
                public void TopLevelTable_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("[tablekey ]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 T
  [
  TS
   K
    tablekey
  ]
".Trim());
                }

                [Fact]
                public void SubTable_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("[x.y]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 T
  [
  TS
   K
    x
    KS
     .
     K
      y
  ]
".Trim());
                }
            }

            public sealed class TableArrays
            {
                [Fact]
                public void ToLevelTableArray_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("[  [tablekey ] ]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 T
  [
  TA
   [
   K
    tablekey
   ]
  ]
".Trim());
                }
            }

            public sealed class Arrays
            {
                [Fact]
                public void EmptyArrayValue_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = []");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    ]
".Trim());
                }

                [Fact]
                public void TwoEmptySubArrays_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = [[], []]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    AI
     V
      A
       [
       ]
     AS
      ,
      AI
       V
        A
         [
         ]
    ]
".Trim());
                }

                [Fact]
                public void ArrayValueWithValue_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = [100]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    AI
     V
      100
    ]
".Trim());
                }

                [Fact]
                public void ArrayWithFinalSeparator_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = [100,]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    AI
     V
      100
     AS
      ,
    ]
".Trim());
                }

                [Fact]
                public void ArrayWithSecondValue_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = [100,200]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    AI
     V
      100
     AS
      ,
      AI
       V
        200
    ]
".Trim());
                }

                [Fact]
                public void EmptyNewlineArray_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse(@"x = [
]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    ]
".Trim());
                }

                [Fact]
                public void ArrayWithValuesAndNewlines_CreatesCorrectAst()
                {
                    // Act
                    var parsed = Parse(@"x = [
                100
,

200

]");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   A
    [
    AI
     V
      100
     AS
      ,
      AI
       V
        200
    ]
".Trim());
                }
            }

            public sealed class InlineTable
            {
                [Fact]
                public void EmptyTable_ProducesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = {}");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   I
    {
    }
".Trim());
                }

                [Fact]
                public void SingleRow_ProducesCorrectAst()
                {
                    // Act
                    var parsed = Parse("x = {x = 1}");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   I
    {
    II
     E
      K
       x
      =
      V
       1
    }
".Trim());
                }

                [Fact]
                public void MultipleRows_ProducesCorrectAst()
                {
                    // Act
                    var parsed = Parse(@"x = {x = 1, y=[2,
                    ]}");

                    // Assert
                    parsed.PrintTree().Trim().Should().Be(@"
S
 E
  K
   x
  =
  V
   I
    {
    II
     E
      K
       x
      =
      V
       1
     NI
      ,
      II
       E
        K
         y
        =
        V
         A
          [
          AI
           V
            2
           AS
            ,
          ]
    }
".Trim());
                }
            }
        }

        private static Node Parse(string input)
        {
            var lexer = new Lexer(input);
            var tokens = lexer.Lex();
            var parser = new Nett.Parser.Parser(new ParseInput(tokens), TomlSettings.DefaultInstance);
            return parser.Parse().NodeOrDefault();
        }
    }
}
