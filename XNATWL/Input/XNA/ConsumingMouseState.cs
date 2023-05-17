using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace XNATWL.Input.XNA
{
    /// <summary>
    /// Class which is aware of the mouse buttons pressed aswell as the last one polled
    /// </summary>
    public class ConsumingMouseState
    {
        public ButtonState Left;
        public ButtonState Right;
        public ButtonState Middle;
        public int X;
        public int Y;
        public bool[] ButtonsActive;
        public int ScrollDelta;
        private int _lastScrollWheelValue;

        /// <summary>
        /// Create a new MouseState that buffers repeated actions
        /// </summary>
        public ConsumingMouseState()
        {
            this._lastScrollWheelValue = 0;
            this.ButtonsActive = new bool[3];
        }

        /// <summary>
        /// Consume the XNA mouse state into an array of the differences between now and the last mouse poll.
        /// </summary>
        /// <param name="mouseState">MouseState from XNA</param>
        /// <returns>Array of mouse state changes</returns>
        public CMSAction[] Consume(MouseState mouseState)
        {
            List<CMSAction> actions = new List<CMSAction>();

            int wheelDelta = mouseState.ScrollWheelValue - this._lastScrollWheelValue;
            if (wheelDelta != 0)
            {
                actions.Add(CMSAction.Scroll);
                this.ScrollDelta = wheelDelta;
                this._lastScrollWheelValue = mouseState.ScrollWheelValue;
            }

            if (this.Left != mouseState.LeftButton)
            {
                this.Left = mouseState.LeftButton;
                this.ButtonsActive[0] = this.Left == ButtonState.Pressed;

                actions.Add(CMSAction.LeftChanged);
            }

            if (this.Right != mouseState.RightButton)
            {
                this.Right = mouseState.RightButton;
                this.ButtonsActive[0] = this.Right == ButtonState.Pressed;

                actions.Add(CMSAction.RightChanged);
            }

            if (this.Middle != mouseState.MiddleButton)
            {
                this.Middle = mouseState.MiddleButton;
                this.ButtonsActive[0] = this.Middle == ButtonState.Pressed;

                actions.Add(CMSAction.MiddleChanged);
            }

            if (this.X != mouseState.X)
            {
                this.X = mouseState.X;

                if (this.ButtonsActive[0])
                {
                    actions.Add(CMSAction.XChangedLeft);
                }
                if (this.ButtonsActive[1])
                {
                    actions.Add(CMSAction.XChangedRight);
                }
                if (this.ButtonsActive[2])
                {
                    actions.Add(CMSAction.XChangedMiddle);
                }
                actions.Add(CMSAction.XChanged);
            }

            if (this.Y != mouseState.Y)
            {
                this.Y = mouseState.Y;

                if (this.ButtonsActive[0])
                {
                    actions.Add(CMSAction.YChangedLeft);
                }
                if (this.ButtonsActive[1])
                {
                    actions.Add(CMSAction.YChangedRight);
                }
                if (this.ButtonsActive[2])
                {
                    actions.Add(CMSAction.YChangedMiddle);
                }
                actions.Add(CMSAction.YChanged);
            }

            return actions.ToArray();
        }
    }

    /// <summary>
    /// Enum denoting what type of change was consumed by ConsumingMouseState
    /// </summary>
    public enum CMSAction
    {
        LeftChanged,
        RightChanged,
        MiddleChanged,
        XChangedLeft,
        YChangedLeft,
        XChangedRight,
        YChangedRight,
        XChangedMiddle,
        YChangedMiddle,
        XChanged,
        YChanged,
        Scroll
    }
}
