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
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    /// <summary>
    /// An implementation to manage a map of theme parameters
    /// </summary>
    public class ParameterMapImpl : ThemeChildImpl, ParameterMap
    {
        internal CascadedHashMap<string, object> _parameters;

        /// <summary>
        /// Initialise a <see cref="ParameterMap"/> implementation
        /// </summary>
        /// <param name="manager">Parent theme manager</param>
        /// <param name="parent">Parent theme info parameter map</param>
        public ParameterMapImpl(ThemeManager manager, ThemeInfoImpl parent) : base(manager, parent)
        {
            this._parameters = new CascadedHashMap<String, Object>();
        }

        /// <summary>
        /// Duplicate parameters into this object's <see cref="CascadedHashMap{K, Object}"/>
        /// </summary>
        /// <param name="src"></param>
        public virtual void Copy(ParameterMapImpl src)
        {
            this._parameters.CollapseAndSetFallback(src._parameters);
        }

        public Font GetFont(String name)
        {
            Font value = (Font) GetParameterValue(name, true, typeof(Font));
            if (value != null)
            {
                return value;
            }
            return _manager.GetDefaultFont();
        }

        public Image GetImage(String name)
        {
            Image img = (Image) GetParameterValue(name, true, typeof(Image));
            if (img == ImageManager.NONE)
            {
                return null;
            }
            return img;
        }

        public MouseCursor GetMouseCursor(String name)
        {
            MouseCursor value = (MouseCursor) GetParameterValue(name, false, typeof(MouseCursor));
            return value;
        }

        public ParameterMap GetParameterMap(String name)
        {
            ParameterMap value = (ParameterMap) GetParameterValue(name, true, typeof(ParameterMap));
            if (value == null)
            {
                return _manager._emptyMap;
            }
            return value;
        }

        public ParameterList GetParameterList(String name)
        {
            ParameterList value = (ParameterList) GetParameterValue(name, true, typeof(ParameterList));
            if (value == null)
            {
                return _manager._emptyList;
            }
            return value;
        }

        public object GetParameter(String name, object defaultValue)
        {
            Object value = GetParameterValue(name, true, typeof(object));
            if (value != null)
            {
                return (object)value;
            }
            return defaultValue;
        }

        public bool GetParameter(String name, bool defaultValue)
        {
            Object value = GetParameterValue(name, true, typeof(bool));
            if (value != null)
            {
                return (bool)value;
            }
            return defaultValue;
        }

        public int GetParameter(String name, int defaultValue)
        {
            Object value = GetParameterValue(name, true, typeof(int));
            if (value != null)
            {
                return (int)value;
            }
            return defaultValue;
        }

        public float GetParameter(String name, float defaultValue)
        {
            Object value = GetParameterValue(name, true, typeof(float));
            if (value != null)
            {
                return (float) value;
            }
            return defaultValue;
        }

        public string GetParameter(String name, String defaultValue)
        {
            string value = (string) GetParameterValue(name, true, typeof(string));
            if (value != null)
            {
                return value;
            }
            return defaultValue;
        }

        public Color GetParameter(String name, Color defaultValue)
        {
            Color value = (Color)GetParameterValue(name, true, typeof(Color));
            if (value != null)
            {
                return value;
            }
            return defaultValue;
        }

        public E GetParameter<E>(String name, E defaultValue) where E : struct, IConvertible
        {
            Type enumType = defaultValue.GetType();
            E? value = (E?) GetParameterValue(name, true, enumType);
            if (value != null)
            {
                return (E) value;
            }
            return defaultValue;
        }

        public object GetParameterValue(String name, bool warnIfNotPresent)
        {
            object value = this._parameters.CascadingEntry(name);
            if (value == null && warnIfNotPresent)
            {
                MissingParameter(name, null);
            }
            return value;
        }

        public object GetParameterValue(String name, bool warnIfNotPresent, Type type)
        {
            return GetParameterValue(name, warnIfNotPresent, type, null);
        }

        public object GetParameterValue(String name, bool warnIfNotPresent, Type type, object defaultValue)
        {
            object value = this._parameters.CascadingEntry(name);

            if (value == null && warnIfNotPresent)
            {
                MissingParameter(name, type);
            }

            if (type.IsPrimitive && value != null)
            {
                if (value is Int16 && type == typeof(Int32))
                {
                    return (int) ((short) value);
                }
                else if (value is Int32 && type == typeof(Int16))
                {
                    return (short) ((int) (value));
                }
                else if (value is Int32 && type == typeof(Char))
                {
                    return (Char) ((int)value);
                }
            }

            if (!type.IsInstanceOfType(value))
            {
                if (value != null)
                {
                    WrongParameterType(name, type, value.GetType());
                }
                return defaultValue;
            }

            return value;
        }

        public T GetParameterValue<T>(String name, bool warnIfNotPresent, Type type, T defaultValue)
        {
            T value = (T)this._parameters.CascadingEntry(name);

            if (value == null && warnIfNotPresent)
            {
                MissingParameter(name, type);
            }

            if (!type.IsInstanceOfType(value))
            {
                if (value != null)
                {
                    WrongParameterType(name, type, value.GetType());
                }
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Debugging function for a lookup using an invalid parameter <see cref="Type"/>
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="foundType">Found type</param>
        protected void WrongParameterType(String paramName, Type expectedType, Type foundType)
        {
            DebugHook.getDebugHook().WrongParameterType(this, paramName, expectedType, foundType, GetParentDescription());
        }

        /// <summary>
        /// Debugging function for a lookup with a non-existent name
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="dataType">Type used in lookup</param>
        protected void MissingParameter(String paramName, Type dataType)
        {
            DebugHook.getDebugHook().MissingParameter(this, paramName, GetParentDescription(), dataType);
        }

        /// <summary>
        /// Replacing given parameter type with a working alternative
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="oldType">Type attempted</param>
        /// <param name="newType">New type</param>
        protected void ReplacingWithDifferentType(String paramName, Type oldType, Type newType)
        {
            DebugHook.getDebugHook().ReplacingWithDifferentType(this, paramName, oldType, newType, GetParentDescription());
        }

        /// <summary>
        /// Look up using name, returning using the most generic data type <see cref="object"/>
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>an <see cref="object"/></returns>
        public object this[string name]
        {
            get
            {
                return this._parameters.CascadingEntry(name);
            }
        }

        /// <summary>
        /// Copy from another parameter dictionary into this one
        /// </summary>
        /// <param name="parameters">Copied dictionary</param>
        public void Put(Dictionary<string, object> parameters)
        {
            foreach (string key in parameters.Keys)
            {
                Put(key, parameters[key]);
            }
        }

        /// <summary>
        /// Put a new parameter by <paramref name="paramName"/> into the cascading dictionary
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="value">New value</param>
        public void Put(string paramName, object value)
        {
            object old = this._parameters.PutCascadingEntry(paramName, value);

            if (old != null && value != null)
            {
                Type oldType = old.GetType();
                Type newType = value.GetType();

                if (oldType != newType && !AreTypesCompatible(oldType, newType))
                {
                    ReplacingWithDifferentType(paramName, oldType, newType);
                }
            }
        }

        /// <summary>
        /// Test if types are assignable to each other
        /// </summary>
        /// <param name="typeA">First type</param>
        /// <param name="typeB">Other type</param>
        /// <returns><b>true</b> if compatible</returns>
        private static bool AreTypesCompatible(Type typeA, Type typeB)
        {
            foreach (Type type in BASE_CLASSES)
            {
                if (type.IsAssignableFrom(typeA) && type.IsAssignableFrom(typeB))
                {
                    return true;
                }
            }
            return false;
        }

        private static Type[] BASE_CLASSES = {
            typeof(Image),
            typeof(Font),
            typeof(MouseCursor)
        };
    }
}
