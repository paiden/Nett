using System;
using System.Collections.Generic;

namespace Nett.Parser
{
    internal sealed class ValueScopeTracker
    {
        private readonly Action<char> rvalueAction;
        private readonly Action<char> lvalueAction;

        private readonly Stack<Tuple<TokenType, Action<char>>> rvalueStack = new Stack<Tuple<TokenType, Action<char>>>();

        public ValueScopeTracker(Action<char> lvalueAction, Action<char> rvalueAction)
        {
            this.lvalueAction = lvalueAction;
            this.rvalueAction = rvalueAction;
            this.ResetStack();
        }

        internal enum ValueScope
        {
            RValue,
            LValue,
        }

        public Action<char> ScopeAction => this.rvalueStack.Peek().Item2;

        public void Emit(TokenType t)
        {
            if (t == TokenType.Assign) { this.rvalueStack.Push(Tuple.Create(t, this.rvalueAction)); }
            else if (InRVal() && t == TokenType.LBrac) { this.rvalueStack.Push(Tuple.Create(t, this.rvalueAction)); }
            else if (InRVal() && t == TokenType.LCurly) { this.rvalueStack.Push(Tuple.Create(t, this.lvalueAction)); }
            else if (t == TokenType.RBrac || t == TokenType.RCurly) { this.PopScope(t); }
            else { this.PopAssignmentRValueScope(); }

            bool InRVal()
                => this.ScopeAction == this.rvalueAction;
        }

        private void PopAssignmentRValueScope()
        {
            if (this.rvalueStack.Peek().Item1 == TokenType.Assign && this.rvalueStack.Count > 1)
            {
                this.rvalueStack.Pop();
            }
        }

        private void PopScope(TokenType t)
        {
            if (this.rvalueStack.Peek().Item1 == this.Complement(t))
            {
                this.rvalueStack.Pop();
                this.PopAssignmentRValueScope();
            }
            else
            {
                this.HandleMismatchedStack();
            }
        }

        private TokenType Complement(TokenType t)
        {
            switch (t)
            {
                case TokenType.RBrac: return TokenType.LBrac;
                case TokenType.RCurly: return TokenType.LCurly;
                case TokenType.LBrac: return TokenType.RBrac;
                case TokenType.LCurly: return TokenType.RCurly;
                default: return t;
            }
        }

        // No real error handling, just reset everything to be LVAL again. This happens when user writes stuff like
        // X = {3]. Maybe do more sophisticated handling later (e.g. examine newline...).
        private void HandleMismatchedStack()
            => this.ResetStack();

        private void ResetStack()
        {
            this.rvalueStack.Clear();
            this.rvalueStack.Push(Tuple.Create(TokenType.Unknown, this.lvalueAction));
        }
    }
}
