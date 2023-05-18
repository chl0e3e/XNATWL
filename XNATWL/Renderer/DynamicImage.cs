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

namespace XNATWL.Renderer
{
    /// <summary>
    /// A dynamic image is created at runtime by the application and can be updated at any time
    /// </summary>
    public interface DynamicImage : Image, Resource
    {
        /// <summary>
        /// Updates the complete image.
        /// </summary>
        /// <param name="data">Texels as contiguous XNA Color elements in a one-dimensional Color array</param>
        void Update(Microsoft.Xna.Framework.Color[] data);
        //void Update(Microsoft.Xna.Framework.Color[] data, DynamicImageFormat format);
        //void Update(Microsoft.Xna.Framework.Color[] data, int stride, DynamicImageFormat format);
        //void Update(int xoffset, int yoffset, int width, int height, byte[] data, DynamicImageFormat format);
        //void Update(int xoffset, int yoffset, int width, int height, byte[] data, int stride, DynamicImageFormat format);
    }

    /// <summary>
    /// Color byte order<br/><br/>
    /// RGBA = Red, Green, Blue, Alpha<br/>
    /// BGRA = Blue, Green, Red, Alpha
    /// </summary>
    public enum DynamicImageFormat
    {
        RGBA,
        BGRA
    }
}
