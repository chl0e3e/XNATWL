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
            HORIZONTAL,
            VERTICAL
        };

        private static int INITIAL_DELAY = 300;
        private static int REPEAT_DELAY = 75;

        private Orientation orientation;
        private Button btnUpLeft;
        private Button btnDownRight;
        private DraggableButton thumb;
        private L dragTimerCB;
        private Timer timer;
        private int trackClicked;
        private int trackClickLimit;
        private Runnable[] callbacks;
        private Renderer.Image trackImageUpLeft;
        private Renderer.Image trackImageDownRight;
        private IntegerModel model;
        private Runnable modelCB;

        private int pageSize;
        private int stepSize;
        private bool scaleThumb;

        private int minValue;
        private int maxValue;
        private int value;

        public event EventHandler<ScrollbarChangedPositionEventArgs> PositionChanged;

        public Scrollbar() : this(Orientation.VERTICAL)
        {
            
        }

        public Scrollbar(Orientation orientation)
        {
            this.orientation = orientation;
            this.btnUpLeft = new Button();
            this.btnDownRight = new Button();
            this.thumb = new DraggableButton();

            if (orientation == Orientation.HORIZONTAL)
            {
                setTheme("hscrollbar");
                btnUpLeft.setTheme("leftbutton");
                btnDownRight.setTheme("rightbutton");
            }
            else
            {
                setTheme("vscrollbar");
                btnUpLeft.setTheme("upbutton");
                btnDownRight.setTheme("downbutton");
            }

            dragTimerCB = new L(this);

            btnUpLeft.setCanAcceptKeyboardFocus(false);
            btnUpLeft.getModel().State += Scrollbar_State;
            btnDownRight.setCanAcceptKeyboardFocus(false);
            btnDownRight.getModel().State += Scrollbar_State;
            thumb.setCanAcceptKeyboardFocus(false);
            thumb.setTheme("thumb");
            thumb.setListener(dragTimerCB);

            add(btnUpLeft);
            add(btnDownRight);
            add(thumb);

            this.pageSize = 10;
            this.stepSize = 1;
            this.maxValue = 100;

            setSize(30, 200);
            setDepthFocusTraversal(false);
        }

        private void Scrollbar_State(object sender, ButtonStateChangedEventArgs e)
        {
            this.updateTimer();
        }

        public Orientation getOrientation()
        {
            return orientation;
        }

        public IntegerModel getModel()
        {
            return model;
        }

        public void setModel(IntegerModel model)
        {
            if (this.model != model)
            {
                if (this.model != null)
                {
                    this.model.Changed -= Model_Changed;
                }
                this.model = model;
                if (model != null)
                {
                    if (modelCB == null)
                    {
                        //modelCB = new Runnable() {
                        //    public void run() {
                        //        syncModel();
                        //    }
                        //};
                    }
                    this.model.Changed += Model_Changed;
                    syncModel();
                }
            }
        }

        private void Model_Changed(object sender, IntegerChangedEventArgs e)
        {
            syncModel();
        }

        public int getValue()
        {
            return value;
        }

        public void setValue(int current)
        {
            setValue(current, true);
        }

        public void setValue(int value, bool fireCallbacks)
        {
            value = range(value);
            int oldValue = this.value;
            if (oldValue != value)
            {
                this.value = value;
                setThumbPos();
                firePropertyChange("value", oldValue, value);
                if (model != null)
                {
                    this.model.Value = value;
                }
                if (fireCallbacks)
                {
                    this.PositionChanged.Invoke(this, new ScrollbarChangedPositionEventArgs());
                }
            }
        }

        public void scroll(int amount)
        {
            if (minValue < maxValue)
            {
                setValue(value + amount);
            }
            else
            {
                setValue(value - amount);
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
        public void scrollToArea(int start, int size, int extra)
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
            start = range(start);
            int pos = value;

            int startWithExtra = range(start - extra);
            if (startWithExtra < pos)
            {
                pos = startWithExtra;
            }
            int pageEnd = pos + pageSize;
            int endWithExtra = end + extra;
            if (endWithExtra > pageEnd)
            {
                pos = range(endWithExtra - pageSize);
                if (pos > startWithExtra)
                {
                    size = end - start;
                    pos = start - Math.Max(0, pageSize - size) / 2;
                }
            }

            setValue(pos);
        }

        public int getMinValue()
        {
            return minValue;
        }

        public int getMaxValue()
        {
            return maxValue;
        }

        public void setMinMaxValue(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.value = range(value);
            setThumbPos();
            thumb.setVisible(minValue != maxValue);
        }

        public int getPageSize()
        {
            return pageSize;
        }

        public void setPageSize(int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException("pageSize < 1");
            }
            this.pageSize = pageSize;
            if (scaleThumb)
            {
                setThumbPos();
            }
        }

        public int getStepSize()
        {
            return stepSize;
        }

        public void setStepSize(int stepSize)
        {
            if (stepSize < 1)
            {
                throw new ArgumentOutOfRangeException("stepSize < 1");
            }
            this.stepSize = stepSize;
        }

        public bool isScaleThumb()
        {
            return scaleThumb;
        }

        public void setScaleThumb(bool scaleThumb)
        {
            this.scaleThumb = scaleThumb;
            setThumbPos();
        }

        public void externalDragStart()
        {
            thumb.getAnimationState().setAnimationState(Button.STATE_PRESSED, true);
            dragTimerCB.dragStarted();
        }

        public void externalDragged(int deltaX, int deltaY)
        {
            dragTimerCB.dragged(deltaX, deltaY);
        }

        public void externalDragStopped()
        {
            // dragTimerCB.dragStopped(); (it's empty anyway)
            thumb.getAnimationState().setAnimationState(Button.STATE_PRESSED, false);
        }

        public bool isUpLeftButtonArmed()
        {
            return btnUpLeft.getModel().Armed;
        }

        public bool isDownRightButtonArmed()
        {
            return btnDownRight.getModel().Armed;
        }

        public bool isThumbDragged()
        {
            return thumb.getModel().Pressed;
        }

        public void setThumbTooltipContent(Object tooltipContent)
        {
            thumb.setTooltipContent(tooltipContent);
        }

        public Object getThumbTooltipContent()
        {
            return thumb.getTooltipContent();
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeScrollbar(themeInfo);
        }

        protected void applyThemeScrollbar(ThemeInfo themeInfo)
        {
            setScaleThumb(themeInfo.getParameter("scaleThumb", false));
            if (orientation == Orientation.HORIZONTAL)
            {
                trackImageUpLeft = (Renderer.Image) themeInfo.getParameterValue("trackImageLeft", false, typeof(Renderer.Image));
                trackImageDownRight = (Renderer.Image)themeInfo.getParameterValue("trackImageRight", false, typeof(Renderer.Image));
            }
            else
            {
                trackImageUpLeft = (Renderer.Image)themeInfo.getParameterValue("trackImageUp", false, typeof(Renderer.Image));
                trackImageDownRight = (Renderer.Image)themeInfo.getParameterValue("trackImageDown", false, typeof(Renderer.Image));
            }
        }

        //@Override
        protected override void paintWidget(GUI gui)
        {
            int x = getInnerX();
            int y = getInnerY();
            if (orientation == Orientation.HORIZONTAL)
            {
                int h = getInnerHeight();
                if (trackImageUpLeft != null)
                {
                    trackImageUpLeft.Draw(getAnimationState(), x, y, thumb.getX() - x, h);
                }
                if (trackImageDownRight != null)
                {
                    int thumbRight = thumb.getRight();
                    trackImageDownRight.Draw(getAnimationState(), thumbRight, y, getInnerRight() - thumbRight, h);
                }
            }
            else
            {
                int w = getInnerWidth();
                if (trackImageUpLeft != null)
                {
                    trackImageUpLeft.Draw(getAnimationState(), x, y, w, thumb.getY() - y);
                }
                if (trackImageDownRight != null)
                {
                    int thumbBottom = thumb.getBottom();
                    trackImageDownRight.Draw(getAnimationState(), x, thumbBottom, w, getInnerBottom() - thumbBottom);
                }
            }
        }

        //@Override
        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            timer = gui.createTimer();
            timer.Tick += Timer_Tick;
            timer.setContinuous(true);
            if (model != null)
            {
                // modelCB is created when the model was set
                this.model.Changed += Model_Changed;
            }
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            this.onTimer(REPEAT_DELAY);
        }

        //@Override
        protected override void beforeRemoveFromGUI(GUI gui)
        {
            base.beforeRemoveFromGUI(gui);
            if (model != null)
            {
                this.model.Changed -= Model_Changed;
            }
            if (timer != null)
            {
                timer.stop();
            }
            timer = null;
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (evt.getEventType() == Event.EventType.MOUSE_BTNUP &&
                    evt.getMouseButton() == Event.MOUSE_LBUTTON)
            {
                trackClicked = 0;
                updateTimer();
            }

            if (!base.handleEvent(evt))
            {
                if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN &&
                        evt.getMouseButton() == Event.MOUSE_LBUTTON)
                {
                    if (isMouseInside(evt))
                    {
                        if (orientation == Orientation.HORIZONTAL)
                        {
                            trackClickLimit = evt.getMouseX();
                            if (evt.getMouseX() < thumb.getX())
                            {
                                trackClicked = -1;
                            }
                            else
                            {
                                trackClicked = 1;
                            }
                        }
                        else
                        {
                            trackClickLimit = evt.getMouseY();
                            if (evt.getMouseY() < thumb.getY())
                            {
                                trackClicked = -1;
                            }
                            else
                            {
                                trackClicked = 1;
                            }
                        }
                        updateTimer();
                    }
                }
            }

            bool page = (evt.getModifiers() & Event.MODIFIER_CTRL) != 0;
            int step = page ? pageSize : stepSize;

            if (evt.getEventType() == Event.EventType.KEY_PRESSED)
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_LEFT:
                        if (orientation == Orientation.HORIZONTAL)
                        {
                            setValue(value - step);
                            return true;
                        }
                        break;
                    case Event.KEY_RIGHT:
                        if (orientation == Orientation.HORIZONTAL)
                        {
                            setValue(value + step);
                            return true;
                        }
                        break;
                    case Event.KEY_UP:
                        if (orientation == Orientation.VERTICAL)
                        {
                            setValue(value - step);
                            return true;
                        }
                        break;
                    case Event.KEY_DOWN:
                        if (orientation == Orientation.VERTICAL)
                        {
                            setValue(value + step);
                            return true;
                        }
                        break;
                    case Event.KEY_PRIOR:
                        if (orientation == Orientation.VERTICAL)
                        {
                            setValue(value - pageSize);
                            return true;
                        }
                        break;
                    case Event.KEY_NEXT:
                        if (orientation == Orientation.VERTICAL)
                        {
                            setValue(value + pageSize);
                            return true;
                        }
                        break;
                }
            }

            if (evt.getEventType() == Event.EventType.MOUSE_WHEEL)
            {
                setValue(value - step * evt.getMouseWheelDelta());
            }

            // eat all mouse events
            return evt.isMouseEvent();
        }

        int range(int current)
        {
            if (minValue < maxValue)
            {
                if (current < minValue)
                {
                    current = minValue;
                }
                else if (current > maxValue)
                {
                    current = maxValue;
                }
            }
            else
            {
                if (current > minValue)
                {
                    current = minValue;
                }
                else if (current < maxValue)
                {
                    current = maxValue;
                }
            }
            return current;
        }

        void onTimer(int nextDelay)
        {
            timer.setDelay(nextDelay);
            if (trackClicked != 0)
            {
                int thumbPos;
                if (orientation == Orientation.HORIZONTAL)
                {
                    thumbPos = thumb.getX();
                }
                else
                {
                    thumbPos = thumb.getY();
                }
                if ((trackClickLimit - thumbPos) * trackClicked > 0)
                {
                    scroll(trackClicked * pageSize);
                }
            }
            else if (btnUpLeft.getModel().Armed)
            {
                scroll(-stepSize);
            }
            else if (btnDownRight.getModel().Armed)
            {
                scroll(stepSize);
            }
        }

        void updateTimer()
        {
            if (timer != null)
            {
                if (trackClicked != 0 ||
                        btnUpLeft.getModel().Armed ||
                        btnDownRight.getModel().Armed)
                {
                    if (!timer.isRunning())
                    {
                        onTimer(INITIAL_DELAY);
                        // onTimer() can call setValue() which calls user code
                        // that user code could potentially remove the Scrollbar from GUI
                        if (timer != null)
                        {
                            timer.start();
                        }
                    }
                }
                else
                {
                    timer.stop();
                }
            }
        }

        void syncModel()
        {
            setMinMaxValue(model.MinValue, model.MaxValue);
            setValue(model.Value);
        }

        //@Override
        public override int getMinWidth()
        {
            if (orientation == Orientation.HORIZONTAL)
            {
                return Math.Max(base.getMinWidth(), btnUpLeft.getMinWidth() + thumb.getMinWidth() + btnDownRight.getMinWidth());
            }
            else
            {
                return Math.Max(base.getMinWidth(), thumb.getMinWidth());
            }
        }

        //@Override
        public override int getMinHeight()
        {
            if (orientation == Orientation.HORIZONTAL)
            {
                return Math.Max(base.getMinHeight(), thumb.getMinHeight());
            }
            else
            {
                return Math.Max(base.getMinHeight(), btnUpLeft.getMinHeight() + thumb.getMinHeight() + btnDownRight.getMinHeight());
            }
        }

        //@Override
        public override int getPreferredWidth()
        {
            return getMinWidth();
        }

        //@Override
        public override int getPreferredHeight()
        {
            return getMinHeight();
        }

        //@Override
        protected override void layout()
        {
            if (orientation == Orientation.HORIZONTAL)
            {
                btnUpLeft.setSize(btnUpLeft.getPreferredWidth(), getHeight());
                btnUpLeft.setPosition(getX(), getY());
                btnDownRight.setSize(btnUpLeft.getPreferredWidth(), getHeight());
                btnDownRight.setPosition(getX() + getWidth() - btnDownRight.getWidth(), getY());
            }
            else
            {
                btnUpLeft.setSize(getWidth(), btnUpLeft.getPreferredHeight());
                btnUpLeft.setPosition(getX(), getY());
                btnDownRight.setSize(getWidth(), btnDownRight.getPreferredHeight());
                btnDownRight.setPosition(getX(), getY() + getHeight() - btnDownRight.getHeight());
            }
            setThumbPos();
        }

        int calcThumbArea()
        {
            if (orientation == Orientation.HORIZONTAL)
            {
                return Math.Max(1, getWidth() - btnUpLeft.getWidth() - thumb.getWidth() - btnDownRight.getWidth());
            }
            else
            {
                return Math.Max(1, getHeight() - btnUpLeft.getHeight() - thumb.getHeight() - btnDownRight.getHeight());
            }
        }

        private void setThumbPos()
        {
            int delta = maxValue - minValue;
            if (orientation == Orientation.HORIZONTAL)
            {
                int thumbWidth = thumb.getPreferredWidth();
                if (scaleThumb)
                {
                    long availArea = Math.Max(1, getWidth() - btnUpLeft.getWidth() - btnDownRight.getWidth());
                    thumbWidth = (int)Math.Max(thumbWidth, availArea * pageSize / (pageSize + delta + 1));
                }
                thumb.setSize(thumbWidth, getHeight());

                int xpos = btnUpLeft.getX() + btnUpLeft.getWidth();
                if (delta != 0)
                {
                    xpos += (value - minValue) * calcThumbArea() / delta;
                }
                thumb.setPosition(xpos, getY());
            }
            else
            {
                int thumbHeight = thumb.getPreferredHeight();
                if (scaleThumb)
                {
                    long availArea = Math.Max(1, getHeight() - btnUpLeft.getHeight() - btnDownRight.getHeight());
                    thumbHeight = (int)Math.Max(thumbHeight, availArea * pageSize / (pageSize + delta + 1));
                }
                thumb.setSize(getWidth(), thumbHeight);

                int ypos = btnUpLeft.getY() + btnUpLeft.getHeight();
                if (delta != 0)
                {
                    ypos += (value - minValue) * calcThumbArea() / delta;
                }
                thumb.setPosition(getX(), ypos);
            }
        }

        class L : DraggableButton.DragListener
        {
            private Scrollbar _scrollbar;
            public L(Scrollbar scrollbar)
            {
                this._scrollbar = scrollbar;
            }
            private int startValue;
            public void dragStarted()
            {
                startValue = this._scrollbar.getValue();
            }
            public void dragged(int deltaX, int deltaY)
            {
                int mouseDelta;
                if (this._scrollbar.getOrientation() == Orientation.HORIZONTAL)
                {
                    mouseDelta = deltaX;
                }
                else
                {
                    mouseDelta = deltaY;
                }
                int delta = (this._scrollbar.getMaxValue() - this._scrollbar.getMinValue()) * mouseDelta / this._scrollbar.calcThumbArea();
                int newValue = this._scrollbar.range(startValue + delta);
                this._scrollbar.setValue(newValue);
            }
            public void dragStopped()
            {
            }
        };
    }

    public class ScrollbarChangedPositionEventArgs : EventArgs
    {
    }
}
