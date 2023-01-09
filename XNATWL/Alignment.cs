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

using System;

namespace XNATWL
{
    public enum eAlignment
    {
        LEFT, CENTER, RIGHT, TOP, BOTTOM, TOPLEFT, TOPRIGHT, BOTTOMLEFT, BOTTOMRIGHT, FILL
    }

    public class Alignment
    {
        public static Alignment LEFT = new Alignment(HAlignment.LEFT, 0, 1);
        public static Alignment CENTER = new Alignment(HAlignment.CENTER, 1, 1);
        public static Alignment RIGHT = new Alignment(HAlignment.RIGHT, 2, 1);
        public static Alignment TOP = new Alignment(HAlignment.CENTER, 1, 0);
        public static Alignment BOTTOM = new Alignment(HAlignment.CENTER, 1, 2);
        public static Alignment TOPLEFT = new Alignment(HAlignment.LEFT, 0, 0);
        public static Alignment TOPRIGHT = new Alignment(HAlignment.RIGHT, 2, 0);
        public static Alignment BOTTOMLEFT = new Alignment(HAlignment.LEFT, 0, 2);
        public static Alignment BOTTOMRIGHT = new Alignment(HAlignment.RIGHT, 2, 2);
        public static Alignment FILL = new Alignment(HAlignment.CENTER,1,1);

        HAlignment fontHAlignment;
        byte hpos;
        byte vpos;

        private Alignment(HAlignment fontHAlignment, int hpos, int vpos)
        {
            this.fontHAlignment = fontHAlignment;
            this.hpos = (byte)hpos;
            this.vpos = (byte)vpos;
        }

        public HAlignment getFontHAlignment()
        {
            return fontHAlignment;
        }

        public static Alignment ByName(string name)
        {
            switch(name.ToUpper())
            {
                case "LEFT":
                    return Alignment.LEFT;
                case "CENTER":
                    return Alignment.CENTER;
                case "RIGHT":
                    return Alignment.RIGHT;
                case "TOP":
                    return Alignment.TOP;
                case "BOTTOM":
                    return Alignment.BOTTOM;
                case "TOPLEFT":
                    return Alignment.TOPLEFT;
                case "TOPRIGHT":
                    return Alignment.TOPRIGHT;
                case "BOTTOMLEFT":
                    return Alignment.BOTTOMLEFT;
                case "FILL":
                    return Alignment.FILL;
            }

            return Alignment.FILL;
        }

        /**
         * Returns the horizontal position for this alignment.
         * @return 0 for left, 1 for center and 2 for right
         */
        public int getHPosition()
        {
            return hpos;
        }

        /**
         * Returns the vertical position for this alignment.
         * @return 0 for top, 1 for center and 2 for bottom
         */
        public int getVPosition()
        {
            return vpos;
        }


        public int computePositionX(int containerWidth, int objectWidth)
        {
            return Math.Max(0, containerWidth - objectWidth) * hpos / 2;
        }

        public int computePositionY(int containerHeight, int objectHeight)
        {
            return Math.Max(0, containerHeight - objectHeight) * vpos / 2;
        }
    }
}
