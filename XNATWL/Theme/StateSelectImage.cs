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

namespace XNATWL.Theme
{
    public class StateSelectImage : Renderer.Image, HasBorder
    {
        public Renderer.Image[] images;
        public StateSelect select;
        public Border border;

        public StateSelectImage(StateSelect select, Border border, params Renderer.Image[] ximages)
        {
            if (!(ximages.Length >= select.Expressions()))
            {
                throw new Exception("Assert exception");
            }
            if (!(ximages.Length <= select.Expressions() + 1))
            {
                throw new Exception("Assert exception");
            }

            this.images = ximages;
            this.select = select;
            this.border = border;
        }

        public int Width
        {
            get
            {
                return images[0].Width;
            }
        }

        public int Height
        {
            get
            {
                return images[0].Height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            Draw(animationState, x, y, Width, Height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int idx = select.Evaluate(animationState);
            if (idx < images.Length)
            {
                images[idx].Draw(animationState, x, y, width, height);
            }
        }

        public Border Border
        {
            get
            {
                return border;
            }
        }

        public Renderer.Image CreateTintedVersion(Color color)
        {
            Renderer.Image[] newImages = new Renderer.Image[images.Length];
            for (int i = 0; i < newImages.Length; i++)
            {
                newImages[i] = images[i].CreateTintedVersion(color);
            }
            return new StateSelectImage(select, border, newImages);
        }

    }
}
