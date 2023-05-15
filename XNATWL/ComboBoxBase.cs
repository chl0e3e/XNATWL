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
using XNATWL.Renderer;

namespace XNATWL
{
    public abstract class ComboBoxBase : Widget
    {
        class ComboBoxBasePopupWindow : PopupWindow
        {
            private ComboBoxBase _comboBoxBase;

            public ComboBoxBasePopupWindow(ComboBoxBase owner) : base(owner)
            {
                this._comboBoxBase = owner;
            }

            protected override void EscapePressed(Event evt)
            {
                this._comboBoxBase.PopupEscapePressed(evt);
            }

        }
        public static StateKey STATE_COMBOBOX_KEYBOARD_FOCUS = StateKey.Get("comboboxKeyboardFocus");

        protected Button _button;
        protected PopupWindow _popup;

        protected ComboBoxBase()
        {
            this._button = new Button(GetAnimationState());
            this._popup = new ComboBoxBasePopupWindow(this);
            _button.Action += (sender, e) =>
            {
                OpenPopup();
            };

            Add(_button);
            SetCanAcceptKeyboardFocus(true);
            SetDepthFocusTraversal(false);
        }

        protected abstract Widget GetLabel();

        protected virtual bool OpenPopup()
        {
            if (_popup.OpenPopup())
            {
                SetPopupSize();
                return true;
            }
            return false;
        }

        public override int GetPreferredInnerWidth()
        {
            return GetLabel().GetPreferredWidth() + _button.GetPreferredWidth();
        }

        public override int GetPreferredInnerHeight()
        {
            return Math.Max(GetLabel().GetPreferredHeight(), _button.GetPreferredHeight());
        }

        public override int GetMinWidth()
        {
            int minWidth = base.GetMinWidth();
            minWidth = Math.Max(minWidth, GetLabel().GetMinWidth() + _button.GetMinWidth());
            return minWidth;
        }

        public override int GetMinHeight()
        {
            int minInnerHeight = Math.Max(GetLabel().GetMinHeight(), _button.GetMinHeight());
            return Math.Max(base.GetMinHeight(), minInnerHeight + GetBorderVertical());
        }

        protected virtual void SetPopupSize()
        {
            int minHeight = _popup.GetMinHeight();
            int popupHeight = ComputeSize(minHeight,
                    _popup.GetPreferredHeight(),
                    _popup.GetMaxHeight());
            int popupMaxBottom = _popup.GetParent().GetInnerBottom();

            if (GetBottom() + minHeight > popupMaxBottom)
            {
                if (GetY() - popupHeight >= _popup.GetParent().GetInnerY())
                {
                    _popup.SetPosition(GetX(), GetY() - popupHeight);
                }
                else
                {
                    _popup.SetPosition(GetX(), popupMaxBottom - minHeight);
                }
            }
            else
            {
                _popup.SetPosition(GetX(), GetBottom());
            }

            popupHeight = Math.Min(popupHeight, popupMaxBottom - _popup.GetY());
            _popup.SetSize(GetWidth(), popupHeight);
        }

        protected override void Layout()
        {
            int btnWidth = _button.GetPreferredWidth();
            int innerHeight = GetInnerHeight();
            int innerX = GetInnerX();
            int innerY = GetInnerY();
            _button.SetPosition(GetInnerRight() - btnWidth, innerY);
            _button.SetSize(btnWidth, innerHeight);
            GetLabel().SetPosition(innerX, innerY);
            GetLabel().SetSize(Math.Max(0, _button.GetX() - innerX), innerHeight);
        }

        protected override void SizeChanged()
        {
            base.SizeChanged();
            if (_popup.IsOpen())
            {
                SetPopupSize();
            }
        }

        private static void SetRecursive(Widget w, StateKey what, bool state)
        {
            w.GetAnimationState().SetAnimationState(what, state);
            for (int i = 0; i < w.GetNumChildren(); ++i)
            {
                Widget child = w.GetChild(i);
                SetRecursive(child, what, state);
            }
        }

        protected override void KeyboardFocusGained()
        {
            base.KeyboardFocusGained();
            SetRecursive(GetLabel(), STATE_COMBOBOX_KEYBOARD_FOCUS, true);
        }

        protected override void KeyboardFocusLost()
        {
            base.KeyboardFocusLost();
            SetRecursive(GetLabel(), STATE_COMBOBOX_KEYBOARD_FOCUS, false);
        }

        /**
         * Called when the escape key is pressed in the open popup.
         * 
         * The default implementation closes the popup.
         * 
         * @param evt the event
         */
        protected virtual void PopupEscapePressed(Event evt)
        {
            _popup.ClosePopup();
        }
    }
}
