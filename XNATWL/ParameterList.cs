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
    /// <summary>
    /// An interface representing a list of theme parameters
    /// </summary>
    public interface ParameterList
    {
        /// <summary>
        /// Size of the list
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Returns the <see cref="Font"/> at the given list index. If no font with that name was found then the default font is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <returns>A font object</returns>
        Font GetFont(int idx);

        /// <summary>
        /// Returns the <see cref="Image"/> at the given list index. If no image with that name was found then null is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <returns>A image object or null.</returns>
        Image GetImage(int idx);

        /// <summary>
        /// Returns the <see cref="MouseCursor"/> at the given list index. If no mouse cursor with that name was found then null is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <returns>A mouse cursor object or null.</returns>
        MouseCursor GetMouseCursor(int idx);

        /// <summary>
        /// Returns a <see cref="ParameterMap"/> at the given list index. If no parameter map with that name was found then an empty map is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <returns>A parameter map object.</returns>
        ParameterMap GetParameterMap(int idx);

        /// <summary>
        /// Returns a <see cref="ParameterList"/> at the given list index. If no parameter map with that name was found then an empty list is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <returns>A parameter list object.</returns>
        ParameterList GetParameterList(int idx);

        /// <summary>
        /// Returns a <see cref="bool"/> at the given list index. If no value with that index was found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <param name="defaultValue">Default value if index not found</param>
        /// <returns>A <see cref="bool"/> value</returns>
        bool GetParameter(int idx, bool defaultValue);

        /// <summary>
        /// Returns an <see cref="int"/> at the given list index. If no value with that index was found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <param name="defaultValue">Default value if index not found</param>
        /// <returns>An <see cref="int"/> value</returns>
        int GetParameter(int idx, int defaultValue);

        /// <summary>
        /// Returns a <see cref="float"/> at the given list index. If no value with that index was found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <param name="defaultValue">Default value if index not found</param>
        /// <returns>A <see cref="float"/> value</returns>
        float GetParameter(int idx, float defaultValue);

        /// <summary>
        /// Returns a <see cref="string"/> at the given list index. If no value with that index was found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <param name="defaultValue">Default value if index not found</param>
        /// <returns>A <see cref="string"/> value</returns>
        string GetParameter(int idx, string defaultValue);

        /// <summary>
        /// Returns a <see cref="Color"/> at the given list index. If no value with that index was found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <param name="defaultValue">Default value if index not found</param>
        /// <returns>A <see cref="Color"/> value</returns>
        Color GetParameter(int idx, Color defaultValue);

        /// <summary>
        /// Returns an <see cref="Enum"/> at the given list index. If no value with that index was found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <typeparam name="E">Enum type</typeparam>
        /// <param name="idx">The index in the list</param>
        /// <param name="defaultValue">Default value if index not found</param>
        /// <returns>A enum value</returns>
        E GetParameter<E>(int idx, E defaultValue) where E : struct, IConvertible;

        /// <summary>
        /// Retrieves a parameter.
        /// </summary>
        /// <param name="idx">The index in the list</param>
        /// <returns>the parameter value</returns>
        object GetParameterValue(int idx);

        /// <summary>
        /// Retrieves a parameter as an object given a type.
        /// </summary>
        /// <param name="idx">The index in the list</param
        /// <param name="type">The type of object</param>
        /// <returns>the parameter value</returns>
        object GetParameterValue(int idx, Type type);

        /// <summary>
        /// Retrieves a parameter as an object given a type.
        /// </summary>
        /// <param name="idx">The index in the list</param
        /// <param name="type">The type of object</param>
        /// <returns>the parameter value</returns>
        T GetParameterValue<T>(int idx, Type type);

    }
}
