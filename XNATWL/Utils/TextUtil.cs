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
using System.Text;

namespace XNATWL.Utils
{
    public class TextUtil
    {
        public static int MAX_ROMAN_INTEGER = 39999;
        private static String ROMAN_NUMBERS = "ↂMↂↁMↁMCMDCDCXCLXLXIXVIVI";
        private static String ROMAN_VALUES = "\u2710\u2328\u1388\u0FA0\u03E8\u0384\u01F4\u0190\x64\x5A\x32\x28\x0A\x09\x05\x04\x01";

        public static string ToCharListNumber(int value, string list)
        {
            if(value < 1)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            int pos = 16;
            char[] tmp = new char[pos];

            do {
                tmp[--pos] = list[--value % list.Length];
                value /= list.Length;
            }while(value > 0);

            return new String(tmp, pos, tmp.Length - pos);
        }

        
        public static String ToRomanNumberString(int value)
        {
            if(value < 1 || value > MAX_ROMAN_INTEGER) {
                throw new ArgumentOutOfRangeException();
            }
            StringBuilder sb = new StringBuilder();
            int idxValues = 0;
            int idxNumbers = 0;
            do {
                int romanValue = ROMAN_VALUES[idxValues];
                int romanNumberLen = (idxValues & 1) + 1;
                while(value >= romanValue) {
                    sb.Append(ROMAN_NUMBERS, idxNumbers, romanNumberLen);
                    value -= romanValue;
                }
                idxNumbers += romanNumberLen;
            } while (++idxValues < 17);

            return sb.ToString();
        }

        public static String NotNull(String str)
        {
            if (str == null)
            {
                return "";
            }
            return str;
        }

        public static int CountNumLines(string str)
        {
            int n = str.Length;
            int count = 0;
            if (n > 0)
            {
                count++;
                for (int i = 0; i < n; i++)
                {
                    if (str[i] == '\n')
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /**
         * Searches for a specific character.
         * @param cs the string to search in
         * @param ch the character to search
         * @param start the start index. must be &gt;= 0.
         * @return the index of the character or cs.length().
         */
        public static int IndexOf(string cs, char ch, int start)
        {
            return IndexOf(cs, ch, start, cs.Length);
        }

        /**
         * Searches for a specific character.
         * @param cs the string to search in
         * @param ch the character to search
         * @param start the start index. must be &gt;= 0.
         * @param end the end index. must be &gt;= start and &lt;= cs.length.
         * @return the index of the character or end.
         */
        public static int IndexOf(string cs, char ch, int start, int end)
        {
            for (; start < end; start++)
            {
                if (cs[start] == ch)
                {
                    return start;
                }
            }
            return end;
        }

        public static int SkipSpaces(string s, int start)
        {
            return SkipSpaces(s, start, s.Length);
        }

        public static int SkipSpaces(string s, int start, int end)
        {
            while (start < end && s[start] == ' ')
            {
                start++;
            }
            return start;
        }

        /**
         * Returns a whitespace trimmed substring.
         * 
         * This method is mostly equivant to
         * <pre>{@code s.subSequence(start).toString().trim() }</pre>
         * 
         * @param s the sequence
         * @param start the start index (inclusive)
         * @return the sub string without leading or trailing whitespace
         * @see Character#isWhitespace(char) 
         */
        public static String Trim(string s, int start)
        {
            return Trim(s, start, s.Length);
        }

        public static int CountElements(String str)
        {
            int count = 0;
            for (int pos = 0; pos < str.Length;)
            {
                count++;
                pos = IndexOf(str, ',', pos) + 1;
            }
            return count;
        }

        public static string ToPrintableString(char ch)
        {
            if (Char.IsControl(ch))
            {
                return "\\x" + Convert.ToByte(ch).ToString("x2"); // https://stackoverflow.com/a/12527716
            }
            else
            {
                return Char.ToString(ch);
            }
        }

        public static int[] ParseIntArray(String str)
        {
            int count = CountElements(str);
            int[] result = new int[count];
            int i = 0;
            foreach(string numStr in str.Split(','))
            {
                result[i++] = int.Parse(numStr);
            }
            /*int[] result = new int[count];
            for (int idx = 0, pos = 0; idx < count; idx++)
            {
                int comma = indexOf(str, ',', pos);
                System.Diagnostics.Debug.WriteLine("Parsing intArray: " + str.Substring(pos, comma));
                result[idx] = Int32.Parse(str.Substring(pos, comma));
                pos = comma + 1;
            }*/
            return result;
        }

        public static String CreateString(char ch, int len)
        {
            char[] buf = new char[len];
            for (int i = 0; i < len; i++)
            {
                buf[i] = ch;
            }
            return new String(buf);
        }

        public static String StripNewLines(String str)
        {
            int idx = str.LastIndexOf('\n');
            if (idx < 0)
            {
                // don't waste time when no newline is present
                return str;
            }
            StringBuilder sb = new StringBuilder(str);
            do
            {
                if (sb[idx] == '\n')
                {
                    sb.Remove(idx, 1);
                }
            } while (--idx >= 0);
            return sb.ToString();
        }

        public static String LimitStringLength(String str, int length)
        {
            if (str.Length > length)
            {
                return str.Substring(0, length);
            }
            return str;
        }

        public static bool IsInteger(String str)
        {
            int idx = 0;
            int len = str.Length;
            if (len > 0 && str[0] == '-')
            {
                idx++;
            }
            if (idx == len)
            {
                return false;
            }
            do
            {
                char ch = str[idx++];
                if (ch < '0' || ch > '9')
                {
                    return false;
                }
            } while (idx < len);
            return true;
        }

        /**
         * Returns a whitespace trimmed substring.
         * 
         * This method is mostly equivant to
         * <pre>{@code s.subSequence(start, end).toString().trim() }</pre>
         * 
         * @param s the sequence
         * @param start the start index (inclusive)
         * @param end the end index (exclusive)
         * @return the sub string without leading or trailing whitespace
         * @see Character#isWhitespace(char) 
         */
        public static String Trim(string s, int start, int end)
        {
            start = SkipSpaces(s, start, end);
            while (end > start && s[end - 1] == ' ')
            {
                end--;
            }
            return s.Substring(start, end - start);
        }
    }
}
