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
            private ComboBoxBase comboBoxBase;

            public ComboBoxBasePopupWindow(ComboBoxBase owner) : base(owner)
            {
                this.comboBoxBase = owner;
            }

            protected override void escapePressed(Event evt)
            {
                this.comboBoxBase.popupEscapePressed(evt);
            }

        }
        public static StateKey STATE_COMBOBOX_KEYBOARD_FOCUS = StateKey.Get("comboboxKeyboardFocus");

        protected Button button;
        protected PopupWindow popup;

        protected ComboBoxBase()
        {
            this.button = new Button(getAnimationState());
            this.popup = new ComboBoxBasePopupWindow(this);
            button.Action += (sender, e) =>
            {
                openPopup();
            };

            add(button);
            setCanAcceptKeyboardFocus(true);
            setDepthFocusTraversal(false);
        }

        protected abstract Widget getLabel();

        protected virtual bool openPopup()
        {
            if (popup.openPopup())
            {
                setPopupSize();
                return true;
            }
            return false;
        }

        public override int getPreferredInnerWidth()
        {
            return getLabel().getPreferredWidth() + button.getPreferredWidth();
        }

        public override int getPreferredInnerHeight()
        {
            return Math.Max(getLabel().getPreferredHeight(), button.getPreferredHeight());
        }

        public override int getMinWidth()
        {
            int minWidth = base.getMinWidth();
            minWidth = Math.Max(minWidth, getLabel().getMinWidth() + button.getMinWidth());
            return minWidth;
        }

        public override int getMinHeight()
        {
            int minInnerHeight = Math.Max(getLabel().getMinHeight(), button.getMinHeight());
            return Math.Max(base.getMinHeight(), minInnerHeight + getBorderVertical());
        }

        protected virtual void setPopupSize()
        {
            int minHeight = popup.getMinHeight();
            int popupHeight = computeSize(minHeight,
                    popup.getPreferredHeight(),
                    popup.getMaxHeight());
            int popupMaxBottom = popup.getParent().getInnerBottom();
            if (getBottom() + minHeight > popupMaxBottom)
            {
                if (getY() - popupHeight >= popup.getParent().getInnerY())
                {
                    popup.setPosition(getX(), getY() - popupHeight);
                }
                else
                {
                    popup.setPosition(getX(), popupMaxBottom - minHeight);
                }
            }
            else
            {
                popup.setPosition(getX(), getBottom());
            }
            popupHeight = Math.Min(popupHeight, popupMaxBottom - popup.getY());
            popup.setSize(getWidth(), popupHeight);
        }

        protected override void layout()
        {
            int btnWidth = button.getPreferredWidth();
            int innerHeight = getInnerHeight();
            int innerX = getInnerX();
            int innerY = getInnerY();
            button.setPosition(getInnerRight() - btnWidth, innerY);
            button.setSize(btnWidth, innerHeight);
            getLabel().setPosition(innerX, innerY);
            getLabel().setSize(Math.Max(0, button.getX() - innerX), innerHeight);
        }

        protected override void sizeChanged()
        {
            base.sizeChanged();
            if (popup.isOpen())
            {
                setPopupSize();
            }
        }

        private static void setRecursive(Widget w, StateKey what, bool state)
        {
            w.getAnimationState().setAnimationState(what, state);
            for (int i = 0; i < w.getNumChildren(); ++i)
            {
                Widget child = w.getChild(i);
                setRecursive(child, what, state);
            }
        }

        protected override void keyboardFocusGained()
        {
            base.keyboardFocusGained();
            setRecursive(getLabel(), STATE_COMBOBOX_KEYBOARD_FOCUS, true);
        }

        protected override void keyboardFocusLost()
        {
            base.keyboardFocusLost();
            setRecursive(getLabel(), STATE_COMBOBOX_KEYBOARD_FOCUS, false);
        }

        /**
         * Called when the escape key is pressed in the open popup.
         * 
         * The default implementation closes the popup.
         * 
         * @param evt the event
         */
        protected virtual void popupEscapePressed(Event evt)
        {
            popup.closePopup();
        }
    }
}
