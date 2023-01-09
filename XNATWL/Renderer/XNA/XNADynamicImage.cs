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
    public class XNADynamicImage : TextureAreaBase, DynamicImage
    {
        private XNARenderer _renderer;
        private Color tintColor;

        public XNADynamicImage(XNARenderer renderer, int width, int height, Color tintColor) : base(null, 0, 0, width, height)
        {
            this.tintColor = tintColor;
            this._renderer = renderer;
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
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
            this.drawQuad(this.tintColor, x, y, this.width, this.height);
        }

        public void Draw(AnimationState state, int x, int y, int width, int height)
        {
            this.drawQuad(this.tintColor, x, y, width, height);
        }

        public void Update(Microsoft.Xna.Framework.Color[] data)
        {
            if (this._texture != null)
            {
                this._texture.Dispose();
            }

            Texture2D texture2D = new Texture2D(this._renderer.GraphicsDevice, this.width, this.height);
            texture2D.SetData(data);
            this._texture = new XNATexture(this._renderer, this.width, this.height, texture2D);
        }
    }
}
