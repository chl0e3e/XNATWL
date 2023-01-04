using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
