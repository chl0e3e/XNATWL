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
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class ParameterListImpl : ThemeChildImpl, ParameterList
    {
        internal List<object> parameters;

        public ParameterListImpl(ThemeManager manager, ThemeInfoImpl parent) : base(manager, parent)
        {
            this.parameters = new List<Object>();
        }

        public int getSize()
        {
            return parameters.Count;
        }

        public Font getFont(int idx)
        {
            Font value = (Font) getParameterValue(idx, typeof(Font));
            if (value != null)
            {
                return value;
            }
            return manager.getDefaultFont();
        }

        public Image getImage(int idx)
        {
            Image img = (Image) getParameterValue(idx, typeof(Image));
            if (img == ImageManager.NONE)
            {
                return null;
            }
            return img;
        }

        public MouseCursor getMouseCursor(int idx)
        {
            return (MouseCursor)getParameterValue(idx, typeof(MouseCursor));
        }

        public ParameterMap getParameterMap(int idx)
        {
            ParameterMap value = (ParameterMap) getParameterValue(idx, typeof(ParameterMap));
            if (value == null)
            {
                return manager.emptyMap;
            }
            return value;
        }

        public ParameterList getParameterList(int idx)
        {
            ParameterList value = (ParameterList) getParameterValue(idx, typeof(ParameterList));
            if (value == null)
            {
                return manager.emptyList;
            }
            return value;
        }

        public bool getParameter(int idx, bool defaultValue)
        {
            object value = getParameterValue(idx, typeof(bool));
            if (value == null)
            {
                return defaultValue;
            }

            return (bool)value;
        }

        public int getParameter(int idx, int defaultValue)
        {
            object value = getParameterValue(idx, typeof(int));
            if (value == null)
            {
                return defaultValue;
            }

            return (int)value;
        }

        public float getParameter(int idx, float defaultValue)
        {
            object value = getParameterValue(idx, typeof(float));
            if (value == null)
            {
                return defaultValue;
            }

            return (float)value;
        }

        public string getParameter(int idx, string defaultValue)
        {
            object value = getParameterValue(idx, typeof(string));
            if (value == null)
            {
                return defaultValue;
            }

            return (string)value;
        }

        public Color getParameter(int idx, Color defaultValue)
        {
            Color value = (Color) getParameterValue(idx, typeof(Color));
            if (value != null)
            {
                return value;
            }
            return defaultValue;
        }

        public E getParameter<E>(int idx, E defaultValue) where E : struct, IConvertible
        {
            E? value = (E?) getParameterValue(idx, defaultValue.GetType());
            if (value != null)
            {
                return (E) value;
            }
            return defaultValue;
        }

        public Object getParameterValue(int idx)
        {
            return parameters[idx];
        }

        public object getParameterValue(int idx, Type type)
        {
            object value = getParameterValue(idx);
            if (value != null && !type.IsInstanceOfType(value))
            {
                wrongParameterType(idx, type, value.GetType());
                return null;
            }
            return value;
        }

        public object getParameterValue<T>(int idx, Type type)
        {
            T value = (T) getParameterValue(idx);
            if (value != null && !type.IsInstanceOfType(value))
            {
                wrongParameterType(idx, type, value.GetType());
                return null;
            }
            return value;
        }

        protected void wrongParameterType(int idx, Type expectedType, Type foundType)
        {
            DebugHook.getDebugHook().wrongParameterType(this, idx, expectedType, foundType, getParentDescription());
        }
    }
}
