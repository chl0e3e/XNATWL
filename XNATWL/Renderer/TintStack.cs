﻿/*
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

namespace XNATWL.Renderer
{
    public class TintStack
    {
        private static float ONE_OVER_255 = 1f;// / 255f;

        TintStack prev;
        TintStack next;
        float r, g, b, a;

        public TintStack()
        {
            this.prev = this;
            this.r = ONE_OVER_255;
            this.g = ONE_OVER_255;
            this.b = ONE_OVER_255;
            this.a = ONE_OVER_255;
        }

        private TintStack(TintStack prev)
        {
            this.prev = prev;
        }

        public TintStack pushReset()
        {
            if (next == null)
            {
                next = new TintStack(this);
            }
            next.r = ONE_OVER_255;
            next.g = ONE_OVER_255;
            next.b = ONE_OVER_255;
            next.a = ONE_OVER_255;
            return next;
        }

        public TintStack push(float r, float g, float b, float a)
        {
            if (next == null)
            {
                next = new TintStack(this);
            }
            next.r = this.r * r;
            next.g = this.g * g;
            next.b = this.b * b;
            next.a = this.a * a;
            return next;
        }

        public TintStack push(Color color)
        {
            return push(
                    color.RedF,
                    color.GreenF,
                    color.BlueF,
                    color.AlphaF) ;
        }

        public Microsoft.Xna.Framework.Color TintColorForXNA(Color color)
        {
            return new Microsoft.Xna.Framework.Color(this.r * color.RedF, this.g * color.GreenF, this.b * color.BlueF, this.a * color.AlphaF);
        }

        public TintStack pop()
        {
            return prev;
        }

        public float getR()
        {
            return r;
        }

        public float getG()
        {
            return g;
        }

        public float getB()
        {
            return b;
        }

        public float getA()
        {
            return a;
        }
    }
}
