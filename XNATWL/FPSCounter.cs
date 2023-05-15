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
using System.Text;

namespace XNATWL
{
    public class FPSCounter : Label
    {
        private long _startTime;
        private int _frames;
        private int _framesToCount = 100;

        private StringBuilder _fmtBuffer;
        private int _decimalPoint;
        private long _scale;

        /**
         * Creates the FPS counter with the given number of integer and decimal digits
         *
         * @param numIntegerDigits number of integer digits - must be >= 2
         * @param numDecimalDigits number of decimal digits - must be >= 0
         */
        public FPSCounter(int numIntegerDigits, int numDecimalDigits)
        {
            if (numIntegerDigits < 2)
            {
                throw new ArgumentOutOfRangeException("numIntegerDigits must be >= 2");
            }
            if (numDecimalDigits < 0)
            {
                throw new ArgumentOutOfRangeException("numDecimalDigits must be >= 0");
            }
            _decimalPoint = numIntegerDigits + 1;
            _startTime = DateTime.Now.Ticks;
            _fmtBuffer = new StringBuilder();
            _fmtBuffer.Length = numIntegerDigits + numDecimalDigits + Math.Sign(numDecimalDigits);

            // compute the scale based on the number of decimal places
            long tmp = (long)1e9;
            for (int i = 0; i < numDecimalDigits; i++)
            {
                tmp *= 10;
            }
            this._scale = tmp;

            // set default text so that initial size is computed correctly
            UpdateText(0);
        }

        /**
         * Creates the FPS counter with 3 integer digits and 2 decimal digits
         * @see #FPSCounter(int, int)
         */
        public FPSCounter() : this(3,2)
        {
        }

        public int GetFramesToCount()
        {
            return _framesToCount;
        }

        /**
         * Specified how many frames to count to compute the FPS. Larger values
         * result in a more accurate result and slower update.
         *
         * @param framesToCount the number of frames to count
         */
        public void SetFramesToCount(int framesToCount)
        {
            if (framesToCount <= 0)
            {
                throw new ArgumentOutOfRangeException("framesToCount < 1");
            }
            this._framesToCount = framesToCount;
        }

        protected override void PaintWidget(GUI gui)
        {
            if (++_frames >= _framesToCount)
            {
                UpdateFPS();
            }
            base.PaintWidget(gui);
        }

        private void UpdateFPS()
        {
            long curTime = DateTime.Now.Ticks;
            long elapsed = curTime - _startTime;
            _startTime = curTime;

            UpdateText((int)((_frames * _scale + (elapsed >> 1)) / elapsed));
            _frames = 0;
        }

        private void UpdateText(int value)
        {
            StringBuilder buf = _fmtBuffer;
            int pos = buf.Length;
            do
            {
                buf[--pos] = (char)('0' + (value % 10));
                value /= 10;
                if (_decimalPoint == pos)
                {
                    buf[--pos] = '.';
                }
            } while (pos > 0);
            if (value > 0)
            {
                // when the frame rate is too high, then we display "999.99"
                pos = buf.Length;
                do
                {
                    buf[--pos] = '9';
                    if (_decimalPoint == pos)
                    {
                        --pos;
                    }
                } while (pos > 0);
            }
            SetCharSequence(buf.ToString());
        }
    }
}
