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
    public interface ParameterMap
    {
        /// <summary>
        /// Returns the <see cref="Font"/> with the given name. If no font with that name was found then the default font is returned.
        /// </summary>
        /// <param name="name">The name of the font</param>
        /// <returns>A <see cref="Font"/> object</returns>
        Font GetFont(string name);

        /// <summary>
        /// Returns the <see cref="Image"/> with the given name. If no image with that name was found then null is returned.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <returns>A <see cref="Image"/> object or null.</returns>
        Image GetImage(string name);

        /// <summary>
        /// Returns the <see cref="MouseCursor"/> with the given name. If no mouse cursor with that name was found then null is returned.
        /// </summary>
        /// <param name="name">The name of the mouse cursor.</param>
        /// <returns>A <see cref="MouseCursor"/> object or null.</returns>
        MouseCursor GetMouseCursor(string name);

        /// <summary>
        /// Returns a <see cref="ParameterMap"/> with the given name. If no parameter map with that name was found then an empty map is returned.
        /// </summary>
        /// <param name="name">The name of the parameter map.</param>
        /// <returns>A <see cref="ParameterMap"/> object.</returns>
        ParameterMap GetParameterMap(string name);

        /// <summary>
        /// Returns a <see cref="ParameterList"/> with the given name. If no parameter map with that name was found then an empty list is returned.
        /// </summary>
        /// <param name="name">The name of the parameter list.</param>
        /// <returns>A <see cref="ParameterList"/> object.</returns>
        ParameterList GetParameterList(string name);

        /// <summary>
        /// Returns the <see cref="object"/> with the given name. If no object with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>an <see cref="object"/></returns>
        object GetParameter(string name, object defaultValue);

        /// <summary>
        /// Returns the <see cref="bool"/> with the given name. If no boolean with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>a <see cref="bool"/></returns>
        bool GetParameter(string name, bool defaultValue);

        /// <summary>
        /// Returns the <see cref="int"/> with the given name. If no integer with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>a <see cref="int"/></returns>
        int GetParameter(string name, int defaultValue);

        /// <summary>
        /// Returns the <see cref="float"/> with the given name. If no float with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>a <see cref="float"/></returns>
        float GetParameter(string name, float defaultValue);

        /// <summary>
        /// Returns the <see cref="string"/> with the given name. If no string with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>a <see cref="string"/></returns>
        string GetParameter(string name, string defaultValue);

        /// <summary>
        /// Returns the <see cref="Color"/> with the given name. If no string with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>a <see cref="Color"/></returns>
        Color GetParameter(string name, Color defaultValue);

        /// <summary>
        /// Returns the <see cref="Enum"/> with the given name. If no string with that name was found then the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <typeparam name="E">Enum type</typeparam>
        /// <param name="name">The name of the object.</param>
        /// <param name="defaultValue">Default value if name not found</param>
        /// <returns>a <see cref="Enum"/></returns>
        E GetParameter<E>(string name, E defaultValue) where E : struct, IConvertible;

        /// <summary>
        /// Retrieves a parameter.
        /// </summary>
        /// <param name="name">the parameter name</param>
        /// <param name="warnIfNotPresent">if true and the parameter was not set then a warning is issued</param>
        /// <returns>the parameter value</returns>
        object GetParameterValue(string name, bool warnIfNotPresent);

        /// <summary>
        /// Retrieves a parameter and ensures that it has the desired type.
        /// </summary>
        /// <param name="name">the parameter name</param>
        /// <param name="warnIfNotPresent">if true a warning is generated if the parameter was not found or has wrong type</param>
        /// <param name="type">the required data type</param>
        /// <returns>the parameter value or null if the type does not match</returns>
        object GetParameterValue(string name, bool warnIfNotPresent, Type type);

        /// <summary>
        /// Retrieves a parameter and ensures that it has the desired type.
        /// </summary>
        /// <typeparam name="T">The desired return type generic</typeparam>
        /// <param name="name">the parameter name</param>
        /// <param name="warnIfNotPresent">if true a warning is generated if the parameter was not found or has wrong type</param>
        /// <param name="type">the required data type</param>
        /// <param name="defaultValue">the default value</param>
        /// <returns>the parameter value or the defaultValue if the type does not match</returns>
        T GetParameterValue<T>(string name, bool warnIfNotPresent, Type type, T defaultValue);

        /// <summary>
        /// Retrieves a parameter.
        /// </summary>
        /// <param name="name">the parameter name</param>
        /// <param name="warnIfNotPresent">if true and the parameter was not set then a warning is issued</param>
        /// <param name="type">the required data type</param>
        /// <param name="defaultValue">the default value</param>
        /// <returns>the parameter value or the defaultValue if the type does not match</returns>
        object GetParameterValue(string name, bool warnIfNotPresent, Type type, object defaultValue);
    }
}
