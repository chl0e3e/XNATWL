using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL
{

    public class MenuManager : PopupWindow
    {

        private bool isMenuBar;
        private Dictionary<MenuElement, Widget> popups;
        private Runnable closeCB;
        private Runnable timerCB;

        private bool mouseOverOwner;
        private Widget lastMouseOverWidget;
        private Timer timer;

        public MenuManager(Widget owner, bool isMenuBar) : base(owner)
        {
            this.isMenuBar = isMenuBar;
            this.popups = new Dictionary<MenuElement, Widget>();
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

        public Runnable getCloseCallback()
        {
            return closeCB;
        }

        bool isSubMenuOpen(Menu menu)
        {
            Widget popup = popups[menu];
            if (popup != null)
            {
                return popup.getParent() == this;
            }
            return false;
        }

        void closeSubMenu(int level)
        {
            while (getNumChildren() > level)
            {
                closeSubMenu();
            }
        }

        internal Widget openSubMenu(int level, Menu menu, Widget btn, bool setPosition)
        {
            Widget popup = popups[menu];
            if (popup == null)
            {
                popup = menu.createPopup(this, level + 1, btn);
                popups.Add(menu, popup);
            }

            if (popup.getParent() == this)
            {
                closeSubMenu(level + 1);
                return popup;
            }

            if (!isOpen())
            {
                if (!openPopup())
                {
                    closePopup();
                    return null;
                }
                getParent().layoutChildFullInnerArea(this);
            }

            while (getNumChildren() > level)
            {
                closeSubMenu();
            }
            add(popup);

            popup.adjustSize();

            if (setPosition)
            {
                int popupWidth = popup.getWidth();
                int popupX = btn.getRight();
                int popupY = btn.getY();

                if (level == 0)
                {
                    popupX = btn.getX();
                    popupY = btn.getBottom();
                }

                if (popupWidth + btn.getRight() > getInnerRight())
                {
                    popupX = btn.getX() - popupWidth;
                    if (popupX < getInnerX())
                    {
                        popupX = getInnerRight() - popupWidth;
                    }
                }

                int popupHeight = popup.getHeight();
                if (popupY + popupHeight > getInnerBottom())
                {
                    popupY = Math.Max(getInnerY(), getInnerBottom() - popupHeight);
                }

                popup.setPosition(popupX, popupY);
            }

            return popup;
        }

        void closeSubMenu()
        {
            removeChild(getNumChildren() - 1);
        }

        //@Override
        public override void closePopup()
        {
            stopTimer();
            GUI gui = getGUI();
            base.closePopup();
            removeAllChildren();
            popups.Clear();
            if (gui != null)
            {
                gui.resendLastMouseMove();
            }
        }

        /**
         * Returns the popup widget for the specified menu
         * @param menu the menu for which to return the popup
         * @return the popup widget or null if not open
         */
        public Widget getPopupForMenu(Menu menu)
        {
            return popups[menu];
        }

        //@Override
        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            timer = gui.createTimer();
            timer.setDelay(300);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            this.popupTimer();
        }

        //@Override
        protected override void layout()
        {
        }

        //@Override
        internal override Widget routeMouseEvent(Event evt)
        {
            mouseOverOwner = false;
            Widget widget = base.routeMouseEvent(evt);
            if (widget == this && isMenuBar && getOwner().isMouseInside(evt))
            {
                Widget menuBarWidget = getOwner().routeMouseEvent(evt);
                if (menuBarWidget != null)
                {
                    mouseOverOwner = true;
                    widget = menuBarWidget;
                }
            }

            Widget mouseOverWidget = getWidgetUnderMouse();
            if (lastMouseOverWidget != mouseOverWidget)
            {
                lastMouseOverWidget = mouseOverWidget;
                if (isMenuBar && widget.getParent() == getOwner() && (widget is Menu.SubMenuBtn))
                {
                    popupTimer();   // no delay on menu bar itself
                }
                else
                {
                    startTimer();
                }
            }

            return widget;
        }

        //@Override
        protected override bool handleEventPopup(Event evt)
        {
            if (isMenuBar && getOwner().handleEvent(evt))
            {
                return true;
            }
            if (base.handleEventPopup(evt))
            {
                return true;
            }
            if (evt.getEventType() == Event.EventType.MOUSE_CLICKED)
            {
                mouseClickedOutside(evt);
                return true;
            }
            return false;
        }

        //@Override
        internal override Widget getWidgetUnderMouse()
        {
            if (mouseOverOwner)
            {
                return getOwner().getWidgetUnderMouse();
            }
            return base.getWidgetUnderMouse();
        }

        void popupTimer()
        {
            if ((lastMouseOverWidget is Menu.SubMenuBtn) && lastMouseOverWidget.isEnabled())
            {
                ((Menu.SubMenuBtn)lastMouseOverWidget).OpenSubMenu();
            }
            else if (lastMouseOverWidget != this)
            {
                int level = 0;
                // search for the MenuPopup containing this widget
                // it knows which menu level we need to close
                for (Widget w = lastMouseOverWidget; w != null; w = w.getParent())
                {
                    if (w is Menu.MenuPopup)
                    {
                        level = ((Menu.MenuPopup)w).level;
                        break;
                    }
                }
                closeSubMenu(level);
            }
        }

        void startTimer()
        {
            if (timer != null)
            {
                timer.stop();
                timer.start();
            }
        }

        void stopTimer()
        {
            if (timer != null)
            {
                timer.stop();
            }
        }
    }

}
