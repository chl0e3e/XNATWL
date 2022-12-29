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

        public abstract bool evaluate(AnimationState animationState);

        public abstract void getUsedStateKeys(BitSet bs);

        internal bool negate;

        public static StateExpression parse(String exp, bool negate)
        {
            StringIterator si = new StringIterator(exp);
            StateExpression expr = parse(si);
            if (si.hasMore())
            {
                si.unexpected();
            }
            expr.negate ^= negate;
            return expr;
        }

        private static StateExpression parse(StringIterator si)
        {
            List<StateExpression> children = new List<StateExpression>();
            char kind = ' ';

            for (; ; )
            {
                if (!si.skipSpaces())
                {
                    si.unexpected();
                }
                char ch = si.peek();
                bool negate = ch == '!';
                if (negate)
                {
                    si.pos++;
                    if (!si.skipSpaces())
                    {
                        si.unexpected();
                    }
                    ch = si.peek();
                }

                StateExpression child = null;
                CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
                if (provider.IsValidIdentifier(ch.ToString()))
                {
                    child = new Check(StateKey.Get(si.getIdent()));
                }
                else if (ch == '(')
                {
                    si.pos++;
                    child = parse(si);
                    si.expect(')');
                }
                else if (ch == ')')
                {
                    break;
                }
                else
                {
                    si.unexpected();
                }

                child.negate = negate;
                children.Add(child);

                if (!si.skipSpaces())
                {
                    break;
                }

                ch = si.peek();
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
                    si.expect(kind);
                }
                si.pos++;
            }

            if (children.Count == 0)
            {
                si.unexpected();
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
            readonly string str;
            internal int pos;

            internal StringIterator(String str)
            {
                this.str = str;
            }

            internal bool hasMore()
            {
                return pos < str.Length;
            }

            internal char peek()
            {
                return str[pos];
            }

            internal void expect(char what)
            {
                if(!hasMore() || peek() != what)
                {
                    throw new ParseException("Expected '"+what+"' got " + describePosition(), pos);
                }
                pos++;
            }

            internal void unexpected()
            {
                throw new ParseException("Unexpected " + describePosition(), pos);
            }

            internal string describePosition()
            {
                if (pos >= str.Length)
                {
                    return "end of expression";
                }
                return "'" + peek() + "' at " + (pos + 1);
            }

            internal bool skipSpaces()
            {
                while (hasMore() && (peek() == " ".ToCharArray()[0]))
                {
                    pos++;
                }
                return hasMore();
            }

            internal string getIdent()
            {
                int start = pos;

                CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
                while (hasMore() && provider.IsValidIdentifier(peek().ToString()))
                {
                    pos++;
                }

                return str.Substring(start, pos);
            }
        }
    }


    public class Logic : StateExpression
    {
        StateExpression[] children;
        bool and;
        bool xor;
        
        public Logic(char kind, params StateExpression[] children)
        {
            if (kind != '|' && kind != '+' && kind != '^')
            {
                throw new ArgumentOutOfRangeException("kind");
            }
            this.children = children;
            this.and = kind == '+';
            this.xor = kind == '^';
        }

        public override bool evaluate(AnimationState animationState)
        {
            bool result = and ^ negate;
            foreach (StateExpression e in children)
            {
                bool value = e.evaluate(animationState);
                if (xor)
                {
                    result ^= value;
                }
                else if (and != value)
                {
                    return result ^ true;
                }
            }
            return result;
        }

        public override void getUsedStateKeys(BitSet bs)
        {
            foreach (StateExpression e in children)
            {
                e.getUsedStateKeys(bs);
            }
        }
    }

    public class Check : StateExpression
    {
        StateKey state;

        public Check(StateKey state)
        {
            this.state = state;
        }

        public override bool evaluate(AnimationState animationState)
        {
            return negate ^ (animationState != null && animationState.GetAnimationState(state));
        }

        public override void getUsedStateKeys(BitSet bs)
        {
            bs.Set(state.ID);
        }
    }
}
