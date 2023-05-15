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
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class GridImage : Image, HasBorder
    {
        private Image[] _images;
        private int[] _weightX;
        private int[] _weightY;
        private Border _border;
        private int _width;
        private int _height;
        private int[] _columnWidth;
        private int[] _rowHeight;
        private int _weightSumX;
        private int _weightSumY;

        public GridImage(Image[] images, int[] weightX, int[] weightY, Border border)
        {
            if (weightX.Length == 0 || weightY.Length == 0)
            {
                throw new ArgumentOutOfRangeException("zero dimension size not allowed");
            }
            if (!(weightX.Length * weightY.Length == images.Length))
            {
                throw new Exception("Assertion exception");
            }
            this._images = images;
            this._weightX = weightX;
            this._weightY = weightY;
            this._border = border;
            this._columnWidth = new int[weightX.Length];
            this._rowHeight = new int[weightY.Length];

            int widthTmp = 0;
            for (int x = 0; x < weightX.Length; x++)
            {
                int widthColumn = 0;
                for (int y = 0; y < weightY.Length; y++)
                {
                    widthColumn = Math.Max(widthColumn, this.GetImage(x, y).Width);
                }
                widthTmp += widthColumn;
                _columnWidth[x] = widthColumn;
            }
            this._width = widthTmp;

            int heightTmp = 0;
            for (int y = 0; y < weightY.Length; y++)
            {
                int heightRow = 0;
                for (int x = 0; x < weightX.Length; x++)
                {
                    heightRow = Math.Max(heightRow, this.GetImage(x, y).Height);
                }
                heightTmp += heightRow;
                _rowHeight[y] = heightRow;
            }
            this._height = heightTmp;

            int tmpSumX = 0;
            foreach (int weight in weightX)
            {
                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException("negative weight in weightX");
                }
                tmpSumX += weight;
            }
            _weightSumX = tmpSumX;

            int tmpSumY = 0;
            foreach (int weight in weightY)
            {
                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException("negative weight in weightY");
                }
                tmpSumY += weight;
            }
            _weightSumY = tmpSumY;

            if (_weightSumX <= 0)
            {
                throw new ArgumentOutOfRangeException("zero weightX not allowed");
            }
            if (_weightSumY <= 0)
            {
                throw new ArgumentOutOfRangeException("zero weightX not allowed");
            }
        }

        private GridImage(Image[] images, GridImage src)
        {
            this._images = images;
            this._weightX = src._weightX;
            this._weightY = src._weightY;
            this._border = src._border;
            this._columnWidth = src._columnWidth;
            this._rowHeight = src._rowHeight;
            this._weightSumX = src._weightSumX;
            this._weightSumY = src._weightSumY;
            this._width = src._width;
            this._height = src._height;
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, _width, _height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int deltaY = height - this._height;
            int remWeightY = _weightSumY;
            for (int yi = 0, idx = 0; yi < _weightY.Length; yi++)
            {
                int heightRow = _rowHeight[yi];
                if (remWeightY > 0)
                {
                    int partY = deltaY * _weightY[yi] / remWeightY;
                    remWeightY -= _weightY[yi];
                    heightRow += partY;
                    deltaY -= partY;
                }

                int tmpX = x;
                int deltaX = width - this._width;
                int remWeightX = _weightSumX;
                for (int xi = 0; xi < _weightX.Length; xi++, idx++)
                {
                    int widthColumn = _columnWidth[xi];
                    if (remWeightX > 0)
                    {
                        int partX = deltaX * _weightX[xi] / remWeightX;
                        remWeightX -= _weightX[xi];
                        widthColumn += partX;
                        deltaX -= partX;
                    }

                    _images[idx].Draw(animationState, tmpX, y, widthColumn, heightRow);
                    tmpX += widthColumn;
                }

                y += heightRow;
            }
        }

        public Border Border
        {
            get
            {
                return _border;
            }
        }

        public Image CreateTintedVersion(Color color)
        {
            Image[] newImages = new Image[_images.Length];
            for (int i = 0; i < newImages.Length; i++)
            {
                newImages[i] = _images[i].CreateTintedVersion(color);
            }
            return new GridImage(newImages, this);
        }

        private Image GetImage(int x, int y)
        {
            return _images[x + y * _weightX.Length];
        }
    }
}
