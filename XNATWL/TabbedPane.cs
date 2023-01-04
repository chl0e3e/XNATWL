using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            TOP,
            LEFT,
            RIGHT,
            BOTTOM
        }

        public bool TabPosition_Horz(TabPosition tabPosition)
        {
            switch (tabPosition)
            {
                case TabPosition.TOP:
                    return true;
                case TabPosition.LEFT:
                    return false;
                case TabPosition.RIGHT:
                    return true;
                case TabPosition.BOTTOM:
                    return false;
            }

            return true;
        }

        private List<Tab> tabs;
        private BoxLayout tabBox;
        private Widget tabBoxClip;
        private Container container;
        Container innerContainer;

        DialogLayout scrollControlls;
        Button btnScrollLeft;
        Button btnScrollRight;

        bool bScrollTabs;
        int tabScrollPosition;
        TabPosition tabPosition;
        Tab activeTab;

        public TabbedPane()
        {
            this.tabs = new List<Tab>();
            this.tabBox = new BoxLayout();
            this.tabBoxClip = new Widget();
            this.container = new Container();
            this.innerContainer = new Container();
            this.tabPosition = TabPosition.TOP;

            tabBox.setTheme("tabbox");
            tabBoxClip.setTheme("");
            innerContainer.setTheme("");
            innerContainer.setClip(true);

            tabBoxClip.add(tabBox);
            container.add(innerContainer);

            base.insertChild(container, 0);
            base.insertChild(tabBoxClip, 1);

            addActionMapping("nextTab", "cycleTabs", +1);
            addActionMapping("prevTab", "cycleTabs", -1);
            setCanAcceptKeyboardFocus(false);
        }

        public TabPosition getTabPosition()
        {
            return tabPosition;
        }

        public void setTabPosition(TabPosition tabPosition)
        {
            if (this.tabPosition != tabPosition)
            {
                this.tabPosition = tabPosition;
                tabBox.setDirection(TabPosition_Horz(tabPosition)
                        ? BoxLayout.Direction.HORIZONTAL
                        : BoxLayout.Direction.VERTICAL);
                invalidateLayout();
            }
        }

        public bool isScrollTabs()
        {
            return bScrollTabs;
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
        public void setScrollTabs(bool scrollTabs)
        {
            if (this.bScrollTabs != scrollTabs)
            {
                this.bScrollTabs = scrollTabs;

                if (scrollControlls == null && scrollTabs)
                {
                    createScrollControlls();
                }

                tabBoxClip.setClip(scrollTabs);
                if (scrollControlls != null)
                {
                    scrollControlls.setVisible(scrollTabs);
                }
                invalidateLayout();
            }
        }

        public Tab addTab(String title, Widget pane)
        {
            Tab tab = new Tab(this);
            tab.setTitle(title);
            tab.setPane(pane);
            tabBox.add(tab.button);
            tabs.Add(tab);

            if (tabs.Count == 1)
            {
                setActiveTab(tab);
            }
            updateTabStates();
            return tab;
        }

        public Tab getActiveTab()
        {
            return activeTab;
        }

        public void setActiveTab(Tab tab)
        {
            if (tab != null)
            {
                validateTab(tab);
            }

            if (activeTab != tab)
            {
                Tab prevTab = activeTab;
                activeTab = tab;

                if (prevTab != null)
                {
                    prevTab.doCallback();
                }
                if (tab != null)
                {
                    tab.doCallback();
                }

                if (bScrollTabs)
                {
                    validateLayout();

                    int pos, end, size;
                    if (TabPosition_Horz(tabPosition))
                    {
                        pos = tab.button.getX() - tabBox.getX();
                        end = tab.button.getWidth() + pos;
                        size = tabBoxClip.getWidth();
                    }
                    else
                    {
                        pos = tab.button.getY() - tabBox.getY();
                        end = tab.button.getHeight() + pos;
                        size = tabBoxClip.getHeight();
                    }
                    int border = (size + 19) / 20;
                    pos -= border;
                    end += border;
                    if (pos < tabScrollPosition)
                    {
                        setScrollPos(pos);
                    }
                    else if (end > tabScrollPosition + size)
                    {
                        setScrollPos(end - size);
                    }
                }

                if (tab != null && tab.pane != null)
                {
                    tab.pane.requestKeyboardFocus();
                }
            }
        }

        public void removeTab(Tab tab)
        {
            validateTab(tab);

            int idx = (tab == activeTab) ? tabs.IndexOf(tab) : -1;
            tab.setPane(null);
            tabBox.removeChild(tab.button);
            tabs.Remove(tab);

            if (idx >= 0 && tabs.Count != 0)
            {
                setActiveTab(tabs[Math.Min(tabs.Count - 1, idx)]);
            }
            updateTabStates();
        }

        public void removeAllTabs()
        {
            innerContainer.removeAllChildren();
            tabBox.removeAllChildren();
            tabs.Clear();
            activeTab = null;
        }

        public int getNumTabs()
        {
            return tabs.Count;
        }

        public Tab getTab(int index)
        {
            return tabs[index];
        }

        public int getActiveTabIndex()
        {
            if (tabs.Count == 0)
            {
                return -1;
            }
            return tabs.IndexOf(activeTab);
        }

        public void cycleTabs(int direction)
        {
            if (tabs.Count != 0)
            {
                int idx = tabs.IndexOf(activeTab);
                if (idx < 0)
                {
                    idx = 0;
                }
                else
                {
                    idx += direction;
                    idx %= tabs.Count;
                    idx += tabs.Count;
                    idx %= tabs.Count;
                }
                setActiveTab(tabs[idx]);
            }
        }

        public override int getMinWidth()
        {
            int minWidth;
            if (TabPosition_Horz(tabPosition))
            {
                int tabBoxWidth;
                if (bScrollTabs)
                {
                    tabBoxWidth = tabBox.getBorderHorizontal() +
                            BoxLayout.computeMinWidthVertical(tabBox) +
                            scrollControlls.getPreferredWidth();
                }
                else
                {
                    tabBoxWidth = tabBox.getMinWidth();
                }
                minWidth = Math.Max(container.getMinWidth(), tabBoxWidth);
            }
            else
            {
                minWidth = container.getMinWidth() + tabBox.getMinWidth();
            }
            return Math.Max(base.getMinWidth(), minWidth + getBorderHorizontal());
        }

        public override int getMinHeight()
        {
            int minHeight;
            if (TabPosition_Horz(tabPosition))
            {
                minHeight = container.getMinHeight() + tabBox.getMinHeight();
            }
            else
            {
                minHeight = Math.Max(container.getMinHeight(), tabBox.getMinHeight());
            }
            return Math.Max(base.getMinHeight(), minHeight + getBorderVertical());
        }

        public override int getPreferredInnerWidth()
        {
            if (TabPosition_Horz(tabPosition))
            {
                int tabBoxWidth;
                if (bScrollTabs)
                {
                    tabBoxWidth = tabBox.getBorderHorizontal() +
                            BoxLayout.computePreferredWidthVertical(tabBox) +
                            scrollControlls.getPreferredWidth();
                }
                else
                {
                    tabBoxWidth = tabBox.getPreferredWidth();
                }
                return Math.Max(container.getPreferredWidth(), tabBoxWidth);
            }
            else
            {
                return container.getPreferredWidth() + tabBox.getPreferredWidth();
            }
        }

        public override int getPreferredInnerHeight()
        {
            if (TabPosition_Horz(tabPosition))
            {
                return container.getPreferredHeight() + tabBox.getPreferredHeight();
            }
            else
            {
                return Math.Max(container.getPreferredHeight(), tabBox.getPreferredHeight());
            }
        }

        protected override void layout()
        {
            int scrollCtrlsWidth = 0;
            int scrollCtrlsHeight = 0;
            int tabBoxWidth = tabBox.getPreferredWidth();
            int tabBoxHeight = tabBox.getPreferredHeight();

            if (bScrollTabs)
            {
                scrollCtrlsWidth = scrollControlls.getPreferredWidth();
                scrollCtrlsHeight = scrollControlls.getPreferredHeight();
            }

            if (TabPosition_Horz(tabPosition))
            {
                tabBoxHeight = Math.Max(scrollCtrlsHeight, tabBoxHeight);
            }
            else
            {
                tabBoxWidth = Math.Max(scrollCtrlsWidth, tabBoxWidth);
            }

            tabBox.setSize(tabBoxWidth, tabBoxHeight);

            switch (tabPosition)
            {
                case TabPosition.TOP:
                    tabBoxClip.setPosition(getInnerX(), getInnerY());
                    tabBoxClip.setSize(Math.Max(0, getInnerWidth() - scrollCtrlsWidth), tabBoxHeight);
                    container.setSize(getInnerWidth(), Math.Max(0, getInnerHeight() - tabBoxHeight));
                    container.setPosition(getInnerX(), tabBoxClip.getBottom());
                    break;

                case TabPosition.LEFT:
                    tabBoxClip.setPosition(getInnerX(), getInnerY());
                    tabBoxClip.setSize(tabBoxWidth, Math.Max(0, getInnerHeight() - scrollCtrlsHeight));
                    container.setSize(Math.Max(0, getInnerWidth() - tabBoxWidth), getInnerHeight());
                    container.setPosition(tabBoxClip.getRight(), getInnerY());
                    break;

                case TabPosition.RIGHT:
                    tabBoxClip.setPosition(getInnerX() - tabBoxWidth, getInnerY());
                    tabBoxClip.setSize(tabBoxWidth, Math.Max(0, getInnerHeight() - scrollCtrlsHeight));
                    container.setSize(Math.Max(0, getInnerWidth() - tabBoxWidth), getInnerHeight());
                    container.setPosition(getInnerX(), getInnerY());
                    break;

                case TabPosition.BOTTOM:
                    tabBoxClip.setPosition(getInnerX(), getInnerY() - tabBoxHeight);
                    tabBoxClip.setSize(Math.Max(0, getInnerWidth() - scrollCtrlsWidth), tabBoxHeight);
                    container.setSize(getInnerWidth(), Math.Max(0, getInnerHeight() - tabBoxHeight));
                    container.setPosition(getInnerX(), getInnerY());
                    break;
            }

            if (scrollControlls != null)
            {
                if (TabPosition_Horz(tabPosition))
                {
                    scrollControlls.setPosition(tabBoxClip.getRight(), tabBoxClip.getY());
                    scrollControlls.setSize(scrollCtrlsWidth, tabBoxHeight);
                }
                else
                {
                    scrollControlls.setPosition(tabBoxClip.getX(), tabBoxClip.getBottom());
                    scrollControlls.setSize(tabBoxWidth, scrollCtrlsHeight);
                }
                setScrollPos(tabScrollPosition);
            }
        }

        private void createScrollControlls()
        {
            scrollControlls = new DialogLayout();
            scrollControlls.setTheme("scrollControls");

            btnScrollLeft = new Button();
            btnScrollLeft.setTheme("scrollLeft");
            btnScrollLeft.Action += (sender, e) =>
            {
                scrollTabs(-1);
            };

            btnScrollRight = new Button();
            btnScrollRight.setTheme("scrollRight");
            btnScrollRight.Action += (sender, e) =>
            {
                scrollTabs(1);
            };

            DialogLayout.Group horz = scrollControlls.createSequentialGroup()
                    .addWidget(btnScrollLeft)
                    .addGap("scrollButtons")
                    .addWidget(btnScrollRight);

            DialogLayout.Group vert = scrollControlls.createParallelGroup()
                    .addWidget(btnScrollLeft)
                    .addWidget(btnScrollRight);

            scrollControlls.setHorizontalGroup(horz);
            scrollControlls.setVerticalGroup(vert);

            base.insertChild(scrollControlls, 2);
        }

        void scrollTabs(int dir)
        {
            dir *= Math.Max(1, tabBoxClip.getWidth() / 10);
            setScrollPos(tabScrollPosition + dir);
        }

        private void setScrollPos(int pos)
        {
            int maxPos;
            if (TabPosition_Horz(tabPosition))
            {
                maxPos = tabBox.getWidth() - tabBoxClip.getWidth();
            }
            else
            {
                maxPos = tabBox.getHeight() - tabBoxClip.getHeight();
            }
            pos = Math.Max(0, Math.Min(pos, maxPos));
            tabScrollPosition = pos;
            if (TabPosition_Horz(tabPosition))
            {
                tabBox.setPosition(tabBoxClip.getX() - pos, tabBoxClip.getY());
            }
            else
            {
                tabBox.setPosition(tabBoxClip.getX(), tabBoxClip.getY() - pos);
            }
            if (scrollControlls != null)
            {
                btnScrollLeft.setEnabled(pos > 0);
                btnScrollRight.setEnabled(pos < maxPos);
            }
        }

        public override void insertChild(Widget child, int index)
        {
            throw new NotImplementedException("use addTab/removeTab");
        }

        public override void removeAllChildren()
        {
            throw new NotImplementedException("use addTab/removeTab");
        }

        public override Widget removeChild(int index)
        {
            throw new NotImplementedException("use addTab/removeTab");
        }

        protected void updateTabStates()
        {
            for (int i = 0, n = tabs.Count; i < n; i++)
            {
                Tab tab = tabs[i];
                AnimationState animationState = tab.button.getAnimationState();
                animationState.setAnimationState(STATE_FIRST_TAB, i == 0);
                animationState.setAnimationState(STATE_LAST_TAB, i == n - 1);
            }
        }

        private void validateTab(Tab tab)
        {
            if (tab.button.getParent() != tabBox)
            {
                throw new ArgumentException("Invalid tab");
            }
        }

        public class Tab : BooleanModel
        {
            protected internal TabButton button;
            protected internal Widget pane;
            protected internal Runnable closeCallback;
            protected internal Object userValue;

            private TabbedPane tabbedPane;

            public event EventHandler<BooleanChangedEventArgs> Changed;

            public Tab(TabbedPane tabbedPane)
            {
                this.tabbedPane = tabbedPane;
                button = new TabButton(this);
            }

            public bool Value
            {
                get
                {
                    return this.tabbedPane.activeTab == this;
                }
                set
                {
                    if (value)
                    {
                        this.tabbedPane.setActiveTab(this);
                    }
                }
            }

            public Widget getPane()
            {
                return pane;
            }

            public void setPane(Widget pane)
            {
                if (this.pane != pane)
                {
                    if (this.pane != null)
                    {
                        this.tabbedPane.innerContainer.removeChild(this.pane);
                    }
                    this.pane = pane;
                    if (pane != null)
                    {
                        pane.setVisible(this.Value);
                        this.tabbedPane.innerContainer.add(pane);
                    }
                }
            }

            public Tab setTitle(String title)
            {
                button.setText(title);
                return this;
            }

            public String getTitle()
            {
                return button.getText();
            }

            public Tab setTooltipContent(Object tooltipContent)
            {
                button.setTooltipContent(tooltipContent);
                return this;
            }

            public Object getUserValue()
            {
                return userValue;
            }

            public void setUserValue(Object userValue)
            {
                this.userValue = userValue;
            }

            /**
             * Sets the user theme for the tab button. If no user theme is set
             * ({@code null}) then it will use "tabbutton" or
             * "tabbuttonWithCloseButton" if a close callback is registered.
             * 
             * @param theme the user theme name - can be null.
             * @return {@code this}
             */
            public Tab setTheme(String theme)
            {
                button.setUserTheme(theme);
                return this;
            }

            public Runnable getCloseCallback()
            {
                return closeCallback;
            }

            public void setCloseCallback(Runnable closeCallback)
            {
                if (this.closeCallback != null)
                {
                    button.removeCloseButton();
                }
                this.closeCallback = closeCallback;
                if (closeCallback != null)
                {
                    button.setCloseButton(closeCallback);
                }
            }

            protected internal void doCallback()
            {
                if (pane != null)
                {
                    pane.setVisible(this.Value);
                }

                this.Changed.Invoke(this, new BooleanChangedEventArgs(this.Value, this.Value));
            }
        }

        public class TabButton : ToggleButton
        {
            Button closeButton;
            Alignment closeButtonAlignment;
            int closeButtonOffsetX;
            int closeButtonOffsetY;
            String userTheme;

            public TabButton(BooleanModel model) : base(model)
            {
                setCanAcceptKeyboardFocus(false);
                closeButtonAlignment = Alignment.RIGHT;
            }

            public void setUserTheme(String userTheme)
            {
                this.userTheme = userTheme;
                doSetTheme();
            }

            private void doSetTheme()
            {
                if (userTheme != null)
                {
                    setTheme(userTheme);
                }
                else if (closeButton != null)
                {
                    setTheme("tabbuttonWithCloseButton");
                }
                else
                {
                    setTheme("tabbutton");
                }
                reapplyTheme();
            }

            protected override void applyTheme(ThemeInfo themeInfo)
            {
                base.applyTheme(themeInfo);
                if (closeButton != null)
                {
                    closeButtonAlignment = (Alignment) themeInfo.getParameter("closeButtonAlignment", Alignment.RIGHT);
                    closeButtonOffsetX = themeInfo.getParameter("closeButtonOffsetX", 0);
                    closeButtonOffsetY = themeInfo.getParameter("closeButtonOffsetY", 0);
                }
                else
                {
                    closeButtonAlignment = Alignment.RIGHT;
                    closeButtonOffsetX = 0;
                    closeButtonOffsetY = 0;
                }
            }

            protected internal void setCloseButton(Runnable callback)
            {
                closeButton = new Button();
                closeButton.setTheme("closeButton");
                doSetTheme();
                add(closeButton);
                closeButton.Action += (sender, e) =>
                {
                    callback.run();
                };
            }

            protected internal void removeCloseButton()
            {
                removeChild(closeButton);
                closeButton = null;
                doSetTheme();
            }

            public override int getPreferredInnerHeight()
            {
                return computeTextHeight();
            }

            public override int getPreferredInnerWidth()
            {
                return computeTextWidth();
            }

            protected override void layout()
            {
                if (closeButton != null)
                {
                    closeButton.adjustSize();
                    closeButton.setPosition(
                            getX() + closeButtonOffsetX + closeButtonAlignment.computePositionX(getWidth(), closeButton.getWidth()),
                            getY() + closeButtonOffsetY + closeButtonAlignment.computePositionY(getHeight(), closeButton.getHeight()));
                }
            }
        }

    }
}
