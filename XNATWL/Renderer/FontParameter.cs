using static XNATWL.Renderer.FontParameter;
using System;
using System.Collections.Generic;

namespace XNATWL.Renderer
{
    public class FontParameter
    {
        static Dictionary<String, Parameter<object>> parameterMap = new Dictionary<String, Parameter<object>>();

        public static Parameter<Color> COLOR = newParameter("color", Color.WHITE);
        public static Parameter<Boolean> UNDERLINE = newParameter("underline", false);
        public static Parameter<Boolean> LINETHROUGH = newParameter("linethrough", false);

        private Object[] values;

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
        public void put<T>(Parameter<T> param, T value)
        {
            if (value != null && !param.dataClass.isInstance(value))
            {
                throw new ClassCastException("value");
            }
            int ordinal = param.ordinal;
            int curLength = values.length;
            if (ordinal >= curLength)
            {
                Object[] tmp = new Object[Math.max(ordinal + 1, curLength * 2)];
                System.arraycopy(values, 0, tmp, 0, curLength);
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
        public T get<T>(Parameter<T> param)
        {
            if (param.ordinal < values.length)
            {
                Object raw = values[param.ordinal];
                if (raw != null)
                {
                    return param.dataClass.cast(raw);
                }
            }
            return param.defaultValue;
        }

        /**
         * Returns an array of all registered parameter
         * @return an array of all registered parameter
         */
        public static Parameter<object>[] getRegisteredParameter()
        {
            synchronized(parameterMap) {
                return parameterMap.values().toArray(new Parameter<?>[parameterMap.size()]);
            }
        }

        /**
         * Returns the parameter instance for the given name
         * @param name the name to look up
         * @return the parameter instance or null when the name is not registered
         */
        public static Parameter<object> getParameter(String name)
        {
            synchronized(parameterMap) {
                return parameterMap.get(name);
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
        public static Parameter<T> newParameter<T>(String name, T defaultValue)
        {
            Type dataClass = (Type)defaultValue.GetType();
            return newParameter(name, dataClass, defaultValue);
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
        public static<T> Parameter<T> newParameter(String name, Type dataClass, T defaultValue)
        {
            if (name == null)
            {
                throw new NullPointerException("name");
            }
            if (dataClass == null)
            {
                throw new NullPointerException("dataClass");
            }

            synchronized(parameterMap) {
                Parameter<object> existing = parameterMap.get(name);
                if (existing != null)
                {
                    if (existing.dataClass != dataClass || !equals(existing.defaultValue, defaultValue))
                    {
                        throw new IllegalStateException("type '" + name + "' already registered but different");
                    }

                    @SuppressWarnings("unchecked")
                    Parameter<T> type = (Parameter<T>)existing;
                    return type;
                }

                Parameter<T> type = new Parameter<T>(name, dataClass, defaultValue, parameterMap.size());
                parameterMap.put(name, type);
                return type;
            }
        }

        private static boolean equals(Object a, Object b)
        {
            return (a == b) || (a != null && a.equals(b));
        }

        public class Parameter<T>
        {
            String name;
            Type dataClass;
            T defaultValue;
            int ordinal;

            Parameter(String name, Type dataClass, T defaultValue, int ordinal)
            {
                this.name = name;
                this.dataClass = dataClass;
                this.defaultValue = defaultValue;
                this.ordinal = ordinal;
            }

            public String getName()
            {
                return name;
            }

            public Type getDataClass()
            {
                return dataClass;
            }

            public T getDefaultValue()
            {
                return defaultValue;
            }

            public String toString()
            {
                return ordinal + ":" + name + ":" + dataClass.getSimpleName();
            }
        }
    }

}