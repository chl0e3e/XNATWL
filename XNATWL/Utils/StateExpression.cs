using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;
using XNATWL.Util;

namespace XNATWL.Utils
{
    public abstract class StateExpression
    {
        internal StateExpression()
        {
        }

        public abstract bool Evaluate(AnimationState animationState);

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


        internal class ParseException : Exception
        {
            public ParseException(string message, int position)
            {

            }
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

                CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
                while (HasMore() && provider.IsValidIdentifier(Peek().ToString()))
                {
                    _pos++;
                }

                return _str.Substring(start, _pos);
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

        public override bool Evaluate(AnimationState animationState)
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

        public override bool Evaluate(AnimationState animationState)
        {
            return _negate ^ (animationState != null && animationState.GetAnimationState(_state));
        }

        public override void GetUsedStateKeys(BitSet bs)
        {
            bs.Set(_state.ID);
        }
    }
}
