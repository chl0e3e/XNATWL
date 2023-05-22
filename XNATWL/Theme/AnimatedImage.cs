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
    /// <summary>
    /// Representing an <see cref="Image"/> extended to use animation
    /// </summary>
    public class AnimatedImage : Image, HasBorder
    {
        /// <summary>
        /// A single element of the <see cref="AnimatedImage"/>
        /// </summary>
        public abstract class Element
        {
            /// <summary>
            /// Animation duration in seconds
            /// </summary>
            protected internal int _duration;

            /// <summary>
            /// Width of the element
            /// </summary>
            public abstract int Width { get; }

            /// <summary>
            /// Height of the element
            /// </summary>
            public abstract int Height { get; }

            /// <summary>
            /// First image in animated set
            /// </summary>
            public abstract Img FirstImg { get; }

            /// <summary>
            /// Render the current element
            /// </summary>
            /// <param name="time">Animation time</param>
            /// <param name="next">Next image in set</param>
            /// <param name="x">X coordinate to render at</param>
            /// <param name="y">Y coordinate to render at </param>
            /// <param name="width">Width of animated image</param>
            /// <param name="height">Height of animated image</param>
            /// <param name="ai">Current <see cref="AnimatedImage"/> state</param>
            /// <param name="animationState">Other related variables for animation</param>
            public abstract void Render(int time, Img next, int x, int y,
                    int width, int height, AnimatedImage ai, Renderer.AnimationState animationState);
        }
        
        /// <summary>
        /// A single element of the <see cref="AnimatedImage"/> using <see cref="Image"/>
        /// </summary>
        public class Img : Element
        {
            /// <summary>
            /// Image to draw with
            /// </summary>
            public Image Image;

            float _r;
            float _g;
            float _b;
            float _a;
            float _zoomX;
            float _zoomY;
            float _zoomCenterX;
            float _zoomCenterY;

            /// <summary>
            /// Single static element in animated set
            /// </summary>
            /// <param name="duration">Image duration</param>
            /// <param name="image">parent Image object used by the renderer</param>
            /// <param name="tintColor">recolour image using color</param>
            /// <param name="zoomX">Zoom X</param>
            /// <param name="zoomY">Zoom Y</param>
            /// <param name="zoomCenterX">Zoom center X</param>
            /// <param name="zoomCenterY">Zoom center Y</param>
            /// <exception cref="ArgumentOutOfRangeException">Duration out of bounds (needs to be more than 0)</exception>
            public Img(int duration, Image image, Color tintColor, float zoomX, float zoomY, float zoomCenterX, float zoomCenterY)
            {
                if (duration < 0)
                {
                    throw new ArgumentOutOfRangeException("duration");
                }

                this.Image = image;
                this._duration = duration;
                this._r = tintColor.RedF;
                this._g = tintColor.GreenF;
                this._b = tintColor.BlueF;
                this._a = tintColor.AlphaF;
                this._zoomX = zoomX;
                this._zoomY = zoomY;
                this._zoomCenterX = zoomCenterX;
                this._zoomCenterY = zoomCenterY;
            }

            public override int Width => Image.Width;

            public override int Height => Image.Height;

            public override Img FirstImg => this;

            public override void Render(int time, Img next, int x, int y, int width, int height, AnimatedImage ai, Renderer.AnimationState animationState)
            {
                float rr = _r, gg = _g, bb = _b, aa = _a;
                float zx = _zoomX, zy = _zoomY, cx = _zoomCenterX, cy = _zoomCenterY;
                if (next != null)
                {
                    float t = time / (float)_duration;
                    rr = Blend(rr, next._r, t);
                    gg = Blend(gg, next._g, t);
                    bb = Blend(bb, next._b, t);
                    aa = Blend(aa, next._a, t);
                    zx = Blend(zx, next._zoomX, t);
                    zy = Blend(zy, next._zoomY, t);
                    cx = Blend(cx, next._zoomCenterX, t);
                    cy = Blend(cy, next._zoomCenterY, t);
                }
                ai._renderer.PushGlobalTintColor(rr * ai._r, gg * ai._g, bb * ai._b, aa * ai._a);
                try
                {
                    int zWidth = (int)(width * zx);
                    int zHeight = (int)(height * zy);
                    Image.Draw(animationState,
                            x + (int)((width - zWidth) * cx),
                            y + (int)((height - zHeight) * cy),
                            zWidth, zHeight);
                }
                finally
                {
                    ai._renderer.PopGlobalTintColor();
                }
            }

            /// <summary>
            /// Blend colour values
            /// </summary>
            /// <param name="a">component to blend</param>
            /// <param name="b">next image's component</param>
            /// <param name="t">animation completion per 0>t>1</param>
            /// <returns>Blended color component</returns>
            private static float Blend(float a, float b, float t)
            {
                return a + (b - a) * t;
            }
        }

        /// <summary>
        /// A repeatedly drawn image element of the <see cref="AnimatedImage"/> using <see cref="Image"/>
        /// </summary>
        public class Repeat : Element
        {
            protected internal Element[] _children;
            protected internal int _repeatCount;
            protected internal int _singleDuration;

            /// <summary>
            /// Repeated element in animated set
            /// </summary>
            /// <param name="children">Images to repeatedly draw</param>
            /// <param name="repeatCount">duration/(repeated number of times) of each in the set</param>
            /// <exception cref="Exception">Image not initialised correctly</exception>
            public Repeat(Element[] children, int repeatCount)
            {
                this._children = children;
                this._repeatCount = repeatCount;
                if(!(repeatCount >= 0))
                {
                    throw new Exception("Assert exception");
                }
                if (!(children.Length > 0))
                {
                    throw new Exception("Assert exception");
                }

                foreach (Element e in children)
                {
                    _duration += e._duration;
                }

                _singleDuration = _duration;
                if (repeatCount == 0)
                {
                    _duration = Int32.MaxValue;
                }
                else
                {
                    _duration *= repeatCount;
                }
            }

            public override int Height
            {
                get
                {
                    int tmp = 0;
                    foreach (Element e in _children)
                    {
                        tmp = Math.Max(tmp, e.Height);
                    }
                    return tmp;
                }
            }

            public override int Width
            {
                get
                {
                    int tmp = 0;
                    foreach (Element e in _children)
                    {
                        tmp = Math.Max(tmp, e.Width);
                    }
                    return tmp;
                }
            }

            public override Img FirstImg => _children[0].FirstImg;

            public override void Render(int time, Img next, int x, int y, int width, int height, AnimatedImage ai, Renderer.AnimationState animationState)
            {
                if (_singleDuration == 0)
                {
                    // animation data is invalid - don't crash
                    return;
                }

                int iteration = 0;
                if (_repeatCount == 0)
                {
                    time %= _singleDuration;
                }
                else
                {
                    iteration = time / _singleDuration;
                    time -= Math.Min(iteration, _repeatCount - 1) * _singleDuration;
                }

                Element e = null;
                for (int i = 0; i < _children.Length; i++)
                {
                    e = _children[i];
                    if (time < e._duration && e._duration > 0)
                    {
                        if (i + 1 < _children.Length)
                        {
                            next = _children[i + 1].FirstImg;
                        }
                        else if (_repeatCount == 0 || iteration + 1 < _repeatCount)
                        {
                            next = FirstImg;
                        }
                        break;
                    }

                    time -= e._duration;
                }

                if (e != null)
                {
                    e.Render(time, next, x, y, width, height, ai, animationState);
                }
            }
        }

        Renderer.Renderer _renderer;
        Element _root;
        StateKey _timeSource;
        Border _border;
        float _r;
        float _g;
        float _b;
        float _a;
        int _width;
        int _height;
        int _frozenTime;

        public AnimatedImage(Renderer.Renderer renderer, Element root, String timeSource, Border border, Color tintColor, int frozenTime)
        {
            this._renderer = renderer;
            this._root = root;
            this._timeSource = StateKey.Get(timeSource);
            this._border = border;
            this._r = tintColor.RedF;
            this._g = tintColor.GreenF;
            this._b = tintColor.BlueF;
            this._a = tintColor.AlphaF;
            this._width = root.Width;
            this._height = root.Height;
            this._frozenTime = frozenTime;
        }

        public AnimatedImage(AnimatedImage src, Color tintColor)
        {
            this._renderer = src._renderer;
            this._root = src._root;
            this._timeSource = src._timeSource;
            this._border = src._border;
            this._r = src._r * tintColor.RedF;
            this._g = src._g * tintColor.GreenF;
            this._b = src._b * tintColor.BlueF;
            this._a = src._a * tintColor.AlphaF;
            this._width = src._width;
            this._height = src._height;
            this._frozenTime = src._frozenTime;
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

        public void Draw (Renderer.AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, _width, _height);
        }

        public void Draw (Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int time = 0;
            if (animationState != null)
            {
                if (_frozenTime < 0 || animationState.ShouldAnimateState(_timeSource))
                {
                    time = animationState.GetAnimationTime(_timeSource);
                }
                else
                {
                    time = _frozenTime;
                }
            }
            _root.Render(time, null, x, y, width, height, this, animationState);
        }

        /// <summary>
        /// Image border
        /// </summary>
        public Border Border
        {
            get
            {
                return _border;
            }
        }

        public Image CreateTintedVersion (Color color)
        {
            return new AnimatedImage(this, color);
        }
    }
}
