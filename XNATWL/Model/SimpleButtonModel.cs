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
using static XNATWL.AnimationState;

namespace XNATWL.Model
{
    /// <summary>
    /// <para>A simple button model.</para>
    /// <para>Supported state bit: hover, armed, pressed.</para>
    /// </summary>
    public class SimpleButtonModel : ButtonModel
    {
        protected static int STATE_MASK_HOVER = 1;
        protected static int STATE_MASK_PRESSED = 2;
        protected static int STATE_MASK_ARMED = 4;
        protected static int STATE_MASK_DISABLED = 8;

        protected internal int _state;

        public virtual bool Selected
        {
            get
            {
                return false;
            }

            set
            {

            }
        }

        public bool Pressed
        {
            get
            {
                return (_state & STATE_MASK_PRESSED) != 0;
            }

            set
            {
                if (value != this.Pressed)
                {
                    bool fireAction = !value && this.Armed && this.Enabled;
                    SetStateBit(STATE_MASK_PRESSED, value);
                    this.State.Invoke(this, new ButtonStateChangedEventArgs());
                    if (fireAction)
                    {
                        this.FireAction();
                    }
                }
            }
        }

        public bool Armed
        {
            get
            {
                return (_state & STATE_MASK_ARMED) != 0;
            }

            set
            {
                if (value != this.Armed)
                {
                    SetStateBit(STATE_MASK_ARMED, value);
                    this.State.Invoke(this, new ButtonStateChangedEventArgs());
                }
            }
        }

        public bool Hover
        {
            get
            {
                return (_state & STATE_MASK_HOVER) != 0;
            }

            set
            {
                if (value != this.Hover)
                {
                    SetStateBit(STATE_MASK_HOVER, value);
                    this.State.Invoke(this, new ButtonStateChangedEventArgs());
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return (_state & STATE_MASK_DISABLED) == 0;
            }

            set
            {
                if (value != this.Enabled)
                {
                    SetStateBit(STATE_MASK_DISABLED, !value);
                    this.State.Invoke(this, new ButtonStateChangedEventArgs());
                }
            }
        }

        protected void SetStateBit(int mask, bool set)
        {
            if (set)
            {
                this._state |= mask;
            }
            else
            {
                this._state &= ~mask;
            }
        }

        protected virtual void FireAction()
        {
            if (this.Action != null)
            {
                this.Action.Invoke(this, new ButtonActionEventArgs());
            }
        }

        protected virtual void FireState()
        {
            if (this.State != null)
            {
                this.State.Invoke(this, new ButtonStateChangedEventArgs());
            }
        }

        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public event EventHandler<ButtonActionEventArgs> Action;
        public event EventHandler<ButtonStateChangedEventArgs> State;
    }
}
