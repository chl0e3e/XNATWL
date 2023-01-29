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
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class Button : TextWidget
    {
        public static StateKey STATE_ARMED = StateKey.Get("armed");
        public static StateKey STATE_PRESSED = StateKey.Get("pressed");
        public static StateKey STATE_SELECTED = StateKey.Get("selected");

        private ButtonModel model;
        private String themeText;
        private String text;
        private int mouseButton;

        public event EventHandler<ButtonActionEventArgs> Action;
        public event EventHandler<ButtonStateChangedEventArgs> State;

        public Button() : this(null, false, null)
        {
            
        }

        public Button(ButtonModel model) : this(null, false, model)
        {
            
        }

        /**
         * Creates a Button with a shared animation state
         *
         * @param animState the animation state to share, can be null
         */
        public Button(AnimationState animState) : this(animState, false, null)
        {
        }

        /**
         * Creates a Button with a shared or inherited animation state
         *
         * @param animState the animation state to share or inherit, can be null
         * @param inherit true if the animation state should be inherited false for sharing
         */
        public Button(AnimationState animState, bool inherit) : this(animState, inherit, null)
        {
        }

        public Button(String text) : this(null, false, null)
        {
            setText(text);
        }

        /**
         * Creates a Button with a shared animation state
         *
         * @param animState the animation state to share, can be null
         * @param model the button behavior model, if null a SimpleButtonModel is created
         */
        public Button(AnimationState animState, ButtonModel model) : this(animState, false, model)
        {
        }

        /**
         * Creates a Button with a shared or inherited animation state
         *
         * @param animState the animation state to share or inherit, can be null
         * @param inherit true if the animation state should be inherited false for sharing
         * @param model the button behavior model, if null a SimpleButtonModel is created
         */
        public Button(AnimationState animState, bool inherit, ButtonModel model) : base(animState, inherit)
        {
            this.mouseButton = Event.MOUSE_LBUTTON;
            //this.stateChangedCB = new Runnable() {
            //    public void run() {
            //        modelStateChanged();
            //    }
            //};
            if (model == null)
            {
                model = new SimpleButtonModel();
            }
            setModel(model);
            setCanAcceptKeyboardFocus(true);
        }

        public ButtonModel getModel()
        {
            return model;
        }

        public void setModel(ButtonModel model)
        {
            if (model == null)
            {
                throw new NullReferenceException("model");
            }
            bool isConnected = getGUI() != null;
            if (this.model != null)
            {
                this.model.State -= Model_State;
                this.model.Action -= Model_Action;
            }
            this.model = model;
            this.model.State += Model_State;
            this.model.Action += Model_Action;
            modelStateChanged();
            AnimationState animationState = getAnimationState();
            animationState.dontAnimate(STATE_ARMED);
            animationState.dontAnimate(STATE_PRESSED);
            animationState.dontAnimate(STATE_HOVER);
            animationState.dontAnimate(STATE_SELECTED);
        }

        public bool hasCallbacks()
        {
            return this.Action.GetInvocationList().Length > 0;
        }

        private void Model_Action(object sender, ButtonActionEventArgs e)
        {
            if (this.Action != null)
            {
                this.Action.Invoke(this, e);
            }
        }

        private void Model_State(object sender, ButtonStateChangedEventArgs e)
        {
            if (this.State != null)
            {
                this.State.Invoke(this, e);
            }
            modelStateChanged();
        }

        //@Override
        internal override void widgetDisabled()
        {
            disarm();
        }

        //@Override
        public override void setEnabled(bool enabled)
        {
            model.Enabled = (enabled);
        }

        public String getText()
        {
            return text;
        }

        public void setText(String text)
        {
            this.text = text;
            updateText();
        }

        public int getMouseButton()
        {
            return mouseButton;
        }

        /**
         * Sets the mouse button to which this button should react.
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
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeButton(themeInfo);
        }

        protected void applyThemeButton(ThemeInfo themeInfo)
        {
            themeText = themeInfo.getParameterValue<string>("text", false, typeof(String), "");
            updateText();
        }

        //@Override
        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            if (model != null)
            {
                this.model.State += Model_State;
                this.model.Action += Model_Action;
            }
        }

        //@Override
        protected override void beforeRemoveFromGUI(GUI gui)
        {
            if (model != null)
            {
                this.model.State -= Model_State;
                this.model.Action -= Model_Action;
            }
            base.beforeRemoveFromGUI(gui);
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
        public override void setVisible(bool visible)
        {
            base.setVisible(visible);
            if (!visible)
            {
                disarm();
            }
        }

        protected void disarm()
        {
            // disarm first to not fire a callback
            model.Hover = (false);
            model.Armed = (false);
            model.Pressed = (false);
        }

        void modelStateChanged()
        {
            base.setEnabled(model.Enabled);
            AnimationState animationState = getAnimationState();
            animationState.setAnimationState(STATE_SELECTED, model.Selected);
            animationState.setAnimationState(STATE_HOVER, model.Hover);
            animationState.setAnimationState(STATE_ARMED, model.Armed);
            animationState.setAnimationState(STATE_PRESSED, model.Pressed);
        }

        void updateText()
        {
            if (text == null)
            {
                base.setCharSequence(TextUtil.notNull(themeText));
            }
            else
            {
                base.setCharSequence(text);
            }
            invalidateLayout();
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (evt.isMouseEvent())
            {
                bool hover = (evt.getEventType() != EventType.MOUSE_EXITED) && isMouseInside(evt);
                model.Hover = (hover);
                model.Armed = (hover && model.Pressed);
            }


            EventType type = evt.getEventType();

            if (type == EventType.MOUSE_BTNDOWN)
            {
                if (evt.getMouseButton() == mouseButton)
                {
                    model.Pressed = (true);
                    model.Armed = (true);
                }
            }
            else if (type == EventType.MOUSE_BTNUP)
            {
                if (evt.getMouseButton() == mouseButton)
                {
                    model.Pressed = (false);
                    model.Armed = (false);
                }
            }
            else if (type == EventType.KEY_PRESSED)
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_RETURN:
                    case Event.KEY_SPACE:
                        if (!evt.isKeyRepeated())
                        {
                            model.Pressed = (true);
                            model.Armed = (true);
                        }
                        return true;
                }
            }
            else if (type == EventType.KEY_RELEASED)
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_RETURN:
                    case Event.KEY_SPACE:
                        model.Pressed = (false);
                        model.Armed = (false);
                        return true;
                }
            }
            else if (type == EventType.POPUP_OPENED)
            {
                model.Hover = (false);
            }
            else if (type == EventType.MOUSE_WHEEL)
            {
                return false;
            }

            if (base.handleEvent(evt))
            {
                return true;
            }
            // eat all mouse events - except moused wheel which was checked above
            return evt.isMouseEvent();
        }

    }

}
