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
using System;

namespace XNATWL.Renderer.XNA
{
    /// <summary>
    /// A <see cref="TextureAreaBase"/> which deals with dynamic images (whose texture is set by calling <see cref="Update(Microsoft.Xna.Framework.Color[])"/>
    /// </summary>
    public class XNADynamicImage : TextureAreaBase, DynamicImage, IDisposable
    {
        private XNARenderer _renderer;
        private Color _tintColor;
        
        /// <summary>
        /// Create a new dynamic image
        /// </summary>
        /// <param name="renderer">Renderer to which the image belongs</param>
        /// <param name="width">Maximum X coordinate</param>
        /// <param name="height">Maximum Y coordinate</param>
        /// <param name="tintColor">After-render tinting</param>
        public XNADynamicImage(XNARenderer renderer, int width, int height, Color tintColor) : base(null, 0, 0, width, height)
        {
            this._tintColor = tintColor;
            this._renderer = renderer;
        }

        public Image CreateTintedVersion(Color color)
        {
            return this;
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Draw(AnimationState state, int x, int y)
        {
            this.DrawQuad(this._tintColor, x, y, this._width, this._height);
        }

        public void Draw(AnimationState state, int x, int y, int width, int height)
        {
            this.DrawQuad(this._tintColor, x, y, width, height);
        }

        public void Update(Microsoft.Xna.Framework.Color[] data)
        {
            if (this._texture != null)
            {
                this._texture.Dispose();
            }

            Texture2D texture2D = new Texture2D(this._renderer.GraphicsDevice, this._width, this._height);
            texture2D.SetData(data);
            this._texture = new XNATexture(this._renderer, this._width, this._height, texture2D);
        }
    }
}
