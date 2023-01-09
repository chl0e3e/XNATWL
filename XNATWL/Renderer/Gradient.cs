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
using System.Collections.Generic;

namespace XNATWL.Renderer
{
    public enum GradientType
    {
        HORIZONTAL,
        VERTICAL
    }

    public enum GradientWrap
    {
        SCALE,
        CLAMP,
        REPEAT,
        MIRROR
    }

    public class Gradient
    {
        private GradientType _gradientType;
        private GradientWrap _gradientWrap;
        private List<GradientStop> _stops;

        public Gradient(GradientType type)
        {
            this._gradientType = type;
            this._gradientWrap = GradientWrap.SCALE;
            this._stops = new List<GradientStop>();
        }

        public GradientType Type
        {
            get
            {
                return this._gradientType;
            }
        }

        public GradientWrap Wrap
        {
            get
            {
                return this._gradientWrap;
            }

            set
            {
                this._gradientWrap = value;
            }
        }

        int Stops
        {
            get
            {
                return this._stops.Count;
            }
        }

        public GradientStop StopAt(int index)
        {
            return this._stops[index];
        }

        public GradientStop[] StopsAsArray
        {
            get
            {
                return this._stops.ToArray();
            }
        }

        public void AddStop(float pos, Color color)
        {
            int numStops = this.Stops;

            if (numStops == 0)
            {
                if (!(pos >= 0))
                {
                    throw new ArgumentOutOfRangeException("first stop must be >= 0.0f");
                }

                if (pos > 0)
                {
                    this._stops.Add(new GradientStop(0.0f, color));
                }
            }

            if (numStops > 0 && !(pos > this._stops[numStops - 1]._pos))
            {
                throw new ArgumentOutOfRangeException("pos must be monotone increasing");
            }

            this._stops.Add(new GradientStop(pos, color));
        }

    }

    public class GradientStop
    {
        internal float _pos;
        internal Color _color;

        public GradientStop(float pos, Color color)
        {
            this._pos = pos;
            this._color = color;
        }

        public float Pos
        {
            get
            {
                return _pos;
            }
        }

        public Color Color
        {
            get
            {
                return _color;
            }
        }
    }
}
