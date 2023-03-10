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
    public class ParameterMapImpl : ThemeChildImpl, ParameterMap
    {
        internal CascadedHashMap<String, Object> parameters;

        public ParameterMapImpl(ThemeManager manager, ThemeInfoImpl parent) : base(manager, parent)
        {
            this.parameters = new CascadedHashMap<String, Object>();
        }

        public virtual void copy(ParameterMapImpl src)
        {
            this.parameters.CollapseAndSetFallback(src.parameters);
        }

        public Font getFont(String name)
        {
            Font value = (Font) getParameterValue(name, true, typeof(Font));
            if (value != null)
            {
                return value;
            }
            return manager.getDefaultFont();
        }

        public Image getImage(String name)
        {
            Image img = (Image) getParameterValue(name, true, typeof(Image));
            if (img == ImageManager.NONE)
            {
                return null;
            }
            return img;
        }

        public MouseCursor getMouseCursor(String name)
        {
            MouseCursor value = (MouseCursor) getParameterValue(name, false, typeof(MouseCursor));
            return value;
        }

        public ParameterMap getParameterMap(String name)
        {
            ParameterMap value = (ParameterMap) getParameterValue(name, true, typeof(ParameterMap));
            if (value == null)
            {
                return manager.emptyMap;
            }
            return value;
        }

        public ParameterList getParameterList(String name)
        {
            ParameterList value = (ParameterList) getParameterValue(name, true, typeof(ParameterList));
            if (value == null)
            {
                return manager.emptyList;
            }
            return value;
        }

        public object getParameter(String name, object defaultValue)
        {
            Object value = getParameterValue(name, true, typeof(object));
            if (value != null)
            {
                return (object)value;
            }
            return defaultValue;
        }

        public bool getParameter(String name, bool defaultValue)
        {
            Object value = getParameterValue(name, true, typeof(bool));
            if (value != null)
            {
                return (bool)value;
            }
            return defaultValue;
        }

        public int getParameter(String name, int defaultValue)
        {
            Object value = getParameterValue(name, true, typeof(int));
            if (value != null)
            {
                return (int)value;
            }
            return defaultValue;
        }

        public float getParameter(String name, float defaultValue)
        {
            Object value = getParameterValue(name, true, typeof(float));
            if (value != null)
            {
                return (float) value;
            }
            return defaultValue;
        }

        public string getParameter(String name, String defaultValue)
        {
            string value = (string) getParameterValue(name, true, typeof(string));
            if (value != null)
            {
                return value;
            }
            return defaultValue;
        }

        public Color getParameter(String name, Color defaultValue)
        {
            Color value = (Color)getParameterValue(name, true, typeof(Color));
            if (value != null)
            {
                return value;
            }
            return defaultValue;
        }

        public E getParameter<E>(String name, E defaultValue) where E : struct, IConvertible
        {
            Type enumType = defaultValue.GetType();
            E? value = (E?) getParameterValue(name, true, enumType);
            if (value != null)
            {
                return (E) value;
            }
            return defaultValue;
        }

        public object getParameterValue(String name, bool warnIfNotPresent)
        {
            object value = this.parameters.CascadingEntry(name);
            if (value == null && warnIfNotPresent)
            {
                missingParameter(name, null);
            }
            return value;
        }

        public object getParameterValue(String name, bool warnIfNotPresent, Type type)
        {
            return getParameterValue(name, warnIfNotPresent, type, null);
        }

        public object getParameterValue(String name, bool warnIfNotPresent, Type type, object defaultValue)
        {
            object value = this.parameters.CascadingEntry(name);

            if (value == null && warnIfNotPresent)
            {
                missingParameter(name, type);
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
                    wrongParameterType(name, type, value.GetType());
                }
                return defaultValue;
            }

            return value;
        }

        public T getParameterValue<T>(String name, bool warnIfNotPresent, Type type, T defaultValue)
        {
            T value = (T) this.parameters.CascadingEntry(name);

            if (value == null && warnIfNotPresent)
            {
                missingParameter(name, type);
            }

            if (!type.IsInstanceOfType(value))
            {
                if (value != null)
                {
                    wrongParameterType(name, type, value.GetType());
                }
                return defaultValue;
            }

            return value;
        }


        protected void wrongParameterType(String paramName, Type expectedType, Type foundType)
        {
            DebugHook.getDebugHook().wrongParameterType(this, paramName, expectedType, foundType, getParentDescription());
        }

        protected void missingParameter(String paramName, Type dataType)
        {
            DebugHook.getDebugHook().missingParameter(this, paramName, getParentDescription(), dataType);
        }

        protected void replacingWithDifferentType(String paramName, Type oldType, Type newType)
        {
            DebugHook.getDebugHook().replacingWithDifferentType(this, paramName, oldType, newType, getParentDescription());
        }

        public object getParam(String name)
        {
            return this.parameters.CascadingEntry(name);
        }

        public void put(Dictionary<string, object> parameters)
        {
            foreach (string key in parameters.Keys)
            {
                put(key, parameters[key]);
            }
        }

        public void put(string paramName, object value)
        {
            object old = this.parameters.PutCascadingEntry(paramName, value);

            if (old != null && value != null)
            {
                Type oldType = old.GetType();
                Type newType = value.GetType();

                if (oldType != newType && !areTypesCompatible(oldType, newType))
                {
                    replacingWithDifferentType(paramName, oldType, newType);
                }
            }
        }

        private static bool areTypesCompatible(Type typeA, Type typeB)
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
