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

namespace XNATWL.Model
{
    /// <summary>
    /// A HSL (Hue, Saturation and Luminance) color space<br/>
    /// Hue is represented in degree from 0 to 360<br/>
    /// Saturation and Luminance in percent from 0 to 100
    /// </summary>
    public class ColorSpaceHSL : AbstractColorSpace
    {
        public ColorSpaceHSL() :
            base("HSL", "Hue", "Saturation", "Lightness")
        {
        }

        public override float ComponentDefaultValueOf(int component)
        {
            return (component == 0) ? 0f : 50f;
        }

        public override float ComponentMaxValueOf(int component)
        {
            return (component == 0) ? 360f : 100f;
        }

        public override string ComponentShortNameOf(int component)
        {
            return "HSL".Substring(component, component + 1);
        }

        public override float[] FromRGB(int rgb)
        {
            float r = ((rgb >> 16) & 255) / 255f;
            float g = ((rgb >> 8) & 255) / 255f;
            float b = ((rgb) & 255) / 255f;

            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);

            float summe = max + min;
            float saturation = max - min;

            if (saturation > 0.0f)
            {
                saturation /= (summe > 1.0f) ? 2.0f - summe : summe;
            }
            return new float[]{
                    360f * Hue(r, g, b, max, min),
                    100f * saturation,
                     50f * summe};
        }

        public override int RGB(float[] color)
        {
            float hue = color[0] / 360f;
            float saturation = color[1] / 100f;
            float lightness = color[2] / 100f;

            float r, g, b;

            if (saturation > 0.0f)
            {
                hue = (hue < 1.0f) ? hue * 6.0f : 0.0f;
                float q = lightness + saturation * ((lightness > 0.5f) ? 1.0f - lightness : lightness);
                float p = 2.0f * lightness - q;
                r = Normalize(q, p, (hue < 4.0f) ? (hue + 2.0f) : (hue - 4.0f));
                g = Normalize(q, p, hue);
                b = Normalize(q, p, (hue < 2.0f) ? (hue + 4.0f) : (hue - 2.0f));
            }
            else
            {
                r = g = b = lightness;
            }

            return (FloatToByte(r) << 16) | (FloatToByte(g) << 8) | FloatToByte(b);
        }

        static float Hue(float red, float green, float blue, float max, float min)
        {
            float hue = max - min;
            if (hue > 0.0f)
            {
                if (max == red)
                {
                    hue = (green - blue) / hue;
                    if (hue < 0.0f)
                    {
                        hue += 6.0f;
                    }
                }
                else if (max == green)
                {
                    hue = 2.0f + (blue - red) / hue;
                }
                else /*max == blue*/
                {
                    hue = 4.0f + (red - green) / hue;
                }
                hue /= 6.0f;
            }
            return hue;
        }

        private static float Normalize(float q, float p, float color)
        {
            if (color < 1.0f)
            {
                return p + (q - p) * color;
            }
            if (color < 3.0f)
            {
                return q;
            }
            if (color < 4.0f)
            {
                return p + (q - p) * (4.0f - color);
            }
            return p;
        }

        private static int FloatToByte(float value)
        {
            return Math.Max(0, Math.Min(255, (int)(255f * value)));
        }
    }
}
