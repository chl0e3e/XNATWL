using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class ParameterStringParser
    {
        private string str;
        private char parameterSeparator;
        private char keyValueSeparator;

        private bool trim;
        private int pos;
        private string key;
        private string value;

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
            this.str = str;
            this.parameterSeparator = parameterSeparator;
            this.keyValueSeparator = keyValueSeparator;
        }

        public bool isTrim()
        {
            return trim;
        }

        /**
         * Enables trimming of white spaces on key and values
         * @param trim true if white spaces should be trimmed
         * @see Character#isWhitespace(char)
         */
        public void setTrim(bool trim)
        {
            this.trim = trim;
        }

        /**
         * Extract the next key-value pair
         * @return true if a pair was extracted false if the end of the string was reached.
         */
        public bool next()
        {
            while (pos < str.Length)
            {
                int kvPairEnd = TextUtil.indexOf(str, parameterSeparator, pos);
                int keyEnd = TextUtil.indexOf(str, keyValueSeparator, pos);
                if (keyEnd < kvPairEnd)
                {
                    key = substring(pos, keyEnd);
                    value = substring(keyEnd + 1, kvPairEnd);
                    pos = kvPairEnd + 1;
                    return true;
                }
                pos = kvPairEnd + 1;
            }
            key = null;
            value = null;
            return false;
        }

        /**
         * Returns the current key
         * @return the current key
         * @see #next()
         */
        public string getKey()
        {
            if (key == null)
            {
                throw new InvalidOperationException("no key-value pair available");
            }

            return key;
        }

        /**
         * Returns the current value
         * @return the current value
         * @see #next()
         */
        public string getValue()
        {
            if (value == null)
            {
                throw new InvalidOperationException("no key-value pair available");
            }

            return value;
        }

        private string substring(int start, int end)
        {
            if (trim)
            {
                return TextUtil.trim(str, start, end);
            }
            return str.Substring(start, end);
        }
    }
}
