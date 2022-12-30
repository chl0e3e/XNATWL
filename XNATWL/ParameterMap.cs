using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL
{
    public interface ParameterMap
    {
        /**
         * Returns the font with the given name.
         * If no font with that name was found then the default font is returned.
         *
         * @param name The name of the font
         * @return A font object
         */
        Font getFont(string name);

        /**
         * Returns the image with the given name.
         * If no image with that name was found then null is returned.
         *
         * @param name The name of the image.
         * @return A image object or null.
         */
        Image getImage(string name);

        /**
         * Returns the mouse cursor with the given name.
         * If no mouse cursor with that name was found then null is returned.
         *
         * @param name The name of the mouse cursor.
         * @return A mouse cursor object or null.
         */
        MouseCursor getMouseCursor(string name);

        /**
         * Returns a parameter map with the given name.
         * If no parameter map with that name was found then an empty map is returned.
         *
         * @param name The name of the parameter map.
         * @return A parameter map object.
         */
        ParameterMap getParameterMap(string name);

        /**
         * Returns a parameter list with the given name.
         * If no parameter map with that name was found then an empty list is returned.
         *
         * @param name The name of the parameter list.
         * @return A parameter list object.
         */
        ParameterList getParameterList(string name);

        bool getParameter(string name, bool defaultValue);

        int getParameter(string name, int defaultValue);

        float getParameter(string name, float defaultValue);

        string getParameter(string name, string defaultValue);

        Color getParameter(string name, Color defaultValue);

        E getParameter<E>(string name, E defaultValue) where E : struct, IConvertible;

        /**
         * Retrives a parameter.
         * @param name the parameter name
         * @param warnIfNotPresent if true and the parameter was not set then a warning is issued
         * @return the parameter value
         */
        Object getParameterValue(string name, bool warnIfNotPresent);

        /**
         * Retrieves a parameter and ensures that it has the desired type.
         * @param <T> The desired return type generic
         * @param name the parameter name
         * @param warnIfNotPresent if true a warning is generated if the parameter was not found or has wrong type
         * @param clazz the required data type
         * @return the parameter value or null if the type does not match
         */
        object getParameterValue(string name, bool warnIfNotPresent, Type type);

        /**
         * Retrieves a parameter and ensures that it has the desired type.
         * @param <T> The desired return type generic
         * @param name the parameter name
         * @param warnIfNotPresent if true a warning is generated if the parameter was not found or has wrong type
         * @param clazz the required data type
         * @param defaultValue the default value
         * @return the parameter value or the defaultValue if the type does not match
         */
        T getParameterValue<T>(string name, bool warnIfNotPresent, Type type, T defaultValue);

        object getParameterValue(string name, bool warnIfNotPresent, Type type, object defaultValue);
    }
}
