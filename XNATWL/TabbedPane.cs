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
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class TabbedPane : Widget
    {
        public static StateKey STATE_FIRST_TAB = StateKey.Get("firstTab");
        public static StateKey STATE_LAST_TAB = StateKey.Get("lastTab");

        public enum TabPosition
        {
            Top,
            Left,
            Right,
            Bottom
        }

        public bool TabPosition_Horz(TabPosition tabPosition)
        {
            switch (tabPosition)
            {
                case TabPosition.Top:
                    return true;
                case TabPosition.Left:
                    return false;
                case TabPosition.Right:
                    return true;
                case TabPosition.Bottom:
                    return false;
            }

            return true;
        }

        private List<Tab> _tabs;
        private BoxLayout _tabBox;
        private Widget _tabBoxClip;
        private Container _container;
        Container _innerContainer;

        DialogLayout _scrollControls;
        Button _btnScrollLeft;
        Button _btnScrollRight;

        bool _bScrollTabs;
        int _tabScrollPosition;
        TabPosition _tabPosition;
        Tab _activeTab;

        public TabbedPane()
        {
            this._tabs = new List<Tab>();
            this._tabBox = new BoxLayout();
            this._tabBoxClip = new Widget();
            this._container = new Container();
            this._innerContainer = new Container();
            this._tabPosition = TabPosition.Top;

            _tabBox.SetTheme("tabbox");
            _tabBoxClip.SetTheme("");
            _innerContainer.SetTheme("");
            _innerContainer.SetClip(true);

            _tabBoxClip.Add(_tabBox);
            _container.Add(_innerContainer);

            base.InsertChild(_container, 0);
            base.InsertChild(_tabBoxClip, 1);

            AddActionMapping("nextTab", "CycleTabs", +1);
            AddActionMapping("prevTab", "CycleTabs", -1);
            SetCanAcceptKeyboardFocus(false);
        }

        public TabPosition GetTabPosition()
        {
            return _tabPosition;
        }

        public void SetTabPosition(TabPosition tabPosition)
        {
            if (this._tabPosition != tabPosition)
            {
                this._tabPosition = tabPosition;
                _tabBox.SetDirection(TabPosition_Horz(tabPosition)
                        ? BoxLayout.Direction.Horizontal
                        : BoxLayout.Direction.Vertical);
                InvalidateLayout();
            }
        }

        public bool IsScrollTabs()
        {
            return _bScrollTabs;
        }

        /**
         * Allow the tabs to be scrolled if they don't fit into the available space.
         *
         * Default is false.
         *
         * If disabled the minimum size of the tabbed pane ensures that all tabs fit.
         * If enabled additional scroll controlls are displayed.
         *
         * @param scrollTabs true if tabs should scroll
         */
        public void SetScrollTabs(bool scrollTabs)
        {
            if (this._bScrollTabs != scrollTabs)
            {
                this._bScrollTabs = scrollTabs;

                if (_scrollControls == null && scrollTabs)
                {
                    CreateScrollControls();
                }

                _tabBoxClip.SetClip(scrollTabs);
                if (_scrollControls != null)
                {
                    _scrollControls.SetVisible(scrollTabs);
                }
                InvalidateLayout();
            }
        }

        public Tab AddTab(String title, Widget pane)
        {
            Tab tab = new Tab(this);
            tab.SetTitle(title);
            tab.SetPane(pane);
            _tabBox.Add(tab._button);
            _tabs.Add(tab);

            if (_tabs.Count == 1)
            {
                SetActiveTab(tab);
            }
            UpdateTabStates();
            return tab;
        }

        public Tab GetActiveTab()
        {
            return _activeTab;
        }

        public void SetActiveTab(Tab tab)
        {
            if (tab != null)
            {
                ValidateTab(tab);
            }

            if (_activeTab != tab)
            {
                Tab prevTab = _activeTab;
                _activeTab = tab;

                if (prevTab != null)
                {
                    prevTab.DoCallback();
                }
                if (tab != null)
                {
                    tab.DoCallback();
                }

                if (_bScrollTabs)
                {
                    ValidateLayout();

                    int pos, end, size;
                    if (TabPosition_Horz(_tabPosition))
                    {
                        pos = tab._button.GetX() - _tabBox.GetX();
                        end = tab._button.GetWidth() + pos;
                        size = _tabBoxClip.GetWidth();
                    }
                    else
                    {
                        pos = tab._button.GetY() - _tabBox.GetY();
                        end = tab._button.GetHeight() + pos;
                        size = _tabBoxClip.GetHeight();
                    }
                    int border = (size + 19) / 20;
                    pos -= border;
                    end += border;
                    if (pos < _tabScrollPosition)
                    {
                        SetScrollPos(pos);
                    }
                    else if (end > _tabScrollPosition + size)
                    {
                        SetScrollPos(end - size);
                    }
                }

                if (tab != null && tab._pane != null)
                {
                    tab._pane.RequestKeyboardFocus();
                }
            }
        }

        public void RemoveTab(Tab tab)
        {
            ValidateTab(tab);

            int idx = (tab == _activeTab) ? _tabs.IndexOf(tab) : -1;
            tab.SetPane(null);
            _tabBox.RemoveChild(tab._button);
            _tabs.Remove(tab);

            if (idx >= 0 && _tabs.Count != 0)
            {
                SetActiveTab(_tabs[Math.Min(_tabs.Count - 1, idx)]);
            }
            UpdateTabStates();
        }

        public void RemoveAllTabs()
        {
            _innerContainer.RemoveAllChildren();
            _tabBox.RemoveAllChildren();
            _tabs.Clear();
            _activeTab = null;
        }

        public int GetNumTabs()
        {
            return _tabs.Count;
        }

        public Tab GetTab(int index)
        {
            return _tabs[index];
        }

        public int GetActiveTabIndex()
        {
            if (_tabs.Count == 0)
            {
                return -1;
            }
            return _tabs.IndexOf(_activeTab);
        }

        public void CycleTabs(int direction)
        {
            if (_tabs.Count != 0)
            {
                int idx = _tabs.IndexOf(_activeTab);
                if (idx < 0)
                {
                    idx = 0;
                }
                else
                {
                    idx += direction;
                    idx %= _tabs.Count;
                    idx += _tabs.Count;
                    idx %= _tabs.Count;
                }
                SetActiveTab(_tabs[idx]);
            }
        }

        public override int GetMinWidth()
        {
            int minWidth;
            if (TabPosition_Horz(_tabPosition))
            {
                int tabBoxWidth;
                if (_bScrollTabs)
                {
                    tabBoxWidth = _tabBox.GetBorderHorizontal() +
                            BoxLayout.ComputeMinWidthVertical(_tabBox) +
                            _scrollControls.GetPreferredWidth();
                }
                else
                {
                    tabBoxWidth = _tabBox.GetMinWidth();
                }
                minWidth = Math.Max(_container.GetMinWidth(), tabBoxWidth);
            }
            else
            {
                minWidth = _container.GetMinWidth() + _tabBox.GetMinWidth();
            }
            return Math.Max(base.GetMinWidth(), minWidth + GetBorderHorizontal());
        }

        public override int GetMinHeight()
        {
            int minHeight;
            if (TabPosition_Horz(_tabPosition))
            {
                minHeight = _container.GetMinHeight() + _tabBox.GetMinHeight();
            }
            else
            {
                minHeight = Math.Max(_container.GetMinHeight(), _tabBox.GetMinHeight());
            }
            return Math.Max(base.GetMinHeight(), minHeight + GetBorderVertical());
        }

        public override int GetPreferredInnerWidth()
        {
            if (TabPosition_Horz(_tabPosition))
            {
                int tabBoxWidth;
                if (_bScrollTabs)
                {
                    tabBoxWidth = _tabBox.GetBorderHorizontal() +
                            BoxLayout.ComputePreferredWidthVertical(_tabBox) +
                            _scrollControls.GetPreferredWidth();
                }
                else
                {
                    tabBoxWidth = _tabBox.GetPreferredWidth();
                }
                return Math.Max(_container.GetPreferredWidth(), tabBoxWidth);
            }
            else
            {
                return _container.GetPreferredWidth() + _tabBox.GetPreferredWidth();
            }
        }

        public override int GetPreferredInnerHeight()
        {
            if (TabPosition_Horz(_tabPosition))
            {
                return _container.GetPreferredHeight() + _tabBox.GetPreferredHeight();
            }
            else
            {
                return Math.Max(_container.GetPreferredHeight(), _tabBox.GetPreferredHeight());
            }
        }

        protected override void Layout()
        {
            int scrollCtrlsWidth = 0;
            int scrollCtrlsHeight = 0;
            int tabBoxWidth = _tabBox.GetPreferredWidth();
            int tabBoxHeight = _tabBox.GetPreferredHeight();

            if (_bScrollTabs)
            {
                scrollCtrlsWidth = _scrollControls.GetPreferredWidth();
                scrollCtrlsHeight = _scrollControls.GetPreferredHeight();
            }

            if (TabPosition_Horz(_tabPosition))
            {
                tabBoxHeight = Math.Max(scrollCtrlsHeight, tabBoxHeight);
            }
            else
            {
                tabBoxWidth = Math.Max(scrollCtrlsWidth, tabBoxWidth);
            }

            _tabBox.SetSize(tabBoxWidth, tabBoxHeight);

            switch (_tabPosition)
            {
                case TabPosition.Top:
                    _tabBoxClip.SetPosition(GetInnerX(), GetInnerY());
                    _tabBoxClip.SetSize(Math.Max(0, GetInnerWidth() - scrollCtrlsWidth), tabBoxHeight);
                    _container.SetSize(GetInnerWidth(), Math.Max(0, GetInnerHeight() - tabBoxHeight));
                    _container.SetPosition(GetInnerX(), _tabBoxClip.GetBottom());
                    break;

                case TabPosition.Left:
                    _tabBoxClip.SetPosition(GetInnerX(), GetInnerY());
                    _tabBoxClip.SetSize(tabBoxWidth, Math.Max(0, GetInnerHeight() - scrollCtrlsHeight));
                    _container.SetSize(Math.Max(0, GetInnerWidth() - tabBoxWidth), GetInnerHeight());
                    _container.SetPosition(_tabBoxClip.GetRight(), GetInnerY());
                    break;

                case TabPosition.Right:
                    _tabBoxClip.SetPosition(GetInnerX() - tabBoxWidth, GetInnerY());
                    _tabBoxClip.SetSize(tabBoxWidth, Math.Max(0, GetInnerHeight() - scrollCtrlsHeight));
                    _container.SetSize(Math.Max(0, GetInnerWidth() - tabBoxWidth), GetInnerHeight());
                    _container.SetPosition(GetInnerX(), GetInnerY());
                    break;

                case TabPosition.Bottom:
                    _tabBoxClip.SetPosition(GetInnerX(), GetInnerY() - tabBoxHeight);
                    _tabBoxClip.SetSize(Math.Max(0, GetInnerWidth() - scrollCtrlsWidth), tabBoxHeight);
                    _container.SetSize(GetInnerWidth(), Math.Max(0, GetInnerHeight() - tabBoxHeight));
                    _container.SetPosition(GetInnerX(), GetInnerY());
                    break;
            }

            if (_scrollControls != null)
            {
                if (TabPosition_Horz(_tabPosition))
                {
                    _scrollControls.SetPosition(_tabBoxClip.GetRight(), _tabBoxClip.GetY());
                    _scrollControls.SetSize(scrollCtrlsWidth, tabBoxHeight);
                }
                else
                {
                    _scrollControls.SetPosition(_tabBoxClip.GetX(), _tabBoxClip.GetBottom());
                    _scrollControls.SetSize(tabBoxWidth, scrollCtrlsHeight);
                }
                SetScrollPos(_tabScrollPosition);
            }
        }

        private void CreateScrollControls()
        {
            _scrollControls = new DialogLayout();
            _scrollControls.SetTheme("scrollControls");

            _btnScrollLeft = new Button();
            _btnScrollLeft.SetTheme("scrollLeft");
            _btnScrollLeft.Action += (sender, e) =>
            {
                ScrollTabs(-1);
            };

            _btnScrollRight = new Button();
            _btnScrollRight.SetTheme("scrollRight");
            _btnScrollRight.Action += (sender, e) =>
            {
                ScrollTabs(1);
            };

            DialogLayout.Group horz = _scrollControls.CreateSequentialGroup()
                    .AddWidget(_btnScrollLeft)
                    .AddGap("scrollButtons")
                    .AddWidget(_btnScrollRight);

            DialogLayout.Group vert = _scrollControls.CreateParallelGroup()
                    .AddWidget(_btnScrollLeft)
                    .AddWidget(_btnScrollRight);

            _scrollControls.SetHorizontalGroup(horz);
            _scrollControls.SetVerticalGroup(vert);

            base.InsertChild(_scrollControls, 2);
        }

        void ScrollTabs(int dir)
        {
            dir *= Math.Max(1, _tabBoxClip.GetWidth() / 10);
            SetScrollPos(_tabScrollPosition + dir);
        }

        private void SetScrollPos(int pos)
        {
            int maxPos;
            if (TabPosition_Horz(_tabPosition))
            {
                maxPos = _tabBox.GetWidth() - _tabBoxClip.GetWidth();
            }
            else
            {
                maxPos = _tabBox.GetHeight() - _tabBoxClip.GetHeight();
            }
            pos = Math.Max(0, Math.Min(pos, maxPos));
            _tabScrollPosition = pos;
            if (TabPosition_Horz(_tabPosition))
            {
                _tabBox.SetPosition(_tabBoxClip.GetX() - pos, _tabBoxClip.GetY());
            }
            else
            {
                _tabBox.SetPosition(_tabBoxClip.GetX(), _tabBoxClip.GetY() - pos);
            }
            if (_scrollControls != null)
            {
                _btnScrollLeft.SetEnabled(pos > 0);
                _btnScrollRight.SetEnabled(pos < maxPos);
            }
        }

        public override void InsertChild(Widget child, int index)
        {
            throw new NotImplementedException("use addTab/removeTab");
        }

        public override void RemoveAllChildren()
        {
            throw new NotImplementedException("use addTab/removeTab");
        }

        public override Widget RemoveChild(int index)
        {
            throw new NotImplementedException("use addTab/removeTab");
        }

        protected void UpdateTabStates()
        {
            for (int i = 0, n = _tabs.Count; i < n; i++)
            {
                Tab tab = _tabs[i];
                AnimationState animationState = tab._button.GetAnimationState();
                animationState.SetAnimationState(STATE_FIRST_TAB, i == 0);
                animationState.SetAnimationState(STATE_LAST_TAB, i == n - 1);
            }
        }

        private void ValidateTab(Tab tab)
        {
            if (tab._button.GetParent() != _tabBox)
            {
                throw new ArgumentException("Invalid tab");
            }
        }

        public class Tab : BooleanModel
        {
            protected internal TabButton _button;
            protected internal Widget _pane;
            protected internal Runnable _closeCallback;
            protected internal Object _userValue;

            private TabbedPane _tabbedPane;

            public event EventHandler<BooleanChangedEventArgs> Changed;

            public Tab(TabbedPane tabbedPane)
            {
                this._tabbedPane = tabbedPane;
                _button = new TabButton(this);
            }

            public bool Value
            {
                get
                {
                    return this._tabbedPane._activeTab == this;
                }
                set
                {
                    if (value)
                    {
                        this._tabbedPane.SetActiveTab(this);
                    }
                }
            }

            public Widget GetPane()
            {
                return _pane;
            }

            public void SetPane(Widget pane)
            {
                if (this._pane != pane)
                {
                    if (this._pane != null)
                    {
                        this._tabbedPane._innerContainer.RemoveChild(this._pane);
                    }
                    this._pane = pane;
                    if (pane != null)
                    {
                        pane.SetVisible(this.Value);
                        this._tabbedPane._innerContainer.Add(pane);
                    }
                }
            }

            public Tab SetTitle(String title)
            {
                _button.SetText(title);
                return this;
            }

            public String GetTitle()
            {
                return _button.GetText();
            }

            public Tab SetTooltipContent(Object tooltipContent)
            {
                _button.SetTooltipContent(tooltipContent);
                return this;
            }

            public Object GetUserValue()
            {
                return _userValue;
            }

            public void SetUserValue(Object userValue)
            {
                this._userValue = userValue;
            }

            /**
             * Sets the user theme for the tab button. If no user theme is set
             * ({@code null}) then it will use "tabbutton" or
             * "tabbuttonWithCloseButton" if a close callback is registered.
             * 
             * @param theme the user theme name - can be null.
             * @return {@code this}
             */
            public Tab SetTheme(String theme)
            {
                _button.SetUserTheme(theme);
                return this;
            }

            public Runnable GetCloseCallback()
            {
                return _closeCallback;
            }

            public void SetCloseCallback(Runnable closeCallback)
            {
                if (this._closeCallback != null)
                {
                    _button.RemoveCloseButton();
                }
                this._closeCallback = closeCallback;
                if (closeCallback != null)
                {
                    _button.SetCloseButton(closeCallback);
                }
            }

            protected internal void DoCallback()
            {
                if (_pane != null)
                {
                    _pane.SetVisible(this.Value);
                }

                this.Changed.Invoke(this, new BooleanChangedEventArgs(this.Value, this.Value));
            }
        }

        public class TabButton : ToggleButton
        {
            Button _closeButton;
            Alignment _closeButtonAlignment;
            int _closeButtonOffsetX;
            int _closeButtonOffsetY;
            String _userTheme;

            public TabButton(BooleanModel model) : base(model)
            {
                SetCanAcceptKeyboardFocus(false);
                _closeButtonAlignment = Alignment.RIGHT;
            }

            public void SetUserTheme(String userTheme)
            {
                this._userTheme = userTheme;
                DoSetTheme();
            }

            private void DoSetTheme()
            {
                if (_userTheme != null)
                {
                    SetTheme(_userTheme);
                }
                else if (_closeButton != null)
                {
                    SetTheme("tabbuttonWithCloseButton");
                }
                else
                {
                    SetTheme("tabbutton");
                }
                ReapplyTheme();
            }

            protected override void ApplyTheme(ThemeInfo themeInfo)
            {
                base.ApplyTheme(themeInfo);
                if (_closeButton != null)
                {
                    _closeButtonAlignment = (Alignment)themeInfo.GetParameter("closeButtonAlignment", Alignment.RIGHT);
                    _closeButtonOffsetX = themeInfo.GetParameter("closeButtonOffsetX", 0);
                    _closeButtonOffsetY = themeInfo.GetParameter("closeButtonOffsetY", 0);
                }
                else
                {
                    _closeButtonAlignment = Alignment.RIGHT;
                    _closeButtonOffsetX = 0;
                    _closeButtonOffsetY = 0;
                }
            }

            protected internal void SetCloseButton(Runnable callback)
            {
                _closeButton = new Button();
                _closeButton.SetTheme("closeButton");
                DoSetTheme();
                Add(_closeButton);
                _closeButton.Action += (sender, e) =>
                {
                    callback.Run();
                };
            }

            protected internal void RemoveCloseButton()
            {
                RemoveChild(_closeButton);
                _closeButton = null;
                DoSetTheme();
            }

            public override int GetPreferredInnerHeight()
            {
                return ComputeTextHeight();
            }

            public override int GetPreferredInnerWidth()
            {
                return ComputeTextWidth();
            }

            protected override void Layout()
            {
                if (_closeButton != null)
                {
                    _closeButton.AdjustSize();
                    _closeButton.SetPosition(
                            GetX() + _closeButtonOffsetX + _closeButtonAlignment.ComputePositionX(GetWidth(), _closeButton.GetWidth()),
                            GetY() + _closeButtonOffsetY + _closeButtonAlignment.ComputePositionY(GetHeight(), _closeButton.GetHeight()));
                }
            }
        }

    }
}
