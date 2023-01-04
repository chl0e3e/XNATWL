using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class Label : TextWidget
    {
        public enum ClickType
        {
            CLICK,
            DOUBLE_CLICK
        }

        private bool autoSize = true;
        private Widget labelFor;
        public event EventHandler<LabelClickEventArgs> Clicked;

        public Label() : this((AnimationState)null, false)
        {
            
        }

        /**
         * Creates a Label with a shared animation state
         *
         * @param animState the animation state to share, can be null
         */
        public Label(AnimationState animState) : this(animState, false)
        {
            
        }

        /**
         * Creates a Label with a shared or inherited animation state
         *
         * @param animState the animation state to share or inherit, can be null
         * @param inherit true if the animation state should be inherited false for sharing
         */
        public Label(AnimationState animState, bool inherit) : base(animState, inherit)
        {
            
        }

        public Label(String text) : this()
        {
            setText(text);
        }

        /*public void addCallback(CallbackWithReason<CallbackReason> cb) {
            callbacks = CallbackSupport.addCallbackToList(callbacks, cb, CallbackWithReason.class);
        }

        public void removeCallback(CallbackWithReason<CallbackReason> cb) {
            callbacks = CallbackSupport.removeCallbackFromList(callbacks, cb);
        }

        protected void doCallback(CallbackReason reason) {
            CallbackSupport.fireCallbacks(callbacks, reason);
        }*/

        public bool isAutoSize()
        {
            return autoSize;
        }

        public void setAutoSize(bool autoSize)
        {
            this.autoSize = autoSize;
        }

        //@Override
        public override void setFont(Font font)
        {
            base.setFont(font);
            if (autoSize)
            {
                invalidateLayout();
            }
        }

        public String getText()
        {
            return base.getCharSequence();
        }

        public void setText(String text)
        {
            text = TextUtil.notNull(text);
            if (!text.Equals(getText()))
            {
                base.setCharSequence(text);
                if (autoSize)
                {
                    invalidateLayout();
                }
            }
        }

        //@Override
        public override Object getTooltipContent()
        {
            Object toolTipContent = base.getTooltipContent();
            if (toolTipContent == null && labelFor != null)
            {
                return labelFor.getTooltipContent();
            }
            return toolTipContent;
        }

        public Widget getLabelFor()
        {
            return labelFor;
        }

        /**
         * Sets the associated widget for this label. This will cause the label to
         * get it's tooltip content from the associated widget and also forward the
         * keyboard focus to it.
         *
         * @param labelFor the associated widget. Can be {@code null}.
         */
        public void setLabelFor(Widget labelFor)
        {
            if (labelFor == this)
            {
                throw new ArgumentOutOfRangeException("labelFor == this");
            }
            this.labelFor = labelFor;
        }

        protected void applyThemeLabel(ThemeInfo themeInfo)
        {
            String themeText = (string) themeInfo.getParameterValue("text", false, typeof(string));
            if (themeText != null)
            {
                setText(themeText);
            }
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeLabel(themeInfo);
        }

        //@Override
        public override bool requestKeyboardFocus()
        {
            if (labelFor != null)
            {
                return labelFor.requestKeyboardFocus();
            }
            else
            {
                return base.requestKeyboardFocus();
            }
        }

        //@Override
        public override int getMinWidth()
        {
            return Math.Max(base.getMinWidth(), getPreferredWidth());
        }

        //@Override
        public override int getMinHeight()
        {
            return Math.Max(base.getMinHeight(), getPreferredHeight());
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            handleMouseHover(evt);
            if (evt.isMouseEvent())
            {
                if (evt.getEventType() == Event.EventType.MOUSE_CLICKED)
                {
                    switch (evt.getMouseClickCount())
                    {
                        case 1:
                            handleClick(false);
                            break;
                        case 2:
                            handleClick(true);
                            break;
                    }
                }
                return evt.getEventType() != Event.EventType.MOUSE_WHEEL;
            }
            return false;
        }

        protected void handleClick(bool doubleClick)
        {
            if (this.Clicked != null)
            {
                this.Clicked.Invoke(this, new LabelClickEventArgs(doubleClick ? ClickType.DOUBLE_CLICK : ClickType.CLICK));
            }
        }
    }

    public class LabelClickEventArgs : EventArgs
    {
        public Label.ClickType ClickType;

        public LabelClickEventArgs(Label.ClickType clickType)
        {
            this.ClickType = ClickType;
        }
    }
}
