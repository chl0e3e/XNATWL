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
    /// <summary>
    /// An <see cref="TextureAreaBase"/> which specifically deals with mouse cursors
    /// </summary>
    public class XNACursor : TextureAreaBase, MouseCursor
    {
        private int _hotSpotX = 0;
        private int _hotSpotY = 0;
        private Image _imageRef = null;

        /// <summary>
        /// A cursor image referenced in another <see cref="XNATexture"/>
        /// </summary>
        /// <param name="texture">Master texture</param>
        /// <param name="x">X coordinate in texture</param>
        /// <param name="y">Y coordinate in texture</param>
        /// <param name="width">Width in texture</param>
        /// <param name="height">Height in texture</param>
        /// <param name="hotSpotX">X hotspot offset in texture</param>
        /// <param name="hotSpotY">Y hotspot offset in texture</param>
        public XNACursor(XNATexture texture, int x, int y, int width, int height, int hotSpotX, int hotSpotY) : base(texture, x, y, width, height)
        {
            this._hotSpotX = hotSpotX;
            this._hotSpotY = hotSpotY;
        }

        /// <summary>
        /// A cursor image referenced in another <see cref="XNATexture"/>
        /// </summary>
        /// <param name="texture">Master texture</param>
        /// <param name="x">X coordinate in texture</param>
        /// <param name="y">Y coordinate in texture</param>
        /// <param name="width">Width in texture</param>
        /// <param name="height">Height in texture</param>
        /// <param name="hotSpotX">X hotspot offset in texture</param>
        /// <param name="hotSpotY">Y hotspot offset in texture</param>
        /// <param name="imageRef">An overriding image to draw instead</param>
        public XNACursor(XNATexture texture, int x, int y, int width, int height, int hotSpotX, int hotSpotY, Image imageRef) : this(texture, x, y, width, height, hotSpotX, hotSpotY)
        {
            this._hotSpotX = hotSpotX;
            this._hotSpotY = hotSpotY;
            this._imageRef = imageRef;
        }

        protected internal override void DrawQuad(Color color, int x, int y, int w, int h)
        {
            if (_imageRef != null)
            {
                _imageRef.Draw(this._texture.Renderer.CursorAnimationState, x - _hotSpotX, y - _hotSpotY);
            }
            else
            {
                this._texture.SpriteBatch.Draw(this._texture.Texture2D, new Microsoft.Xna.Framework.Rectangle(x - _hotSpotX, y - _hotSpotY, w, h), new Microsoft.Xna.Framework.Rectangle(this._tx0, this._ty0, this._textureWidth, this._textureHeight), this._texture.Renderer.TintStack.XNATint(color));
            }
        }
    }
}
