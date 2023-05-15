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
    public class DesktopArea : Widget
    {
        public DesktopArea()
        {
            SetFocusKeyEnabled(false);
        }

        protected override void KeyboardFocusChildChanged(Widget child)
        {
            base.KeyboardFocusChildChanged(child);
            if (child != null)
            {
                int fromIdx = GetChildIndex(child);
                System.Diagnostics.Debug.Assert(fromIdx >= 0);
                int numChildren = GetNumChildren();
                if (fromIdx < numChildren - 1)
                {
                    MoveChild(fromIdx, numChildren - 1);
                }
            }
        }

        protected override void Layout()
        {
            // make sure that all children are still inside
            RestrictChildrenToInnerArea();
        }

        protected void RestrictChildrenToInnerArea()
        {
            int top = GetInnerY();
            int left = GetInnerX();
            int right = GetInnerRight();
            int bottom = GetInnerBottom();
            int width = Math.Max(0, right - left);
            int height = Math.Max(0, bottom - top);

            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget w = GetChild(i);
                w.SetSize(
                        Math.Min(Math.Max(width, w.GetMinWidth()), w.GetWidth()),
                        Math.Min(Math.Max(height, w.GetMinHeight()), w.GetHeight()));
                w.SetPosition(
                        Math.Max(left, Math.Min(right - w.GetWidth(), w.GetX())),
                        Math.Max(top, Math.Min(bottom - w.GetHeight(), w.GetY())));
            }
        }

    }
}
