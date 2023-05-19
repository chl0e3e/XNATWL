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
using System.Linq;
using XNATWL.Utils;

namespace XNATWL.TextAreaModel
{
    /// <summary>
    /// A <see cref="Style"/> which is constructed from a CSS style string.
    /// </summary>
    public class CSSStyle : Style
    {
        internal CSSStyle()
        {
        }

        /// <summary>
        /// Parse a CSS style from a single line of attribute:value
        /// </summary>
        /// <param name="cssStyle">CSS attribute:value string</param>
        public CSSStyle(string cssStyle)
        {
            this.ParseCSS(cssStyle);
        }

        /// <summary>
        /// Parse a CSS style from a single line of attribute:value, and specify a parent and/or <see cref="StyleSheetKey"/>
        /// </summary>
        /// <param name="cssStyle">CSS attribute:value string</param>
        public CSSStyle(Style parent, StyleSheetKey styleSheetKey, string cssStyle) : base(parent, styleSheetKey)
        {
            this.ParseCSS(cssStyle);
        }

        /// <summary>
        /// Parse a line of CSS
        /// </summary>
        /// <param name="style">Line of CSS</param>
        private void ParseCSS(string style)
        {
            ParameterStringParser psp = new ParameterStringParser(style, ';', ':');
            psp.SetTrim(true);
            while (psp.Next())
            {
                try
                {
                    this.ParseCSSAttribute(psp.GetKey(), psp.GetValue());
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    System.Diagnostics.Debug.WriteLine(typeof(CSSStyle).Name + " - Unable to parse CSS attribute: " + psp.GetKey() + "=" + psp.GetValue(), ex);
                }
            }
        }

        /// <summary>
        /// Parse a specific attribute for the current style in the stylesheet
        /// </summary>
        /// <param name="key">Attribute name</param>
        /// <param name="value">Value of attribute</param>
        /// <exception cref="ArgumentOutOfRangeException">The CSS attribute is unsupported</exception>
        internal void ParseCSSAttribute(string key, string value)
        {
            if (key.StartsWith("margin"))
            {
                this.ParseBox(key.Substring(6), value, StyleAttribute.MARGIN);
                return;
            }
            if (key.StartsWith("padding"))
            {
                this.ParseBox(key.Substring(7), value, StyleAttribute.PADDING);
                return;
            }
            if (key.StartsWith("font"))
            {
                this.ParseFont(key, value);
                return;
            }
            if ("text-indent".Equals(key))
            {
                this.ParseValueUnit(StyleAttribute.TEXT_INDENT, value);
                return;
            }
            if ("-twl-font".Equals(key))
            {
                this.Put(StyleAttribute.FONT_FAMILIES, new List<string> { value });
                return;
            }
            if ("-twl-hover".Equals(key))
            {
                this.ParseEnum(StyleAttribute.INHERIT_HOVER, INHERITHOVER, value);
                return;
            }
            if ("text-align".Equals(key))
            {
                this.ParseEnum(StyleAttribute.HORIZONTAL_ALIGNMENT, value);
                return;
            }
            if ("text-decoration".Equals(key))
            {
                this.ParseEnum(StyleAttribute.TEXT_DECORATION, TEXTDECORATION, value);
                return;
            }
            if ("vertical-align".Equals(key))
            {
                this.ParseEnum(StyleAttribute.VERTICAL_ALIGNMENT, value);
                return;
            }
            if ("white-space".Equals(key))
            {
                this.ParseEnum(StyleAttribute.PREFORMATTED, PRE, value);
                return;
            }
            if ("word-wrap".Equals(key))
            {
                this.ParseEnum(StyleAttribute.BREAKWORD, BREAKWORD, value);
                return;
            }
            if ("list-style-image".Equals(key))
            {
                this.ParseURL(StyleAttribute.LIST_STYLE_IMAGE, value);
                return;
            }
            if ("list-style-type".Equals(key))
            {
                this.ParseEnum(StyleAttribute.LIST_STYLE_TYPE, OLT, value);
                return;
            }
            if ("clear".Equals(key))
            {
                this.ParseEnum(StyleAttribute.CLEAR, value);
                return;
            }
            if ("float".Equals(key))
            {
                this.ParseEnum(StyleAttribute.FLOAT_POSITION, value);
                return;
            }
            if ("display".Equals(key))
            {
                this.ParseEnum(StyleAttribute.DISPLAY, value);
                return;
            }
            if ("width".Equals(key))
            {
                this.ParseValueUnit(StyleAttribute.WIDTH, value);
                return;
            }
            if ("height".Equals(key))
            {
                this.ParseValueUnit(StyleAttribute.HEIGHT, value);
                return;
            }
            if ("background-image".Equals(key))
            {
                this.ParseURL(StyleAttribute.BACKGROUND_IMAGE, value);
                return;
            }
            if ("background-color".Equals(key) || "-twl-background-color".Equals(key))
            {
                this.ParseColor(StyleAttribute.BACKGROUND_COLOR, value);
                return;
            }
            if ("color".Equals(key))
            {
                this.ParseColor(StyleAttribute.COLOR, value);
                return;
            }
            if ("tab-size".Equals(key) || "-moz-tab-size".Equals(key))
            {
                this.ParseInteger(StyleAttribute.TAB_SIZE, value);
                return;
            }
            throw new ArgumentOutOfRangeException("Unsupported key: " + key);
        }

        /// <summary>
        /// Parse a CSS box attribute (specifies a top, left, right or bottom)
        /// </summary>
        /// <param name="key">Inset direction</param>
        /// <param name="value">Value in pixels</param>
        /// <param name="box">containing relevant output attribute keys</param>
        /// <exception cref="ArgumentOutOfRangeException">Non-standard box units parsed</exception>
        private void ParseBox(string key, string value, BoxAttribute box)
        {
            if ("-top".Equals(key))
            {
                this.ParseValueUnit(box.Top, value);
            }
            else if ("-left".Equals(key))
            {
                this.ParseValueUnit(box.Left, value);
            }
            else if ("-right".Equals(key))
            {
                this.ParseValueUnit(box.Right, value);
            }
            else if ("-bottom".Equals(key))
            {
                this.ParseValueUnit(box.Bottom, value);
            }
            else if ("".Equals(key))
            {
                Value[] vu = ParseValueUnits(value);

                switch (vu.Length)
                {
                    case 1:
                        Put(box.Top, vu[0]);
                        Put(box.Left, vu[0]);
                        Put(box.Right, vu[0]);
                        Put(box.Bottom, vu[0]);
                        break;
                    case 2: // TB, LR
                        Put(box.Top, vu[0]);
                        Put(box.Left, vu[1]);
                        Put(box.Right, vu[1]);
                        Put(box.Bottom, vu[0]);
                        break;
                    case 3: // T, LR, B
                        Put(box.Top, vu[0]);
                        Put(box.Left, vu[1]);
                        Put(box.Right, vu[1]);
                        Put(box.Bottom, vu[2]);
                        break;
                    case 4: // T, R, B, L
                        Put(box.Top, vu[0]);
                        Put(box.Left, vu[3]);
                        Put(box.Right, vu[1]);
                        Put(box.Bottom, vu[2]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid number of margin values: " + vu.Length);
                }
            }
        }

        /// <summary>
        /// Parse a font attribute to render with
        /// </summary>
        /// <param name="key">Font attribute name</param>
        /// <param name="value">Font attribute value</param>
        private void ParseFont(string key, string value)
        {
            if ("font-family".Equals(key))
            {
                this.ParseList(StyleAttribute.FONT_FAMILIES, value);
                return;
            }
            if ("font-weight".Equals(key))
            {
                Int32 weight;
                if (!WEIGHTS.ContainsKey(value))
                {
                    weight = Int32.Parse(value);
                }
                else
                {
                    weight = WEIGHTS[value];
                }

                this.Put(StyleAttribute.FONT_WEIGHT, weight);
                return;
            }
            if ("font-size".Equals(key))
            {
                this.ParseValueUnit(StyleAttribute.FONT_SIZE, value);
                return;
            }
            if ("font-style".Equals(key))
            {
                this.ParseEnum(StyleAttribute.FONT_ITALIC, ITALIC, value);
                return;
            }
            if ("font".Equals(key))
            {
                value = this.ParseStartsWith(StyleAttribute.FONT_WEIGHT, WEIGHTS, value);
                value = this.ParseStartsWith(StyleAttribute.FONT_ITALIC, ITALIC, value);
                if (value.Length > 0 && "1234567890".Contains(value[0]))
                {
                    int end = TextUtil.IndexOf(value, ' ', 0);
                    this.ParseValueUnit(StyleAttribute.FONT_SIZE, value.Substring(0, end));
                    end = TextUtil.SkipSpaces(value, end);
                    value = value.Substring(end);
                }
                this.ParseList(StyleAttribute.FONT_FAMILIES, value);
            }
        }

        /// <summary>
        /// Parse a value unit using one of many measurement units
        /// </summary>
        /// <param name="value">Value to parse</param>
        /// <returns>Value using a unit-respecting object</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Value ParseValueUnit(string value)
        {
            Value.Unit unit;
            int suffixLength = 2;
            if (value.EndsWith("px"))
            {
                unit = Value.Unit.PX;
            }
            else if (value.EndsWith("pt"))
            {
                unit = Value.Unit.PT;
            }
            else if (value.EndsWith("em"))
            {
                unit = Value.Unit.EM;
            }
            else if (value.EndsWith("ex"))
            {
                unit = Value.Unit.EX;
            }
            else if (value.EndsWith("%"))
            {
                suffixLength = 1;
                unit = Value.Unit.PERCENT;
            }
            else if ("0".Equals(value))
            {
                return Value.ZERO_PX;
            }
            else if ("auto".Equals(value))
            {
                return Value.AUTO;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unknown numeric suffix: " + value);
            }

            string numberPart = TextUtil.Trim(value, 0, value.Length - suffixLength);
            return new Value(float.Parse(numberPart), unit);
        }

        /// <summary>
        /// Parse a string of value units
        /// </summary>
        /// <param name="value">Value(s) to parse</param>
        /// <returns>Array of values using a unit-respecting object</returns>
        private Value[] ParseValueUnits(string value)
        {
            string[] parts = value.Split(new string[] { "\\s+" }, StringSplitOptions.None);
            Value[] result = new Value[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = this.ParseValueUnit(parts[i]);
            }
            return result;
        }

        /// <summary>
        /// Parse a value unit and set it for a <see cref="StyleAttribute"/>
        /// </summary>
        /// <param name="attribute">Target attribute</param>
        /// <param name="value">Value to parse</param>
        private void ParseValueUnit(StyleAttribute attribute, string value)
        {
            this.Put(attribute, ParseValueUnit(value));
        }

        /// <summary>
        /// Parse an integer into a <see cref="StyleAttribute"/>
        /// </summary>
        /// <param name="attribute">Target attribute</param>
        /// <param name="value">Integer value to parse as a string</param>
        private void ParseInteger(StyleAttribute<Int32> attribute, string value)
        {
            if ("inherit".Equals(value))
            {
                this.Put(attribute, null);
            }
            else
            {
                int intval = Int32.Parse(value);
                this.Put(attribute, intval);
            }
        }

        /// <summary>
        /// Parse a lookup map for <see cref="StyleAttribute"/>
        /// </summary>
        /// <typeparam name="T">attribute type</typeparam>
        /// <param name="attribute">target attribute</param>
        /// <param name="map">value map</param>
        /// <param name="value">value to look up</param>
        /// <exception cref="ArgumentOutOfRangeException">enum value not in map</exception>
        private void ParseEnum<T>(StyleAttribute<T> attribute, Dictionary<string, T> map, string value)
        {
            if (!map.ContainsKey(value))
            {
                throw new ArgumentOutOfRangeException("Unknown value: " + value);
            }

            T obj = map[value];
            this.Put(attribute, obj);
        }

        /// <summary>
        /// Parse an enum value into a <see cref="StyleAttribute"/>
        /// </summary>
        /// <typeparam name="E">Enum type</typeparam>
        /// <param name="attribute">target attribute</param>
        /// <param name="value">enum value</param>
        private void ParseEnum<E>(StyleAttribute<E> attribute, string value) where E : struct, IConvertible
        {
            object obj = Enum.Parse(attribute.DataType, value.ToUpper());
            Put(attribute, obj);
        }

        /// <summary>
        /// Parse the first word of a string and lookup using a map and return the rest
        /// </summary>
        /// <typeparam name="E">Generic attribute type target</typeparam>
        /// <param name="attribute">Target attribute</param>
        /// <param name="map">Lookup map</param>
        /// <param name="value">Value to look up</param>
        /// <returns>The rest of the words in the string</returns>
        private string ParseStartsWith<E>(StyleAttribute<E> attribute, Dictionary<string, E> map, string value)
        {
            int end = TextUtil.IndexOf(value, ' ', 0);
            E obj = map[value.Substring(0, end)];
            if (obj != null)
            {
                end = TextUtil.SkipSpaces(value, end);
                value = value.Substring(end);
            }
            Put(attribute, obj);
            return value;
        }

        /// <summary>
        /// Parse a URL stripping any CSS 'function' conveniences
        /// </summary>
        /// <param name="attribute">Target attribute</param>
        /// <param name="value">URL string</param>
        private void ParseURL(StyleAttribute<string> attribute, string value)
        {
            Put(attribute, StripURL(value));
        }

        /// <summary>
        /// Trim a substring of a string
        /// </summary>
        /// <param name="value">specified string</param>
        /// <param name="start">substring start</param>
        /// <param name="end">substring end</param>
        /// <returns>string containing the trimmed substring</returns>
        static string StripTrim(string value, int start, int end)
        {
            return TextUtil.Trim(value, start, value.Length - end);
        }


        /// <summary>
        /// Strip a URL of any CSS 'function' conveniences
        /// </summary>
        /// <param name="value">URL string</param>
        public static string StripURL(string value)
        {
            if (value.StartsWith("url(") && value.EndsWith(")"))
            {
                value = StripQuotes(StripTrim(value, 4, 1));
            }
            return value;
        }

        /// <summary>
        /// Strip surrounding quotes, if any (only index 0 and index count - 1)
        /// </summary>
        /// <param name="value">string with maybe something to strip</param>
        /// <returns>Stripped text if the quotes were there</returns>
        static string StripQuotes(string value)
        {
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 1);
            }
            return value;
        }

        /// <summary>
        /// Parse RGBA colour to a <see cref="Color"/> object
        /// </summary>
        /// <param name="attribute">Target attribute</param>
        /// <param name="value">Color value</param>
        /// <exception cref="ArgumentOutOfRangeException">Unable to parse CSS color</exception>
        private void ParseColor(StyleAttribute<Color> attribute, string value)
        {
            Color color;

            if (value.StartsWith("rgb(") && value.EndsWith(")"))
            {
                value = StripTrim(value, 4, 1);
                byte[] rgb = ParseRGBA(value, 3);
                color = new Color(rgb[0], rgb[1], rgb[2], (byte)255);
            }
            else if (value.StartsWith("rgba(") && value.EndsWith(")"))
            {
                value = StripTrim(value, 5, 1);
                byte[] rgba = ParseRGBA(value, 4);
                color = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
            }
            else
            {
                color = Color.Parse(value);
                if (color == null)
                {
                    throw new ArgumentOutOfRangeException("unknown color name: " + value);
                }
            }

            Put(attribute, color);
        }

        /// <summary>
        /// Parse RGBA as byte array containing <paramref name="numElements"/> elements
        /// </summary>
        /// <param name="value">RGBA string</param>
        /// <param name="numElements">Number of components</param>
        /// <returns>Byte array of colors</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified numElements does not match the possible components of a colour's string representation</exception>
        private byte[] ParseRGBA(string value, int numElements)
        {
            string[] parts = value.Split(',');
            if (parts.Length != numElements)
            {
                throw new ArgumentOutOfRangeException("3 values required for rgb()");
            }
            byte[] rgba = new byte[numElements];
            for (int i = 0; i < numElements; i++)
            {
                string part = parts[i].Trim();
                int v;
                if (i == 3)
                {
                    // handle alpha component specially
                    float f = float.Parse(part);
                    v = (int)Math.Round(f * 255.0f);
                }
                else
                {
                    bool percent = part.EndsWith("%");
                    if (percent)
                    {
                        part = StripTrim(value, 0, 1);
                    }
                    v = Int32.Parse(part);
                    if (percent)
                    {
                        v = 255 * v / 100;
                    }
                }
                rgba[i] = (byte)Math.Max(0, Math.Min(255, v));
            }
            return rgba;
        }

        /// <summary>
        /// Parse a list from a string into a <see cref="StyleAttribute{List{string}}"/> starting at index 0
        /// </summary>
        /// <param name="attribute">Target attribute</param>
        /// <param name="value">CSS list value</param>
        private void ParseList(StyleAttribute<List<string>> attribute, string value)
        {
            Put(attribute, ParseList(value, 0));
        }

        /// <summary>
        /// Parses a CSS string list (comma-delimited) from a given string and its starting index
        /// </summary>
        /// <param name="value"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static List<String> ParseList(string value, int idx)
        {
            idx = TextUtil.SkipSpaces(value, idx);
            if (idx >= value.Length)
            {
                return null;
            }

            char startChar = value[idx];
            int end;
            string part;

            if (startChar == '"' || startChar == '\'')
            {
                ++idx;
                end = TextUtil.IndexOf(value, startChar, idx);
                part = value.Substring(idx, end);
                end = TextUtil.SkipSpaces(value, ++end);
                if (end < value.Length && value[end] != ',')
                {
                    throw new ArgumentOutOfRangeException("',' expected at " + idx);
                }
            }
            else
            {
                end = TextUtil.IndexOf(value, ',', idx);
                part = TextUtil.Trim(value, idx, end);
            }

            List<string> result = ParseList(value, end + 1);
            if (result == null)
            {
                return new List<string> { part };
            }
            else
            {
                return new List<string> { part, result[0] };
            }
        }

        static Dictionary<string, Boolean> PRE = new Dictionary<string, Boolean>();
        static Dictionary<string, Boolean> BREAKWORD = new Dictionary<string, Boolean>();
        static Dictionary<string, OrderedListType> OLT = new Dictionary<string, OrderedListType>();
        static Dictionary<string, Boolean> ITALIC = new Dictionary<string, Boolean>();
        static Dictionary<string, Int32> WEIGHTS = new Dictionary<string, Int32>();
        static Dictionary<string, TextDecoration> TEXTDECORATION = new Dictionary<string, TextDecoration>();
        static Dictionary<string, Boolean> INHERITHOVER = new Dictionary<string, Boolean>();

        /// <summary>
        /// Create a Roman numeral list type specifying if it is lowercase or uppercase
        /// </summary>
        /// <param name="lowercase">Lowercase roman numbers</param>
        /// <returns><see cref="OrderedListType"/></returns>
        static OrderedListType CreateRoman(bool lowercase)
        {
            return new RomanOrderedListType(lowercase);
        }

        /// <summary>
        /// List type using Roman numerals
        /// </summary>
        class RomanOrderedListType : OrderedListType
        {
            private bool _lowercase;
            public RomanOrderedListType(bool lowercase)
            {
                this._lowercase = lowercase;
            }

            /// <summary>
            /// Format integer using Roman numerals
            /// </summary>
            /// <param name="nr"></param>
            /// <returns></returns>
            public override string Format(int nr)
            {
                if (nr >= 1 && nr <= TextUtil.MAX_ROMAN_INTEGER)
                {
                    string str = TextUtil.ToRomanNumberString(nr);
                    return this._lowercase ? str.ToLower() : str;
                }
                else
                {
                    return nr.ToString();
                }
            }
        }

        static CSSStyle()
        {
            PRE.Add("pre", true);
            PRE.Add("normal", false);

            BREAKWORD.Add("normal", false);
            BREAKWORD.Add("break-word", true);

            OrderedListType upper_alpha = new OrderedListType("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            OrderedListType lower_alpha = new OrderedListType("abcdefghijklmnopqrstuvwxyz");
            OLT.Add("decimal", OrderedListType.DECIMAL);
            OLT.Add("upper-alpha", upper_alpha);
            OLT.Add("lower-alpha", lower_alpha);
            OLT.Add("upper-latin", upper_alpha);
            OLT.Add("lower-latin", lower_alpha);
            OLT.Add("upper-roman", CreateRoman(false));
            OLT.Add("lower-roman", CreateRoman(true));
            OLT.Add("lower-greek", new OrderedListType("αβγδεζηθικλμνξοπρστυφχψω"));
            OLT.Add("upper-norwegian", new OrderedListType("ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ"));
            OLT.Add("lower-norwegian", new OrderedListType("abcdefghijklmnopqrstuvwxyzæøå"));
            OLT.Add("upper-russian-short", new OrderedListType("АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЭЮЯ"));
            OLT.Add("lower-russian-short", new OrderedListType("абвгдежзиклмнопрстуфхцчшщэюя"));

            ITALIC.Add("normal", false);
            ITALIC.Add("italic", true);
            ITALIC.Add("oblique", true);

            WEIGHTS.Add("normal", 400);
            WEIGHTS.Add("bold", 700);

            TEXTDECORATION.Add("none", TextDecoration.NONE);
            TEXTDECORATION.Add("underline", TextDecoration.UNDERLINE);
            TEXTDECORATION.Add("line-through", TextDecoration.LINE_THROUGH);

            INHERITHOVER.Add("inherit", true);
            INHERITHOVER.Add("normal", false);
        }
    }
}
