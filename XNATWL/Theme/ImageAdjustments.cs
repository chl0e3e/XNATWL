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
using XNATWL.Utils;
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    internal class ImageAdjustments : Image, HasBorder
    {
        internal Renderer.Image image;
        Border border;
        Border inset;
        int sizeOverwriteH;
        int sizeOverwriteV;
        bool center;
        internal StateExpression condition;

        public ImageAdjustments(Renderer.Image image, Border border, Border inset,
                int sizeOverwriteH, int sizeOverwriteV,
                bool center, StateExpression condition)
        {
            this.image = image;
            this.border = border;
            this.inset = inset;
            this.sizeOverwriteH = sizeOverwriteH;
            this.sizeOverwriteV = sizeOverwriteV;
            this.center = center;
            this.condition = condition;
        }

        public int Width
        {
            get
            {
                if (sizeOverwriteH >= 0)
                {
                    return sizeOverwriteH;
                }
                else if (inset != null)
                {
                    return image.Width + inset.BorderLeft + inset.BorderRight;
                }
                else
                {
                    return image.Width;
                }
            }
        }

        public int Height
        {
            get
            {
                if (sizeOverwriteV >= 0)
                {
                    return sizeOverwriteV;
                }
                else if (inset != null)
                {
                    return image.Height + inset.BorderTop + inset.BorderBottom;
                }
                else
                {
                    return image.Height;
                }
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            if (condition == null || condition.Evaluate(animationState))
            {
                if (inset != null)
                {
                    x += inset.BorderLeft;
                    y += inset.BorderTop;
                    width = Math.Max(0, width - inset.BorderLeft - inset.BorderRight);
                    height = Math.Max(0, height - inset.BorderTop - inset.BorderBottom);
                }
                if (center)
                {
                    int w = Math.Min(width, image.Width);
                    int h = Math.Min(height, image.Height);
                    x += (width - w) / 2;
                    y += (height - h) / 2;
                    width = w;
                    height = h;
                }
                image.Draw(animationState, x, y, width, height);
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, image.Width, image.Height);
        }

        public Border Border
        {
            get
            {
                return this.border;
            }
        }

        public Renderer.Image CreateTintedVersion(Color color)
        {
            return new ImageAdjustments(image.CreateTintedVersion(color), border,
                    inset, sizeOverwriteH, sizeOverwriteV, center, condition);
        }

        public bool IsSimple()
        {
            // used for ImageManager.parseStateSelect
            // only check parameters affecting rendering (except condition)
            return !center && inset == null && sizeOverwriteH < 0 && sizeOverwriteV < 0;
        }
    }
}
