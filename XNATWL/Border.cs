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

namespace XNATWL
{
    public class Border
    {
        public static Border ZERO = new Border(0);

        private int _top;
        private int _left;
        private int _bottom;
        private int _right;

        public Border(int all)
        {
            this._top = all;
            this._left = all;
            this._bottom = all;
            this._right = all;
        }

        public Border(Utils.Number all)
        {
            this._top = all.IntValue();
            this._left = all.IntValue();
            this._bottom = all.IntValue();
            this._right = all.IntValue();
        }

        public Border(int horz, int vert)
        {
            this._top = vert;
            this._left = horz;
            this._bottom = vert;
            this._right = horz;
        }

        public Border(Utils.Number horz, Utils.Number vert)
        {
            this._top = vert.IntValue();
            this._left = horz.IntValue();
            this._bottom = vert.IntValue();
            this._right = horz.IntValue();
        }

        public Border(int top, int left, int bottom, int right)
        {
            this._top = top;
            this._left = left;
            this._bottom = bottom;
            this._right = right;
        }

        public Border(Utils.Number top, Utils.Number left, Utils.Number bottom, Utils.Number right)
        {
            this._top = top.IntValue();
            this._left = left.IntValue();
            this._bottom = bottom.IntValue();
            this._right = right.IntValue();
        }

        public int BorderBottom
        {
            get
            {
                return _bottom;
            }
        }

        public int BorderLeft
        {
            get
            {
                return _left;
            }
        }

        public int BorderRight
        {
            get
            {
                return _right;
            }
        }

        public int BorderTop
        {
            get
            {
                return _top;
            }
        }

        public int Bottom
        {
            get
            {
                return _bottom;
            }
        }

        public int Left
        {
            get
            {
                return _left;
            }
        }

        public int Right
        {
            get
            {
                return _right;
            }
        }

        public int Top
        {
            get
            {
                return _top;
            }
        }

        public override string ToString()
        {
            return "[Border top=" + _top + " left=" + _left + " bottom=" + _bottom + " right=" + _right + "]";
        }
    }
}
