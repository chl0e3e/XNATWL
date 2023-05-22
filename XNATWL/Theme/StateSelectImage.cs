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
    /// <summary>
    /// An <see cref="Image"/> rendered from an array of <see cref="Images"/> and expressions evaluated by a <see cref="StateSelect"/>
    /// </summary>
    public class StateSelectImage : Renderer.Image, HasBorder
    {
        private Border _border;
        private Renderer.Image[] _images;
        private StateSelect _select;

        /// <summary>
        /// Construct a new container for a collection of <see cref="Image"/>s to render decided by given <see cref="StateSelect"/>
        /// </summary>
        /// <param name="select">Expressions/conditions</param>
        /// <param name="border">Image <see cref="Border"/></param>
        /// <param name="ximages">Image collection</param>
        /// <exception cref="Exception"></exception>
        public StateSelectImage(StateSelect select, Border border, params Renderer.Image[] images)
        {
            if (!(images.Length >= select.Expressions()))
            {
                throw new Exception("Assert exception");
            }
            if (!(images.Length <= select.Expressions() + 1))
            {
                throw new Exception("Assert exception");
            }

            this._images = images;
            this._select = select;
            this._border = border;
        }

        public Renderer.Image[] Images
        {
            get
            {
                return _images;
            }
        }

        public StateSelect Select
        {
            get
            {
                return _select;
            }
        }

        public int Width
        {
            get
            {
                return _images[0].Width;
            }
        }

        public int Height
        {
            get
            {
                return _images[0].Height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            Draw(animationState, x, y, Width, Height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int idx = _select.Evaluate(animationState);
            if (idx < _images.Length)
            {
                _images[idx].Draw(animationState, x, y, width, height);
            }
        }

        public Border Border
        {
            get
            {
                return this._border;
            }
        }

        public Renderer.Image CreateTintedVersion(Color color)
        {
            Renderer.Image[] newImages = new Renderer.Image[_images.Length];
            for (int i = 0; i < newImages.Length; i++)
            {
                newImages[i] = _images[i].CreateTintedVersion(color);
            }
            return new StateSelectImage(_select, Border, newImages);
        }

    }
}
