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
using System.Security.Claims;
using XNATWL.IO;

namespace XNATWL.TextAreaModel
{
    /// <summary>
    /// Stores the styles which should be applied to a certain element.
    /// </summary>
    public class Style
    {
        private Style _parent;
        private StyleSheetKey _styleSheetKey;
        private object[] _values;

        /// <summary>
        /// Creates an empty <see cref="Style"/> without a parent, class reference and no attributes
        /// </summary>
        public Style() : this(null, null)
        {

        }

        /// <summary>
        /// Creates a <see cref="Style"/> with the given parent and class reference.
        /// </summary>
        /// <param name="parent">the parent style. Can be null.</param>
        /// <param name="styleSheetKey">key for style sheet lookup. Can be null.</param>
        public Style(Style parent, StyleSheetKey styleSheetKey)
        {
            this._parent = parent;
            this._styleSheetKey = styleSheetKey;
        }

        /// <summary>
        /// Creates a <see cref="Style"/> with the given parent and class reference and copies the given attributes.
        /// </summary>
        /// <param name="parent">the parent style. Can be null.</param>
        /// <param name="styleSheetKey">key for style sheet lookup. Can be null.</param>
        /// <param name="values">a map with attributes for this Style. Can be null.</param>
        public Style(Style parent, StyleSheetKey styleSheetKey, Dictionary<StyleAttribute, Object> values) : this(parent, styleSheetKey)
        {
            if (values != null)
            {
                PutAll(values);
            }
        }

        /// <summary>
        /// Duplicate <see cref="Style"/> values and relations into a new <see cref="Style"/> object
        /// </summary>
        /// <param name="src"></param>
        protected internal Style(Style src)
        {
            this._parent = src._parent;
            this._styleSheetKey = src._styleSheetKey;
            this._values = (src._values != null) ? (object[]) src._values.Clone() : null;
        }

        /// <summary>
        /// Get value at given index. Used when a CSS value specifies multiple values (i.e. '1px 1px 1px 1px').
        /// </summary>
        /// <param name="idx">Value index</param>
        /// <returns>CSS value</returns>
        protected object RawGet(int idx)
        {
            object[] vals = _values;
            if (vals != null)
            {
                return vals[idx];
            }
            return null;
        }

        /// <summary>
        /// The parent of this <see cref="Style"/> or null. The parent is used to lookup attributes which can be inherited and are not specified in this Style.
        /// </summary>
        public Style Parent
        {
            get
            {
                return this._parent;
            }
        }

        /// <summary>
        /// The style sheet key for this <see cref="Style"/> or null. It is used to lookup attributes which are not set in this Style.
        /// </summary>
        public StyleSheetKey StyleSheetKey
        {
            get
            {
                return this._styleSheetKey;
            }
        }

        /// <summary>
        /// Retrieves the value of the specified attribute without resolving the style.
        /// 
        /// If the attribute is not set in this Style and a <see cref="StyleSheetResolver"/> was
        /// specified then the lookup is continued in the style sheet.
        /// </summary>
        /// <typeparam name="V">The data type of the attribute</typeparam>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <param name="resolver">A <see cref="StyleSheetResolver"/> to resolve the style sheet key. Can be null.</param>
        /// <returns>The attribute value if it was set, or the default value of the attribute</returns>
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

        /// <summary>
        /// Retrieves the value of the specified attribute without resolving the style.
        /// 
        /// If the attribute is not set in this <see cref="Style"/> and a <see cref="StyleSheetResolver"/> was
        /// specified then the lookup is continued in the style sheet.
        /// </summary>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <param name="resolver">A <see cref="StyleSheetResolver"/> to resolve the style sheet key. Can be null.</param>
        /// <returns>The attribute value if it was set, or the default value of the attribute</returns>
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

        /// <summary>
        /// Resolves the <see cref="Style"/> in which the specified attribute is defined.
        /// 
        /// If a <see cref="StyleSheetResolver"/> is specified then this method will treat
        /// style sheet styles referenced by this Style as if they are part
        /// of a Style in this chain.
        /// </summary>
        /// <param name="style">Style to query</param>
        /// <param name="ord">Ordinal in style values</param>
        /// <param name="resolver"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Resolves the <see cref="Style"/> in which the specified attribute is defined.
        /// 
        /// If a attribute does not cascade then this method does nothing.
        /// 
        /// If a <see cref="StyleSheetResolver"/> is specified then this method will treat
        /// style sheet styles referenced by this Style as if they are part
        /// of a <see cref="Style"/> in this chain.
        /// </summary>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <param name="resolver">A <see cref="StyleSheetResolver"/> to resolve the style sheet key. Can be null.</param>
        /// <returns>The Style which defined the specified attribute, will never return null.</returns>
        public Style Resolve(StyleAttribute attribute, StyleSheetResolver resolver)
        {
            if (!attribute.Inherited)
            {
                return this;
            }

            return DoResolve(this, attribute.Ordinal, resolver);
        }

        /// <summary>
        /// Ensure that <see cref="_values"/> is an initialised array
        /// </summary>
        protected void EnsureValues()
        {
            if (this._values == null)
            {
                this._values = new Object[StyleAttribute.Attributes];
            }
        }

        /// <summary>
        /// Set the value for a given attribute
        /// </summary>
        /// <param name="attribute">Attribute reference</param>
        /// <param name="value">New value</param>
        /// <exception cref="ArgumentOutOfRangeException">Cannot set or reference a null attribute</exception>
        protected internal void Put(StyleAttribute attribute, Object value)
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

        /// <summary>
        /// Retrieves the value (of type V) given the specified attribute from the resolved style
        /// </summary>
        /// <typeparam name="V">The data type of the attribute</typeparam>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <param name="resolver">A <see cref="StyleSheetResolver"/> to resolve the style sheet key. Can be null.</param>
        /// <returns>The attribute value if it was set, or the default value of the attribute</returns>
        public V Get<V>(StyleAttribute<V> attribute, StyleSheetResolver resolver)
        {
            return this.Resolve(attribute, resolver).GetNoResolve(attribute, resolver);
        }

        /// <summary>
        /// Retrieves the value of the specified attribute from the resolved style as an object.
        /// </summary>
        /// <typeparam name="V">The data type of the attribute</typeparam>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <param name="resolver">A <see cref="StyleSheetResolver"/> to resolve the style sheet key. Can be null.</param>
        /// <returns>The attribute value if it was set, or the default value of the attribute</returns>
        public object GetAsObject(StyleAttribute attribute, StyleSheetResolver resolver)
        {
            return this.Resolve(attribute, resolver).GetNoResolve(attribute, resolver);
        }

        /// <summary>
        /// Retrieves the value (of type V) of the specified attribute without resolving the style
        /// </summary>
        /// <typeparam name="V">The data type of the attribute</typeparam>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <returns>the attribute value or null (no default value)</returns>
        public V GetRaw<V>(StyleAttribute<V> attribute)
        {
            object value = this.RawGet(attribute.Ordinal);
            return (V) value;
        }

        /// <summary>
        /// Retrieves the value (as an <see cref="object"/>) of the specified attribute without resolving the style
        /// </summary>
        /// <typeparam name="V">The data type of the attribute</typeparam>
        /// <param name="attribute">The attribute to lookup.</param>
        /// <returns>the attribute value or null (no default value)</returns>
        public object GetRawAsObject(StyleAttribute attribute)
        {
            object value = this.RawGet(attribute.Ordinal);
            return this.RawGet(attribute.Ordinal);
        }

        /// <summary>
        /// Merge the given attribute dictionary containing <see cref="StyleAttribute"/>[] with this style
        /// </summary>
        /// <param name="values"></param>
        internal void PutAll(Dictionary<StyleAttribute, object> values)
        {
            foreach (StyleAttribute key in values.Keys)
            {
                Put(key, values[key]);
            }
        }

        /// <summary>
        /// Merge the given values from another <see cref="Style"/>
        /// </summary>
        /// <param name="src">Style to merge or overwrite with</param>
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

        /// <summary>
        /// Creates a copy of this <see cref="Style"/> and sets the specified attributes. It is possible to set a attribute to null to 'unset' it.
        /// </summary>
        /// <param name="values">The attributes to set in the new Style</param>
        /// <returns>a new Style with the same parent, styleSheetKey and modified attributes.</returns>
        public Style With(Dictionary<StyleAttribute, object> values)
        {
            Style newStyle = new Style(this);
            newStyle.PutAll(values);
            return newStyle;
        }

        /// <summary>
        /// Creates a copy of this <see cref="Style"/> and sets the specified attribute. It is possible to set a attribute to null to 'unset' it.
        /// </summary>
        /// <typeparam name="V">The data type of the attribute</typeparam>
        /// <param name="attribute">The attribute to set.</param>
        /// <param name="value">The new value of that attribute. Can be null.</param>  
        /// <returns>a new Style with the same parent, styleSheetKey and modified attribute.</returns>
        public Style With<V>(StyleAttribute<V> attribute, V value)
        {
            Style newStyle = new Style(this);
            newStyle.Put(attribute, value);
            return newStyle;
        }

        /// <summary>
        /// Returns a Style which doesn't contain any value for an attribute where
        /// <see cref="StyleAttribute.Inherited"/> is false.
        /// </summary>
        /// <returns>a Style with the same parent, styleSheetKey and modified attribute.</returns>
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

        /// <summary>
        /// Copy and retain inherited attriubte values
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Return the CSS style as a map of <see cref="StyleAttribute"/> to value
        /// </summary>
        /// <returns>Dictionary of CSS attributes</returns>
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
