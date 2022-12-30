using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class TextUtil
    {
        public static int MAX_ROMAN_INTEGER = 39999;
        private static String ROMAN_NUMBERS = "ↂMↂↁMↁMCMDCDCXCLXLXIXVIVI";
        private static String ROMAN_VALUES = "\u2710\u2328\u1388\u0FA0\u03E8\u0384\u01F4\u0190\144\132\62\50\12\11\5\4\1";

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


        /**
         * Searches for a specific character.
         * @param cs the string to search in
         * @param ch the character to search
         * @param start the start index. must be &gt;= 0.
         * @return the index of the character or cs.length().
         */
        public static int indexOf(string cs, char ch, int start)
        {
            return indexOf(cs, ch, start, cs.Length);
        }

        /**
         * Searches for a specific character.
         * @param cs the string to search in
         * @param ch the character to search
         * @param start the start index. must be &gt;= 0.
         * @param end the end index. must be &gt;= start and &lt;= cs.length.
         * @return the index of the character or end.
         */
        public static int indexOf(string cs, char ch, int start, int end)
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

        public static int skipSpaces(string s, int start)
        {
            return skipSpaces(s, start, s.Length);
        }

        public static int skipSpaces(string s, int start, int end)
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
        public static String trim(string s, int start)
        {
            return trim(s, start, s.Length);
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
        public static String trim(string s, int start, int end)
        {
            start = skipSpaces(s, start, end);
            while (end > start && s[end - 1] == ' ')
            {
                end--;
            }
            return s.Substring(start, end - start);
        }
    }
}
