using System;
using System.Collections.Generic;
using FluentAssertions;
using Nett.Parser;
using Xunit;

namespace Nett.Tests.Internal.Parser
{
    public sealed class ValueScopeTrackerTests
    {
        private static readonly Action<char> LVal = new Action<char>(c => { });
        private static readonly Action<char> RVal = new Action<char>(c => { });

        public static TheoryData<string, Action<char>> InputOnlyCases
            => new TheoryData<string, Action<char>>()
            {
                { "", LVal },
                { "X", LVal },
                { "X=", RVal },
                { "X={", LVal},
                { "X={}", LVal },
                { "X=[", RVal },
                { "[", LVal },
            };

        [Theory]
        [MemberData(nameof(InputOnlyCases))]
        public void Feed_InputOnly_HasCorrectFinalScope(string input, Action<char> expected)
        {
            // Act
            var t = CreateTracker(input);

            // Assert
            t.ScopeAction.Should().Be(expected);
        }

        [Fact]
        public void Emit_WhenEmitingNonAssignTokenInsideRootRValueScope_ChangesScopeToLValue()
        {
            // Arrange
            var t = CreateTracker("X=");

            // Act
            t.Emit(TokenType.BareKey);

            // Assert
            t.ScopeAction.Should().Be(LVal);
        }

        [Fact]
        public void Emit_WhenEmitingAssignTokenInsideRootRValueScope_LeavesScopeAtRValue()
        {
            // Arrange
            var t = CreateTracker("X=");

            // Act
            t.Emit(TokenType.Assign);

            // Assert
            t.ScopeAction.Should().Be(RVal);
        }

        [Fact]
        public void Emit_WhenOpenInlineTableEmitted_HasLValueScope()
        {
            // Act
            var t = CreateTracker("x={y=[1]");

            // Assert
            t.ScopeAction.Should().Be(LVal);
        }

        private static ValueScopeTracker CreateTracker(string feed)
        {
            var t = new ValueScopeTracker(LVal, RVal);
            foreach (var c in ToTokenTypes(feed))
            {
                t.Emit(c);
            }

            return t;
        }

        private static IEnumerable<TokenType> ToTokenTypes(string feed)
        {
            foreach (var t in feed)
            {
                switch (t)
                {
                    case '[': yield return TokenType.LBrac; break;
                    case '{': yield return TokenType.LCurly; break;
                    case ']': yield return TokenType.RBrac; break;
                    case '=': yield return TokenType.Assign; break;
                    case ' ': continue;
                    default: yield return TokenType.BareKey; break;
                }
            }
        }
    }
}
