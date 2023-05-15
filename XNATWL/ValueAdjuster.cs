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

namespace XNATWL.Model
{
    public abstract class ValueAdjuster : Widget
    {
        public static StateKey STATE_EDIT_ACTIVE = StateKey.Get("editActive");

        private static int INITIAL_DELAY = 300;
        private static int REPEAT_DELAY = 75;

        private DraggableButton _label;
        private EditField _editField;
        private Button _decButton;
        private Button _incButton;
        private L _listeners;
        private Timer _timer;

        private String _displayPrefix;
        private String _displayPrefixTheme = "";
        private bool _useMouseWheel = true;
        private bool _acceptValueOnFocusLoss = true;
        private bool _wasInEditOnFocusLost;
        private int _width;

        public ValueAdjuster()
        {
            this._label = new DraggableButton(GetAnimationState(), true);
            // EditField always inherits from the passed animation state
            this._editField = new EditField(GetAnimationState());
            this._decButton = new Button(GetAnimationState(), true);
            this._incButton = new Button(GetAnimationState(), true);

            _label.SetClip(true);
            _label.SetTheme("valueDisplay");
            _editField.SetTheme("valueEdit");
            _decButton.SetTheme("decButton");
            _incButton.SetTheme("incButton");

            _decButton.GetModel().State += ValueAdjuster_State;
            _incButton.GetModel().State += ValueAdjuster_State;

            _listeners = new L(this);
            _label.Action += Label_Action;
            _label.SetListener(_listeners);

            _editField.SetVisible(false);
            _editField.Callback += EditField_Callback;

            Add(_label);
            Add(_editField);
            Add(_decButton);
            Add(_incButton);
            SetCanAcceptKeyboardFocus(true);
            SetDepthFocusTraversal(false);
        }

        private void EditField_Callback(object sender, EditFieldCallbackEventArgs e)
        {
            this.HandleEditCallback(e.Key);
        }

        private void Label_Action(object sender, ButtonActionEventArgs e)
        {
            StartEdit();
        }

        private void ValueAdjuster_State(object sender, ButtonStateChangedEventArgs e)
        {
            UpdateTimer();
        }

        public String GetDisplayPrefix()
        {
            return _displayPrefix;
        }

        /**
         * Sets the display prefix which is displayed before the value.
         *
         * If this is property is null then the value from the theme is used,
         * otherwise this one.
         *
         * @param displayPrefix the prefix or null
         */
        public void SetDisplayPrefix(String displayPrefix)
        {
            this._displayPrefix = displayPrefix;
            SetDisplayText();
        }

        public bool IsUseMouseWheel()
        {
            return _useMouseWheel;
        }

        /**
         * Controls the behavior on focus loss when editing the value.
         * If true then the value is accepted (like pressing RETURN).
         * If false then it is discard (like pressing ESCAPE).
         * 
         * Default is true.
         *
         * @param acceptValueOnFocusLoss true if focus loss should accept the edited value.
         */
        public void SetAcceptValueOnFocusLoss(bool acceptValueOnFocusLoss)
        {
            this._acceptValueOnFocusLoss = acceptValueOnFocusLoss;
        }

        public bool IsAcceptValueOnFocusLoss()
        {
            return _acceptValueOnFocusLoss;
        }

        /**
         * Controls if the ValueAdjuster should respond to the mouse wheel or not
         *
         * @param useMouseWheel true if the mouse wheel is used
         */
        public void SetUseMouseWheel(bool useMouseWheel)
        {
            this._useMouseWheel = useMouseWheel;
        }

        public override void SetTooltipContent(Object tooltipContent)
        {
            base.SetTooltipContent(tooltipContent);
            _label.SetTooltipContent(tooltipContent);
        }

        public void StartEdit()
        {
            if (_label.IsVisible())
            {
                _editField.SetErrorMessage(null);
                _editField.SetText(OnEditStart());
                _editField.SetVisible(true);
                _editField.RequestKeyboardFocus();
                _editField.SelectAll();
                _editField.GetAnimationState().SetAnimationState(EditField.STATE_HOVER, _label.GetModel().Hover);
                _label.SetVisible(false);
                GetAnimationState().SetAnimationState(STATE_EDIT_ACTIVE, true);
            }
        }

        public void CancelEdit()
        {
            if (_editField.IsVisible())
            {
                OnEditCanceled();
                _label.SetVisible(true);
                _editField.SetVisible(false);
                _label.GetModel().Hover = _editField.GetAnimationState().GetAnimationState(Label.STATE_HOVER);
                GetAnimationState().SetAnimationState(STATE_EDIT_ACTIVE, false);
            }
        }

        public void CancelOrAcceptEdit()
        {
            if (_editField.IsVisible())
            {
                if (_acceptValueOnFocusLoss)
                {
                    OnEditEnd(_editField.GetText());
                }

                CancelEdit();
            }
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeValueAdjuster(themeInfo);
        }

        protected void ApplyThemeValueAdjuster(ThemeInfo themeInfo)
        {
            _width = themeInfo.GetParameter("width", 100);
            _displayPrefixTheme = themeInfo.GetParameter("displayPrefix", "");
            _useMouseWheel = themeInfo.GetParameter("useMouseWheel", _useMouseWheel);
        }

        public override int GetMinWidth()
        {
            int minWidth = base.GetMinWidth();
            minWidth = Math.Max(minWidth,
                    GetBorderHorizontal() +
                    _decButton.GetMinWidth() +
                    Math.Max(_width, _label.GetMinWidth()) +
                    _incButton.GetMinWidth());
            return minWidth;
        }

        public override int GetMinHeight()
        {
            int minHeight = _label.GetMinHeight();
            minHeight = Math.Max(minHeight, _decButton.GetMinHeight());
            minHeight = Math.Max(minHeight, _incButton.GetMinHeight());
            minHeight += GetBorderVertical();
            return Math.Max(minHeight, base.GetMinHeight());
        }

        public override int GetPreferredInnerWidth()
        {
            return _decButton.GetPreferredWidth() +
                    Math.Max(_width, _label.GetPreferredWidth()) +
                    _incButton.GetPreferredWidth();
        }

        public override int GetPreferredInnerHeight()
        {
            return Math.Max(Math.Max(
                    _decButton.GetPreferredHeight(),
                    _incButton.GetPreferredHeight()),
                    _label.GetPreferredHeight());
        }

        //@Override
        protected override void KeyboardFocusLost()
        {
            _wasInEditOnFocusLost = _editField.IsVisible();
            CancelOrAcceptEdit();
            _label.GetAnimationState().SetAnimationState(STATE_KEYBOARD_FOCUS, false);
        }

        protected override void KeyboardFocusGained()
        {
            // keep in this method to not change subclassing behavior
            _label.GetAnimationState().SetAnimationState(STATE_KEYBOARD_FOCUS, true);
        }

        protected override void KeyboardFocusGained(FocusGainedCause cause, Widget previousWidget)
        {
            KeyboardFocusGained();
            CheckStartEditOnFocusGained(cause, previousWidget);
        }

        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            if (!visible)
            {
                CancelEdit();
            }
        }

        internal override void WidgetDisabled()
        {
            CancelEdit();
        }

        protected override void Layout()
        {
            int height = GetInnerHeight();
            int y = GetInnerY();
            _decButton.SetPosition(GetInnerX(), y);
            _decButton.SetSize(_decButton.GetPreferredWidth(), height);
            _incButton.SetPosition(GetInnerRight() - _incButton.GetPreferredWidth(), y);
            _incButton.SetSize(_incButton.GetPreferredWidth(), height);
            int labelX = _decButton.GetRight();
            int labelWidth = Math.Max(0, _incButton.GetX() - labelX);
            _label.SetSize(labelWidth, height);
            _label.SetPosition(labelX, y);
            _editField.SetSize(labelWidth, height);
            _editField.SetPosition(labelX, y);
        }

        protected void SetDisplayText()
        {
            String prefix = (_displayPrefix != null) ? _displayPrefix : _displayPrefixTheme;
            _label.SetText(prefix + FormatText());
        }

        protected abstract String FormatText();

        void CheckStartEditOnFocusGained(FocusGainedCause cause, Widget previousWidget)
        {
            if (cause == FocusGainedCause.FocusKey)
            {
                if (previousWidget != null && !(previousWidget is ValueAdjuster))
                {
                    previousWidget = previousWidget.GetParent();
                }
                if (previousWidget != this && (previousWidget is ValueAdjuster))
                {
                    if (((ValueAdjuster)previousWidget)._wasInEditOnFocusLost)
                    {
                        StartEdit();
                    }
                }
            }
        }

        void OnTimer(int nextDelay)
        {
            _timer.SetDelay(nextDelay);
            if (_incButton.GetModel().Armed)
            {
                CancelEdit();
                DoIncrement();
            }
            else if (_decButton.GetModel().Armed)
            {
                CancelEdit();
                DoDecrement();
            }
        }

        void UpdateTimer()
        {
            if (_timer != null)
            {
                if (_incButton.GetModel().Armed || _decButton.GetModel().Armed)
                {
                    if (!_timer.IsRunning())
                    {
                        OnTimer(INITIAL_DELAY);
                        _timer.Start();
                    }
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            _timer = gui.CreateTimer();
            _timer.Tick += Timer_Tick;
            _timer.SetContinuous(true);
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            OnTimer(REPEAT_DELAY);
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            base.BeforeRemoveFromGUI(gui);
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = null;
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.IsKeyEvent())
            {
                if (evt.IsKeyPressedEvent() && evt.GetKeyCode() == Event.KEY_ESCAPE && _listeners.dragActive)
                {
                    _listeners.dragActive = false;
                    OnDragCancelled();
                    return true;
                }
                if (!_editField.IsVisible())
                {
                    if (evt.GetEventType() == EventType.KEY_PRESSED)
                    {
                        switch (evt.GetKeyCode())
                        {
                            case Event.KEY_RIGHT:
                                DoIncrement();
                                return true;
                            case Event.KEY_LEFT:
                                DoDecrement();
                                return true;
                            case Event.KEY_RETURN:
                            case Event.KEY_SPACE:
                                StartEdit();
                                return true;
                            default:
                                if (evt.HasKeyCharNoModifiers() && ShouldStartEdit(evt.GetKeyChar()))
                                {
                                    StartEdit();
                                    _editField.HandleEvent(evt);
                                    return true;
                                }
                                break;
                        }
                    }

                    return false;
                }
            }
            else if (!_editField.IsVisible() && _useMouseWheel && evt.GetEventType() == EventType.MOUSE_WHEEL)
            {
                if (evt.GetMouseWheelDelta() < 0)
                {
                    DoDecrement();
                }
                else if (evt.GetMouseWheelDelta() > 0)
                {
                    DoIncrement();
                }
                return true;
            }
            return base.HandleEvent(evt);
        }

        protected abstract String OnEditStart();
        protected abstract bool OnEditEnd(String text);
        protected abstract String ValidateEdit(String text);
        protected abstract void OnEditCanceled();
        protected abstract bool ShouldStartEdit(char ch);

        protected abstract void OnDragStart();
        protected abstract void OnDragUpdate(int dragDelta);
        protected abstract void OnDragCancelled();
        protected void OnDragEnd() { }

        protected abstract void DoDecrement();
        protected abstract void DoIncrement();

        void HandleEditCallback(int key)
        {
            switch (key)
            {
                case Event.KEY_RETURN:
                    if (OnEditEnd(_editField.GetText()))
                    {
                        _label.SetVisible(true);
                        _editField.SetVisible(false);
                    }
                    break;

                case Event.KEY_ESCAPE:
                    CancelEdit();
                    break;

                default:
                    _editField.SetErrorMessage(ValidateEdit(_editField.GetText()));
                    break;
            }
        }

        protected abstract void SyncWithModel();

        class L : DraggableButton.DragListener
        {
            private ValueAdjuster _valueAdjuster;

            public L(ValueAdjuster valueAdjuster)
            {
                this._valueAdjuster = valueAdjuster;
            }
            internal bool dragActive;
            public void DragStarted()
            {
                dragActive = true;
                this._valueAdjuster.OnDragStart();
            }
            public void Dragged(int deltaX, int deltaY)
            {
                if (dragActive)
                {
                    this._valueAdjuster.OnDragUpdate(deltaX);
                }
            }
            public void DragStopped()
            {
                dragActive = false;
                this._valueAdjuster.OnDragEnd();
            }
        }
    }
}
