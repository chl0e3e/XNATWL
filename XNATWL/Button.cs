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

        private ButtonModel _model;
        private String _themeText;
        private String _text;
        private int _mouseButton;

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
            SetText(text);
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
            this._mouseButton = Event.MOUSE_LBUTTON;
            //this.stateChangedCB = new Runnable() {
            //    public void run() {
            //        modelStateChanged();
            //    }
            //};
            if (model == null)
            {
                model = new SimpleButtonModel();
            }
            SetModel(model);
            SetCanAcceptKeyboardFocus(true);
        }

        public ButtonModel GetModel()
        {
            return _model;
        }

        public void SetModel(ButtonModel model)
        {
            if (model == null)
            {
                throw new NullReferenceException("model");
            }
            bool isConnected = GetGUI() != null;
            if (this._model != null)
            {
                this._model.State -= Model_State;
                this._model.Action -= Model_Action;
            }
            this._model = model;
            this._model.State += Model_State;
            this._model.Action += Model_Action;
            ModelStateChanged();
            AnimationState animationState = GetAnimationState();
            animationState.DontAnimate(STATE_ARMED);
            animationState.DontAnimate(STATE_PRESSED);
            animationState.DontAnimate(STATE_HOVER);
            animationState.DontAnimate(STATE_SELECTED);
        }

        public bool HasCallbacks()
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
            ModelStateChanged();
        }

        internal override void WidgetDisabled()
        {
            Disarm();
        }

        public override void SetEnabled(bool enabled)
        {
            _model.Enabled = (enabled);
        }

        public String GetText()
        {
            return _text;
        }

        public void SetText(String text)
        {
            this._text = text;
            UpdateText();
        }

        public int GetMouseButton()
        {
            return _mouseButton;
        }

        /**
         * Sets the mouse button to which this button should react.
         * The default is {@link Event#MOUSE_LBUTTON}
         * @param mouseButton the mouse button
         */
        public void SetMouseButton(int mouseButton)
        {
            if (mouseButton < Event.MOUSE_LBUTTON || mouseButton > Event.MOUSE_RBUTTON)
            {
                throw new ArgumentOutOfRangeException("mouseButton");
            }
            this._mouseButton = mouseButton;
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeButton(themeInfo);
        }

        protected void ApplyThemeButton(ThemeInfo themeInfo)
        {
            _themeText = themeInfo.GetParameterValue<string>("text", false, typeof(String), "");
            UpdateText();
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            if (this._model != null)
            {
                this._model.Connect();
            }
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            if (this._model != null)
            {
                this._model.Disconnect();
            }
            base.BeforeRemoveFromGUI(gui);
        }

        public override int GetMinWidth()
        {
            return Math.Max(base.GetMinWidth(), GetPreferredWidth());
        }

        public override int GetMinHeight()
        {
            return Math.Max(base.GetMinHeight(), GetPreferredHeight());
        }

        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            if (!visible)
            {
                Disarm();
            }
        }

        protected void Disarm()
        {
            // disarm first to not fire a callback
            _model.Hover = (false);
            _model.Armed = (false);
            _model.Pressed = (false);
        }

        void ModelStateChanged()
        {
            base.SetEnabled(_model.Enabled);
            AnimationState animationState = GetAnimationState();
            animationState.SetAnimationState(STATE_SELECTED, _model.Selected);
            animationState.SetAnimationState(STATE_HOVER, _model.Hover);
            animationState.SetAnimationState(STATE_ARMED, _model.Armed);
            animationState.SetAnimationState(STATE_PRESSED, _model.Pressed);
        }

        void UpdateText()
        {
            if (_text == null)
            {
                base.SetCharSequence(TextUtil.NotNull(_themeText));
            }
            else
            {
                base.SetCharSequence(_text);
            }
            InvalidateLayout();
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.IsMouseEvent())
            {
                bool hover = (evt.GetEventType() != EventType.MOUSE_EXITED) && IsMouseInside(evt);
                _model.Hover = (hover);
                _model.Armed = (hover && _model.Pressed);
            }


            EventType type = evt.GetEventType();

            if (type == EventType.MOUSE_BTNDOWN)
            {
                if (evt.GetMouseButton() == _mouseButton)
                {
                    _model.Pressed = (true);
                    _model.Armed = (true);
                }
            }
            else if (type == EventType.MOUSE_BTNUP)
            {
                if (evt.GetMouseButton() == _mouseButton)
                {
                    _model.Pressed = (false);
                    _model.Armed = (false);
                }
            }
            else if (type == EventType.KEY_PRESSED)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_RETURN:
                    case Event.KEY_SPACE:
                        if (!evt.IsKeyRepeated())
                        {
                            _model.Pressed = (true);
                            _model.Armed = (true);
                        }
                        return true;
                }
            }
            else if (type == EventType.KEY_RELEASED)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_RETURN:
                    case Event.KEY_SPACE:
                        _model.Pressed = (false);
                        _model.Armed = (false);
                        return true;
                }
            }
            else if (type == EventType.POPUP_OPENED)
            {
                _model.Hover = (false);
            }
            else if (type == EventType.MOUSE_WHEEL)
            {
                return false;
            }

            if (base.HandleEvent(evt))
            {
                return true;
            }
            // eat all mouse events - except moused wheel which was checked above
            return evt.IsMouseEvent();
        }
    }
}
