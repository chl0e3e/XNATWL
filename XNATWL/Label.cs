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
            Click,
            DoubleClick
        }

        private bool _autoSize = true;
        private Widget _labelFor;

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
            SetText(text);
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

        public bool IsAutoSize()
        {
            return _autoSize;
        }

        public void SetAutoSize(bool autoSize)
        {
            this._autoSize = autoSize;
        }

        //@Override
        public override void SetFont(Font font)
        {
            base.SetFont(font);

            if (_autoSize)
            {
                InvalidateLayout();
            }
        }

        public String GetText()
        {
            return base.GetCharSequence();
        }

        public void SetText(String text)
        {
            text = TextUtil.NotNull(text);
            if (!text.Equals(GetText()))
            {
                base.SetCharSequence(text);
                if (_autoSize)
                {
                    InvalidateLayout();
                }
            }
        }

        //@Override
        public override Object GetTooltipContent()
        {
            Object toolTipContent = base.GetTooltipContent();
            if (toolTipContent == null && _labelFor != null)
            {
                return _labelFor.GetTooltipContent();
            }
            return toolTipContent;
        }

        public Widget GetLabelFor()
        {
            return _labelFor;
        }

        /**
         * Sets the associated widget for this label. This will cause the label to
         * get it's tooltip content from the associated widget and also forward the
         * keyboard focus to it.
         *
         * @param labelFor the associated widget. Can be {@code null}.
         */
        public void SetLabelFor(Widget labelFor)
        {
            if (labelFor == this)
            {
                throw new ArgumentOutOfRangeException("labelFor == this");
            }
            this._labelFor = labelFor;
        }

        protected void ApplyThemeLabel(ThemeInfo themeInfo)
        {
            String themeText = (string) themeInfo.GetParameterValue("text", false, typeof(string));
            if (themeText != null)
            {
                SetText(themeText);
            }
        }

        //@Override
        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeLabel(themeInfo);
        }

        //@Override
        public override bool RequestKeyboardFocus()
        {
            if (_labelFor != null)
            {
                return _labelFor.RequestKeyboardFocus();
            }
            else
            {
                return base.RequestKeyboardFocus();
            }
        }

        //@Override
        public override int GetMinWidth()
        {
            return Math.Max(base.GetMinWidth(), GetPreferredWidth());
        }

        //@Override
        public override int GetMinHeight()
        {
            return Math.Max(base.GetMinHeight(), GetPreferredHeight());
        }

        //@Override
        public override bool HandleEvent(Event evt)
        {
            HandleMouseHover(evt);
            if (evt.IsMouseEvent())
            {
                if (evt.GetEventType() == EventType.MOUSE_CLICKED)
                {
                    switch (evt.GetMouseClickCount())
                    {
                        case 1:
                            HandleClick(false);
                            break;
                        case 2:
                            HandleClick(true);
                            break;
                    }
                }
                return evt.GetEventType() != EventType.MOUSE_WHEEL;
            }
            return false;
        }

        protected void HandleClick(bool doubleClick)
        {
            if (this.Clicked != null)
            {
                this.Clicked.Invoke(this, new LabelClickEventArgs(doubleClick ? ClickType.DoubleClick : ClickType.Click));
            }
        }
    }

    public class LabelClickEventArgs : EventArgs
    {
        public Label.ClickType ClickType;

        public LabelClickEventArgs(Label.ClickType clickType)
        {
            this.ClickType = clickType;
        }
    }
}
