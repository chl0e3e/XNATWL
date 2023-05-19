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
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace XNATWL.TextAreaModel
{
    public class StyleAttribute
    {
        internal static List<StyleAttribute> ATTRIBUTES = new List<StyleAttribute>();

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

        /// <summary>
        /// The number of implemented StyleAttributes.
        /// </summary>
        public static int Attributes
        {
            get
            {
                return ATTRIBUTES.Count;
            }
        }

        protected internal bool _inherited;
        protected internal Type _dataType;
        protected internal object _defaultValue;
        protected internal int _ordinal;

        /// <summary>
        /// A inherited attribute will be looked up in the parent style if it is not set.
        /// </summary>
        public bool Inherited
        {
            get
            {
                return this._inherited;
            }
        }

        /// <summary>
        /// Type representing the value of the attribute
        /// </summary>
        public Type DataType
        {
            get
            {
                return this._dataType;
            }
        }

        /// <summary>
        /// Default value of the attribute
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return this._defaultValue;
            }
        }

        /// <summary>
        /// A unique id for this <see cref="StyleAttribute"/>. This value is may change when this class is modified and should not be used for persistent storage.
        /// </summary>
        public int Ordinal
        {
            get
            {
                return this._ordinal;
            }
        }
    }

    public class StyleAttribute<T> : StyleAttribute
    {
        /// <summary>
        /// Returns the name of this StyleAttribute. This method uses reflection to search for the field name.
        /// </summary>
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

        /// <summary>
        /// Default value of the attribute
        /// </summary>
        public new T DefaultValue
        {
            get
            {
                return (T) this._defaultValue;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        protected internal StyleAttribute(bool inherited, Type dataType, T defaultValue)
        {
            this._inherited = inherited;
            this._dataType = dataType;
            this._defaultValue = (object) defaultValue;
            this._ordinal = StyleAttribute.ATTRIBUTES.Count;
            StyleAttribute.ATTRIBUTES.Add(this);
        }

        /// <summary>
        /// Returns the StyleAttribute given it's unique id.
        /// </summary>
        /// <param name="ordinal">the unique id of the desired StyleAttribute.</param>
        /// <returns>the StyleAttribute given it's unique id.</returns>
        public static StyleAttribute<T> ByOrdinal(int ordinal)
        {
            return (StyleAttribute<T>) StyleAttribute.ATTRIBUTES[ordinal];
        }

        /// <summary>
        /// Returns the StyleAttribute given it's name.
        /// </summary>
        /// <param name="name">the name of the StyleAttribute.</param>
        /// <returns>the StyleAttribute</returns>
        /// <exception cref="ArgumentOutOfRangeException">if no StyleAttribute with the given name exists</exception>
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
