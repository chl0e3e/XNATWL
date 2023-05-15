﻿/*
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

namespace XNATWL.TextAreaModel
{
    public class Style
    {
        private Style _parent;
        private StyleSheetKey _styleSheetKey;
        private object[] _values;

        public Style(Style parent, StyleSheetKey styleSheetKey)
        {
            this._parent = parent;
            this._styleSheetKey = styleSheetKey;
        }

        public Style() : this(null, null)
        {

        }

        public Style(Style parent, StyleSheetKey styleSheetKey, Dictionary<StyleAttribute, Object> values) : this(parent, styleSheetKey)
        {
            if (values != null)
            {
                PutAll(values);
            }
        }

        internal Style(Style src)
        {
            this._parent = src._parent;
            this._styleSheetKey = src._styleSheetKey;
            this._values = (src._values != null) ? (object[]) src._values.Clone() : null;
        }

        protected object RawGet(int idx)
        {
            object[] vals = _values;
            if (vals != null)
            {
                return vals[idx];
            }
            return null;
        }

        public Style Parent
        {
            get
            {
                return this._parent;
            }
        }

        public StyleSheetKey StyleSheetKey
        {
            get
            {
                return this._styleSheetKey;
            }
        }

        public V GetNoResolve<V>(StyleAttribute<V> attribute, StyleSheetResolver resolver)
        {
            Object value = this.RawGet(attribute.Ordinal);

            if (value == null)
            {
                if (resolver != null && _styleSheetKey != null)
                {
                    Style styleSheetStyle = resolver.Resolve(this);
                    if (styleSheetStyle != null)
                    {
                        value = styleSheetStyle.RawGet(attribute.Ordinal);
                    }
                }

                if (value == null)
                {
                    return (V) attribute.DefaultValue;
                }
            }

            return (V) value;
        }

        public object GetNoResolve(StyleAttribute attribute, StyleSheetResolver resolver)
        {
            Object value = this.RawGet(attribute.Ordinal);

            if (value == null)
            {
                if (resolver != null && _styleSheetKey != null)
                {
                    Style styleSheetStyle = resolver.Resolve(this);
                    if (styleSheetStyle != null)
                    {
                        value = styleSheetStyle.RawGet(attribute.Ordinal);
                    }
                }

                if (value == null)
                {
                    return attribute.DefaultValue;
                }
            }

            return value;
        }

        private static Style DoResolve(Style style, int ord, StyleSheetResolver resolver)
        {
            for (; ; )
            {
                if (style._parent == null)
                {
                    return style;
                }

                if (style.RawGet(ord) != null)
                {
                    return style;
                }

                if (resolver != null && style._styleSheetKey != null)
                {
                    Style styleSheetStyle = resolver.Resolve(style);
                    if (styleSheetStyle != null && styleSheetStyle.RawGet(ord) != null)
                    {
                        // return main style here because class style has no parent chain
                        return style;
                    }
                }

                style = style._parent;
            }

        }

        public Style Resolve(StyleAttribute attribute, StyleSheetResolver resolver)
        {
            if (!attribute.Inherited)
            {
                return this;
            }

            return DoResolve(this, attribute.Ordinal, resolver);
        }

        protected void EnsureValues()
        {
            if (this._values == null)
            {
                this._values = new Object[StyleAttribute.Attributes];
            }
        }

        internal void Put(StyleAttribute attribute, Object value)
        {
            if (attribute == null)
            {
                throw new ArgumentOutOfRangeException("attribute is null");
            }

            if (value == null)
            {
                if (_values == null)
                {
                    return;
                }
            }
            else
            {
                if (!attribute.DataType.IsInstanceOfType(value))
                {
                    throw new ArgumentOutOfRangeException("value is a " + value.GetType().FullName + " but must be a " + attribute.DataType.FullName);
                }

                this.EnsureValues();
            }

            _values[attribute.Ordinal] = value;
        }

        public V Get<V>(StyleAttribute<V> attribute, StyleSheetResolver resolver)
        {
            return this.Resolve(attribute, resolver).GetNoResolve(attribute, resolver);
        }

        public object GetAsObject(StyleAttribute attribute, StyleSheetResolver resolver)
        {
            return this.Resolve(attribute, resolver).GetNoResolve(attribute, resolver);
        }

        public V GetRaw<V>(StyleAttribute<V> attribute)
        {
            object value = this.RawGet(attribute.Ordinal);
            return (V) value;
        }

        public object GetRawAsObject(StyleAttribute attribute)
        {
            object value = this.RawGet(attribute.Ordinal);
            return this.RawGet(attribute.Ordinal);
        }

        internal void PutAll(Dictionary<StyleAttribute, object> values)
        {
            foreach (StyleAttribute key in values.Keys)
            {
                Put(key, values[key]);
            }
        }

        internal void PutAll(Style src)
        {
            if (src._values != null)
            {
                this.EnsureValues();

                for (int i = 0, n = _values.Length; i < n; i++)
                {
                    object value = src._values[i];

                    if (value != null)
                    {
                        this._values[i] = value;
                    }
                }
            }
        }

        public Style With(Dictionary<StyleAttribute, object> values)
        {
            Style newStyle = new Style(this);
            newStyle.PutAll(values);
            return newStyle;
        }

        public Style With<V>(StyleAttribute<V> attribute, V value)
        {
            Style newStyle = new Style(this);
            newStyle.Put(attribute, value);
            return newStyle;
        }

        /**
         * Returns a Style which doesn't contain any value for an attribute where
         * {@link StyleAttribute#isInherited() } returns false.
         * 
         * @return a Style with the same parent, styleSheetKey and modified attribute.
         */
        public Style WithoutNonInheritable()
        {
            if (_values != null)
            {
                for (int i = 0, n = _values.Length; i < n; i++)
                {
                    if (_values[i] != null && !StyleAttribute.ATTRIBUTES[i].Inherited)
                    {
                        return this.WithoutNonInheritableCopy();
                    }
                }
            }
            return this;
        }

        private Style WithoutNonInheritableCopy()
        {
            Style result = new Style(_parent, _styleSheetKey);

            for (int i = 0, n = _values.Length; i < n; i++)
            {
                object value = _values[i];

                if (value != null)
                {
                    StyleAttribute attribute = StyleAttribute.ATTRIBUTES[i];

                    if (attribute.Inherited)
                    {
                        result.Put(attribute, value);
                    }
                }
            }

            return result;
        }

        public Dictionary<StyleAttribute, object> ToMap()
        {
            Dictionary<StyleAttribute, object> result = new Dictionary<StyleAttribute, object>();

            for (int ord = 0; ord < _values.Length; ord++)
            {
                Object value = _values[ord];
                if (value != null)
                {
                    result.Add(StyleAttribute.ATTRIBUTES[ord], value);
                }
            }

            return result;
        }
    }
}
