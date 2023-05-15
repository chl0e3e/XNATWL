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
        protected int _tx0;
        protected int _ty0;
        protected int _textureWidth;
        protected int _textureHeight;
        protected int _width;
        protected int _height;
        protected XNATexture _texture;

        public TextureAreaBase(XNATexture texture, int x, int y, int width, int height)
        {
            this._tx0 = x;
            this._ty0 = y;
            this._textureWidth = width;
            this._textureHeight = height;
            this._width = width;
            this._height = height;
            this._texture = texture;
        }

        public TextureAreaBase(TextureAreaBase src)
        {
            this._tx0 = src._tx0;
            this._ty0 = src._ty0;
            this._textureWidth = src._textureWidth;
            this._textureHeight = src._textureHeight;
            this._width = src._width;
            this._height = src._height;
            this._texture = src._texture;
        }

        public int getWidth()
        {
            return this._textureWidth;
        }

        public int getHeight()
        {
            return this._textureHeight;
        }

        internal virtual void DrawQuad(Color color, int x, int y, int w, int h)
        {
            this._texture.SpriteBatch.Draw(this._texture.Texture2D, new Microsoft.Xna.Framework.Rectangle(x, y, w, h), new Microsoft.Xna.Framework.Rectangle(this._tx0, this._ty0, this._textureWidth, this._textureHeight), this._texture.Renderer.TintStack.TintColorForXNA(color));
        }
    }
}
