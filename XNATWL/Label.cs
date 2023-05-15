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
            String themeText = (string) themeInfo.GetParameterValue("text", false, typeof(string));
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
                if (evt.getEventType() == EventType.MOUSE_CLICKED)
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
                return evt.getEventType() != EventType.MOUSE_WHEEL;
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
