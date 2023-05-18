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
    /// <summary>
    /// An area inside an XNATexture, used for spriting. It extends <see cref="TextureAreaBase"/> with repeated drawing methods and animation state handles
    /// </summary>
    public class TextureArea : TextureAreaBase, Image, SupportsDrawRepeat, QueriablePixels
    {
        protected static int REPEAT_CACHE_SIZE = 10;

        protected Color _tintColor;
        protected int _repeatCacheID = -1;

        /// <summary>
        /// Construct a sub-texture by using an <see cref="XNATexture"/> as a sprite sheet
        /// </summary>
        /// <param name="texture">Master texture</param>
        /// <param name="x">Sprite X</param>
        /// <param name="y">Sprite Y</param>
        /// <param name="width">Sprite width</param>
        /// <param name="height">Sprite height</param>
        /// <param name="tintColor">Tint to recolour</param>
        public TextureArea(XNATexture texture, int x, int y, int width, int height, Color tintColor) : base(texture, x, y, width, height)
        {
            this._tintColor = (tintColor == null) ? Color.WHITE : tintColor;
        }

        /// <summary>
        /// Duplicate a texture area constructed using the same <see cref="TextureArea"/> class
        /// </summary>
        /// <param name="src">Duplicated texture</param>
        /// <param name="tintColor">Tint to recolour</param>
        public TextureArea(TextureArea src, Color tintColor) : base(src)
        {
            this._tintColor = tintColor;
        }

        public int PixelValueAt(int x, int y)
        {
            int texWidth = _texture.Width;
            int texHeight = _texture.Height;

            int baseX = (int)(_tx0 * texWidth);
            int baseY = (int)(_ty0 * texHeight);

            if (_tx0 > _width)
            {
                x = baseX - x;
            }
            else
            {
                x = baseX + x;
            }

            if (_ty0 > _height)
            {
                y = baseY - y;
            }
            else
            {
                y = baseY + y;
            }

            if (x < 0)
            {
                x = 0;
            }
            else if (x >= texWidth)
            {
                x = texWidth - 1;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y >= texHeight)
            {
                y = texHeight - 1;
            }

            return this._texture.PixelValueAt(x, y);
        }

        public void Draw(AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, _width, _height);
        }

        public void Draw(AnimationState animationState, int x, int y, int w, int h)
        {
            DrawQuad(this._tintColor, x, y, w, h);
        }

        public void Draw(AnimationState animationState, int x, int y, int width, int height, int repeatCountX, int repeatCountY)
        {
            if ((repeatCountX * this._width != width) || (repeatCountY * this._height != height))
            {
                DrawRepeatSlow(x, y, width, height, repeatCountX, repeatCountY);
                return;
            }

            if (repeatCountX < REPEAT_CACHE_SIZE || repeatCountY < REPEAT_CACHE_SIZE)
            {
                DrawRepeat(x, y, repeatCountX, repeatCountY);
                return;
            }

            DrawRepeatCached(x, y, repeatCountX, repeatCountY);
        }

        /// <summary>
        /// Draw image repeatedly using a slow method and without an AnimationState
        /// </summary>
        /// <param name="x">Left coordinate</param>
        /// <param name="y">Top coordinate</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="repeatCountX">Number of times to repeat the image on the X axis</param>
        /// <param name="repeatCountY">Number of times to repeat the image on the Y axis</param>
        private void DrawRepeatSlow(int x, int y, int width, int height, int repeatCountX, int repeatCountY)
        {
            while (repeatCountY > 0)
            {
                int rowHeight = height / repeatCountY;

                int cx = 0;
                for (int xi = 0; xi < repeatCountX;)
                {
                    int nx = ++xi * width / repeatCountX;
                    DrawQuad(this._tintColor, x + cx, y, nx - cx, rowHeight);
                    cx = nx;
                }

                y += rowHeight;
                height -= rowHeight;
                repeatCountY--;
            }
        }

        /// <summary>
        /// Draw image repeatedly without an AnimationState
        /// </summary>
        /// <param name="x">Left coordinate</param>
        /// <param name="y">Top coordinate</param>
        /// <param name="repeatCountX">Number of times to repeat the image on the X axis</param>
        /// <param name="repeatCountY">Number of times to repeat the image on the Y axis</param>
        protected void DrawRepeat(int x, int y, int repeatCountX, int repeatCountY)
        {
            int w = _width;
            int h = _height;
            //GL11.glBegin(GL11.GL_QUADS);
            while (repeatCountY-- > 0)
            {
                int curX = x;
                int cntX = repeatCountX;
                while (cntX-- > 0)
                {
                    DrawQuad(this._tintColor, curX, y, w, h);
                    curX += w;
                }
                y += h;
            }
            //GL11.glEnd();
        }

        /// <summary>
        /// Cache-draw image repeatedly without an AnimationState
        /// </summary>
        /// <param name="x">Left coordinate</param>
        /// <param name="y">Top coordinate</param>
        /// <param name="repeatCountX">Number of times to repeat the image on the X axis</param>
        /// <param name="repeatCountY">Number of times to repeat the image on the Y axis</param>
        protected void DrawRepeatCached(int x, int y, int repeatCountX, int repeatCountY)
        {
            if (_repeatCacheID < 0)
            {
                CreateRepeatCache();
            }

            int cacheBlocksX = repeatCountX / REPEAT_CACHE_SIZE;
            int repeatsByCacheX = cacheBlocksX * REPEAT_CACHE_SIZE;

            if (repeatCountX > repeatsByCacheX)
            {
                DrawRepeat(x + _width * repeatsByCacheX, y,
                        repeatCountX - repeatsByCacheX, repeatCountY);
            }

            do
            {
                throw new NotImplementedException();
                //GL11.glPushMatrix();
                //GL11.glTranslatef(x, y, 0f);
                //GL11.glCallList(repeatCacheID);

                for (int i = 1; i < cacheBlocksX; i++)
                {
                    //GL11.glTranslatef(width * REPEAT_CACHE_SIZE, 0f, 0f);
                    //GL11.glCallList(repeatCacheID);
                }

                //GL11.glPopMatrix();
                repeatCountY -= REPEAT_CACHE_SIZE;
                y += _height * REPEAT_CACHE_SIZE;
            } while (repeatCountY >= REPEAT_CACHE_SIZE);

            if (repeatCountY > 0)
            {
                DrawRepeat(x, y, repeatsByCacheX, repeatCountY);
            }
        }

        /// <summary>
        /// <b>NOT IMPLEMENTED:</b> Provide a cached texture for a repeated draw
        /// </summary>
        /// <exception cref="NotImplementedException">This function is not implemented</exception>
        protected void CreateRepeatCache()
        {
            throw new NotImplementedException();
            //repeatCacheID = GL11.glGenLists(1);
            //texture.renderer.textureAreas.add(this);

            //GL11.glNewList(repeatCacheID, GL11.GL_COMPILE);
            //drawRepeat(0, 0, REPEAT_CACHE_SIZE, REPEAT_CACHE_SIZE);
            //GL11.glEndList();
        }

        /// <summary>
        /// <b>NOT IMPLEMENTED:</b> Destroy a cached texture provided by <see cref="CreateRepeatCache"/>
        /// </summary>
        /// <exception cref="NotImplementedException">This function is not implemented</exception>
        void DestroyRepeatCache()
        {
            throw new NotImplementedException();
            //GL11.glDeleteLists(repeatCacheID, 1);
            //repeatCacheID = -1;
        }

        /// <summary>
        /// Multiply current texture tint by a new one
        /// </summary>
        /// <param name="color">Newly tinted colour</param>
        /// <returns>Tinted image</returns>
        /// <exception cref="NullReferenceException"></exception>
        public Image CreateTintedVersion(Color color)
        {
            if (color == null)
            {
                throw new NullReferenceException("color");
            }
            Color newTintColor = _tintColor.Multiply(color);
            if (newTintColor.Equals(_tintColor))
            {
                return this;
            }
            return new TextureArea(this, newTintColor);
        }
    }
}
