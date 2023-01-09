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
using System.Reflection;

namespace XNATWL
{
    public class Color
    {
        public static Color BLACK = new Color(0xFF000000);
        public static Color SILVER = new Color(0xFFC0C0C0);
        public static Color GRAY = new Color(0xFF808080);
        public static Color WHITE = new Color(0xFFFFFFFF);
        public static Color MAROON = new Color(0xFF800000);
        public static Color RED = new Color(0xFFFF0000);
        public static Color PURPLE = new Color(0xFF800080);
        public static Color FUCHSIA = new Color(0xFFFF00FF);
        public static Color GREEN = new Color(0xFF008000);
        public static Color LIME = new Color(0xFF00FF00);
        public static Color OLIVE = new Color(0xFF808000);
        public static Color ORANGE = new Color(0xFFFFA500);
        public static Color YELLOW = new Color(0xFFFFFF00);
        public static Color NAVY = new Color(0xFF000080);
        public static Color BLUE = new Color(0xFF0000FF);
        public static Color TEAL = new Color(0xFF008080);
        public static Color AQUA = new Color(0xFF00FFFF);
        public static Color SKYBLUE = new Color(0xFF87CEEB);

        public static Color LIGHTBLUE    = new Color(0xFFADD8E6);
        public static Color LIGHTCORAL   = new Color(0xFFF08080);
        public static Color LIGHTCYAN    = new Color(0xFFE0FFFF);
        public static Color LIGHTGRAY    = new Color(0xFFD3D3D3);
        public static Color LIGHTGREEN   = new Color(0xFF90EE90);
        public static Color LIGHTPINK    = new Color(0xFFFFB6C1);
        public static Color LIGHTSALMON  = new Color(0xFFFFA07A);
        public static Color LIGHTSKYBLUE = new Color(0xFF87CEFA);
        public static Color LIGHTYELLOW  = new Color(0xFFFFFFE0);

        public static Color TRANSPARENT = new Color(0);

        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public int Red
        {
            get
            {
                return R & 255;
            }
        }

        public int Green
        {
            get
            {
                return G & 255;
            }
        }

        public int Blue
        {
            get
            {
                return B & 255;
            }
        }

        public int Alpha
        {
            get
            {
                return A & 255;
            }
        }
        
        public float RedF
        {
            get
            {
                return (R & 255) * (1.0f / 255f);
            }
        }

        public float GreenF
        {
            get
            {
                return (G & 255) * (1.0f / 255f);
            }
        }

        public float BlueF
        {
            get
            {
                return (B & 255) * (1.0f / 255f);
            }
        }

        public float AlphaF
        {
            get
            {
                return (A & 255) * (1.0f / 255f);
            }
        }

        public int ARGB
        {
            get
            {
                return ((A & 255) << 24) |
                        ((R & 255) << 16) |
                        ((G & 255) << 8) |
                        ((B & 255));
            }
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        /**
         * Creates a new color object from an integer in ARGB order.
         * 
         * bits  0- 7 are blue
         * bits  8-15 are green
         * bits 16-23 are red
         * bits 24-31 are alpha
         * 
         * @param argb the color value as integer
         */
        public Color(int argb)
        {
            this.A = (byte)(argb >> 24);
            this.R = (byte)(argb >> 16);
            this.G = (byte)(argb >> 8);
            this.B = (byte)(argb);
        }

        public Color(uint argb) : this((int)argb)
        {
           
        }

        public void WriteToFloatArray(float[] dst, int off)
        {
            dst[off + 0] = RedF;
            dst[off + 1] = GreenF;
            dst[off + 2] = BlueF;
            dst[off + 3] = AlphaF;
        }

        public static Color ByName(string name)
        {
            name = name.ToUpper();

            foreach (FieldInfo fieldInfo in typeof(Color).GetFields())
            {
                if (fieldInfo.FieldType == typeof(Color) && fieldInfo.IsStatic && name == fieldInfo.Name)
                {
                    return (Color) fieldInfo.GetValue(null);
                }
            }

            return null;
        }

        public static Color Parse(string value)
        {
            if (value.Length > 0 && value[0] == '#')
            {
                String hexcode = value.Substring(1);
                uint rgb4, a, r, g, b;
                switch (value.Length) {
                    case 4:
                        rgb4 = uint.Parse(hexcode, System.Globalization.NumberStyles.HexNumber);
                        r = ((rgb4 >> 8) & 0xF) * 0x11;
                        g = ((rgb4 >> 4) & 0xF) * 0x11;
                        b = ((rgb4) & 0xF) * 0x11;
                        return new Color(0xFF000000 | (r << 16) | (g << 8) | b);
                    case 5:
                        rgb4 = uint.Parse(hexcode, System.Globalization.NumberStyles.HexNumber);
                        a = ((rgb4 >> 12) & 0xF) * 0x11;
                        r = ((rgb4 >> 8) & 0xF) * 0x11;
                        g = ((rgb4 >> 4) & 0xF) * 0x11;
                        b = ((rgb4) & 0xF) * 0x11;
                        return new Color((a << 24) | (r << 16) | (g << 8) | b);
                    case 7:
                        return new Color(0xFF000000 | uint.Parse(hexcode, System.Globalization.NumberStyles.HexNumber));
                    case 9:
                        return new Color((uint)long.Parse(hexcode, System.Globalization.NumberStyles.HexNumber));
                    default:
                        throw new ArgumentOutOfRangeException("Can't parse '" + value + "' as hex color");
                }
            }

            return Color.ByName(value);
        }

        public override string ToString()
        {
            if (A != 0)
            {
                return string.Format("#{0:x8}", this.ARGB);
            }
            else
            {
                return string.Format("#{0:x8}", this.ARGB & 0xFFFFFF);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Color)) {
                return false;
            }

            Color other = (Color) obj;
            return this.ARGB == other.ARGB;
        }


        public override int GetHashCode()
        {
            return this.ARGB;
        }

        public Color Multiply(Color other)
        {
            byte mul(byte a, byte b)
            {
                return (byte)(((a & 255) * (b & 255)) / 255);
            }

            return new Color(mul(R, other.R), mul(G, other.G), mul(B, other.B), mul(A, other.A));
        }
    }
}
