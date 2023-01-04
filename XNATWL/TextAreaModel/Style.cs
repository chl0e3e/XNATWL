using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextAreaModel
{
    public class Style
    {
        private Style parent;
        private StyleSheetKey styleSheetKey;
        private object[] values;

        public Style(Style parent, StyleSheetKey styleSheetKey)
        {
            this.parent = parent;
            this.styleSheetKey = styleSheetKey;
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
            this.parent = src.parent;
            this.styleSheetKey = src.styleSheetKey;
            this.values = (src.values != null) ? (object[]) src.values.Clone() : null;
        }

        protected object RawGet(int idx)
        {
            object[] vals = values;
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
                return this.parent;
            }
        }

        public StyleSheetKey StyleSheetKey
        {
            get
            {
                return this.styleSheetKey;
            }
        }

        public V GetNoResolve<V>(StyleAttribute<V> attribute, StyleSheetResolver resolver)
        {
            Object value = this.RawGet(attribute.Ordinal);

            if (value == null)
            {
                if (resolver != null && styleSheetKey != null)
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
                if (resolver != null && styleSheetKey != null)
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
                if (style.parent == null)
                {
                    return style;
                }

                if (style.RawGet(ord) != null)
                {
                    return style;
                }

                if (resolver != null && style.styleSheetKey != null)
                {
                    Style styleSheetStyle = resolver.Resolve(style);
                    if (styleSheetStyle != null && styleSheetStyle.RawGet(ord) != null)
                    {
                        // return main style here because class style has no parent chain
                        return style;
                    }
                }

                style = style.parent;
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
            if (this.values == null)
            {
                this.values = new Object[StyleAttribute.Attributes];
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
                if (values == null)
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

            values[attribute.Ordinal] = value;
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
            if (src.values != null)
            {
                this.EnsureValues();

                for (int i = 0, n = values.Length; i < n; i++)
                {
                    object value = src.values[i];

                    if (value != null)
                    {
                        this.values[i] = value;
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
            if (values != null)
            {
                for (int i = 0, n = values.Length; i < n; i++)
                {
                    if (values[i] != null && !StyleAttribute.ATTRIBUTES[i].Inherited)
                    {
                        return this.WithoutNonInheritableCopy();
                    }
                }
            }
            return this;
        }

        private Style WithoutNonInheritableCopy()
        {
            Style result = new Style(parent, styleSheetKey);

            for (int i = 0, n = values.Length; i < n; i++)
            {
                object value = values[i];

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

        public Dictionary<StyleAttribute, object> toMap()
        {
            Dictionary<StyleAttribute, object> result = new Dictionary<StyleAttribute, object>();

            for (int ord = 0; ord < values.Length; ord++)
            {
                Object value = values[ord];
                if (value != null)
                {
                    result.Add(StyleAttribute.ATTRIBUTES[ord], value);
                }
            }

            return result;
        }
    }
}
