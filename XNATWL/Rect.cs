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

namespace XNATWL
{
    public class Rect
    {
        private int _x0;
        private int _y0;
        private int _x1;
        private int _y1;

        public Rect()
        {
        }

        public Rect(int x, int y, int w, int h)
        {
            SetXYWH(x, y, w, h);
        }

        public Rect(Rect src)
        {
            Set(src.X, src.Y, src.Right, src.Bottom);
        }

        public void SetXYWH(int x, int y, int w, int h)
        {
            this._x0 = x;
            this._y0 = y;
            this._x1 = x + Math.Max(0, w);
            this._y1 = y + Math.Max(0, h);
        }

        public void Set(int x0, int y0, int x1, int y1)
        {
            this._x0 = x0;
            this._y0 = y0;
            this._x1 = x1;
            this._y1 = y1;
        }

        public void Set(Rect src)
        {
            this._x0 = src._x0;
            this._y0 = src._y0;
            this._x1 = src._x1;
            this._y1 = src._y1;
        }

        /**
         * Computes the intersection of this rectangle with the other rectangle.
         * If they don't overlapp then this rect will be set to zero width and height.
         *
         * @param other The other rectangle to compute the intersection with
         */
        public void Intersect(Rect other)
        {
            _x0 = Math.Max(_x0, other._x0);
            _y0 = Math.Max(_y0, other._y0);
            _x1 = Math.Min(_x1, other._x1);
            _y1 = Math.Min(_y1, other._y1);
            if (_x1 < _x0 || _y1 < _y0)
            {
                _x1 = _x0;
                _y1 = _y0;
            }
        }

        public bool IsInside(int x, int y)
        {
            return (x >= _x0) && (y >= _y0) && (x < _x1) && (y < _y1);
        }

        public int X
        {
            get
            {
                return _x0;
            }
        }

        public int Y
        {
            get
            {
                return _y0;
            }
        }

        public int Right
        {
            get
            {
                return _x1;
            }
        }

        public int Bottom
        {
            get
            {
                return _y1;
            }
        }

        public int Width
        {
            get
            {
                return _x1 - _x0;
            }
        }

        public int Height
        {
            get
            {
                return _y1 - _y0;
            }
        }

        public int CenterX
        {
            get
            {
                return (_x0 + _x1) / 2;
            }
        }

        public int CenterY
        {
            get
            {
                return (_y0 + _y1) / 2;
            }
        }

        public Dimension Size
        {
            get
            {
                return new Dimension(this.Width, this.Height);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _x1 <= _x0 || _y1 <= _y0;
            }
        }

        public override string ToString()
        {
            return "Rect[x0=" + _x0 + ", y0=" + _y0 + ", x1=" + _x1 + ", y1=" + _y1 + ']';
        }
    }
}
