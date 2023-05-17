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

using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;

namespace XNATWL.Model
{
    /// <summary>
    /// A color space used by the color selector widget.
    /// <br/>
    /// It supports a variable number of color components.
    /// <br/>
    /// It does not include an alpha channel.
    /// </summary>
    public interface ColorSpace
    {
        /// <summary>
        /// Identifying name of the ColorSpace
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// The number of component for this model. Must be >= 3.
        /// </summary>
        int Components
        {
            get;
        }

        /// <summary>
        /// Returns the name of the specified color component.
        /// </summary>
        /// <param name="component">the color component index</param>
        /// <returns>the name of the color component</returns>
        string ComponentNameOf(int component);

        /// <summary>
        /// A short version of the component name for use in UIs. For best results
        /// all short names should have the same length.
        /// </summary>
        /// <param name="component">the color component index</param>
        /// <returns>the short name of the color component</returns>
        string ComponentShortNameOf(int component);

        /// <summary>
        /// Returns the minimum allowed value for the specified component.
        /// </summary>
        /// <param name="component">the color component index</param>
        /// <returns>the minimum value</returns>
        float ComponentMinValueOf(int component);

        /// <summary>
        /// Returns the maximum allowed value for the specified component.
        /// </summary>
        /// <param name="component">the color component index</param>
        /// <returns>the maximum value</returns>
        float ComponentMaxValueOf(int component);

        /// <summary>
        /// Returns the default component for the initial color
        /// </summary>
        /// <param name="component">the color component index</param>
        /// <returns>the color component index</returns>
        float ComponentDefaultValueOf(int component);

        /// <summary>
        /// Converts the specified color into a RGB value without alpha part.<br/>
        /// This convertion is not exact.<br/>
        /// <br/><br/>
        /// bits  0- 7 are blue,
        /// bits  8-15 are green,
        /// bits 16-23 are red,
        /// bits 24-31 must be 0
        /// </summary>
        /// <param name="color">the color values</param>
        /// <returns>the RGB value</returns>
        int RGB(float[] color);

        /// <summary>
        /// Converts the given RGB value into color values for this color space.
        /// </summary>
        /// <param name="rgb">the RGB value</param>
        /// <returns>the color values coresponding to the RGB value</returns>
        float[] FromRGB(int rgb);
    }
}
