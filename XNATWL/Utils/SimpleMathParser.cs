using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.StateExpression;
using XNATWL.Renderer;

namespace XNATWL.Utils
{
    public class SimpleMathParser
    {
        public interface Interpreter
        {
            void accessVariable(string name);
            void accessField(string field);
            void accessArray();
            void loadConst(Number n);
            void add();
            void sub();
            void mul();
            void div();
            void callFunction(string name, int args);
            void negate();
        }

        string str;
        Interpreter interpreter;
        int pos;

        private SimpleMathParser(string str, Interpreter interpreter)
        {
            this.str = str;
            this.interpreter = interpreter;
        }

        public static void interpret(string str, Interpreter interpreter)
        {
            new SimpleMathParser(str, interpreter).parse(false);
        }

        public static int interpretArray(string str, Interpreter interpreter)
        {
            return new SimpleMathParser(str, interpreter).parse(true);
        }

        private int parse(bool allowArray)
        {
            try
            {
                if (peek() == -1)
                {
                    if (allowArray)
                    {
                        return 0;
                    }
                    unexpected(-1);
                }
                int count = 0;
                for (; ; )
                {
                    count++;
                    parseAddSub();
                    int ch = peek();
                    if (ch == -1)
                    {
                        return count;
                    }
                    if (ch != ',' || !allowArray)
                    {
                        unexpected(ch);
                    }
                    pos++;
                }
            }
            catch (ParseException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ParseException("Unable to execute", pos, ex);
            }
        }

        private void parseAddSub()
        {
            parseMulDiv();
            for (; ; )
            {
                int ch = peek();
                switch (ch)
                {
                    case '+':
                        pos++;
                        parseMulDiv();
                        interpreter.add();
                        break;
                    case '-':
                        pos++;
                        parseMulDiv();
                        interpreter.sub();
                        break;
                    default:
                        return;
                }
            }
        }

        private void parseMulDiv()
        {
            parseIdentOrConst();
            for (; ; )
            {
                int ch = peek();
                switch (ch)
                {
                    case '*':
                        pos++;
                        parseIdentOrConst();
                        interpreter.mul();
                        break;
                    case '/':
                        pos++;
                        parseIdentOrConst();
                        interpreter.div();
                        break;
                    default:
                        return;
                }
            }
        }

        private void parseIdentOrConst()
        {
            int ch = peek();
            if (ch == '\'' || CharUtil.IsCSharpIdentifier((char)ch))
            {
                string ident = parseIdent();
                ch = peek();
                if (ch == '(')
                {
                    pos++;
                    parseCall(ident);
                    return;
                }
                interpreter.accessVariable(ident);
                while (ch == '.' || ch == '[')
                {
                    pos++;
                    if (ch == '.')
                    {
                        string field = parseIdent();
                        interpreter.accessField(field);
                    }
                    else
                    {
                        parseIdentOrConst();
                        expect(']');
                        interpreter.accessArray();
                    }
                    ch = peek();
                }
            }
            else if (ch == '-')
            {
                pos++;
                parseIdentOrConst();
                interpreter.negate();
            }
            else if (ch == '.' || ch == '+' || CharUtil.IsDigit((char)ch))
            {
                parseConst();
            }
            else if (ch == '(')
            {
                pos++;
                parseAddSub();
                expect(')');
            }
        }

        private void parseCall(string name)
        {
            int count = 1;
            parseAddSub();
            for (; ; )
            {
                int ch = peek();
                if (ch == ')')
                {
                    pos++;
                    interpreter.callFunction(name, count);
                    return;
                }
                if (ch == ',')
                {
                    pos++;
                    count++;
                    parseAddSub();
                }
                else
                {
                    unexpected(ch);
                }
            }
        }

        private void parseConst()
        {
            int len = str.Length;
            int start = pos;
            Number n;
            switch (str[pos])
            {
                case '+':
                    // skip
                    start = ++pos;
                    break;
                case '0':
                    if (pos + 1 < len && str[pos + 1] == 'x')
                    {
                        pos += 2;
                        parseHexInt();
                        return;
                    }
                    break;
            }
            while (pos < len && CharUtil.IsDigit(str[pos]))
            {
                pos++;
            }
            if (pos < len && str[pos] == '.')
            {
                pos++;
                while (pos < len && CharUtil.IsDigit(str[pos]))
                {
                    pos++;
                }
                if (pos - start <= 1)
                {
                    unexpected(-1);
                }
                n = new Number(float.Parse(str.Substring(start, pos - start)));
            }
            else
            {
                n = new Number(int.Parse(str.Substring(start, pos - start)));
            }
            interpreter.loadConst(n);
        }

        private void parseHexInt()
        {
            int len = str.Length;
            int start = pos;
            while (pos < len && "0123456789abcdefABCDEF".IndexOf(str[pos]) >= 0)
            {
                pos++;
            }
            if (pos - start > 8)
            {
                throw new ParseException("Number to large at " + pos, pos);
            }
            if (pos == start)
            {
                unexpected((pos < len) ? str[pos] : -1);
            }
            interpreter.loadConst(new Number(long.Parse(str.Substring(start, pos - start), System.Globalization.NumberStyles.HexNumber)));
        }

        private bool skipSpaces()
        {
            for (; ; )
            {
                if (pos == str.Length)
                {
                    return false;
                }
                if (!CharUtil.IsWhitespace(str[pos]))
                {
                    return true;
                }
                pos++;
            }
        }

        private int peek()
        {
            if (skipSpaces())
            {
                return str[pos];
            }
            return -1;
        }

        private string parseIdent()
        {
            System.Diagnostics.Debug.WriteLine("pos: " + pos);
            System.Diagnostics.Debug.WriteLine("str: " + str);
            if (str[pos] == '\'')
            {
                int istart = ++pos;
                pos = TextUtil.indexOf(str, '\'', pos);
                string ident = str.Substring(istart, pos - istart);
                expect('\'');
                return ident;
            }
            int start = pos;
            System.Diagnostics.Debug.WriteLine("loop2");
            System.Diagnostics.Debug.WriteLine("start2: " + start);
            System.Diagnostics.Debug.WriteLine("str2: " + str);
            while (pos < str.Length && CharUtil.IsCSharpIdentifier(str[pos]))
            {
                pos++;
            }
            if (pos > str.Length)
            {
                pos = str.Length;
            }
            System.Diagnostics.Debug.WriteLine("pos2: " + pos);
            System.Diagnostics.Debug.WriteLine("str2: " + str);
            return str.Substring(start, pos - start);
        }

        private void expect(int what)
        {
            int ch = peek();
            if (ch != what)
            {
                unexpected(ch);
            }
            else
            {
                pos++;
            }
        }

        private void unexpected(int ch)
        {
            if (ch < 0)
            {
                throw new ParseException("Unexpected end of string", pos);
            }
            throw new ParseException("Unexpected character '" + (char)ch + "' at " + pos, pos);
        }
    }
}
