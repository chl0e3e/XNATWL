﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XNATWL.TextArea
{
    public class StyleAttribute
    {
        internal static List<StyleAttribute> ATTRIBUTES = new List<StyleAttribute>();

        public static int Attributes
        {
            get
            {
                return ATTRIBUTES.Count;
            }
        }

        internal bool inherited;
        internal Type dataType;
        internal object defaultValue;
        internal int ordinal;

        public bool Inherited
        {
            get
            {
                return this.inherited;
            }
        }

        public Type DataType
        {
            get
            {
                return this.dataType;
            }
        }

        public object DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
        }

        public int Ordinal
        {
            get
            {
                return this.ordinal;
            }
        }
    }

    public class StyleAttribute<T> : StyleAttribute
    {
        // cascading attributes
        public static StyleAttribute<HAlignment> HORIZONTAL_ALIGNMENT = new StyleAttribute<HAlignment>(true, typeof(HAlignment), HAlignment.LEFT);
        public static StyleAttribute<VAlignment> VERTICAL_ALIGNMENT = new StyleAttribute<VAlignment>(true, typeof(VAlignment), VAlignment.BOTTOM);
        public static StyleAttribute<Value> TEXT_INDENT = new StyleAttribute<Value>(true, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<TextDecoration> TEXT_DECORATION = new StyleAttribute<TextDecoration>(true, typeof(TextDecoration), TextDecoration.NONE);
        public static StyleAttribute<TextDecoration> TEXT_DECORATION_HOVER = new StyleAttribute<TextDecoration>(true, typeof(TextDecoration), TextDecoration.NONE);
        public static StyleAttribute<List<string>> FONT_FAMILIES = new StyleAttribute<List<string>>(true, typeof(List<string>), new List<string> { "default" });
        public static StyleAttribute<Value> FONT_SIZE = new StyleAttribute<Value>(true, typeof(Value), new Value(14, Value.Unit.PX));
        public static StyleAttribute<Int32> FONT_WEIGHT = new StyleAttribute<Int32>(true, typeof(Int32), 400);
        public static StyleAttribute<bool> FONT_ITALIC = new StyleAttribute<bool>(true, typeof(bool), false);
        public static StyleAttribute<Int32> TAB_SIZE = new StyleAttribute<Int32>(true, typeof(Int32), 8);
        public static StyleAttribute<string> LIST_STYLE_IMAGE = new StyleAttribute<string>(true, typeof(string), "ul-bullet");
        public static StyleAttribute<OrderedListType> LIST_STYLE_TYPE = new StyleAttribute<OrderedListType>(true, typeof(OrderedListType), OrderedListType.DECIMAL);
        public static StyleAttribute<bool> PREFORMATTED = new StyleAttribute<bool>(true, typeof(bool), false);
        public static StyleAttribute<bool> BREAKWORD = new StyleAttribute<bool>(true, typeof(bool), false);
        public static StyleAttribute<Color> COLOR = new StyleAttribute<Color>(true, typeof(Color), Color.WHITE);
        public static StyleAttribute<Color> COLOR_HOVER = new StyleAttribute<Color>(true, typeof(Color), null);
        public static StyleAttribute<bool> INHERIT_HOVER = new StyleAttribute<bool>(true, typeof(bool), false);

        // non cascading attribute
        public static StyleAttribute<Clear> CLEAR = new StyleAttribute<Clear>(false, typeof(Clear), Clear.NONE);
        public static StyleAttribute<Display> DISPLAY = new StyleAttribute<Display>(false, typeof(Display), Display.INLINE);
        public static StyleAttribute<FloatPosition> FLOAT_POSITION = new StyleAttribute<FloatPosition>(false, typeof(FloatPosition), FloatPosition.NONE);
        public static StyleAttribute<Value> WIDTH = new StyleAttribute<Value>(false, typeof(Value), Value.AUTO);
        public static StyleAttribute<Value> HEIGHT = new StyleAttribute<Value>(false, typeof(Value), Value.AUTO);
        public static StyleAttribute<string> BACKGROUND_IMAGE = new StyleAttribute<string>(false, typeof(string), null);
        public static StyleAttribute<Color> BACKGROUND_COLOR = new StyleAttribute<Color>(false, typeof(Color), Color.TRANSPARENT);
        public static StyleAttribute<Color> BACKGROUND_COLOR_HOVER = new StyleAttribute<Color>(false, typeof(Color), Color.TRANSPARENT);
        public static StyleAttribute<Value> MARGIN_TOP = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> MARGIN_LEFT = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> MARGIN_RIGHT = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> MARGIN_BOTTOM = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> PADDING_TOP = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> PADDING_LEFT = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> PADDING_RIGHT = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);
        public static StyleAttribute<Value> PADDING_BOTTOM = new StyleAttribute<Value>(false, typeof(Value), Value.ZERO_PX);

        // boxes
        public static BoxAttribute MARGIN = new BoxAttribute(MARGIN_TOP, MARGIN_LEFT, MARGIN_RIGHT, MARGIN_BOTTOM);
        public static BoxAttribute PADDING = new BoxAttribute(PADDING_TOP, PADDING_LEFT, PADDING_RIGHT, PADDING_BOTTOM);


        /**
         * Returns the name of this StyleAttribute.
         * This method uses reflection to search for the field name.
         * @return the name of this StyleAttribute.
         */
        public String Name
        {
            get
            {
                try
                {
                    //
                    foreach (FieldInfo field in typeof(StyleAttribute<T>).GetFields())
                    {
                        if (field.IsStatic && field.GetValue(null) == this)
                        {
                            return field.Name;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ignore
                }
                return "?";
            }
        }

        T DefaultValue
        {
            get
            {
                return (T) this.defaultValue;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        private StyleAttribute(bool inherited, Type dataType, T defaultValue)
        {
            this.inherited = inherited;
            this.dataType = dataType;
            this.defaultValue = (object) defaultValue;
            this.ordinal = StyleAttribute.ATTRIBUTES.Count;
            StyleAttribute.ATTRIBUTES.Add(this);
        }

        /**
         * Returns the StyleAttribute given it's unique id.
         * @param ordinal the unique id of the desired StyleAttribute.
         * @return the StyleAttribute given it's unique id.
         * @throws IndexOutOfBoundsException if the given id is invalid.
         * @see #ordinal()
         */
        public static StyleAttribute<T> ByOrdinal(int ordinal)
        {
            return (StyleAttribute<T>) StyleAttribute.ATTRIBUTES[ordinal];
        }

        /**
         * Returns the StyleAttribute given it's name.
         * @param name the name of the StyleAttribute.
         * @return the StyleAttribute
         * @throws IllegalArgumentException if no StyleAttribute with the given name exists.
         * @see #name() 
         */
        public static StyleAttribute<T> ByName(String name)
        {
            try
            {
                //
                FieldInfo field = typeof(StyleAttribute<T>).GetField(name);
                if (field.IsStatic && field.FieldType == typeof(StyleAttribute<T>))
                {
                    return (StyleAttribute<T>) field.GetValue(null);
                }
            }
            catch (Exception e)
            {
                // ignore
            }

            throw new ArgumentOutOfRangeException("No style attribute " + name);
        }
    }
}
