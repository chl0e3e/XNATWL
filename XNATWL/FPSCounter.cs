using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL
{
    public class FPSCounter : Label
    {
        private long startTime;
        private int frames;
        private int framesToCount = 100;

        private StringBuilder fmtBuffer;
        private int decimalPoint;
        private long scale;

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
            decimalPoint = numIntegerDigits + 1;
            startTime = DateTime.Now.Ticks;
            fmtBuffer = new StringBuilder();
            fmtBuffer.Length = numIntegerDigits + numDecimalDigits + Math.Sign(numDecimalDigits);

            // compute the scale based on the number of decimal places
            long tmp = (long)1e9;
            for (int i = 0; i < numDecimalDigits; i++)
            {
                tmp *= 10;
            }
            this.scale = tmp;

            // set default text so that initial size is computed correctly
            updateText(0);
        }

        /**
         * Creates the FPS counter with 3 integer digits and 2 decimal digits
         * @see #FPSCounter(int, int)
         */
        public FPSCounter() : this(3,2)
        {
        }

        public int getFramesToCount()
        {
            return framesToCount;
        }

        /**
         * Specified how many frames to count to compute the FPS. Larger values
         * result in a more accurate result and slower update.
         *
         * @param framesToCount the number of frames to count
         */
        public void setFramesToCount(int framesToCount)
        {
            if (framesToCount <= 0)
            {
                throw new ArgumentOutOfRangeException("framesToCount < 1");
            }
            this.framesToCount = framesToCount;
        }

        protected override void paintWidget(GUI gui)
        {
            if (++frames >= framesToCount)
            {
                updateFPS();
            }
            base.paintWidget(gui);
        }

        private void updateFPS()
        {
            long curTime = DateTime.Now.Ticks;
            long elapsed = curTime - startTime;
            startTime = curTime;

            updateText((int)((frames * scale + (elapsed >> 1)) / elapsed));
            frames = 0;
        }

        private void updateText(int value)
        {
            StringBuilder buf = fmtBuffer;
            int pos = buf.Length;
            do
            {
                buf[--pos] = (char)('0' + (value % 10));
                value /= 10;
                if (decimalPoint == pos)
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
                    if (decimalPoint == pos)
                    {
                        --pos;
                    }
                } while (pos > 0);
            }
            setCharSequence(buf.ToString());
        }
    }
}
