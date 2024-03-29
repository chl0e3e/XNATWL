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

namespace XNATWL.Model
{
    /// <summary>
    /// A generic button model. Allows to separate button behavior from the button Widget.<br/><br/>
    /// <b>A ButtonModel should not be shared between Button instances.</b>
    /// </summary>
    public interface ButtonModel
    {
        bool Selected
        {
            get;
            set;
        }

        bool Pressed
        {
            get;
            set;
        }

        bool Armed
        {
            get;
            set;
        }

        bool Hover
        {
            get;
            set;
        }

        bool Enabled
        {
            get;
            set;
        }

        event EventHandler<ButtonActionEventArgs> Action;
        event EventHandler<ButtonStateChangedEventArgs> State;

        /// <summary>
        /// Called when the Button is placed in the GUI tree.
        /// Callbacks to other models should only be installed after this call.
        /// </summary>
        void Connect();
        /// <summary>
        /// Called when the Button is no longer part of the GUI tree.
        /// Callbacks to other models should be removed.
        /// </summary>
        void Disconnect();
    }

    public class ButtonActionEventArgs : EventArgs
    {
        public ButtonActionEventArgs()
        {

        }
    }

    public class ButtonStateChangedEventArgs : EventArgs
    {
        public ButtonStateChangedEventArgs()
        {

        }
    }
}
