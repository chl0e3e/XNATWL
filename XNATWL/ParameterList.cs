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
