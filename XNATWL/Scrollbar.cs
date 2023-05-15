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
using XNATWL.Utils;

namespace XNATWL
{
    public class Scrollbar : Widget
    {
        public enum Orientation
        {
            Horizontal,
            Vertical
        };

        private static int INITIAL_DELAY = 300;
        private static int REPEAT_DELAY = 75;

        private Orientation _orientation;
        private Button _btnUpLeft;
        private Button _btnDownRight;
        private DraggableButton _thumb;
        private L _dragTimerCB;
        private Timer _timer;
        private int _trackClicked;
        private int _trackClickLimit;
        private Renderer.Image _trackImageUpLeft;
        private Renderer.Image _trackImageDownRight;
        private IntegerModel _model;
        private Runnable _modelCB;

        private int _pageSize;
        private int _stepSize;
        private bool _scaleThumb;

        private int _minValue;
        private int _maxValue;
        private int _value;

        public event EventHandler<ScrollbarChangedPositionEventArgs> PositionChanged;

        public Scrollbar() : this(Orientation.Vertical)
        {
            
        }

        public Scrollbar(Orientation orientation)
        {
            this._orientation = orientation;
            this._btnUpLeft = new Button();
            this._btnDownRight = new Button();
            this._thumb = new DraggableButton();

            if (orientation == Orientation.Horizontal)
            {
                SetTheme("hscrollbar");
                _btnUpLeft.SetTheme("leftbutton");
                _btnDownRight.SetTheme("rightbutton");
            }
            else
            {
                SetTheme("vscrollbar");
                _btnUpLeft.SetTheme("upbutton");
                _btnDownRight.SetTheme("downbutton");
            }

            _dragTimerCB = new L(this);

            _btnUpLeft.SetCanAcceptKeyboardFocus(false);
            _btnUpLeft.GetModel().State += Scrollbar_State;
            _btnDownRight.SetCanAcceptKeyboardFocus(false);
            _btnDownRight.GetModel().State += Scrollbar_State;
            _thumb.SetCanAcceptKeyboardFocus(false);
            _thumb.SetTheme("thumb");
            _thumb.SetListener(_dragTimerCB);

            Add(_btnUpLeft);
            Add(_btnDownRight);
            Add(_thumb);

            this._pageSize = 10;
            this._stepSize = 1;
            this._maxValue = 100;

            SetSize(30, 200);
            SetDepthFocusTraversal(false);
        }

        private void Scrollbar_State(object sender, ButtonStateChangedEventArgs e)
        {
            this.UpdateTimer();
        }

        public Orientation GetOrientation()
        {
            return _orientation;
        }

        public IntegerModel GetModel()
        {
            return _model;
        }

        public void SetModel(IntegerModel model)
        {
            if (this._model != model)
            {
                if (this._model != null)
                {
                    this._model.Changed -= Model_Changed;
                }
                this._model = model;
                if (model != null)
                {
                    if (_modelCB == null)
                    {
                        //modelCB = new Runnable() {
                        //    public void run() {
                        //        syncModel();
                        //    }
                        //};
                    }
                    this._model.Changed += Model_Changed;
                    SyncModel();
                }
            }
        }

        private void Model_Changed(object sender, IntegerChangedEventArgs e)
        {
            SyncModel();
        }

        public int GetValue()
        {
            return _value;
        }

        public void SetValue(int current)
        {
            SetValue(current, true);
        }

        public void SetValue(int value, bool fireCallbacks)
        {
            value = Range(value);
            int oldValue = this._value;
            if (oldValue != value)
            {
                this._value = value;
                SetThumbPos();
                FirePropertyChange("value", oldValue, value);
                if (_model != null)
                {
                    this._model.Value = value;
                }
                if (fireCallbacks)
                {
                    this.PositionChanged.Invoke(this, new ScrollbarChangedPositionEventArgs());
                }
            }
        }

        public void Scroll(int amount)
        {
            if (_minValue < _maxValue)
            {
                SetValue(_value + amount);
            }
            else
            {
                SetValue(_value - amount);
            }
        }

        /**
         * Tries to make the specified area completely visible. If it is larger
         * then the page size then it scrolls to the start of the area.
         * 
         * @param start the position of the area
         * @param size size of the area
         * @param extra the extra space which should be visible around the area
         */
        public void ScrollToArea(int start, int size, int extra)
        {
            if (size <= 0)
            {
                return;
            }
            if (extra < 0)
            {
                extra = 0;
            }

            int end = start + size;
            start = Range(start);
            int pos = _value;

            int startWithExtra = Range(start - extra);
            if (startWithExtra < pos)
            {
                pos = startWithExtra;
            }
            int pageEnd = pos + _pageSize;
            int endWithExtra = end + extra;
            if (endWithExtra > pageEnd)
            {
                pos = Range(endWithExtra - _pageSize);
                if (pos > startWithExtra)
                {
                    size = end - start;
                    pos = start - Math.Max(0, _pageSize - size) / 2;
                }
            }

            SetValue(pos);
        }

        public int GetMinValue()
        {
            return _minValue;
        }

        public int GetMaxValue()
        {
            return _maxValue;
        }

        public void SetMinMaxValue(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._value = Range(_value);
            SetThumbPos();
            _thumb.SetVisible(minValue != maxValue);
        }

        public int GetPageSize()
        {
            return _pageSize;
        }

        public void SetPageSize(int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException("pageSize < 1");
            }
            this._pageSize = pageSize;
            if (_scaleThumb)
            {
                SetThumbPos();
            }
        }

        public int GetStepSize()
        {
            return _stepSize;
        }

        public void SetStepSize(int stepSize)
        {
            if (stepSize < 1)
            {
                throw new ArgumentOutOfRangeException("stepSize < 1");
            }
            this._stepSize = stepSize;
        }

        public bool IsScaleThumb()
        {
            return _scaleThumb;
        }

        public void SetScaleThumb(bool scaleThumb)
        {
            this._scaleThumb = scaleThumb;
            SetThumbPos();
        }

        public void ExternalDragStart()
        {
            _thumb.GetAnimationState().SetAnimationState(Button.STATE_PRESSED, true);
            _dragTimerCB.DragStarted();
        }

        public void ExternalDragged(int deltaX, int deltaY)
        {
            _dragTimerCB.Dragged(deltaX, deltaY);
        }

        public void ExternalDragStopped()
        {
            // dragTimerCB.dragStopped(); (it's empty anyway)
            _thumb.GetAnimationState().SetAnimationState(Button.STATE_PRESSED, false);
        }

        public bool IsUpLeftButtonArmed()
        {
            return _btnUpLeft.GetModel().Armed;
        }

        public bool IsDownRightButtonArmed()
        {
            return _btnDownRight.GetModel().Armed;
        }

        public bool IsThumbDragged()
        {
            return _thumb.GetModel().Pressed;
        }

        public void SetThumbTooltipContent(Object tooltipContent)
        {
            _thumb.SetTooltipContent(tooltipContent);
        }

        public Object GetThumbTooltipContent()
        {
            return _thumb.GetTooltipContent();
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeScrollbar(themeInfo);
        }

        protected void ApplyThemeScrollbar(ThemeInfo themeInfo)
        {
            SetScaleThumb(themeInfo.GetParameter("scaleThumb", false));
            if (_orientation == Orientation.Horizontal)
            {
                _trackImageUpLeft = (Renderer.Image) themeInfo.GetParameterValue("trackImageLeft", false, typeof(Renderer.Image));
                _trackImageDownRight = (Renderer.Image)themeInfo.GetParameterValue("trackImageRight", false, typeof(Renderer.Image));
            }
            else
            {
                _trackImageUpLeft = (Renderer.Image)themeInfo.GetParameterValue("trackImageUp", false, typeof(Renderer.Image));
                _trackImageDownRight = (Renderer.Image)themeInfo.GetParameterValue("trackImageDown", false, typeof(Renderer.Image));
            }
        }

        protected override void PaintWidget(GUI gui)
        {
            int x = GetInnerX();
            int y = GetInnerY();
            if (_orientation == Orientation.Horizontal)
            {
                int h = GetInnerHeight();
                if (_trackImageUpLeft != null)
                {
                    _trackImageUpLeft.Draw(GetAnimationState(), x, y, _thumb.GetX() - x, h);
                }
                if (_trackImageDownRight != null)
                {
                    int thumbRight = _thumb.GetRight();
                    _trackImageDownRight.Draw(GetAnimationState(), thumbRight, y, GetInnerRight() - thumbRight, h);
                }
            }
            else
            {
                int w = GetInnerWidth();
                if (_trackImageUpLeft != null)
                {
                    _trackImageUpLeft.Draw(GetAnimationState(), x, y, w, _thumb.GetY() - y);
                }
                if (_trackImageDownRight != null)
                {
                    int thumbBottom = _thumb.GetBottom();
                    _trackImageDownRight.Draw(GetAnimationState(), x, thumbBottom, w, GetInnerBottom() - thumbBottom);
                }
            }
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            _timer = gui.CreateTimer();
            _timer.Tick += Timer_Tick;
            _timer.SetContinuous(true);
            if (_model != null)
            {
                // modelCB is created when the model was set
                this._model.Changed += Model_Changed;
            }
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            this.OnTimer(REPEAT_DELAY);
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            base.BeforeRemoveFromGUI(gui);
            if (_model != null)
            {
                this._model.Changed -= Model_Changed;
            }
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = null;
        }

        //@Override
        public override bool HandleEvent(Event evt)
        {
            if (evt.GetEventType() == EventType.MOUSE_BTNUP &&
                    evt.GetMouseButton() == Event.MOUSE_LBUTTON)
            {
                _trackClicked = 0;
                UpdateTimer();
            }

            if (!base.HandleEvent(evt))
            {
                if (evt.GetEventType() == EventType.MOUSE_BTNDOWN &&
                        evt.GetMouseButton() == Event.MOUSE_LBUTTON)
                {
                    if (IsMouseInside(evt))
                    {
                        if (_orientation == Orientation.Horizontal)
                        {
                            _trackClickLimit = evt.GetMouseX();
                            if (evt.GetMouseX() < _thumb.GetX())
                            {
                                _trackClicked = -1;
                            }
                            else
                            {
                                _trackClicked = 1;
                            }
                        }
                        else
                        {
                            _trackClickLimit = evt.GetMouseY();
                            if (evt.GetMouseY() < _thumb.GetY())
                            {
                                _trackClicked = -1;
                            }
                            else
                            {
                                _trackClicked = 1;
                            }
                        }
                        UpdateTimer();
                    }
                }
            }

            bool page = (evt.GetModifiers() & Event.MODIFIER_CTRL) != 0;
            int step = page ? _pageSize : _stepSize;

            if (evt.GetEventType() == EventType.KEY_PRESSED)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_LEFT:
                        if (_orientation == Orientation.Horizontal)
                        {
                            SetValue(_value - step);
                            return true;
                        }
                        break;
                    case Event.KEY_RIGHT:
                        if (_orientation == Orientation.Horizontal)
                        {
                            SetValue(_value + step);
                            return true;
                        }
                        break;
                    case Event.KEY_UP:
                        if (_orientation == Orientation.Vertical)
                        {
                            SetValue(_value - step);
                            return true;
                        }
                        break;
                    case Event.KEY_DOWN:
                        if (_orientation == Orientation.Vertical)
                        {
                            SetValue(_value + step);
                            return true;
                        }
                        break;
                    case Event.KEY_PRIOR:
                        if (_orientation == Orientation.Vertical)
                        {
                            SetValue(_value - _pageSize);
                            return true;
                        }
                        break;
                    case Event.KEY_NEXT:
                        if (_orientation == Orientation.Vertical)
                        {
                            SetValue(_value + _pageSize);
                            return true;
                        }
                        break;
                }
            }

            if (evt.GetEventType() == EventType.MOUSE_WHEEL)
            {
                SetValue(_value - step * evt.GetMouseWheelDelta());
            }

            // eat all mouse events
            return evt.IsMouseEvent();
        }

        int Range(int current)
        {
            if (_minValue < _maxValue)
            {
                if (current < _minValue)
                {
                    current = _minValue;
                }
                else if (current > _maxValue)
                {
                    current = _maxValue;
                }
            }
            else
            {
                if (current > _minValue)
                {
                    current = _minValue;
                }
                else if (current < _maxValue)
                {
                    current = _maxValue;
                }
            }
            return current;
        }

        void OnTimer(int nextDelay)
        {
            _timer.SetDelay(nextDelay);
            if (_trackClicked != 0)
            {
                int thumbPos;
                if (_orientation == Orientation.Horizontal)
                {
                    thumbPos = _thumb.GetX();
                }
                else
                {
                    thumbPos = _thumb.GetY();
                }
                if ((_trackClickLimit - thumbPos) * _trackClicked > 0)
                {
                    Scroll(_trackClicked * _pageSize);
                }
            }
            else if (_btnUpLeft.GetModel().Armed)
            {
                Scroll(-_stepSize);
            }
            else if (_btnDownRight.GetModel().Armed)
            {
                Scroll(_stepSize);
            }
        }

        void UpdateTimer()
        {
            if (_timer != null)
            {
                if (_trackClicked != 0 ||
                        _btnUpLeft.GetModel().Armed ||
                        _btnDownRight.GetModel().Armed)
                {
                    if (!_timer.IsRunning())
                    {
                        OnTimer(INITIAL_DELAY);
                        // onTimer() can call setValue() which calls user code
                        // that user code could potentially remove the Scrollbar from GUI
                        if (_timer != null)
                        {
                            _timer.Start();
                        }
                    }
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        void SyncModel()
        {
            SetMinMaxValue(_model.MinValue, _model.MaxValue);
            SetValue(_model.Value);
        }

        //@Override
        public override int GetMinWidth()
        {
            if (_orientation == Orientation.Horizontal)
            {
                return Math.Max(base.GetMinWidth(), _btnUpLeft.GetMinWidth() + _thumb.GetMinWidth() + _btnDownRight.GetMinWidth());
            }
            else
            {
                return Math.Max(base.GetMinWidth(), _thumb.GetMinWidth());
            }
        }

        //@Override
        public override int GetMinHeight()
        {
            if (_orientation == Orientation.Horizontal)
            {
                return Math.Max(base.GetMinHeight(), _thumb.GetMinHeight());
            }
            else
            {
                return Math.Max(base.GetMinHeight(), _btnUpLeft.GetMinHeight() + _thumb.GetMinHeight() + _btnDownRight.GetMinHeight());
            }
        }

        //@Override
        public override int GetPreferredWidth()
        {
            return GetMinWidth();
        }

        //@Override
        public override int GetPreferredHeight()
        {
            return GetMinHeight();
        }

        //@Override
        protected override void Layout()
        {
            if (_orientation == Orientation.Horizontal)
            {
                _btnUpLeft.SetSize(_btnUpLeft.GetPreferredWidth(), GetHeight());
                _btnUpLeft.SetPosition(GetX(), GetY());
                _btnDownRight.SetSize(_btnUpLeft.GetPreferredWidth(), GetHeight());
                _btnDownRight.SetPosition(GetX() + GetWidth() - _btnDownRight.GetWidth(), GetY());
            }
            else
            {
                _btnUpLeft.SetSize(GetWidth(), _btnUpLeft.GetPreferredHeight());
                _btnUpLeft.SetPosition(GetX(), GetY());
                _btnDownRight.SetSize(GetWidth(), _btnDownRight.GetPreferredHeight());
                _btnDownRight.SetPosition(GetX(), GetY() + GetHeight() - _btnDownRight.GetHeight());
            }
            SetThumbPos();
        }

        int CalcThumbArea()
        {
            if (_orientation == Orientation.Horizontal)
            {
                return Math.Max(1, GetWidth() - _btnUpLeft.GetWidth() - _thumb.GetWidth() - _btnDownRight.GetWidth());
            }
            else
            {
                return Math.Max(1, GetHeight() - _btnUpLeft.GetHeight() - _thumb.GetHeight() - _btnDownRight.GetHeight());
            }
        }

        private void SetThumbPos()
        {
            int delta = _maxValue - _minValue;
            if (_orientation == Orientation.Horizontal)
            {
                int thumbWidth = _thumb.GetPreferredWidth();
                if (_scaleThumb)
                {
                    long availArea = Math.Max(1, GetWidth() - _btnUpLeft.GetWidth() - _btnDownRight.GetWidth());
                    thumbWidth = (int)Math.Max(thumbWidth, availArea * _pageSize / (_pageSize + delta + 1));
                }
                _thumb.SetSize(thumbWidth, GetHeight());

                int xpos = _btnUpLeft.GetX() + _btnUpLeft.GetWidth();
                if (delta != 0)
                {
                    xpos += (_value - _minValue) * CalcThumbArea() / delta;
                }
                _thumb.SetPosition(xpos, GetY());
            }
            else
            {
                int thumbHeight = _thumb.GetPreferredHeight();
                if (_scaleThumb)
                {
                    long availArea = Math.Max(1, GetHeight() - _btnUpLeft.GetHeight() - _btnDownRight.GetHeight());
                    thumbHeight = (int)Math.Max(thumbHeight, availArea * _pageSize / (_pageSize + delta + 1));
                }
                _thumb.SetSize(GetWidth(), thumbHeight);

                int ypos = _btnUpLeft.GetY() + _btnUpLeft.GetHeight();
                if (delta != 0)
                {
                    ypos += (_value - _minValue) * CalcThumbArea() / delta;
                }
                _thumb.SetPosition(GetX(), ypos);
            }
        }

        class L : DraggableButton.DragListener
        {
            private Scrollbar _scrollbar;
            public L(Scrollbar scrollbar)
            {
                this._scrollbar = scrollbar;
            }
            private int _startValue;
            public void DragStarted()
            {
                _startValue = this._scrollbar.GetValue();
            }
            public void Dragged(int deltaX, int deltaY)
            {
                int mouseDelta;
                if (this._scrollbar.GetOrientation() == Orientation.Horizontal)
                {
                    mouseDelta = deltaX;
                }
                else
                {
                    mouseDelta = deltaY;
                }
                int delta = (this._scrollbar.GetMaxValue() - this._scrollbar.GetMinValue()) * mouseDelta / this._scrollbar.CalcThumbArea();
                int newValue = this._scrollbar.Range(_startValue + delta);
                this._scrollbar.SetValue(newValue);
            }
            public void DragStopped()
            {
            }
        };
    }

    public class ScrollbarChangedPositionEventArgs : EventArgs
    {
    }
}
