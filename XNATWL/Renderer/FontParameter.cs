using static XNATWL.Renderer.FontParameter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XNATWL.Renderer
{
    public class FontParameter
    {
        static Dictionary<String, object> parameterMap = new Dictionary<String, object>();

        public static Parameter<Color> COLOR = NewParameter("color", Color.WHITE);
        public static Parameter<bool> UNDERLINE = NewParameter("underline", false);
        public static Parameter<bool> LINETHROUGH = NewParameter("linethrough", false);

        private object[] values;

        public FontParameter()
        {
            this.values = new Object[8];
        }

        public FontParameter(FontParameter baseParameter)
        {
            this.values = (object[]) baseParameter.values.Clone();
        }

        /**
         * Sets a parameter value
         * @param <T> the type of the parameter
         * @param param the parameter
         * @param value the value or null to revert to it's default value
         */
        public void Put<T>(Parameter<T> param, T value)
        {
            if (value != null && !param._dataClass.IsInstanceOfType(value))
            {
                throw new Exception("value casting failed");
            }
            int ordinal = param._ordinal;
            int curLength = values.Length;
            if (ordinal >= curLength)
            {
                Object[] tmp = new Object[Math.Max(ordinal + 1, curLength * 2)];
                Array.Copy(values, 0, tmp, 0, curLength);
                values = tmp;
            }
            values[ordinal] = value;
        }

        /**
         * Returns the value of the specified parameter
         * @param <T> the type of the parameter
         * @param param the parameter
         * @return the parameter value or it's default value when the parameter was not set
         */
        public T Get<T>(Parameter<T> param)
        {
            if (param._ordinal < values.Length)
            {
                Object raw = values[param._ordinal];
                if (raw != null)
                {
                    return (T) raw;
                }
            }
            return param.getDefaultValue();
        }

        /**
         * Returns an array of all registered parameter
         * @return an array of all registered parameter
         */
        public static object[] RegisteredParameter()
        {
            lock (parameterMap) {
                return parameterMap.Values.ToArray();
            }
        }

        /**
         * Returns the parameter instance for the given name
         * @param name the name to look up
         * @return the parameter instance or null when the name is not registered
         */
        public static object ParameterByName(String name)
        {
            lock(parameterMap) {
                return parameterMap[name];
            }
        }

        /**
         * Registers a new parameter.
         * 
         * <p>The data class is extracted from the default value.</p>
         * <p>If the name is already registered then the existing parameter is returned.</p>
         * 
         * @param <T> the data type of the parameter
         * @param name the parameter name
         * @param defaultValue the default value
         * @return the parameter instance
         * @throws NullPointerException when one of the parameters is null
         * @throws IllegalStateException when the name is already registered but with
         *                               different dataClass or defaultValue
         */
        public static Parameter<T> NewParameter<T>(String name, T defaultValue)
        {
            Type dataClass = (Type)defaultValue.GetType();
            return NewParameter(name, dataClass, defaultValue);
        }

        /**
         * Registers a new parameter.
         * 
         * <p>If the name is already registered then the existing parameter is returned.</p>
         * 
         * @param <T> the data type of the parameter
         * @param name the parameter name
         * @param dataClass the data class
         * @param defaultValue the default value - can be null.
         * @return the parameter instance
         * @throws NullPointerException when name or dataClass is null
         * @throws IllegalStateException when the name is already registered but with
         *                               different dataClass or defaultValue
         */
        public static Parameter<T> NewParameter<T>(String name, Type dataClass, T defaultValue)
        {
            lock(parameterMap)
            {
                object existing = parameterMap[name];
                if (existing != null)
                {
                    var existingsType = existing.GetType();
                    if (existingsType != typeof(Parameter<T>))
                    {
                        throw new Exception("Asset exception");
                    }

                    if ((Type) existingsType.GetField("dataClass").GetValue(existing) != dataClass || !_equals(existingsType.GetField("defaultValue").GetValue(existing), defaultValue))
                    {
                        throw new InvalidOperationException("type '" + name + "' already registered but different");
                    }

                    return (Parameter<T>)existing;
                }

                Parameter<T> type = new Parameter<T>(name, dataClass, defaultValue, parameterMap.Count);
                parameterMap.Add(name, type);
                return type;
            }
        }

        private static bool _equals(Object a, Object b)
        {
            return (a == b) || (a != null && a.Equals(b));
        }

        public class Parameter
        {
            internal String _name;
            internal Type _dataClass;
            internal object _defaultValue;
            internal int _ordinal;

            internal Parameter(String name, Type dataClass, object defaultValue, int ordinal)
            {
                this._name = name;
                this._dataClass = dataClass;
                this._defaultValue = defaultValue;
                this._ordinal = ordinal;
            }

            public String getName()
            {
                return _name;
            }

            public Type getDataClass()
            {
                return _dataClass;
            }

            public object getDefaultValue()
            {
                return _defaultValue;
            }

            public override String ToString()
            {
                return _ordinal + ":" + _name + ":" + _dataClass.Name;
            }
        }

        public class Parameter<T> : Parameter
        {
            internal Parameter(String name, Type dataClass, T defaultValue, int ordinal) : base(name, dataClass, defaultValue, ordinal)
            {

            }

            public new T getDefaultValue()
            {
                return (T) _defaultValue;
            }
        }
    }

}