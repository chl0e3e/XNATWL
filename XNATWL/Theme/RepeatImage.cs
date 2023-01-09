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
    public class RepeatImage : Image, HasBorder, SupportsDrawRepeat
    {
        private Image baseImage;
        private Border border;
        private bool repeatX;
        private bool repeatY;
        private SupportsDrawRepeat sdr;

        public RepeatImage(Image baseImage, Border border, bool repeatX, bool repeatY)
        {
            if (!repeatX && !repeatY)
            {
                throw new Exception("assert exception");
            }

            this.baseImage = baseImage;
            this.border = border;
            this.repeatX = repeatX;
            this.repeatY = repeatY;

            if (baseImage is SupportsDrawRepeat)
            {
                sdr = (SupportsDrawRepeat)baseImage;
            }
            else
            {
                sdr = this;
            }
        }

        public int Width
        {
            get
            {
                return baseImage.Width;
            }
        }

        public int Height
        {
            get
            {
                return baseImage.Height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            baseImage.Draw(animationState, x, y);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int countX = repeatX ? Math.Max(1, width / baseImage.Width) : 1;
            int countY = repeatY ? Math.Max(1, height / baseImage.Height) : 1;
            sdr.Draw(animationState, x, y, width, height, countX, countY);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height, int repeatCountX, int repeatCountY)
        {
            while (repeatCountY > 0)
            {
                int rowHeight = height / repeatCountY;

                int cx = 0;
                for (int xi = 0; xi < repeatCountX;)
                {
                    int nx = ++xi * width / repeatCountX;
                    baseImage.Draw(animationState, x + cx, y, nx - cx, rowHeight);
                    cx = nx;
                }

                y += rowHeight;
                height -= rowHeight;
                repeatCountY--;
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
            return new RepeatImage(baseImage.CreateTintedVersion(color), border, repeatX, repeatY);
        }
    }
}
