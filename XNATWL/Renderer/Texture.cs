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
    /// Enum for clockwise rotations of a texture
    /// </summary>
    public enum TextureRotation
    {
        None,
        Clockwise90,
        Clockwise180,
        Clockwise270
    }

    /// <summary>
    /// A texture class. Can not be used for rendering directly.
    /// </summary>
    public interface Texture
    {
        /// <summary>
        /// The width in pixels of this texture.
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// The height in pixels of this texture.
        /// </summary>
        int Height
        {
            get;
        }

        /// <summary>
        /// Creates an image from a sub section of this texture.
        /// </summary>
        /// <param name="x">left coordinate in the texture of the image</param>
        /// <param name="y">y top coordinate in the texture of the image</param>
        /// <param name="width">width in pixels of the image - if negative the image is horizontaly flipped</param>
        /// <param name="height">height in pixels of the image - if negative the image is vertically flipped</param>
        /// <param name="tintColor">the tintColor - may be null</param>
        /// <param name="tiled"><strong>true</strong> if this image should do tiled rendering</param>
        /// <param name="rotation">the rotation to apply to this sub section while rendering</param>
        /// <returns>an image object</returns>
        Image GetImage(int x, int y, int width, int height, Color tintColor, bool tiled, TextureRotation rotation);

        MouseCursor CreateCursor(int x, int y, int width, int height, int hotSpotX, int hotSpotY, Image imageRef);

        /// <summary>
        /// After calling this function <see cref="GetImage"/> and <see cref="CreateCursor"/> may fail to work
        /// </summary>
        void ThemeLoadingDone();
    }
}
