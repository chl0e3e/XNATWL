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
using System.Collections.Generic;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    public class ParserUtil
    {

        private ParserUtil()
        {
        }

        public static void checkNameNotEmpty(String name, XMLParser xmlp)
        {
            if (name == null)
            {
                throw xmlp.error("missing 'name' on '" + xmlp.getName() + "'");
            }
            if (name.Length == 0)
            {
                throw xmlp.error("empty name not allowed");
            }
            if ("none".Equals(name))
            {
                throw xmlp.error("can't use reserved name \"none\"");
            }
            if (name.IndexOf('*') >= 0)
            {
                throw xmlp.error("'*' is not allowed in names");
            }
            if (name.IndexOf('/') >= 0)
            {
                throw xmlp.error("'/' is not allowed in names");
            }
        }

        public static Border parseBorderFromAttribute(XMLParser xmlp, String attribute)
        {
            String value = xmlp.getAttributeValue(null, attribute);
            if (value == null)
            {
                return null;
            }
            return parseBorder(xmlp, value);
        }

        public static Border parseBorder(XMLParser xmlp, String value)
        {
            try
            {
                int[] values = TextUtil.parseIntArray(value);
                switch (values.Length)
                {
                    case 1:
                        return new Border(values[0]);
                    case 2:
                        return new Border(values[0], values[1]);
                    case 4:
                        return new Border(values[0], values[1], values[2], values[3]);
                    default:
                        throw xmlp.error("Unsupported border format");
                }
            }
            catch (FormatException ex)
            {
                throw xmlp.error("Unable to parse border size", ex);
            }
        }

        public static Color parseColorFromAttribute(XMLParser xmlp, String attribute, ParameterMapImpl constants, Color defaultColor)
        {
            String value = xmlp.getAttributeValue(null, attribute);
            if (value == null)
            {
                return defaultColor;
            }
            return parseColor(xmlp, value, constants);
        }

        public static Color parseColor(XMLParser xmlp, String value, ParameterMapImpl constants)
        {
            try
            {
                Color color = Color.Parse(value);
                if (color == null && constants != null)
                {
                    color = (Color) constants.getParameterValue(value, false, typeof(Color));
                }
                if (color == null)
                {
                    throw xmlp.error("Unknown color name: " + value);
                }
                return color;
            }
            catch (FormatException ex)
            {
                throw xmlp.error("unable to parse color code", ex);
            }
        }

        public static String appendDot(String name)
        {
            int len = name.Length;
            if (len > 0 && name[len - 1] != '.')
            {
                name = name + ".";
            }
            return name;
        }

        public static int[] parseIntArrayFromAttribute(XMLParser xmlp, String attribute)
        {
            try
            {
                String value = xmlp.getAttributeNotNull(attribute);
                return TextUtil.parseIntArray(value);
            }
            catch (FormatException ex)
            {
                throw xmlp.error("Unable to parse", ex);
            }
        }

        public static int parseIntExpressionFromAttribute(XMLParser xmlp, String attribute, int defaultValue, AbstractMathInterpreter interpreter)
        {
            try
            {
                String value = xmlp.getAttributeValue(null, attribute);
                if (value == null)
                {
                    return defaultValue;
                }
                if (TextUtil.isInteger(value))
                {
                    return Int32.Parse(value);
                }
                Number n = interpreter.execute(value);
                if (n.IsRational()) {
                    if (n.intValue() != n.doubleValue())
                    {
                        throw xmlp.error("Not an integer");
                    }
                }
                return n.intValue();
            }
            catch (FormatException ex)
            {
                throw xmlp.error("Unable to parse", ex);
            }
            catch (ParseException ex)
            {
                throw xmlp.error("Unable to parse", ex);
            }
        }

        public static SortedDictionary<String, V> find<V>(SortedDictionary<String, V> map, String baseName)
        {
            SortedDictionary<string, V> subMap = new SortedDictionary<string, V>();
            bool adding = false;
            foreach(string key in map.Keys)
            {
                if (key.StartsWith(baseName))
                {
                    adding = true;
                    subMap[key] = map[key];
                }
            }
            return subMap;
        }

        public static Dictionary<String, V> resolve<V>(SortedDictionary<String, V> map, String reference, String name, V mapToNull)
        {
            name = ParserUtil.appendDot(name);
            int refLen = reference.Length - 1;
            reference = reference.Substring(0, refLen);

            SortedDictionary<String, V> matched = find(map, reference);
            if (matched.Count == 0)
            {
                return new Dictionary<string, V>(matched);
            }

            Dictionary<String, V> result = new Dictionary<String, V>();
            foreach (string texEntryKey in matched.Keys)
            {
                if (!texEntryKey.StartsWith(reference))
                {
                    throw new Exception("Assertion failed");
                }
                object value = matched[texEntryKey];
                if (value.Equals(mapToNull))
                {
                    value = null;
                }
                var key = name + texEntryKey.Substring(refLen);
                result.Add(key, (V)value);
            }

            return result;
        }

        public static StateExpression parseCondition(XMLParser xmlp)
        {
            String expression = xmlp.getAttributeValue(null, "if");
            bool negate = expression == null;
            if (expression == null)
            {
                expression = xmlp.getAttributeValue(null, "unless");
            }
            if (expression != null)
            {
                try
                {
                    return StateExpression.Parse(expression, negate);
                }
                catch (ParseException ex)
                {
                    throw xmlp.error("Unable to parse condition", ex);
                }
            }
            return null;
        }

    }
}
