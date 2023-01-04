using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL.TextAreaModel
{
    public class CSSStyle : Style
    {
        internal CSSStyle()
        {
        }

        public CSSStyle(string cssStyle)
        {
            parseCSS(cssStyle);
        }

        public CSSStyle(Style parent, StyleSheetKey styleSheetKey, string cssStyle) : base(parent, styleSheetKey)
        {
            parseCSS(cssStyle);
        }

        private void parseCSS(string style)
        {
            ParameterStringParser psp = new ParameterStringParser(style, ';', ':');
            psp.setTrim(true);
            while(psp.next()) {
                try {
                    parseCSSAttribute(psp.getKey(), psp.getValue());
                } catch(ArgumentOutOfRangeException ex) {
                    System.Diagnostics.Debug.WriteLine(typeof(CSSStyle).Name + " - Unable to parse CSS attribute: " + psp.getKey() + "=" + psp.getValue(), ex);
                }
            }
        }

        internal void parseCSSAttribute(string key, string value)
        {
            if(key.StartsWith("margin")) {
                parseBox(key.Substring(6), value, StyleAttribute.MARGIN);
                return;
            }
            if(key.StartsWith("padding")) {
                parseBox(key.Substring(7), value, StyleAttribute.PADDING);
                return;
            }
            if(key.StartsWith("font")) {
                parseFont(key, value);
                return;
            }
            if("text-indent".Equals(key)) {
                parseValueUnit(StyleAttribute.TEXT_INDENT, value);
                return;
            }
            if("-twl-font".Equals(key)) {
                Put(StyleAttribute.FONT_FAMILIES, new List<string> { value });
                return;
            }
            if("-twl-hover".Equals(key)) {
                parseEnum(StyleAttribute.INHERIT_HOVER, INHERITHOVER, value);
                return;
            }
            if("text-align".Equals(key)) {
                parseEnum(StyleAttribute.HORIZONTAL_ALIGNMENT, value);
                return;
            }
            if("text-decoration".Equals(key)) {
                parseEnum(StyleAttribute.TEXT_DECORATION, TEXTDECORATION, value);
                return;
            }
            if("vertical-align".Equals(key)) {
                parseEnum(StyleAttribute.VERTICAL_ALIGNMENT, value);
                return;
            }
            if("white-space".Equals(key)) {
                parseEnum(StyleAttribute.PREFORMATTED, PRE, value);
                return;
            }
            if("word-wrap".Equals(key)) {
                parseEnum(StyleAttribute.BREAKWORD, BREAKWORD, value);
                return;
            }
            if("list-style-image".Equals(key)) {
                parseURL(StyleAttribute.LIST_STYLE_IMAGE, value);
                return;
            }
            if("list-style-type".Equals(key)) {
                parseEnum(StyleAttribute.LIST_STYLE_TYPE, OLT, value);
                return;
            }
            if("clear".Equals(key)) {
                parseEnum(StyleAttribute.CLEAR, value);
                return;
            }
            if("float".Equals(key)) {
                parseEnum(StyleAttribute.FLOAT_POSITION, value);
                return;
            }
            if("display".Equals(key)) {
                parseEnum(StyleAttribute.DISPLAY, value);
                return;
            }
            if("width".Equals(key)) {
                parseValueUnit(StyleAttribute.WIDTH, value);
                return;
            }
            if("height".Equals(key)) {
                parseValueUnit(StyleAttribute.HEIGHT, value);
                return;
            }
            if("background-image".Equals(key)) {
                parseURL(StyleAttribute.BACKGROUND_IMAGE, value);
                return;
            }
            if("background-color".Equals(key) || "-twl-background-color".Equals(key)) {
                parseColor(StyleAttribute.BACKGROUND_COLOR, value);
                return;
            }
            if("color".Equals(key)) {
                parseColor(StyleAttribute.COLOR, value);
                return;
            }
            if("tab-size".Equals(key) || "-moz-tab-size".Equals(key)) {
                parseInteger(StyleAttribute.TAB_SIZE, value);
                return;
            }
            throw new ArgumentOutOfRangeException("Unsupported key: " + key);
        }

        private void parseBox(string key, string value, BoxAttribute box) {
            if("-top".Equals(key)) {
                parseValueUnit(box.Top, value);
            } else if("-left".Equals(key)) {
                parseValueUnit(box.Left, value);
            } else if("-right".Equals(key)) {
                parseValueUnit(box.Right, value);
            } else if("-bottom".Equals(key)) {
                parseValueUnit(box.Bottom, value);
            } else if("".Equals(key)) {
                Value[] vu = parseValueUnits(value);

                switch(vu.Length) {
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
    
        private void parseFont(string key, string value) {
            if("font-family".Equals(key)) {
                parseList(StyleAttribute.FONT_FAMILIES, value);
                return;
            }
            if("font-weight".Equals(key))
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

                Put(StyleAttribute.FONT_WEIGHT, weight);
                return;
            }
            if("font-size".Equals(key)) {
                parseValueUnit(StyleAttribute.FONT_SIZE, value);
                return;
            }
            if("font-style".Equals(key)) {
                parseEnum(StyleAttribute.FONT_ITALIC, ITALIC, value);
                return;
            }
            if("font".Equals(key)) {
                value = parseStartsWith(StyleAttribute.FONT_WEIGHT, WEIGHTS, value);
                value = parseStartsWith(StyleAttribute.FONT_ITALIC, ITALIC, value);
                if(value.Length > 0 && "1234567890".Contains(value[0])) {
                    int end = TextUtil.indexOf(value, ' ', 0);
                    parseValueUnit(StyleAttribute.FONT_SIZE, value.Substring(0, end));
                    end = TextUtil.skipSpaces(value, end);
                    value = value.Substring(end);
                }
                parseList(StyleAttribute.FONT_FAMILIES, value);
            }
        }
    
        private Value parseValueUnit(string value) {
            Value.Unit unit;
            int suffixLength = 2;
            if(value.EndsWith("px")) {
                unit = Value.Unit.PX;
            } else if(value.EndsWith("pt")) {
                unit = Value.Unit.PT;
            } else if(value.EndsWith("em")) {
                unit = Value.Unit.EM;
            } else if(value.EndsWith("ex")) {
                unit = Value.Unit.EX;
            } else if(value.EndsWith("%")) {
                suffixLength = 1;
                unit = Value.Unit.PERCENT;
            } else if("0".Equals(value)) {
                return Value.ZERO_PX;
            } else if("auto".Equals(value)) {
                return Value.AUTO;
            } else {
                throw new ArgumentOutOfRangeException("Unknown numeric suffix: " + value);
            }

            string numberPart = TextUtil.trim(value, 0, value.Length - suffixLength);
            return new Value(float.Parse(numberPart), unit);
        }

        private Value[] parseValueUnits(string value) {
            string[] parts = value.Split(new string[] { "\\s+" }, StringSplitOptions.None);
            Value[] result = new Value[parts.Length];
            for(int i=0 ; i<parts.Length ; i++) {
                result[i] = parseValueUnit(parts[i]);
            }
            return result;
        }

        private void parseValueUnit(StyleAttribute attribute, string value) {
            Put(attribute, parseValueUnit(value));
        }

        private void parseInteger(StyleAttribute<Int32> attribute, string value) {
            if("inherit".Equals(value)) {
                Put(attribute, null);
            } else {
                int intval = Int32.Parse(value);
                Put(attribute, intval);
            }
        }
    
        private void parseEnum<T>(StyleAttribute<T> attribute, Dictionary<string, T> map, string value)
        {
            if (!map.ContainsKey(value))
            {
                throw new ArgumentOutOfRangeException("Unknown value: " + value);
            }

            T obj = map[value];
            Put(attribute, obj);
        }

        private void parseEnum<E>(StyleAttribute<E> attribute, string value) where E : struct, IConvertible
        {
            object obj = Enum.Parse(attribute.DataType, value.ToUpper());
            Put(attribute, obj);
        }
    
        private string parseStartsWith<E>(StyleAttribute<E> attribute, Dictionary<string, E> map, string value) {
            int end = TextUtil.indexOf(value, ' ', 0);
            E obj = map[value.Substring(0, end)];
            if(obj != null) {
                end = TextUtil.skipSpaces(value, end);
                value = value.Substring(end);
            }
            Put(attribute, obj);
            return value;
        }

        private void parseURL(StyleAttribute<string> attribute, string value) {
            Put(attribute, stripURL(value));
        }

        static string stripTrim(string value, int start, int end) {
            return TextUtil.trim(value, start, value.Length - end);
        }
    
        public static string stripURL(string value) {
            if(value.StartsWith("url(") && value.EndsWith(")")) {
                value = stripQuotes(stripTrim(value, 4, 1));
            }
            return value;
        }
    
        static string stripQuotes(string value) {
            if((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'"))) {
                value = value.Substring(1, value.Length - 1);
            }
            return value;
        }

        private void parseColor(StyleAttribute<Color> attribute, string value) {
            Color color;
            if(value.StartsWith("rgb(") && value.EndsWith(")")) {
                value = stripTrim(value, 4, 1);
                byte[] rgb = parseRGBA(value, 3);
                color = new Color(rgb[0], rgb[1], rgb[2], (byte)255);
            } else if(value.StartsWith("rgba(") && value.EndsWith(")")) {
                value = stripTrim(value, 5, 1);
                byte[] rgba = parseRGBA(value, 4);
                color = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
            } else {
                color = Color.Parse(value);
                if(color == null) {
                    throw new ArgumentOutOfRangeException("unknown color name: " + value);
                }
            }
            Put(attribute, color);
        }
    
        private byte[] parseRGBA(string value, int numElements) {
            string[] parts = value.Split(',');
            if(parts.Length != numElements) {
                throw new ArgumentOutOfRangeException("3 values required for rgb()");
            }
            byte[] rgba = new byte[numElements];
            for(int i=0 ; i<numElements ; i++) {
                string part = parts[i].Trim();
                int v;
                if(i == 3) {
                    // handle alpha component specially
                    float f = float.Parse(part);
                    v = (int) Math.Round(f * 255.0f);
                } else {
                    bool percent = part.EndsWith("%");
                    if(percent) {
                        part = stripTrim(value, 0, 1);
                    }
                    v = Int32.Parse(part);
                    if(percent) {
                        v = 255*v / 100;
                    }
                }
                rgba[i] = (byte)Math.Max(0, Math.Min(255, v));
            }
            return rgba;
        }
    
        private void parseList(StyleAttribute<List<string>> attribute, string value) {
            Put(attribute, parseList(value, 0));
        }
    
        public static List<String> parseList(string value, int idx) {
            idx = TextUtil.skipSpaces(value, idx);
            if(idx >= value.Length) {
                return null;
            }

            char startChar = value[idx];
            int end;
            string part;

            if(startChar == '"' || startChar == '\'') {
                ++idx;
                end = TextUtil.indexOf(value, startChar, idx);
                part = value.Substring(idx, end);
                end = TextUtil.skipSpaces(value, ++end);
                if(end < value.Length && value[end] != ',') {
                    throw new ArgumentOutOfRangeException("',' expected at " + idx);
                }
            } else {
                end = TextUtil.indexOf(value, ',', idx);
                part = TextUtil.trim(value, idx, end);
            }

            List<string> result = parseList(value, end + 1);
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

        static OrderedListType createRoman(bool lowercase)
        {
            return new RomanOrderedListType(lowercase);
        }

        class RomanOrderedListType : OrderedListType
        {
            private bool _lowercase;
            public RomanOrderedListType(bool lowercase)
            {
                this._lowercase = lowercase;
            }

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
    
        static CSSStyle() {
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
            OLT.Add("upper-roman", createRoman(false));
            OLT.Add("lower-roman", createRoman(true));
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
