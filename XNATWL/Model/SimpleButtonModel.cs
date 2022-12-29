using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleButtonModel : ButtonModel
    {
        protected static int STATE_MASK_HOVER = 1;
        protected static int STATE_MASK_PRESSED = 2;
        protected static int STATE_MASK_ARMED = 4;
        protected static int STATE_MASK_DISABLED = 8;

        private int _state;

        public bool Selected {
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
                        this.Action.Invoke(this, new ButtonActionEventArgs());
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

        public event EventHandler<ButtonActionEventArgs> Action;
        public event EventHandler<ButtonStateChangedEventArgs> State;
    }
}
