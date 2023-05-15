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
        internal Renderer.Image _image;
        Border _border;
        Border _inset;
        int _sizeOverwriteH;
        int _sizeOverwriteV;
        bool _center;
        internal StateExpression _condition;

        public ImageAdjustments(Renderer.Image image, Border border, Border inset,
                int sizeOverwriteH, int sizeOverwriteV,
                bool center, StateExpression condition)
        {
            this._image = image;
            this._border = border;
            this._inset = inset;
            this._sizeOverwriteH = sizeOverwriteH;
            this._sizeOverwriteV = sizeOverwriteV;
            this._center = center;
            this._condition = condition;
        }

        public int Width
        {
            get
            {
                if (_sizeOverwriteH >= 0)
                {
                    return _sizeOverwriteH;
                }
                else if (_inset != null)
                {
                    return _image.Width + _inset.BorderLeft + _inset.BorderRight;
                }
                else
                {
                    return _image.Width;
                }
            }
        }

        public int Height
        {
            get
            {
                if (_sizeOverwriteV >= 0)
                {
                    return _sizeOverwriteV;
                }
                else if (_inset != null)
                {
                    return _image.Height + _inset.BorderTop + _inset.BorderBottom;
                }
                else
                {
                    return _image.Height;
                }
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            if (_condition == null || _condition.Evaluate(animationState))
            {
                if (_inset != null)
                {
                    x += _inset.BorderLeft;
                    y += _inset.BorderTop;
                    width = Math.Max(0, width - _inset.BorderLeft - _inset.BorderRight);
                    height = Math.Max(0, height - _inset.BorderTop - _inset.BorderBottom);
                }
                if (_center)
                {
                    int w = Math.Min(width, _image.Width);
                    int h = Math.Min(height, _image.Height);
                    x += (width - w) / 2;
                    y += (height - h) / 2;
                    width = w;
                    height = h;
                }
                _image.Draw(animationState, x, y, width, height);
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, _image.Width, _image.Height);
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
            return new ImageAdjustments(_image.CreateTintedVersion(color), _border,
                    _inset, _sizeOverwriteH, _sizeOverwriteV, _center, _condition);
        }

        public bool IsSimple()
        {
            // used for ImageManager.parseStateSelect
            // only check parameters affecting rendering (except condition)
            return !_center && _inset == null && _sizeOverwriteH < 0 && _sizeOverwriteV < 0;
        }
    }
}
