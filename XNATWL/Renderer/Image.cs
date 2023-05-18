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

using Microsoft.Xna.Framework;

namespace XNATWL.Renderer
{
    /// <summary>
    /// An image object can be used for rendering.
    /// </summary>
    public interface Image
    {
        /// <summary>
        /// The width in pixels of the image
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// The height in pixels of the image
        /// </summary>
        int Height
        {
            get;
        }

        /// <summary>
        /// Draws the image in it's original size at the given location
        /// </summary>
        /// <param name="state">A time source for animation - may be null</param>
        /// <param name="x">left coordinate</param>
        /// <param name="y">top coordinate</param>
        void Draw(AnimationState state, int x, int y);

        /// <summary>
        /// Draws the image scaled to the given size at the given location
        /// </summary>
        /// <param name="state">A time source for animation - may be null</param>
        /// <param name="x">left coordinate</param>
        /// <param name="y">top coordinate</param>
        /// <param name="width">the width in pixels</param>
        /// <param name="height">the height in pixels</param>
        void Draw(AnimationState state, int x, int y, int width, int height);

        /// <summary>
        /// Creates a new image with is tinted with the specified color.
        /// <para>Tinting works by multiplying the color of the image's pixels with the specified color.</para>
        /// </summary>
        /// <param name="color">The color used for tinting.</param>
        /// <returns>a new Image object.</returns>
        Image CreateTintedVersion(Color color);
    }
}
