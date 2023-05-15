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

namespace XNATWL.Utils
{
    public class ClipStack
    {
        private Entry[] _clipRects;
        private int _numClipRects;

        public ClipStack()
        {
            this._clipRects = new Entry[8];
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
        public void Push(int x, int y, int w, int h)
        {
            Entry tos = Push();
            tos.SetXYWH(x, y, w, h);
            Intersect(tos);
        }

        /**
         * Pushes the intersection of the new clip region and the current clip region
         * onto the stack.
         * 
         * @param rect the new clip region.
         * @throws NullPointerException if rect is null
         * @see #pop() 
         */
        public void Push(Rect rect)
        {
            if (rect == null)
            {
                throw new NullReferenceException("rect");
            }
            Entry tos = Push();
            tos.Set(rect);
            Intersect(tos);
        }

        /**
         * Pushes an "disable clipping" onto the stack.
         * @see #pop() 
         */
        public void PushDisable()
        {
            Entry rect = Push();
            rect._disabled = true;
        }

        /**
         * Removes the active clip regions from the stack.
         * @throws IllegalStateException when no clip regions are on the stack
         */
        public void Pop()
        {
            if (_numClipRects == 0)
            {
                Underflow();
            }
            _numClipRects--;
        }

        /**
         * Checks if the top of stack is an empty region (nothing will be rendered).
         * This can be used to speedup rendering by skipping all rendering when the
         * clip region is empty.
         * @return true if the TOS is an empty region
         */
        public bool IsClipEmpty()
        {
            Entry tos = _clipRects[_numClipRects - 1];
            return tos.IsEmpty && !tos._disabled;
        }

        /**
         * Retrieves the active clip region from the top of the stack
         * @param rect the rect coordinates - may not be updated when clipping is disabled
         * @return true if clipping is active, false if clipping is disabled
         */
        public bool GetClipRect(Rect rect)
        {
            if (_numClipRects == 0)
            {
                return false;
            }
            Entry tos = _clipRects[_numClipRects - 1];
            rect.Set(tos);
            return !tos._disabled;
        }

        /**
         * Returns the current number of entries in the clip stack
         * @return the number of entries
         */
        public int GetStackSize()
        {
            return _numClipRects;
        }

        /**
         * Clears the clip stack
         */
        public void ClearStack()
        {
            _numClipRects = 0;
        }

        protected Entry Push()
        {
            if (_numClipRects == _clipRects.Length)
            {
                Grow();
            }
            Entry rect;
            if ((rect = _clipRects[_numClipRects]) == null)
            {
                rect = new Entry();
                _clipRects[_numClipRects] = rect;
            }
            rect._disabled = false;
            _numClipRects++;
            return rect;
        }

        protected void Intersect(Rect tos)
        {
            if (_numClipRects > 1)
            {
                Entry prev = _clipRects[_numClipRects - 2];
                if (!prev._disabled)
                {
                    tos.Intersect(prev);
                }
            }
        }

        private void Grow()
        {
            Entry[] newRects = new Entry[_numClipRects * 2];
            Array.Copy(_clipRects, 0, newRects, 0, _numClipRects);
            _clipRects = newRects;
        }

        private void Underflow()
        {
            throw new InvalidOperationException("empty");
        }

        protected class Entry : Rect
        {
            internal bool _disabled;
        }
    }
}
