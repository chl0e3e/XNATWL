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
using XNATWL.Utils;

namespace XNATWL
{
    public class MenuManager : PopupWindow
    {
        private bool _isMenuBar;
        private Dictionary<MenuElement, Widget> _popups;

        private bool _mouseOverOwner;
        private Widget _lastMouseOverWidget;
        private Timer _timer;

        public MenuManager(Widget owner, bool isMenuBar) : base(owner)
        {
            this._isMenuBar = isMenuBar;
            this._popups = new Dictionary<MenuElement, Widget>();
            /*this.closeCB = new Runnable() {
                public void run() {
                    closePopup();
                }
            };
            this.timerCB = new Runnable() {
                public void run() {
                    popupTimer();
                }
            };*/
        }

        bool IsSubMenuOpen(Menu menu)
        {
            Widget popup = _popups[menu];
            if (popup != null)
            {
                return popup.GetParent() == this;
            }
            return false;
        }

        void CloseSubMenu(int level)
        {
            while (GetNumChildren() > level)
            {
                CloseSubMenu();
            }
        }

        internal Widget OpenSubMenu(int level, Menu menu, Widget btn, bool setPosition)
        {
            Widget popup = null;
            if (_popups.ContainsKey(menu))
            {
                popup = _popups[menu];
            }
            if (popup == null)
            {
                popup = menu.CreatePopup(this, level + 1, btn);
                _popups.Add(menu, popup);
            }

            if (popup.GetParent() == this)
            {
                CloseSubMenu(level + 1);
                return popup;
            }

            if (!IsOpen())
            {
                if (!OpenPopup())
                {
                    ClosePopup();
                    return null;
                }
                GetParent().LayoutChildFullInnerArea(this);
            }

            while (GetNumChildren() > level)
            {
                CloseSubMenu();
            }
            Add(popup);

            popup.AdjustSize();

            if (setPosition)
            {
                int popupWidth = popup.GetWidth();
                int popupX = btn.GetRight();
                int popupY = btn.GetY();

                if (level == 0)
                {
                    popupX = btn.GetX();
                    popupY = btn.GetBottom();
                }

                if (popupWidth + btn.GetRight() > GetInnerRight())
                {
                    popupX = btn.GetX() - popupWidth;
                    if (popupX < GetInnerX())
                    {
                        popupX = GetInnerRight() - popupWidth;
                    }
                }

                int popupHeight = popup.GetHeight();
                if (popupY + popupHeight > GetInnerBottom())
                {
                    popupY = Math.Max(GetInnerY(), GetInnerBottom() - popupHeight);
                }

                popup.SetPosition(popupX, popupY);
            }

            return popup;
        }

        void CloseSubMenu()
        {
            RemoveChild(GetNumChildren() - 1);
        }

        public override void ClosePopup()
        {
            StopTimer();
            GUI gui = GetGUI();
            base.ClosePopup();
            RemoveAllChildren();
            _popups.Clear();
            if (gui != null)
            {
                gui.ResendLastMouseMove();
            }
        }

        /**
         * Returns the popup widget for the specified menu
         * @param menu the menu for which to return the popup
         * @return the popup widget or null if not open
         */
        public Widget GetPopupForMenu(Menu menu)
        {
            return _popups[menu];
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            _timer = gui.CreateTimer();
            _timer.SetDelay(300);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            this.PopupTimer();
        }

        protected override void Layout()
        {
        }

        internal override Widget RouteMouseEvent(Event evt)
        {
            _mouseOverOwner = false;
            Widget widget = base.RouteMouseEvent(evt);
            if (widget == this && _isMenuBar && GetOwner().IsMouseInside(evt))
            {
                Widget menuBarWidget = GetOwner().RouteMouseEvent(evt);
                if (menuBarWidget != null)
                {
                    _mouseOverOwner = true;
                    widget = menuBarWidget;
                }
            }

            Widget mouseOverWidget = GetWidgetUnderMouse();
            if (_lastMouseOverWidget != mouseOverWidget)
            {
                _lastMouseOverWidget = mouseOverWidget;
                if (_isMenuBar && widget.GetParent() == GetOwner() && (widget is Menu.SubMenuBtn))
                {
                    PopupTimer();   // no delay on menu bar itself
                }
                else
                {
                    StartTimer();
                }
            }

            return widget;
        }

        protected override bool HandleEventPopup(Event evt)
        {
            if (_isMenuBar && GetOwner().HandleEvent(evt))
            {
                return true;
            }
            if (base.HandleEventPopup(evt))
            {
                return true;
            }
            if (evt.GetEventType() == EventType.MOUSE_CLICKED)
            {
                MouseClickedOutside(evt);
                return true;
            }
            return false;
        }

        internal override Widget GetWidgetUnderMouse()
        {
            if (_mouseOverOwner)
            {
                return GetOwner().GetWidgetUnderMouse();
            }
            return base.GetWidgetUnderMouse();
        }

        void PopupTimer()
        {
            if ((_lastMouseOverWidget is Menu.SubMenuBtn) && _lastMouseOverWidget.IsEnabled())
            {
                ((Menu.SubMenuBtn)_lastMouseOverWidget).OpenSubMenu();
            }
            else if (_lastMouseOverWidget != this)
            {
                int level = 0;
                // search for the MenuPopup containing this widget
                // it knows which menu level we need to close
                for (Widget w = _lastMouseOverWidget; w != null; w = w.GetParent())
                {
                    if (w is Menu.MenuPopup)
                    {
                        level = ((Menu.MenuPopup)w)._level;
                        break;
                    }
                }
                CloseSubMenu(level);
            }
        }

        void StartTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }
    }
}
