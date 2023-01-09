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

        private int top;
        private int left;
        private int bottom;
        private int right;

        public Border(int all)
        {
            this.top = all;
            this.left = all;
            this.bottom = all;
            this.right = all;
        }

        public Border(Utils.Number all)
        {
            this.top = all.intValue();
            this.left = all.intValue();
            this.bottom = all.intValue();
            this.right = all.intValue();
        }

        public Border(int horz, int vert)
        {
            this.top = vert;
            this.left = horz;
            this.bottom = vert;
            this.right = horz;
        }

        public Border(Utils.Number horz, Utils.Number vert)
        {
            this.top = vert.intValue();
            this.left = horz.intValue();
            this.bottom = vert.intValue();
            this.right = horz.intValue();
        }

        public Border(int top, int left, int bottom, int right)
        {
            this.top = top;
            this.left = left;
            this.bottom = bottom;
            this.right = right;
        }

        public Border(Utils.Number top, Utils.Number left, Utils.Number bottom, Utils.Number right)
        {
            this.top = top.intValue();
            this.left = left.intValue();
            this.bottom = bottom.intValue();
            this.right = right.intValue();
        }

        public int BorderBottom
        {
            get
            {
                return bottom;
            }
        }

        public int BorderLeft
        {
            get
            {
                return left;
            }
        }

        public int BorderRight
        {
            get
            {
                return right;
            }
        }

        public int BorderTop
        {
            get
            {
                return top;
            }
        }

        public int Bottom
        {
            get
            {
                return bottom;
            }
        }

        public int Left
        {
            get
            {
                return left;
            }
        }

        public int Right
        {
            get
            {
                return right;
            }
        }

        public int Top
        {
            get
            {
                return top;
            }
        }

        public override string ToString()
        {
            return "[Border top=" + top + " left=" + left + " bottom=" + bottom + " right=" + right + "]";
        }
    }
}
