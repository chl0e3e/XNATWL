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

        private DraggableButton label;
        private EditField editField;
        private Button decButton;
        private Button incButton;
        private Runnable timerCallback;
        private L listeners;
        private Timer timer;

        private String displayPrefix;
        private String displayPrefixTheme = "";
        private bool useMouseWheel = true;
        private bool acceptValueOnFocusLoss = true;
        private bool wasInEditOnFocusLost;
        private int width;

        public ValueAdjuster()
        {
            this.label = new DraggableButton(getAnimationState(), true);
            // EditField always inherits from the passed animation state
            this.editField = new EditField(getAnimationState());
            this.decButton = new Button(getAnimationState(), true);
            this.incButton = new Button(getAnimationState(), true);

            label.setClip(true);
            label.setTheme("valueDisplay");
            editField.setTheme("valueEdit");
            decButton.setTheme("decButton");
            incButton.setTheme("incButton");

            decButton.getModel().State += ValueAdjuster_State;
            incButton.getModel().State += ValueAdjuster_State;

            listeners = new L(this);
            label.Action += Label_Action;
            label.setListener(listeners);

            editField.setVisible(false);
            editField.Callback += EditField_Callback;

            add(label);
            add(editField);
            add(decButton);
            add(incButton);
            setCanAcceptKeyboardFocus(true);
            setDepthFocusTraversal(false);
        }

        private void EditField_Callback(object sender, EditFieldCallbackEventArgs e)
        {
            this.handleEditCallback(e.Key);
        }

        private void Label_Action(object sender, ButtonActionEventArgs e)
        {
            startEdit();
        }

        private void ValueAdjuster_State(object sender, ButtonStateChangedEventArgs e)
        {
            updateTimer();
        }

        public String getDisplayPrefix()
        {
            return displayPrefix;
        }

        /**
         * Sets the display prefix which is displayed before the value.
         *
         * If this is property is null then the value from the theme is used,
         * otherwise this one.
         *
         * @param displayPrefix the prefix or null
         */
        public void setDisplayPrefix(String displayPrefix)
        {
            this.displayPrefix = displayPrefix;
            setDisplayText();
        }

        public bool isUseMouseWheel()
        {
            return useMouseWheel;
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
        public void setAcceptValueOnFocusLoss(bool acceptValueOnFocusLoss)
        {
            this.acceptValueOnFocusLoss = acceptValueOnFocusLoss;
        }

        public bool isAcceptValueOnFocusLoss()
        {
            return acceptValueOnFocusLoss;
        }

        /**
         * Controls if the ValueAdjuster should respond to the mouse wheel or not
         *
         * @param useMouseWheel true if the mouse wheel is used
         */
        public void setUseMouseWheel(bool useMouseWheel)
        {
            this.useMouseWheel = useMouseWheel;
        }

        //@Override
        public override void setTooltipContent(Object tooltipContent)
        {
            base.setTooltipContent(tooltipContent);
            label.setTooltipContent(tooltipContent);
        }

        public void startEdit()
        {
            if (label.isVisible())
            {
                editField.setErrorMessage(null);
                editField.setText(onEditStart());
                editField.setVisible(true);
                editField.requestKeyboardFocus();
                editField.selectAll();
                editField.getAnimationState().setAnimationState(EditField.STATE_HOVER, label.getModel().Hover);
                label.setVisible(false);
                getAnimationState().setAnimationState(STATE_EDIT_ACTIVE, true);
            }
        }

        public void cancelEdit()
        {
            if (editField.isVisible())
            {
                onEditCanceled();
                label.setVisible(true);
                editField.setVisible(false);
                label.getModel().Hover = editField.getAnimationState().GetAnimationState(Label.STATE_HOVER);
                getAnimationState().setAnimationState(STATE_EDIT_ACTIVE, false);
            }
        }

        public void cancelOrAcceptEdit()
        {
            if (editField.isVisible())
            {
                if (acceptValueOnFocusLoss)
                {
                    onEditEnd(editField.getText());
                }
                cancelEdit();
            }
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeValueAdjuster(themeInfo);
        }

        protected void applyThemeValueAdjuster(ThemeInfo themeInfo)
        {
            width = themeInfo.GetParameter("width", 100);
            displayPrefixTheme = themeInfo.GetParameter("displayPrefix", "");
            useMouseWheel = themeInfo.GetParameter("useMouseWheel", useMouseWheel);
        }

        //@Override
        public override int getMinWidth()
        {
            int minWidth = base.getMinWidth();
            minWidth = Math.Max(minWidth,
                    getBorderHorizontal() +
                    decButton.getMinWidth() +
                    Math.Max(width, label.getMinWidth()) +
                    incButton.getMinWidth());
            return minWidth;
        }

        //@Override
        public override int getMinHeight()
        {
            int minHeight = label.getMinHeight();
            minHeight = Math.Max(minHeight, decButton.getMinHeight());
            minHeight = Math.Max(minHeight, incButton.getMinHeight());
            minHeight += getBorderVertical();
            return Math.Max(minHeight, base.getMinHeight());
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            return decButton.getPreferredWidth() +
                    Math.Max(width, label.getPreferredWidth()) +
                    incButton.getPreferredWidth();
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            return Math.Max(Math.Max(
                    decButton.getPreferredHeight(),
                    incButton.getPreferredHeight()),
                    label.getPreferredHeight());
        }

        //@Override
        protected override void keyboardFocusLost()
        {
            wasInEditOnFocusLost = editField.isVisible();
            cancelOrAcceptEdit();
            label.getAnimationState().setAnimationState(STATE_KEYBOARD_FOCUS, false);
        }

        //@Override
        protected override void keyboardFocusGained()
        {
            // keep in this method to not change subclassing behavior
            label.getAnimationState().setAnimationState(STATE_KEYBOARD_FOCUS, true);
        }

        //@Override
        protected override void keyboardFocusGained(FocusGainedCause cause, Widget previousWidget)
        {
            keyboardFocusGained();
            checkStartEditOnFocusGained(cause, previousWidget);
        }

        //@Override
        public override void setVisible(bool visible)
        {
            base.setVisible(visible);
            if (!visible)
            {
                cancelEdit();
            }
        }

        //@Override
        internal override void widgetDisabled()
        {
            cancelEdit();
        }

        //@Override
        protected override void layout()
        {
            int height = getInnerHeight();
            int y = getInnerY();
            decButton.setPosition(getInnerX(), y);
            decButton.setSize(decButton.getPreferredWidth(), height);
            incButton.setPosition(getInnerRight() - incButton.getPreferredWidth(), y);
            incButton.setSize(incButton.getPreferredWidth(), height);
            int labelX = decButton.getRight();
            int labelWidth = Math.Max(0, incButton.getX() - labelX);
            label.setSize(labelWidth, height);
            label.setPosition(labelX, y);
            editField.setSize(labelWidth, height);
            editField.setPosition(labelX, y);
        }

        protected void setDisplayText()
        {
            String prefix = (displayPrefix != null) ? displayPrefix : displayPrefixTheme;
            label.setText(prefix + formatText());
        }

        protected abstract String formatText();

        void checkStartEditOnFocusGained(FocusGainedCause cause, Widget previousWidget)
        {
            if (cause == FocusGainedCause.FOCUS_KEY)
            {
                if (previousWidget != null && !(previousWidget is ValueAdjuster)) {
                    previousWidget = previousWidget.getParent();
                }
                if (previousWidget != this && (previousWidget is ValueAdjuster)) {
                    if (((ValueAdjuster)previousWidget).wasInEditOnFocusLost)
                    {
                        startEdit();
                    }
                }
            }
        }

        void onTimer(int nextDelay)
        {
            timer.setDelay(nextDelay);
            if (incButton.getModel().Armed)
            {
                cancelEdit();
                doIncrement();
            }
            else if (decButton.getModel().Armed)
            {
                cancelEdit();
                doDecrement();
            }
        }

        void updateTimer()
        {
            if (timer != null)
            {
                if (incButton.getModel().Armed || decButton.getModel().Armed)
                {
                    if (!timer.isRunning())
                    {
                        onTimer(INITIAL_DELAY);
                        timer.start();
                    }
                }
                else
                {
                    timer.stop();
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
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            onTimer(REPEAT_DELAY);
        }

        //@Override
        protected override void beforeRemoveFromGUI(GUI gui)
        {
            base.beforeRemoveFromGUI(gui);
            if (timer != null)
            {
                timer.stop();
            }
            timer = null;
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (evt.isKeyEvent())
            {
                if (evt.isKeyPressedEvent() && evt.getKeyCode() == Event.KEY_ESCAPE && listeners.dragActive)
                {
                    listeners.dragActive = false;
                    onDragCancelled();
                    return true;
                }
                if (!editField.isVisible())
                {
                    if (evt.getEventType() == EventType.KEY_PRESSED)
                    {
                        switch (evt.getKeyCode())
                        {
                            case Event.KEY_RIGHT:
                                doIncrement();
                                return true;
                            case Event.KEY_LEFT:
                                doDecrement();
                                return true;
                            case Event.KEY_RETURN:
                            case Event.KEY_SPACE:
                                startEdit();
                                return true;
                            default:
                                if (evt.hasKeyCharNoModifiers() && shouldStartEdit(evt.getKeyChar()))
                                {
                                    startEdit();
                                    editField.handleEvent(evt);
                                    return true;
                                }
                                break;
                        }
                    }

                    return false;
                }
            }
            else if (!editField.isVisible() && useMouseWheel && evt.getEventType() == EventType.MOUSE_WHEEL)
            {
                if (evt.getMouseWheelDelta() < 0)
                {
                    doDecrement();
                }
                else if (evt.getMouseWheelDelta() > 0)
                {
                    doIncrement();
                }
                return true;
            }
            return base.handleEvent(evt);
        }

        protected abstract String onEditStart();
        protected abstract bool onEditEnd(String text);
        protected abstract String validateEdit(String text);
        protected abstract void onEditCanceled();
        protected abstract bool shouldStartEdit(char ch);

        protected abstract void onDragStart();
        protected abstract void onDragUpdate(int dragDelta);
        protected abstract void onDragCancelled();
        protected void onDragEnd() { }

        protected abstract void doDecrement();
        protected abstract void doIncrement();

        void handleEditCallback(int key)
        {
            switch (key)
            {
                case Event.KEY_RETURN:
                    if (onEditEnd(editField.getText()))
                    {
                        label.setVisible(true);
                        editField.setVisible(false);
                    }
                    break;

                case Event.KEY_ESCAPE:
                    cancelEdit();
                    break;

                default:
                    editField.setErrorMessage(validateEdit(editField.getText()));
                    break;
            }
        }

        protected abstract void syncWithModel();

        class L : DraggableButton.DragListener
        {
            private ValueAdjuster _valueAdjuster;

            public L(ValueAdjuster valueAdjuster)
            {
                this._valueAdjuster = valueAdjuster;
            }
            internal bool dragActive;
            public void dragStarted()
            {
                dragActive = true;
                this._valueAdjuster.onDragStart();
            }
            public void dragged(int deltaX, int deltaY)
            {
                if (dragActive)
                {
                    this._valueAdjuster.onDragUpdate(deltaX);
                }
            }
            public void dragStopped()
            {
                dragActive = false;
                this._valueAdjuster.onDragEnd();
            }
        }
    }

}
