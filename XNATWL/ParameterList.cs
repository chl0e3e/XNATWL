using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL
{
    public interface ParameterList
    {
        int getSize();

        /**
         * Returns the font at the given list index.
         * If no font with that name was found then the default font is returned.
         *
         * @param idx The index in the list
         * @return A font object
         */
        Font getFont(int idx);

        /**
         * Returns the image at the given list index.
         * If no image with that name was found then null is returned.
         *
         * @param idx The index in the list
         * @return A image object or null.
         */
        Image getImage(int idx);

        /**
         * Returns the mouse cursor at the given list index.
         * If no mouse cursor with that name was found then null is returned.
         *
         * @param idx The index in the list
         * @return A mouse cursor object or null.
         */
        MouseCursor getMouseCursor(int idx);

        /**
         * Returns a parameter map at the given list index.
         * If no parameter map with that name was found then an empty map is returned.
         *
         * @param idx The index in the list
         * @return A parameter map object.
         */
        ParameterMap getParameterMap(int idx);

        /**
         * Returns a parameter list at the given list index.
         * If no parameter map with that name was found then an empty list is returned.
         *
         * @param idx The index in the list
         * @return A parameter list object.
         */
        ParameterList getParameterList(int idx);

        bool getParameter(int idx, bool defaultValue);

        int getParameter(int idx, int defaultValue);

        float getParameter(int idx, float defaultValue);

        string getParameter(int idx, string defaultValue);

        Color getParameter(int idx, Color defaultValue);

        E getParameter<E>(int idx, E defaultValue) where E : struct, IConvertible;

        /**
         * Retrives a parameter.
         * @param idx The index in the list
         * @return the parameter value
         */
        Object getParameterValue(int idx);

        object getParameterValue(int idx, Type type);

        object getParameterValue<T>(int idx, Type type);

    }
}
