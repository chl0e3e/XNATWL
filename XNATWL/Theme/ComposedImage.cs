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

using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class ComposedImage : Image, HasBorder
    {
        private Image[] _layers;
        private Border _border;

        public ComposedImage(Image[] layers, Border border) : base()
        {
            this._layers = layers;
            this._border = border;
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            Draw(animationState, x, y, Width, Height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            foreach (Image layer in _layers)
            {
                layer.Draw(animationState, x, y, width, height);
            }
        }

        public int Height
        {
            get
            {
                return _layers[0].Height;
            }
        }

        public int Width
        {
            get
            {
                return _layers[0].Width;
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
            Image[] newLayers = new Image[_layers.Length];
            for (int i = 0; i < newLayers.Length; i++)
            {
                newLayers[i] = _layers[i].CreateTintedVersion(color);
            }
            return new ComposedImage(newLayers, _border);
        }
    }
}
