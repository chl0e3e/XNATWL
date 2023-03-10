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
        private Image[] images;
        private int[] weightX;
        private int[] weightY;
        private Border border;
        private int width;
        private int height;
        private int[] columnWidth;
        private int[] rowHeight;
        private int weightSumX;
        private int weightSumY;

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
            this.images = images;
            this.weightX = weightX;
            this.weightY = weightY;
            this.border = border;
            this.columnWidth = new int[weightX.Length];
            this.rowHeight = new int[weightY.Length];

            int widthTmp = 0;
            for (int x = 0; x < weightX.Length; x++)
            {
                int widthColumn = 0;
                for (int y = 0; y < weightY.Length; y++)
                {
                    widthColumn = Math.Max(widthColumn, this.GetImage(x, y).Width);
                }
                widthTmp += widthColumn;
                columnWidth[x] = widthColumn;
            }
            this.width = widthTmp;

            int heightTmp = 0;
            for (int y = 0; y < weightY.Length; y++)
            {
                int heightRow = 0;
                for (int x = 0; x < weightX.Length; x++)
                {
                    heightRow = Math.Max(heightRow, this.GetImage(x, y).Height);
                }
                heightTmp += heightRow;
                rowHeight[y] = heightRow;
            }
            this.height = heightTmp;

            int tmpSumX = 0;
            foreach (int weight in weightX)
            {
                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException("negative weight in weightX");
                }
                tmpSumX += weight;
            }
            weightSumX = tmpSumX;

            int tmpSumY = 0;
            foreach (int weight in weightY)
            {
                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException("negative weight in weightY");
                }
                tmpSumY += weight;
            }
            weightSumY = tmpSumY;

            if (weightSumX <= 0)
            {
                throw new ArgumentOutOfRangeException("zero weightX not allowed");
            }
            if (weightSumY <= 0)
            {
                throw new ArgumentOutOfRangeException("zero weightX not allowed");
            }
        }

        private GridImage(Image[] images, GridImage src)
        {
            this.images = images;
            this.weightX = src.weightX;
            this.weightY = src.weightY;
            this.border = src.border;
            this.columnWidth = src.columnWidth;
            this.rowHeight = src.rowHeight;
            this.weightSumX = src.weightSumX;
            this.weightSumY = src.weightSumY;
            this.width = src.width;
            this.height = src.height;
        }


        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, width, height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int deltaY = height - this.height;
            int remWeightY = weightSumY;
            for (int yi = 0, idx = 0; yi < weightY.Length; yi++)
            {
                int heightRow = rowHeight[yi];
                if (remWeightY > 0)
                {
                    int partY = deltaY * weightY[yi] / remWeightY;
                    remWeightY -= weightY[yi];
                    heightRow += partY;
                    deltaY -= partY;
                }

                int tmpX = x;
                int deltaX = width - this.width;
                int remWeightX = weightSumX;
                for (int xi = 0; xi < weightX.Length; xi++, idx++)
                {
                    int widthColumn = columnWidth[xi];
                    if (remWeightX > 0)
                    {
                        int partX = deltaX * weightX[xi] / remWeightX;
                        remWeightX -= weightX[xi];
                        widthColumn += partX;
                        deltaX -= partX;
                    }

                    images[idx].Draw(animationState, tmpX, y, widthColumn, heightRow);
                    tmpX += widthColumn;
                }

                y += heightRow;
            }
        }

        public Border Border
        {
            get
            {
                return border;
            }
        }

        public Image CreateTintedVersion(Color color)
        {
            Image[] newImages = new Image[images.Length];
            for (int i = 0; i < newImages.Length; i++)
            {
                newImages[i] = images[i].CreateTintedVersion(color);
            }
            return new GridImage(newImages, this);
        }

        private Image GetImage(int x, int y)
        {
            return images[x + y * weightX.Length];
        }
    }
}
