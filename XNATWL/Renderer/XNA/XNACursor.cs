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
    public class XNACursor : TextureAreaBase, MouseCursor
    {
        private int hotSpotX = 0;
        private int hotSpotY = 0;
        private Image imageRef = null;

        public XNACursor(XNATexture texture, int x, int y, int width, int height, int hotSpotX, int hotSpotY) : base(texture, x, y, width, height)
        {
            this.hotSpotX = hotSpotX;
            this.hotSpotY = hotSpotY;
        }

        public XNACursor(XNATexture texture, int x, int y, int width, int height, int hotSpotX, int hotSpotY, Image imageRef) : this(texture, x, y, width, height, hotSpotX, hotSpotY)
        {
            this.hotSpotX = hotSpotX;
            this.hotSpotY = hotSpotY;
            this.imageRef = imageRef;
        }

        internal override void drawQuad(Color color, int x, int y, int w, int h)
        {
            if (imageRef != null)
            {
                imageRef.Draw(this._texture.Renderer.CursorAnimationState, x - hotSpotX, y - hotSpotY);
            }
            else
            {
                this._texture.SpriteBatch.Draw(this._texture.Texture2D, new Microsoft.Xna.Framework.Rectangle(x - hotSpotX, y - hotSpotY, w, h), new Microsoft.Xna.Framework.Rectangle(this.tx0, this.ty0, this.tw, this.th), this._texture.Renderer.TintStack.TintColorForXNA(color));
            }
        }
    }
}
