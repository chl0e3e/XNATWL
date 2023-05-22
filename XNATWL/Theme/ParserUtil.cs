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
    /// <summary>
    /// Utility for parsing various values from theme XML
    /// </summary>
    public class ParserUtil
    {
        /// <summary>
        /// Check name is not empty, invalid or uses a wildcard selector illegally
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="xmlp"><see cref="XMLParser"/> to throw error on</param>
        public static void CheckNameNotEmpty(String name, XMLParser xmlp)
        {
            if (name == null)
            {
                throw xmlp.Error("missing 'name' on '" + xmlp.GetName() + "'");
            }
            if (name.Length == 0)
            {
                throw xmlp.Error("empty name not allowed");
            }
            if ("none".Equals(name))
            {
                throw xmlp.Error("can't use reserved name \"none\"");
            }
            if (name.IndexOf('*') >= 0)
            {
                throw xmlp.Error("'*' is not allowed in names");
            }
            if (name.IndexOf('/') >= 0)
            {
                throw xmlp.Error("'/' is not allowed in names");
            }
        }

        /// <summary>
        /// Parse <see cref="Border"/> from <see cref="XMLParser"/> with a look up using <paramref name="attribute"/> parameter
        /// </summary>
        /// <param name="xmlp"><see cref="XMLParser"/></param>
        /// <param name="attribute">attribute name</param>
        /// <returns><see cref="Border"/> object</returns>
        public static Border ParseBorderFromAttribute(XMLParser xmlp, string attribute)
        {
            string value = xmlp.GetAttributeValue(null, attribute);
            if (value == null)
            {
                return null;
            }
            return ParseBorder(xmlp, value);
        }

        /// <summary>
        /// Parse <see cref="Border"/> from comma delimited int array
        /// </summary>
        /// <param name="xmlp"><see cref="XMLParser"/></param>
        /// <param name="value">values as comma delimited int array</param>
        /// <returns><see cref="Border"/> object</returns>
        public static Border ParseBorder(XMLParser xmlp, String value)
        {
            try
            {
                int[] values = TextUtil.ParseIntArray(value);
                switch (values.Length)
                {
                    case 1:
                        return new Border(values[0]);
                    case 2:
                        return new Border(values[0], values[1]);
                    case 4:
                        return new Border(values[0], values[1], values[2], values[3]);
                    default:
                        throw xmlp.Error("Unsupported border format");
                }
            }
            catch (FormatException ex)
            {
                throw xmlp.Error("Unable to parse border size", ex);
            }
        }

        /// <summary>
        /// Parse XML attribute using <see cref="Color.Parse(string)"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="attribute">Attribute to read from in XML</param>
        /// <param name="constants">Constants dictionary to look up</param>
        /// <param name="defaultColor">If colour not found, use this</param>
        /// <returns><see cref="Color"/> object for drawing</returns>
        public static Color ParseColorFromAttribute(XMLParser xmlp, String attribute, ParameterMapImpl constants, Color defaultColor)
        {
            String value = xmlp.GetAttributeValue(null, attribute);
            if (value == null)
            {
                return defaultColor;
            }
            return ParseColor(xmlp, value, constants);
        }

        /// <summary>
        /// Parse colour string using <see cref="Color.Parse(string)"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="value">colour string</param>
        /// <param name="constants">named colour constants map</param>
        /// <returns><see cref="Color"/> object for drawing</returns>
        public static Color ParseColor(XMLParser xmlp, String value, ParameterMapImpl constants)
        {
            try
            {
                Color color = Color.Parse(value);
                if (color == null && constants != null)
                {
                    color = (Color) constants.GetParameterValue(value, false, typeof(Color));
                }
                if (color == null)
                {
                    throw xmlp.Error("Unknown color name: " + value);
                }
                return color;
            }
            catch (FormatException ex)
            {
                throw xmlp.Error("unable to parse color code", ex);
            }
        }

        /// <summary>
        /// Append dot to the string if it doesn't end with one
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns>string suffixed with dot</returns>
        public static String AppendDot(String name)
        {
            int len = name.Length;
            if (len > 0 && name[len - 1] != '.')
            {
                name = name + ".";
            }
            return name;
        }
        
        /// <summary>
        /// Parse int array from an XML attribute using <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="attribute">attribute name</param>
        /// <returns>int array</returns>
        public static int[] ParseIntArrayFromAttribute(XMLParser xmlp, String attribute)
        {
            try
            {
                String value = xmlp.GetAttributeNotNull(attribute);
                return TextUtil.ParseIntArray(value);
            }
            catch (FormatException ex)
            {
                throw xmlp.Error("Unable to parse", ex);
            }
        }

        /// <summary>
        /// Parse int expression from an attribute pointed to by <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="attribute">attribute name</param>
        /// <param name="defaultValue">The default value returned when attribute value could not be found</param>
        /// <param name="interpreter">Math interpreter</param>
        /// <returns>integer</returns>
        public static int ParseIntExpressionFromAttribute(XMLParser xmlp, String attribute, int defaultValue, AbstractMathInterpreter interpreter)
        {
            try
            {
                String value = xmlp.GetAttributeValue(null, attribute);
                if (value == null)
                {
                    return defaultValue;
                }
                if (TextUtil.IsInteger(value))
                {
                    return Int32.Parse(value);
                }
                Number n = interpreter.Execute(value);
                if (n.IsRational()) {
                    if (n.IntValue() != n.DoubleValue())
                    {
                        throw xmlp.Error("Not an integer");
                    }
                }
                return n.IntValue();
            }
            catch (FormatException ex)
            {
                throw xmlp.Error("Unable to parse", ex);
            }
            catch (ParseException ex)
            {
                throw xmlp.Error("Unable to parse", ex);
            }
        }

        /// <summary>
        /// Searches a sorted dictionary for <paramref name="baseName"/>
        /// </summary>
        /// <typeparam name="V">dictionary's value object type</typeparam>
        /// <param name="map">sorted dictionary to search</param>
        /// <param name="baseName">prefix to match</param>
        /// <returns>Matching entries</returns>
        public static SortedDictionary<String, V> Find<V>(SortedDictionary<String, V> map, String baseName)
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

        /// <summary>
        /// Resolving a sub-dictionary given a parameter name's prefix and what to map null objects to
        /// </summary>
        /// <typeparam name="V">dictionary's value object type</typeparam>
        /// <param name="map">sorted dictionary to search</param>
        /// <param name="reference">prefix to match</param>
        /// <param name="name">name to match</param>
        /// <param name="mapToNull">null object value</param>
        /// <returns>resolved dictionary</returns>
        /// <exception cref="Exception">reference does not match expected <see cref="Find{V}(SortedDictionary{string, V}, string)"/> output</exception>
        public static Dictionary<String, V> Resolve<V>(SortedDictionary<String, V> map, String reference, String name, V mapToNull)
        {
            name = ParserUtil.AppendDot(name);
            int refLen = reference.Length - 1;
            reference = reference.Substring(0, refLen);

            SortedDictionary<String, V> matched = Find(map, reference);
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

        /// <summary>
        /// Parse <see cref="StateExpression"/> from XML parser
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <returns>conditions</returns>
        public static StateExpression ParseCondition(XMLParser xmlp)
        {
            String expression = xmlp.GetAttributeValue(null, "if");
            bool negate = expression == null;
            if (expression == null)
            {
                expression = xmlp.GetAttributeValue(null, "unless");
            }
            if (expression != null)
            {
                try
                {
                    return StateExpression.Parse(expression, negate);
                }
                catch (ParseException ex)
                {
                    throw xmlp.Error("Unable to parse condition", ex);
                }
            }
            return null;
        }

    }
}
