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
    public class ParameterStringParser
    {
        private string _str;
        private char _parameterSeparator;
        private char _keyValueSeparator;

        private bool _trim;
        private int _pos;
        private string _key;
        private string _value;

        /**
         * Creates a new parser object.
         * 
         * @param str the string to parse
         * @param parameterSeparator the character which separates key-value pairs from each other
         * @param keyValueSeparator the character which separates key and value from each other
         */
        public ParameterStringParser(string str, char parameterSeparator, char keyValueSeparator)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (parameterSeparator == keyValueSeparator)
            {
                throw new ArgumentOutOfRangeException("parameterSeperator == keyValueSeperator");
            }
            this._str = str;
            this._parameterSeparator = parameterSeparator;
            this._keyValueSeparator = keyValueSeparator;
        }

        public bool IsTrim()
        {
            return _trim;
        }

        /**
         * Enables trimming of white spaces on key and values
         * @param trim true if white spaces should be trimmed
         * @see Character#isWhitespace(char)
         */
        public void SetTrim(bool trim)
        {
            this._trim = trim;
        }

        /**
         * Extract the next key-value pair
         * @return true if a pair was extracted false if the end of the string was reached.
         */
        public bool Next()
        {
            while (_pos < _str.Length)
            {
                int kvPairEnd = TextUtil.IndexOf(_str, _parameterSeparator, _pos);
                int keyEnd = TextUtil.IndexOf(_str, _keyValueSeparator, _pos);
                if (keyEnd < kvPairEnd)
                {
                    _key = Substring(_pos, keyEnd);
                    _value = Substring(keyEnd + 1, kvPairEnd);
                    _pos = kvPairEnd + 1;
                    return true;
                }
                _pos = kvPairEnd + 1;
            }
            _key = null;
            _value = null;
            return false;
        }

        /**
         * Returns the current key
         * @return the current key
         * @see #next()
         */
        public string GetKey()
        {
            if (_key == null)
            {
                throw new InvalidOperationException("no key-value pair available");
            }

            return _key;
        }

        /**
         * Returns the current value
         * @return the current value
         * @see #next()
         */
        public string GetValue()
        {
            if (_value == null)
            {
                throw new InvalidOperationException("no key-value pair available");
            }

            return _value;
        }

        private string Substring(int start, int end)
        {
            if (_trim)
            {
                return TextUtil.Trim(_str, start, end);
            }
            return _str.Substring(start, end);
        }
    }
}
