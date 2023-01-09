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

namespace XNATWL.Renderer.XNA
{
    public class TextureAreaBase
    {
        protected int tx0;
        protected int ty0;
        protected int tw;
        protected int th;
        protected int width;
        protected int height;
        protected XNATexture _texture;
        protected bool beginDraw = false;

        public TextureAreaBase(XNATexture texture, int x, int y, int width, int height)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("No texture!");
            }

            this.tx0 = x;
            this.ty0 = y;
            this.tw = width;
            this.th = height;
            this.width = width;
            this.height = height;
            this._texture = texture;
        }

        public TextureAreaBase(TextureAreaBase src)
        {
            this.tx0 = src.tx0;
            this.ty0 = src.ty0;
            this.tw = src.tw;
            this.th = src.th;
            this.width = src.width;
            this.height = src.height;
            this._texture = src._texture;
        }

        public int getWidth()
        {
            return this.tw;
        }

        public int getHeight()
        {
            return this.th;
        }

        internal virtual void drawQuad(Color color, int x, int y, int w, int h)
        {
            this._texture.SpriteBatch.Draw(this._texture.Texture2D, new Microsoft.Xna.Framework.Rectangle(x, y, w, h), new Microsoft.Xna.Framework.Rectangle(this.tx0, this.ty0, this.tw, this.th), this._texture.Renderer.TintStack.TintColorForXNA(color));
        }
    }
}
