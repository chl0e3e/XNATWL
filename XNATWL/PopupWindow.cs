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
using XNATWL.Utils;

namespace XNATWL
{
    public class PopupWindow : Container
    {
        private Widget _owner;

        private bool _closeOnClickedOutside = true;
        private bool _closeOnEscape = true;
        private Runnable _requestCloseCallback;

        /**
         * Creates a new pop-up window.
         *
         * @param owner The owner of this pop-up
         */
        public PopupWindow(Widget owner)
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

        public bool IsCloseOnClickedOutside()
        {
            return _closeOnClickedOutside;
        }

        /**
         * Controls if this pop-up window should close when a mouse click
         * happens outside of it's area. This is useful for context menus or
         * drop down combo boxes.
         *
         * Default is true.
         *
         * @param closeOnClickedOutside true if it should close on clicks outside it's area
         */
        public void SetCloseOnClickedOutside(bool closeOnClickedOutside)
        {
            this._closeOnClickedOutside = closeOnClickedOutside;
        }

        public bool IsCloseOnEscape()
        {
            return _closeOnEscape;
        }

        /**
         * Controls if this pop-up should close when the escape key is pressed.
         *
         * Default is true.
         *
         * @param closeOnEscape true if it should close on escape
         */
        public void SetCloseOnEscape(bool closeOnEscape)
        {
            this._closeOnEscape = closeOnEscape;
        }

        public Runnable GetRequestCloseCallback()
        {
            return _requestCloseCallback;
        }

        /**
         * Sets a callback to be called when {@link #requestPopupClose()} is executed.
         * This will override the default behavior of closing the popup.
         * 
         * @param requestCloseCallback the new callback or null
         */
        public void SetRequestCloseCallback(Runnable requestCloseCallback)
        {
            this._requestCloseCallback = requestCloseCallback;
        }

        /**
         * Opens the pop-up window with it's current size and position.
         * In order for this to work the owner must be part of the GUI tree.
         *
         * When a pop-up window is shown it is always visible and enabled.
         * 
         * @return true if the pop-up window could be opened.
         * @see #getOwner() 
         * @see #getGUI()
         */
        public virtual bool OpenPopup()
        {
            GUI gui = _owner.GetGUI();
            if (gui != null)
            {
                // a popup can't be invisible or disabled when it should open
                base.SetVisible(true);
                base.SetEnabled(true);
                // owner's hasOpenPopups flag is handled by GUI
                gui.OpenPopup(this);
                RequestKeyboardFocus();
                FocusFirstChild();
                // check isOpen() to make sure that the popup hasn't been close
                // in the focus transfer.
                // This can happen if {@code setVisible(false)} is called inside
                // {@code keyboardFocusLost()}
                return IsOpen();
            }
            return false;
        }

        /**
         * Opens the pop-up window, calls {@code adjustSize} and centers the pop-up on
         * the screen.
         *
         * @see #adjustSize()
         * @see #centerPopup()
         */
        public void OpenPopupCentered()
        {
            if (OpenPopup())
            {
                AdjustSize();
                CenterPopup();
            }
        }

        /**
         * Opens the pop-up window with the specified size and centers the pop-up on
         * the screen.
         *
         * If the specified size is larger then the available space then it is
         * reduced to the available space.
         * 
         * @param width the desired width
         * @param height the desired height
         * @see #centerPopup()
         */
        public void OpenPopupCentered(int width, int height)
        {
            if (OpenPopup())
            {
                SetSize(Math.Min(GetParent().GetInnerWidth(), width),
                        Math.Min(GetParent().GetInnerHeight(), height));
                CenterPopup();
            }
        }

        /**
         * Closes this pop-up window. Keyboard focus is transfered to it's owner.
         */
        public virtual void ClosePopup()
        {
            GUI gui = GetGUI();
            if (gui != null)
            {
                // owner's hasOpenPopups flag is handled by GUI
                gui.ClosePopup(this);
                _owner.RequestKeyboardFocus();
            }
        }

        /**
         * Checks if this pop-up window is currently open
         * @return true if it is open
         */
        public bool IsOpen()
        {
            return GetParent() != null;
        }

        /**
         * Centers the pop-up on the screen.
         * If the pop-up is not open then this method does nothing.
         *
         * @see #isOpen()
         */
        public void CenterPopup()
        {
            Widget parent = GetParent();
            if (parent != null)
            {
                SetPosition(
                        parent.GetInnerX() + (parent.GetInnerWidth() - GetWidth()) / 2,
                        parent.GetInnerY() + (parent.GetInnerHeight() - GetHeight()) / 2);
            }
        }

        /**
         * Binds the current drag event (even if it's not yet started) to this pop-up.
         * The mouse drag events will be send as normal mouse move events.
         * The optional callback will be called when the drag event is finished.
         *
         * @param cb the optional callback which should be called at the end of the bound drag.
         * @return true if the binding was successful, false if not.
         */
        public bool BindMouseDrag(Runnable cb)
        {
            GUI gui = GetGUI();
            if (gui != null)
            {
                return gui.BindDragEvent(this, cb);
            }
            return false;
        }

        public override int GetPreferredWidth()
        {
            int parentWidth = (GetParent() != null) ? GetParent().GetInnerWidth() : short.MaxValue;
            return Math.Min(parentWidth, base.GetPreferredWidth());
        }

        public override int GetPreferredHeight()
        {
            int parentHeight = (GetParent() != null) ? GetParent().GetInnerHeight() : short.MaxValue;
            return Math.Min(parentHeight, base.GetPreferredHeight());
        }

        /**
         * This method is final to ensure correct event handling for pop-ups.
         * To customize the event handling override the {@link #handleEventPopup(de.matthiasmann.twl.Event) }.
         * 
         * @param evt the event
         * @return always returns true
         */
        public override bool HandleEvent(Event evt)
        {
            if (HandleEventPopup(evt))
            {
                return true;
            }
            if (evt.GetEventType() == EventType.MOUSE_CLICKED &&
                    !IsInside(evt.GetMouseX(), evt.GetMouseY()))
            {
                MouseClickedOutside(evt);
                return true;
            }
            if (evt.IsKeyPressedEvent() && evt.GetKeyCode() == Event.KEY_ESCAPE)
            {
                EscapePressed(evt);
                return true;
            }
            // eat all events
            return true;
        }

        /**
         * This method can be overriden to customize the event handling of a
         * pop-up window.
         * <p>The default implementation calls {@link Widget#handleEvent(de.matthiasmann.twl.Event) }</p>
         * 
         * @param evt the event
         * @return true if the event has been handled, false otherwise.
         */
        protected virtual bool HandleEventPopup(Event evt)
        {
            return base.HandleEvent(evt);
        }

        public override bool IsMouseInside(Event evt)
        {
            return true;    // :P
        }

        /**
         * Called when escape if pressed and {@code closeOnEscape} is enabled.
         * Also called by the default implementation of {@code mouseClickedOutside}
         * when {@code closeOnClickedOutside} is active.
         *
         * <p>By default it calls {@link #closePopup() } except when a
         * {@link #setRequestCloseCallback(java.lang.Runnable) } had been set.</p>
         * 
         * @see #setCloseOnEscape(bool)
         * @see #mouseClickedOutside(de.matthiasmann.twl.Event)
         */
        protected void RequestPopupClose()
        {
            if (_requestCloseCallback != null)
            {
                _requestCloseCallback.Run();
            }
            else
            {
                ClosePopup();
            }
        }

        /**
         * Called when a mouse click happened outside the pop-up window area.
         *
         * The default implementation calls {@code requestPopupClose} when
         * {@code closeOnClickedOutside} is active.
         *
         * @param evt The click event
         * @see #setCloseOnClickedOutside(bool) 
         */
        protected void MouseClickedOutside(Event evt)
        {
            if (_closeOnClickedOutside)
            {
                RequestPopupClose();
            }
        }

        /**
         * Called when the escape key was pressed.
         *
         * The default implementation calls {@code requestPopupClose} when
         * {@code closeOnEscape} is active.
         *
         * @param evt The click event
         * @see #setCloseOnEscape(bool)
         */
        protected virtual void EscapePressed(Event evt)
        {
            if (_closeOnEscape)
            {
                RequestPopupClose();
            }
        }

        internal override void SetParent(Widget parent)
        {
            if (!(parent is GUI))
            {
                throw new ArgumentOutOfRangeException("PopupWindow can't be used as child widget");
            }

            base.SetParent(parent);
        }
    }

}
