using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class ParameterListImpl : ThemeChildImpl, ParameterList
    {
        List<object> parameters;

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
