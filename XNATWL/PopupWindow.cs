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
        private Widget owner;

        private bool closeOnClickedOutside = true;
        private bool closeOnEscape = true;
        private Runnable requestCloseCallback;

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
            this.owner = owner;
        }

        public Widget getOwner()
        {
            return owner;
        }

        public bool isCloseOnClickedOutside()
        {
            return closeOnClickedOutside;
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
        public void setCloseOnClickedOutside(bool closeOnClickedOutside)
        {
            this.closeOnClickedOutside = closeOnClickedOutside;
        }

        public bool isCloseOnEscape()
        {
            return closeOnEscape;
        }

        /**
         * Controls if this pop-up should close when the escape key is pressed.
         *
         * Default is true.
         *
         * @param closeOnEscape true if it should close on escape
         */
        public void setCloseOnEscape(bool closeOnEscape)
        {
            this.closeOnEscape = closeOnEscape;
        }

        public Runnable getRequestCloseCallback()
        {
            return requestCloseCallback;
        }

        /**
         * Sets a callback to be called when {@link #requestPopupClose()} is executed.
         * This will override the default behavior of closing the popup.
         * 
         * @param requestCloseCallback the new callback or null
         */
        public void setRequestCloseCallback(Runnable requestCloseCallback)
        {
            this.requestCloseCallback = requestCloseCallback;
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
        public virtual bool openPopup()
        {
            GUI gui = owner.getGUI();
            if (gui != null)
            {
                // a popup can't be invisible or disabled when it should open
                base.setVisible(true);
                base.setEnabled(true);
                // owner's hasOpenPopups flag is handled by GUI
                gui.openPopup(this);
                requestKeyboardFocus();
                focusFirstChild();
                // check isOpen() to make sure that the popup hasn't been close
                // in the focus transfer.
                // This can happen if {@code setVisible(false)} is called inside
                // {@code keyboardFocusLost()}
                return isOpen();
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
        public void openPopupCentered()
        {
            if (openPopup())
            {
                adjustSize();
                centerPopup();
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
        public void openPopupCentered(int width, int height)
        {
            if (openPopup())
            {
                setSize(Math.Min(getParent().getInnerWidth(), width),
                        Math.Min(getParent().getInnerHeight(), height));
                centerPopup();
            }
        }

        /**
         * Closes this pop-up window. Keyboard focus is transfered to it's owner.
         */
        public virtual void closePopup()
        {
            GUI gui = getGUI();
            if (gui != null)
            {
                // owner's hasOpenPopups flag is handled by GUI
                gui.closePopup(this);
                owner.requestKeyboardFocus();
            }
        }

        /**
         * Checks if this pop-up window is currently open
         * @return true if it is open
         */
        public bool isOpen()
        {
            return getParent() != null;
        }

        /**
         * Centers the pop-up on the screen.
         * If the pop-up is not open then this method does nothing.
         *
         * @see #isOpen()
         */
        public void centerPopup()
        {
            Widget parent = getParent();
            if (parent != null)
            {
                setPosition(
                        parent.getInnerX() + (parent.getInnerWidth() - getWidth()) / 2,
                        parent.getInnerY() + (parent.getInnerHeight() - getHeight()) / 2);
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
        public bool bindMouseDrag(Runnable cb)
        {
            GUI gui = getGUI();
            if (gui != null)
            {
                return gui.bindDragEvent(this, cb);
            }
            return false;
        }

        public override int getPreferredWidth()
        {
            int parentWidth = (getParent() != null) ? getParent().getInnerWidth() : short.MaxValue;
            return Math.Min(parentWidth, base.getPreferredWidth());
        }

        public override int getPreferredHeight()
        {
            int parentHeight = (getParent() != null) ? getParent().getInnerHeight() : short.MaxValue;
            return Math.Min(parentHeight, base.getPreferredHeight());
        }

        /**
         * This method is final to ensure correct event handling for pop-ups.
         * To customize the event handling override the {@link #handleEventPopup(de.matthiasmann.twl.Event) }.
         * 
         * @param evt the event
         * @return always returns true
         */
        public override bool handleEvent(Event evt)
        {
            if (handleEventPopup(evt))
            {
                return true;
            }
            if (evt.getEventType() == Event.EventType.MOUSE_CLICKED &&
                    !isInside(evt.getMouseX(), evt.getMouseY()))
            {
                mouseClickedOutside(evt);
                return true;
            }
            if (evt.isKeyPressedEvent() && evt.getKeyCode() == Event.KEY_ESCAPE)
            {
                escapePressed(evt);
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
        protected virtual bool handleEventPopup(Event evt)
        {
            return base.handleEvent(evt);
        }

        protected bool isMouseInside(Event evt)
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
        protected void requestPopupClose()
        {
            if (requestCloseCallback != null)
            {
                requestCloseCallback.run();
            }
            else
            {
                closePopup();
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
        protected void mouseClickedOutside(Event evt)
        {
            if (closeOnClickedOutside)
            {
                requestPopupClose();
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
        protected virtual void escapePressed(Event evt)
        {
            if (closeOnEscape)
            {
                requestPopupClose();
            }
        }

        internal override void setParent(Widget parent)
        {
            if (!(parent is GUI))
            {
                throw new ArgumentOutOfRangeException("PopupWindow can't be used as child widget");
            }

            base.setParent(parent);
        }
    }

}
