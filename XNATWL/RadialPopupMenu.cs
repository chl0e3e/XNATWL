using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class RadialPopupMenu : PopupWindow
    {
        private List<RoundButton> buttons;

        private int radius;
        private int buttonRadius;
        private int mouseButton;
        int buttonRadiusSqr;

        public RadialPopupMenu(Widget owner) : base(owner)
        {
            this.buttons = new List<RoundButton>();
        }

        public int getButtonRadius()
        {
            return buttonRadius;
        }

        public void setButtonRadius(int buttonRadius)
        {
            if (buttonRadius < 0)
            {
                throw new ArgumentOutOfRangeException("buttonRadius");
            }
            this.buttonRadius = buttonRadius;
            this.buttonRadiusSqr = buttonRadius * buttonRadius;
            invalidateLayout();
        }

        public int getRadius()
        {
            return radius;
        }

        public void setRadius(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException("radius");
            }
            this.radius = radius;
            invalidateLayout();
        }

        public int getMouseButton()
        {
            return mouseButton;
        }

        /**
         * Sets the mouse button to which the buttons of this radial menu should react.
         * The default is {@link Event#MOUSE_LBUTTON}
         * @param mouseButton the mouse button
         */
        public void setMouseButton(int mouseButton)
        {
            if (mouseButton < Event.MOUSE_LBUTTON || mouseButton > Event.MOUSE_RBUTTON)
            {
                throw new ArgumentOutOfRangeException("mouseButton");
            }
            this.mouseButton = mouseButton;
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                buttons[i].setMouseButton(mouseButton);
            }
        }

        public Button addButton(String theme, Action cb)
        {
            RoundButton button = new RoundButton(this);
            button.setTheme(theme);
            button.Action += (sender, e) =>
            {
                cb();
            };
            button.setMouseButton(mouseButton);
            addButton(button);
            return button;
        }

        public void removeButton(RoundButton btn)
        {
            int idx = buttons.IndexOf(btn);
            if (idx >= 0)
            {
                buttons.RemoveAt(idx);
                removeChild(btn);
            }
        }

        protected void addButton(RoundButton button)
        {
            if (button == null)
            {
                throw new NullReferenceException("button");
            }
            buttons.Add(button);
            add(button);
        }

        class MouseDrag : Runnable
        {
            private RadialPopupMenu menu;
            public MouseDrag(RadialPopupMenu menu)
            {
                this.menu = menu;
            }
            public override void run()
            {
                this.menu.boundDragEventFinished();
            }
        }

        public override bool openPopup()
        {
            if (base.openPopup())
            {
                if (bindMouseDrag(new MouseDrag(this)))
                {
                    setAllButtonsPressed();
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
        public bool openPopupAt(int centerX, int centerY)
        {
            if (openPopup())
            {
                adjustSize();
                Widget parent = getParent();
                int width = getWidth();
                int height = getHeight();
                setPosition(
                        limit(centerX - width / 2, parent.getInnerX(), parent.getInnerRight() - width),
                        limit(centerY - height / 2, parent.getInnerY(), parent.getInnerBottom() - height));
                return true;
            }
            return false;
        }

        protected static int limit(int value, int min, int max)
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
        public bool openPopup(Event evt)
        {
            if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN)
            {
                setMouseButton(evt.getMouseButton());
                return openPopupAt(evt.getMouseX(), evt.getMouseY());
            }
            return false;
        }

        public override int getPreferredInnerWidth()
        {
            return 2 * (radius + buttonRadius);
        }

        public override int getPreferredInnerHeight()
        {
            return 2 * (radius + buttonRadius);
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeRadialPopupMenu(themeInfo);
        }

        protected void applyThemeRadialPopupMenu(ThemeInfo themeInfo)
        {
            setRadius(themeInfo.getParameter("radius", 40));
            setButtonRadius(themeInfo.getParameter("buttonRadius", 40));
        }

        protected override void layout()
        {
            layoutRadial();
        }

        protected void layoutRadial()
        {
            int numButtons = buttons.Count;
            if (numButtons > 0)
            {
                int centerX = getInnerX() + getInnerWidth() / 2;
                int centerY = getInnerY() + getInnerHeight() / 2;
                float toRad = (float)(2.0 * Math.PI) / numButtons;
                for (int i = 0; i < numButtons; i++)
                {
                    float rad = i * toRad;
                    int btnCenterX = centerX + (int)(radius * Math.Sin(rad));
                    int btnCenterY = centerY - (int)(radius * Math.Cos(rad));
                    RoundButton button = buttons[i];
                    button.setPosition(btnCenterX - buttonRadius, btnCenterY - buttonRadius);
                    button.setSize(2 * buttonRadius, 2 * buttonRadius);
                }
            }
        }

        protected void setAllButtonsPressed()
        {
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                ButtonModel model = buttons[i].getModel();
                model.Pressed = true;
                model.Armed = model.Hover;
            }
        }

        protected void boundDragEventFinished()
        {
            closePopup();
        }

        public class RoundButton : Button
        {
            private RadialPopupMenu radialPopupMenu;
            public RoundButton(RadialPopupMenu radialPopupMenu)
            {
                this.radialPopupMenu = radialPopupMenu;
            }
            public override bool isInside(int x, int y)
            {
                int dx = x - (getX() + getWidth() / 2);
                int dy = y - (getY() + getHeight() / 2);
                return dx * dx + dy * dy <= this.radialPopupMenu.buttonRadiusSqr;
            }
        }
    }

}
