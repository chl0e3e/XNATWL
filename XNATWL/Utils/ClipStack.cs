using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class ClipStack
    {
        private Entry[] clipRects;
        private int numClipRects;

        public ClipStack()
        {
            this.clipRects = new Entry[8];
        }

        /**
         * Pushes the intersection of the new clip region and the current clip region
         * onto the stack.
         * 
         * @param x the left start
         * @param y the top start
         * @param w the width
         * @param h the height
         * @see #pop() 
         */
        public void push(int x, int y, int w, int h)
        {
            Entry tos = push();
            tos.SetXYWH(x, y, w, h);
            intersect(tos);
        }

        /**
         * Pushes the intersection of the new clip region and the current clip region
         * onto the stack.
         * 
         * @param rect the new clip region.
         * @throws NullPointerException if rect is null
         * @see #pop() 
         */
        public void push(Rect rect)
        {
            if (rect == null)
            {
                throw new NullReferenceException("rect");
            }
            Entry tos = push();
            tos.Set(rect);
            intersect(tos);
        }

        /**
         * Pushes an "disable clipping" onto the stack.
         * @see #pop() 
         */
        public void pushDisable()
        {
            Entry rect = push();
            rect.disabled = true;
        }

        /**
         * Removes the active clip regions from the stack.
         * @throws IllegalStateException when no clip regions are on the stack
         */
        public void pop()
        {
            if (numClipRects == 0)
            {
                underflow();
            }
            numClipRects--;
        }

        /**
         * Checks if the top of stack is an empty region (nothing will be rendered).
         * This can be used to speedup rendering by skipping all rendering when the
         * clip region is empty.
         * @return true if the TOS is an empty region
         */
        public bool isClipEmpty()
        {
            Entry tos = clipRects[numClipRects - 1];
            return tos.IsEmpty && !tos.disabled;
        }

        /**
         * Retrieves the active clip region from the top of the stack
         * @param rect the rect coordinates - may not be updated when clipping is disabled
         * @return true if clipping is active, false if clipping is disabled
         */
        public bool getClipRect(Rect rect)
        {
            if (numClipRects == 0)
            {
                return false;
            }
            Entry tos = clipRects[numClipRects - 1];
            rect.Set(tos);
            return !tos.disabled;
        }

        /**
         * Returns the current number of entries in the clip stack
         * @return the number of entries
         */
        public int getStackSize()
        {
            return numClipRects;
        }

        /**
         * Clears the clip stack
         */
        public void clearStack()
        {
            numClipRects = 0;
        }

        protected Entry push()
        {
            if (numClipRects == clipRects.Length)
            {
                grow();
            }
            Entry rect;
            if ((rect = clipRects[numClipRects]) == null)
            {
                rect = new Entry();
                clipRects[numClipRects] = rect;
            }
            rect.disabled = false;
            numClipRects++;
            return rect;
        }

        protected void intersect(Rect tos)
        {
            if (numClipRects > 1)
            {
                Entry prev = clipRects[numClipRects - 2];
                if (!prev.disabled)
                {
                    tos.Intersect(prev);
                }
            }
        }

        private void grow()
        {
            Entry[] newRects = new Entry[numClipRects * 2];
            Array.Copy(clipRects, 0, newRects, 0, numClipRects);
            clipRects = newRects;
        }

        private void underflow()
        {
            throw new InvalidOperationException("empty");
        }

        protected class Entry : Rect
        {
            internal bool disabled;
        }
    }

}
