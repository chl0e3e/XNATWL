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
using System.Collections.Generic;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class RadialPopupMenu : PopupWindow
    {
        private List<RoundButton> _buttons;

        private int _radius;
        private int _buttonRadius;
        private int _mouseButton;
        int _buttonRadiusSqr;

        public RadialPopupMenu(Widget owner) : base(owner)
        {
            this._buttons = new List<RoundButton>();
        }

        public int GetButtonRadius()
        {
            return _buttonRadius;
        }

        public void SetButtonRadius(int buttonRadius)
        {
            if (buttonRadius < 0)
            {
                throw new ArgumentOutOfRangeException("buttonRadius");
            }
            this._buttonRadius = buttonRadius;
            this._buttonRadiusSqr = buttonRadius * buttonRadius;
            InvalidateLayout();
        }

        public int GetRadius()
        {
            return _radius;
        }

        public void SetRadius(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException("radius");
            }
            this._radius = radius;
            InvalidateLayout();
        }

        public int GetMouseButton()
        {
            return _mouseButton;
        }

        /**
         * Sets the mouse button to which the buttons of this radial menu should react.
         * The default is {@link Event#MOUSE_LBUTTON}
         * @param mouseButton the mouse button
         */
        public void SetMouseButton(int mouseButton)
        {
            if (mouseButton < Event.MOUSE_LBUTTON || mouseButton > Event.MOUSE_RBUTTON)
            {
                throw new ArgumentOutOfRangeException("mouseButton");
            }
            this._mouseButton = mouseButton;
            for (int i = 0, n = _buttons.Count; i < n; i++)
            {
                _buttons[i].SetMouseButton(mouseButton);
            }
        }

        public Button AddButton(String theme, Action cb)
        {
            RoundButton button = new RoundButton(this);
            button.SetTheme(theme);
            button.Action += (sender, e) =>
            {
                cb();
            };
            button.SetMouseButton(_mouseButton);
            AddButton(button);
            return button;
        }

        public void RemoveButton(RoundButton btn)
        {
            int idx = _buttons.IndexOf(btn);
            if (idx >= 0)
            {
                _buttons.RemoveAt(idx);
                RemoveChild(btn);
            }
        }

        protected void AddButton(RoundButton button)
        {
            if (button == null)
            {
                throw new NullReferenceException("button");
            }
            _buttons.Add(button);
            Add(button);
        }

        class MouseDrag : Runnable
        {
            private RadialPopupMenu _menu;
            public MouseDrag(RadialPopupMenu menu)
            {
                this._menu = menu;
            }
            public override void Run()
            {
                this._menu.BoundDragEventFinished();
            }
        }

        public override bool OpenPopup()
        {
            if (base.OpenPopup())
            {
                if (BindMouseDrag(new MouseDrag(this)))
                {
                    SetAllButtonsPressed();
                }
                return true;
            }
            return false;
        }

        /**
         * Opens the radial popup menu around the specified coordinate
         *
         * @param centerX the X coordinate of the popup center
         * @param centerY the Y coordinate of the popup center
         * @return true if the popup was opened
         */
        public bool OpenPopupAt(int centerX, int centerY)
        {
            if (OpenPopup())
            {
                AdjustSize();
                Widget parent = GetParent();
                int width = GetWidth();
                int height = GetHeight();
                SetPosition(
                        Limit(centerX - width / 2, parent.GetInnerX(), parent.GetInnerRight() - width),
                        Limit(centerY - height / 2, parent.GetInnerY(), parent.GetInnerBottom() - height));
                return true;
            }
            return false;
        }

        protected static int Limit(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        /**
         * Opens the radial popup menu around the current mouse position
         * and uses the event's mouse button as button activator.
         *
         * @param evt the {@link Event.Type#MOUSE_BTNDOWN} event
         * @return true if the popup was opened
         */
        public bool OpenPopup(Event evt)
        {
            if (evt.GetEventType() == EventType.MOUSE_BTNDOWN)
            {
                SetMouseButton(evt.GetMouseButton());
                return OpenPopupAt(evt.GetMouseX(), evt.GetMouseY());
            }
            return false;
        }

        public override int GetPreferredInnerWidth()
        {
            return 2 * (_radius + _buttonRadius);
        }

        public override int GetPreferredInnerHeight()
        {
            return 2 * (_radius + _buttonRadius);
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeRadialPopupMenu(themeInfo);
        }

        protected void ApplyThemeRadialPopupMenu(ThemeInfo themeInfo)
        {
            SetRadius(themeInfo.GetParameter("radius", 40));
            SetButtonRadius(themeInfo.GetParameter("buttonRadius", 40));
        }

        protected override void Layout()
        {
            LayoutRadial();
        }

        protected void LayoutRadial()
        {
            int numButtons = _buttons.Count;
            if (numButtons > 0)
            {
                int centerX = GetInnerX() + GetInnerWidth() / 2;
                int centerY = GetInnerY() + GetInnerHeight() / 2;
                float toRad = (float)(2.0 * Math.PI) / numButtons;
                for (int i = 0; i < numButtons; i++)
                {
                    float rad = i * toRad;
                    int btnCenterX = centerX + (int)(_radius * Math.Sin(rad));
                    int btnCenterY = centerY - (int)(_radius * Math.Cos(rad));
                    RoundButton button = _buttons[i];
                    button.SetPosition(btnCenterX - _buttonRadius, btnCenterY - _buttonRadius);
                    button.SetSize(2 * _buttonRadius, 2 * _buttonRadius);
                }
            }
        }

        protected void SetAllButtonsPressed()
        {
            for (int i = 0, n = _buttons.Count; i < n; i++)
            {
                ButtonModel model = _buttons[i].GetModel();
                model.Pressed = true;
                model.Armed = model.Hover;
            }
        }

        protected void BoundDragEventFinished()
        {
            ClosePopup();
        }

        public class RoundButton : Button
        {
            private RadialPopupMenu _radialPopupMenu;
            public RoundButton(RadialPopupMenu radialPopupMenu)
            {
                this._radialPopupMenu = radialPopupMenu;
            }
            public override bool IsInside(int x, int y)
            {
                int dx = x - (GetX() + GetWidth() / 2);
                int dy = y - (GetY() + GetHeight() / 2);
                return dx * dx + dy * dy <= this._radialPopupMenu._buttonRadiusSqr;
            }
        }
    }

}
