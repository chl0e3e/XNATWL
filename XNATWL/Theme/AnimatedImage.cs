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
    public class AnimatedImage : Image, HasBorder
    {
        public abstract class Element
        {
            internal int duration;

            public abstract int getWidth();
            public abstract int getHeight();
            public abstract Img getFirstImg();
            public abstract void render(int time, Img next, int x, int y,
                    int width, int height, AnimatedImage ai, Renderer.AnimationState animationState);
        }

        public class Img : Element
        {
            public Image image;
            float r;
            float g;
            float b;
            float a;
            float zoomX;
            float zoomY;
            float zoomCenterX;
            float zoomCenterY;

            public Img(int duration, Image image, Color tintColor, float zoomX, float zoomY, float zoomCenterX, float zoomCenterY)
            {
                if (duration < 0)
                {
                    throw new ArgumentOutOfRangeException("duration");
                }
                this.duration = duration;
                this.image = image;
                this.r = tintColor.RedF;
                this.g = tintColor.GreenF;
                this.b = tintColor.BlueF;
                this.a = tintColor.AlphaF;
                this.zoomX = zoomX;
                this.zoomY = zoomY;
                this.zoomCenterX = zoomCenterX;
                this.zoomCenterY = zoomCenterY;
            }

            public override int getWidth()
            {
                return image.Width;
            }

            public override int getHeight()
            {
                return image.Height;
            }

            public override Img getFirstImg()
            {
                return this;
            }

            public override void render(int time, Img next, int x, int y, int width, int height, AnimatedImage ai, Renderer.AnimationState animationState)
            {
                float rr = r, gg = g, bb = b, aa = a;
                float zx = zoomX, zy = zoomY, cx = zoomCenterX, cy = zoomCenterY;
                if (next != null)
                {
                    float t = time / (float)duration;
                    rr = blend(rr, next.r, t);
                    gg = blend(gg, next.g, t);
                    bb = blend(bb, next.b, t);
                    aa = blend(aa, next.a, t);
                    zx = blend(zx, next.zoomX, t);
                    zy = blend(zy, next.zoomY, t);
                    cx = blend(cx, next.zoomCenterX, t);
                    cy = blend(cy, next.zoomCenterY, t);
                }
                ai.renderer.PushGlobalTintColor(rr * ai.r, gg * ai.g, bb * ai.b, aa * ai.a);
                try
                {
                    int zWidth = (int)(width * zx);
                    int zHeight = (int)(height * zy);
                    image.Draw(animationState,
                            x + (int)((width - zWidth) * cx),
                            y + (int)((height - zHeight) * cy),
                            zWidth, zHeight);
                }
                finally
                {
                    ai.renderer.PopGlobalTintColor();
                }
            }

            private static float blend(float a, float b, float t)
            {
                return a + (b - a) * t;
            }
        }

        public class Repeat : Element
        {
            public Element[] children;
            public int repeatCount;
            public int singleDuration;

            public Repeat(Element[] children, int repeatCount)
            {
                this.children = children;
                this.repeatCount = repeatCount;
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
                    duration += e.duration;
                }
                singleDuration = duration;
                if (repeatCount == 0)
                {
                    duration = Int32.MaxValue;
                }
                else
                {
                    duration *= repeatCount;
                }
            }

            //@Override
            public override int getHeight()
            {
                int tmp = 0;
                foreach (Element e in children)
                {
                    tmp = Math.Max(tmp, e.getHeight());
                }
                return tmp;
            }

            //@Override
            public override int getWidth()
            {
                int tmp = 0;
                foreach (Element e in children)
                {
                    tmp = Math.Max(tmp, e.getWidth());
                }
                return tmp;
            }

            public override Img getFirstImg()
            {
                return children[0].getFirstImg();
            }

            public override void render(int time, Img next, int x, int y, int width, int height, AnimatedImage ai, Renderer.AnimationState animationState)
            {
                if (singleDuration == 0)
                {
                    // animation data is invalid - don't crash
                    return;
                }

                int iteration = 0;
                if (repeatCount == 0)
                {
                    time %= singleDuration;
                }
                else
                {
                    iteration = time / singleDuration;
                    time -= Math.Min(iteration, repeatCount - 1) * singleDuration;
                }

                Element e = null;
                for (int i = 0; i < children.Length; i++)
                {
                    e = children[i];
                    if (time < e.duration && e.duration > 0)
                    {
                        if (i + 1 < children.Length)
                        {
                            next = children[i + 1].getFirstImg();
                        }
                        else if (repeatCount == 0 || iteration + 1 < repeatCount)
                        {
                            next = getFirstImg();
                        }
                        break;
                    }

                    time -= e.duration;
                }

                if (e != null)
                {
                    e.render(time, next, x, y, width, height, ai, animationState);
                }
            }
        }

        Renderer.Renderer renderer;
        Element root;
        StateKey timeSource;
        Border border;
        float r;
        float g;
        float b;
        float a;
        int width;
        int height;
        int frozenTime;

        public AnimatedImage(Renderer.Renderer renderer, Element root, String timeSource, Border border, Color tintColor, int frozenTime)
        {
            this.renderer = renderer;
            this.root = root;
            this.timeSource = StateKey.Get(timeSource);
            this.border = border;
            this.r = tintColor.RedF;
            this.g = tintColor.GreenF;
            this.b = tintColor.BlueF;
            this.a = tintColor.AlphaF;
            this.width = root.getWidth();
            this.height = root.getHeight();
            this.frozenTime = frozenTime;
        }

        public AnimatedImage(AnimatedImage src, Color tintColor)
        {
            this.renderer = src.renderer;
            this.root = src.root;
            this.timeSource = src.timeSource;
            this.border = src.border;
            this.r = src.r * tintColor.RedF;
            this.g = src.g * tintColor.GreenF;
            this.b = src.b * tintColor.BlueF;
            this.a = src.a * tintColor.AlphaF;
            this.width = src.width;
            this.height = src.height;
            this.frozenTime = src.frozenTime;
        }

        public int getWidth()
        {
            return width;
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

        public void Draw (Renderer.AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, width, height);
        }

        public void Draw (Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int time = 0;
            if (animationState != null)
            {
                if (frozenTime < 0 || animationState.ShouldAnimateState(timeSource))
                {
                    time = animationState.GetAnimationTime(timeSource);
                }
                else
                {
                    time = frozenTime;
                }
            }
            root.render(time, null, x, y, width, height, this, animationState);
        }

        public Border Border
        {
            get
            {
                return border;
            }
        }

        public Image CreateTintedVersion (Color color)
        {
            return new AnimatedImage(this, color);
        }
    }
}
