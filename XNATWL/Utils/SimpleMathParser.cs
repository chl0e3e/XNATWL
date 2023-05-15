/*
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

namespace XNATWL.Utils
{
    public class SimpleMathParser
    {
        public interface Interpreter
        {
            void AccessVariable(string name);
            void AccessField(string field);
            void AccessArray();
            void LoadConst(Number n);
            void Add();
            void Sub();
            void Mul();
            void Div();
            void CallFunction(string name, int args);
            void Negate();
        }

        string _str;
        Interpreter _interpreter;
        int _pos;

        private SimpleMathParser(string str, Interpreter interpreter)
        {
            this._str = str;
            this._interpreter = interpreter;
        }

        public static void Interpret(string str, Interpreter interpreter)
        {
            new SimpleMathParser(str, interpreter).Parse(false);
        }

        public static int InterpretArray(string str, Interpreter interpreter)
        {
            return new SimpleMathParser(str, interpreter).Parse(true);
        }

        private int Parse(bool allowArray)
        {
            try
            {
                if (Peek() == -1)
                {
                    if (allowArray)
                    {
                        return 0;
                    }
                    Unexpected(-1);
                }
                int count = 0;
                for (; ; )
                {
                    count++;
                    ParseAddSub();
                    int ch = Peek();
                    if (ch == -1)
                    {
                        return count;
                    }
                    if (ch != ',' || !allowArray)
                    {
                        Unexpected(ch);
                    }
                    _pos++;
                }
            }
            catch (ParseException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ParseException("Unable to execute", _pos, ex);
            }
        }

        private void ParseAddSub()
        {
            ParseMulDiv();
            for (; ; )
            {
                int ch = Peek();
                switch (ch)
                {
                    case '+':
                        _pos++;
                        ParseMulDiv();
                        _interpreter.Add();
                        break;
                    case '-':
                        _pos++;
                        ParseMulDiv();
                        _interpreter.Sub();
                        break;
                    default:
                        return;
                }
            }
        }

        private void ParseMulDiv()
        {
            ParseIdentOrConst();
            for (; ; )
            {
                int ch = Peek();
                switch (ch)
                {
                    case '*':
                        _pos++;
                        ParseIdentOrConst();
                        _interpreter.Mul();
                        break;
                    case '/':
                        _pos++;
                        ParseIdentOrConst();
                        _interpreter.Div();
                        break;
                    default:
                        return;
                }
            }
        }

        private void ParseIdentOrConst()
        {
            int ch = Peek();
            if (ch == '\'' || CharUtil.IsCSharpIdentifier((char)ch))
            {
                string ident = ParseIdent();
                ch = Peek();
                if (ch == '(')
                {
                    _pos++;
                    ParseCall(ident);
                    return;
                }
                _interpreter.AccessVariable(ident);
                while (ch == '.' || ch == '[')
                {
                    _pos++;
                    if (ch == '.')
                    {
                        string field = ParseIdent();
                        _interpreter.AccessField(field);
                    }
                    else
                    {
                        ParseIdentOrConst();
                        Expect(']');
                        _interpreter.AccessArray();
                    }
                    ch = Peek();
                }
            }
            else if (ch == '-')
            {
                _pos++;
                ParseIdentOrConst();
                _interpreter.Negate();
            }
            else if (ch == '.' || ch == '+' || CharUtil.IsDigit((char)ch))
            {
                ParseConst();
            }
            else if (ch == '(')
            {
                _pos++;
                ParseAddSub();
                Expect(')');
            }
        }

        private void ParseCall(string name)
        {
            int count = 1;
            ParseAddSub();
            for (; ; )
            {
                int ch = Peek();
                if (ch == ')')
                {
                    _pos++;
                    _interpreter.CallFunction(name, count);
                    return;
                }
                if (ch == ',')
                {
                    _pos++;
                    count++;
                    ParseAddSub();
                }
                else
                {
                    Unexpected(ch);
                }
            }
        }

        private void ParseConst()
        {
            int len = _str.Length;
            int start = _pos;
            Number n;
            switch (_str[_pos])
            {
                case '+':
                    // skip
                    start = ++_pos;
                    break;
                case '0':
                    if (_pos + 1 < len && _str[_pos + 1] == 'x')
                    {
                        _pos += 2;
                        ParseHexInt();
                        return;
                    }
                    break;
            }
            while (_pos < len && CharUtil.IsDigit(_str[_pos]))
            {
                _pos++;
            }
            if (_pos < len && _str[_pos] == '.')
            {
                _pos++;
                while (_pos < len && CharUtil.IsDigit(_str[_pos]))
                {
                    _pos++;
                }
                if (_pos - start <= 1)
                {
                    Unexpected(-1);
                }
                n = new Number(float.Parse(_str.Substring(start, _pos - start)));
            }
            else
            {
                n = new Number(int.Parse(_str.Substring(start, _pos - start)));
            }
            _interpreter.LoadConst(n);
        }

        private void ParseHexInt()
        {
            int len = _str.Length;
            int start = _pos;
            while (_pos < len && "0123456789abcdefABCDEF".IndexOf(_str[_pos]) >= 0)
            {
                _pos++;
            }
            if (_pos - start > 8)
            {
                throw new ParseException("Number to large at " + _pos, _pos);
            }
            if (_pos == start)
            {
                Unexpected((_pos < len) ? _str[_pos] : -1);
            }
            _interpreter.LoadConst(new Number(long.Parse(_str.Substring(start, _pos - start), System.Globalization.NumberStyles.HexNumber)));
        }

        private bool SkipSpaces()
        {
            for (; ; )
            {
                if (_pos == _str.Length)
                {
                    return false;
                }
                if (!CharUtil.IsWhitespace(_str[_pos]))
                {
                    return true;
                }
                _pos++;
            }
        }

        private int Peek()
        {
            if (SkipSpaces())
            {
                return _str[_pos];
            }
            return -1;
        }

        private string ParseIdent()
        {
            if (_str[_pos] == '\'')
            {
                int istart = ++_pos;
                _pos = TextUtil.IndexOf(_str, '\'', _pos);
                string ident = _str.Substring(istart, _pos - istart);
                Expect('\'');
                return ident;
            }
            int start = _pos;
            while (_pos < _str.Length && CharUtil.IsCSharpIdentifier(_str[_pos]))
            {
                _pos++;
            }
            if (_pos > _str.Length)
            {
                _pos = _str.Length;
            }
            return _str.Substring(start, _pos - start);
        }

        private void Expect(int what)
        {
            int ch = Peek();
            if (ch != what)
            {
                Unexpected(ch);
            }
            else
            {
                _pos++;
            }
        }

        private void Unexpected(int ch)
        {
            if (ch < 0)
            {
                throw new ParseException("Unexpected end of string", _pos);
            }
            throw new ParseException("Unexpected character '" + (char)ch + "' at " + _pos, _pos);
        }
    }
}
