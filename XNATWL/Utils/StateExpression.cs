﻿/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using XNATWL.Renderer;
using XNATWL.Util;

namespace XNATWL.Utils
{
    public abstract class StateExpression
    {
        internal StateExpression()
        {
        }

        public abstract bool Evaluate(Renderer.AnimationState animationState);

        public abstract void GetUsedStateKeys(BitSet bs);

        internal bool _negate;

        public static StateExpression Parse(String exp, bool negate)
        {
            StringIterator si = new StringIterator(exp);
            StateExpression expr = Parse(si);
            if (si.HasMore())
            {
                si.Unexpected();
            }
            expr._negate ^= negate;
            return expr;
        }

        private static StateExpression Parse(StringIterator si)
        {
            List<StateExpression> children = new List<StateExpression>();
            char kind = ' ';

            for (; ; )
            {
                if (!si.SkipSpaces())
                {
                    si.Unexpected();
                }
                char ch = si.Peek();
                bool negate = ch == '!';
                if (negate)
                {
                    si._pos++;
                    if (!si.SkipSpaces())
                    {
                        si.Unexpected();
                    }
                    ch = si.Peek();
                }

                StateExpression child = null;
                CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
                if (provider.IsValidIdentifier(ch.ToString()))
                {
                    child = new Check(StateKey.Get(si.GetIdent()));
                }
                else if (ch == '(')
                {
                    si._pos++;
                    child = Parse(si);
                    si.Expect(')');
                }
                else if (ch == ')')
                {
                    break;
                }
                else
                {
                    si.Unexpected();
                }

                child._negate = negate;
                children.Add(child);

                if (!si.SkipSpaces())
                {
                    break;
                }

                ch = si.Peek();
                if ("|+^".IndexOf(ch) < 0)
                {
                    break;
                }

                if (children.Count == 1)
                {
                    kind = ch;
                }
                else if (kind != ch)
                {
                    si.Expect(kind);
                }
                si._pos++;
            }

            if (children.Count == 0)
            {
                si.Unexpected();
            }

            if(!(kind != ' ' || children.Count == 1))
            {
                throw new Exception("Assertion failed");
            }

            if (children.Count == 1)
            {
                return children[0];
            }

            return new Logic(kind, children.ToArray());
        }

        internal class StringIterator
        {
            readonly string _str;
            internal int _pos;

            internal StringIterator(String str)
            {
                this._str = str;
            }

            internal bool HasMore()
            {
                return _pos < _str.Length;
            }

            internal char Peek()
            {
                return _str[_pos];
            }

            internal void Expect(char what)
            {
                if(!HasMore() || Peek() != what)
                {
                    throw new ParseException("Expected '"+what+"' got " + DescribePosition(), _pos);
                }
                _pos++;
            }

            internal void Unexpected()
            {
                throw new ParseException("Unexpected " + DescribePosition(), _pos);
            }

            internal string DescribePosition()
            {
                if (_pos >= _str.Length)
                {
                    return "end of expression";
                }
                return "'" + Peek() + "' at " + (_pos + 1);
            }

            internal bool SkipSpaces()
            {
                while (HasMore() && (Peek() == " ".ToCharArray()[0]))
                {
                    _pos++;
                }
                return HasMore();
            }

            internal string GetIdent()
            {
                int start = _pos;

                while (HasMore() && CharUtil.IsCSharpIdentifier(Peek()))
                {
                    _pos++;
                }

                return _str.Substring(start, _pos - start);
            }
        }
    }

    public class Logic : StateExpression
    {
        StateExpression[] _children;
        bool _and;
        bool _xor;
        
        public Logic(char kind, params StateExpression[] children)
        {
            if (kind != '|' && kind != '+' && kind != '^')
            {
                throw new ArgumentOutOfRangeException("kind");
            }
            this._children = children;
            this._and = kind == '+';
            this._xor = kind == '^';
        }

        public override bool Evaluate(Renderer.AnimationState animationState)
        {
            bool result = _and ^ _negate;
            foreach (StateExpression e in _children)
            {
                bool value = e.Evaluate(animationState);
                if (_xor)
                {
                    result ^= value;
                }
                else if (_and != value)
                {
                    return result ^ true;
                }
            }
            return result;
        }

        public override void GetUsedStateKeys(BitSet bs)
        {
            foreach (StateExpression e in _children)
            {
                e.GetUsedStateKeys(bs);
            }
        }
    }

    public class Check : StateExpression
    {
        StateKey _state;

        public Check(StateKey state)
        {
            this._state = state;
        }

        public override bool Evaluate(Renderer.AnimationState animationState)
        {
            return _negate ^ (animationState != null && animationState.GetAnimationState(_state));
        }

        public override void GetUsedStateKeys(BitSet bs)
        {
            bs.Set(_state.ID);
        }
    }
}
