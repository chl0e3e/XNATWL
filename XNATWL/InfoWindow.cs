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
    public class InfoWindow : Container
    {
        private Widget _owner;

        public InfoWindow(Widget owner)
        {
            if (owner == null)
            {
                throw new NullReferenceException("owner");
            }

            this._owner = owner;
        }

        public Widget GetOwner()
        {
            return _owner;
        }

        public bool IsOpen()
        {
            return GetParent() != null;
        }

        public bool OpenInfo()
        {
            if (GetParent() != null)
            {
                return true;
            }
            if (IsParentInfoWindow(_owner))
            {
                return false;
            }
            GUI gui = _owner.GetGUI();
            if (gui != null)
            {
                gui.OpenInfo(this);
                FocusFirstChild();
                return true;
            }
            return false;
        }

        public void CloseInfo()
        {
            GUI gui = GetGUI();
            if (gui != null)
            {
                gui.CloseInfo(this);
            }
        }

        /**
         * Called after the info window has been closed
         */
        protected internal virtual void InfoWindowClosed()
        {
        }

        private static bool IsParentInfoWindow(Widget w)
        {
            while (w != null)
            {
                if (w is InfoWindow)
                {
                    return true;
                }
                w = w.GetParent();
            }
            return false;
        }
    }
}
