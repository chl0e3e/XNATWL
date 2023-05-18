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

using Microsoft.Xna.Framework.Graphics;

namespace XNATWL.Renderer.XNA
{
    public class XNATexture : Texture, Resource, QueriablePixels
    {
        public int Width
        {
            get
            {
                return this._width;
            }
        }

        public int Height
        {
            get
            {
                return this._height;
            }
        }

        private Texture2D _texture;
        private int _width;
        private int _height;
        private SpriteBatch _batch;
        private XNARenderer _renderer;

        /// <summary>
        /// Target renderer
        /// </summary>
        public XNARenderer Renderer
        {
            get
            {
                return _renderer;
            }
        }

        /// <summary>
        /// XNA representation of the texture
        /// </summary>
        public Texture2D Texture2D
        {
            get
            {
                return _texture;
            }
        }

        /// <summary>
        /// XNA implementation for batching sprite draw calls
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get
            {
                return _batch;
            }
        }

        /// <summary>
        /// New <see cref="XNATexture"/> given the XNA internal representation of 2D textures ( <see cref="Texture2D"/> )
        /// </summary>
        /// <param name="renderer">Parent renderer</param>
        /// <param name="spriteBatch">Batch to render with</param>
        /// <param name="width">Full width of the texture</param>
        /// <param name="height">Full height of the texture</param>
        /// <param name="texture">XNA texture representation</param>
        public XNATexture(XNARenderer renderer, SpriteBatch spriteBatch, int width, int height, Texture2D texture)
        {
            this._texture = texture;
            this._width = width;
            this._height = height;
            this._batch = spriteBatch;
            this._renderer = renderer;
        }

        /// <summary>
        /// New <see cref="XNATexture"/> given the XNA internal representation of 2D textures ( <see cref="Texture2D"/> )
        /// </summary>
        /// <param name="renderer">Parent renderer</param>
        /// <param name="width">Full width of the texture</param>
        /// <param name="height">Full height of the texture</param>
        /// <param name="texture">XNA texture representation</param>
        public XNATexture(XNARenderer renderer, int width, int height, Texture2D texture) : this(renderer, renderer.SpriteBatch, width, height, texture)
        {
        }

        /// <summary>
        /// Create a new XNACursor using this texture
        /// </summary>
        /// <param name="x">X of cursor</param>
        /// <param name="y">Y of cursor</param>
        /// <param name="width">Width of cursor</param>
        /// <param name="height">Height of cursor</param>
        /// <param name="hotSpotX">Hotspot offset X</param>
        /// <param name="hotSpotY">Hotspot offset Y</param>
        /// <param name="imageRef">Reference image</param>
        /// <returns>new <see cref="XNACursor"/></returns>
        public MouseCursor CreateCursor(int x, int y, int width, int height, int hotSpotX, int hotSpotY, Image imageRef)
        {
            return new XNACursor(this, x, y, width, height, hotSpotX, hotSpotY, imageRef);
        }

        public Image GetImage(int x, int y, int width, int height, Color tintColor, bool tiled, TextureRotation rotation)
        {
            return new TextureArea(this, x, y, width, height, tintColor);
        }

        public int PixelValueAt(int x, int y)
        {
            return 0;
        }

        public void ThemeLoadingDone()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //this._batch.Dispose();
            this._texture.Dispose();
            //throw new NotImplementedException();
        }
    }
}
