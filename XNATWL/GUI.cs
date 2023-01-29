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
using System.Collections.Generic;
using static XNATWL.Utils.Logger;
using XNATWL.Renderer;
using XNATWL.Theme;
using XNATWL.Utils;

namespace XNATWL
{
    public class GUI : Widget
    {
        public interface MouseIdleListener
        {
            void mouseEnterIdle();
            void mouseExitIdle();
        }

        /**
         * A completion listener for async jobs. It is invoked via
         * {@link #invokeLater(java.lang.Runnable) }
         *
         * @param <V> the result type of the async job
         */
        public interface AsyncCompletionListener<V>
        {
            /**
             * The job has completed normally
             *
             * @param result the result of the async job or {@code null} if it was a {@code Runnable}
             */
            void completed(V result);

            /**
             * The job has failed with an exception
             *
             * @param ex the exception thrown by the async job
             */
            void failed(Exception ex);
        }

        private static int DRAG_DIST = 3;
        private static int DBLCLICK_TIME = 500;   // ms
        private static int KEYREPEAT_INITIAL_DELAY = 250; // ms
        private static int KEYREPEAT_INTERVAL_DELAY = 1000 / 30;    // ms
        private static int NO_REPEAT = 0;

        private int tooltipOffsetX = 0;
        private int tooltipOffsetY = 0;
        private int tooltipDelay = 1000;  // 1 sec in ms
        private int tooltipReappearDelay = 100;

        private Renderer.Renderer renderer;
        private Input.Input input;

        internal long curTime;
        private int deltaTime;

        private Widget rootPane;
        internal bool hasInvalidLayouts;

        Event evt;
        private bool wasInside;
        private bool dragActive;
        private int mouseClickCount;
        private int dragButton = -1;
        private int mouseDownX;
        private int mouseDownY;
        private int mouseLastX;
        private int mouseLastY;
        private int mouseClickedX;
        private int mouseClickedY;
        private long mouseEventTime;
        private long tooltipEventTime;
        private long mouseClickedTime;
        private long keyEventTime;
        private int keyRepeatDelay;
        private bool popupEventOccured;
        private Widget lastMouseDownWidget;
        private Widget lastMouseClickWidget;
        private PopupWindow boundDragPopup;
        private Runnable boundDragCallback;
        private Widget focusKeyWidget;

        private int mouseIdleTime = 60;
        private bool mouseIdleState;
        private MouseIdleListener mouseIdleListener;

        private InfoWindow activeInfoWindow;
        private Widget infoWindowPlaceholder;

        private TooltipWindow tooltipWindow;
        private Label tooltipLabel;
        private Widget tooltipOwner;
        private bool hadOpenTooltip;
        private long tooltipClosedTime;

        internal List<Timer> activeTimers;
        //ExecutorService executorService;

        private Object invokeLock;
        private Runnable[] invokeLaterQueue;
        private int invokeLaterQueueSize;
        private Runnable[] invokeRunnables;

        /**
         * Constructs a new GUI manager with the given renderer and a default root
         * pane.
         *
         * This default root pane has no theme (eg "") and can't receive keyboard
         * focus.
         *
         * @param renderer the renderer
         * @see #GUI(de.matthiasmann.twl.Widget, de.matthiasmann.twl.renderer.Renderer.Renderer)
         */
        public GUI(Renderer.Renderer renderer) : this(new Widget(), renderer)
        {
            rootPane.setTheme("");
            rootPane.setFocusKeyEnabled(false);
        }

        /**
         * Constructs a new GUI manager with the given renderer, root pane and a
         * input source obtained from the renderer.
         * 
         * @param rootPane the root pane
         * @param renderer the renderer
         * @see Renderer.Renderer#getInput() 
         */
        public GUI(Widget rootPane, Renderer.Renderer renderer) : this(rootPane, renderer, renderer.Input)
        {
            
        }

        /**
         * Constructs a new GUI manager with the given renderer, input source and root pane
         *
         * @param rootPane the root pane
         * @param renderer the renderer
         * @param input the input source, can be null.
         */
        public GUI(Widget rootPane, Renderer.Renderer renderer, Input.Input input)
        {
            if (rootPane == null)
            {
                throw new ArgumentNullException("rootPane is null");
            }
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer is null");
            }

            this.guiInstance = this;
            this.renderer = renderer;
            this.input = input;
            this.evt = new Event();
            this.rootPane = rootPane;
            this.rootPane.setFocusKeyEnabled(false);

            this.infoWindowPlaceholder = new Widget();
            this.infoWindowPlaceholder.setTheme("");

            this.tooltipLabel = new Label();
            this.tooltipWindow = new TooltipWindow();
            this.tooltipWindow.setVisible(false);

            this.activeTimers = new List<Timer>();
            //this.executorService = Executors.newSingleThreadExecutor(new TF());    // thread creatation is lazy
            this.invokeLock = new Object();
            this.invokeLaterQueue = new Runnable[16];
            this.invokeRunnables = new Runnable[16];

            setTheme("");
            setFocusKeyEnabled(false);
            setSize();

            // insert rootPane (user provided class) last incase it invokes methods
            // which access GUI state (like requestKeyboardFocus) in overridable
            // methods (like afterAddToGUI)
            base.insertChild(infoWindowPlaceholder, 0);
            base.insertChild(tooltipWindow, 1);
            base.insertChild(rootPane, 0);

            resyncTimerAfterPause();
        }

        /**
         * Applies the specified theme to this UI tree.
         * If a widget in the tree has an empty theme name then it
         * is omitted from this process but it children are still processed.
         * 
         * @param themeManager the theme manager that should be used
         * @throws java.lang.NullPointerException if themeManager is null
         * @see Widget#setTheme(java.lang.String)
         */
        //@Override
        public override void applyTheme(ThemeManager themeManager)
        {
            if (themeManager == null)
            {
                throw new ArgumentOutOfRangeException("themeManager is null");
            }

            base.applyTheme(themeManager);
        }

        public Widget getRootPane()
        {
            return rootPane;
        }

        public void setRootPane(Widget rootPane)
        {
            if (rootPane == null)
            {
                throw new ArgumentOutOfRangeException("rootPane is null");
            }
            this.rootPane = rootPane;
            base.removeChild(0);
            base.insertChild(rootPane, 0);
        }

        public Renderer.Renderer getRenderer()
        {
            return renderer;
        }

        public Input.Input getInput()
        {
            return input;
        }

        public MouseSensitiveRectangle createMouseSenitiveRectangle()
        {
            return new GUIMouseSensitiveRectangle(this);
        }

        class GUIMouseSensitiveRectangle : MouseSensitiveRectangle
        {
            private GUI gui;

            public GUIMouseSensitiveRectangle(GUI gui) : base()
            {
                this.gui = gui;
            }

            public override bool isMouseOver()
            {
                return isInside(this.gui.evt.mouseX, this.gui.evt.mouseY);
            }
        }

        /**
         * Creates a new UI timer.
         * @return new Timer(this)
         */
        public Timer createTimer()
        {
            return new Timer(this);
        }

        /**
         * Returns the current UI time in milliseconds.
         * This time is updated via {@link #updateTime() }
         *
         * @return the current UI time in milliseconds.
         */
        public long getCurrentTime()
        {
            return curTime;
        }

        /**
         * Returns the delta time to the previous frame in milliseconds.
         * This time is updated via {@link #updateTime() }
         * 
         * @return the delta time
         */
        public int getCurrentDeltaTime()
        {
            return deltaTime;
        }

        /**
         * Queues a Runnable to be executed in the GUI main loop.
         * This method is thread safe.
         * 
         * @param runnable  the Runnable to execute
         * @see Widget#getGUI()
         */
        public void invokeLater(Runnable runnable)
        {
            if (runnable == null)
            {
                throw new ArgumentOutOfRangeException("runnable is null");
            }
            lock(invokeLock)
            {
                if (invokeLaterQueueSize == invokeLaterQueue.Length)
                {
                    growInvokeLaterQueue();
                }
                invokeLaterQueue[invokeLaterQueueSize++] = runnable;
            }
        }

        /**
         * Performs a job async in the background. After the job has completed (normally
         * or by throwing an exception) the completion listener is executed via
         * {@link #invokeLater(java.lang.Runnable) }
         *
         * If the job is canceled before it is started then the listener is not executed.
         *
         * This method is thread safe.
         *
         * @param <V> the result type of the job
         * @param job the job to execute
         * @param listener the listener which will be called once the job is finished
         * @return a Future representing pending completion of the job
         * @see Widget#getGUI() 
         */
        /*public<V> Future<V> invokeAsync(Callable<V> job, AsyncCompletionListener<V> listener)
        {
            if (job == null)
            {
                throw new ArgumentOutOfRangeException("job is null");
            }
            if (listener == null)
            {
                throw new ArgumentOutOfRangeException("listener is null");
            }
            return executorService.submit((Callable<V>)new AC<V>(job, null, listener));
        }
        */
        /**
         * Performs a job async in the background. After the job has completed (normally
         * or by throwing an exception) the completion listener is executed via
         * {@link #invokeLater(java.lang.Runnable) }
         *
         * If the job is canceled before it is started then the listener is not executed.
         *
         * This method is thread safe.
         *
         * @param <V> the result type of the listener. The job always returns null.
         * @param job the job to execute
         * @param listener the listener which will be called once the job is finished
         * @return a Future representing pending completion of the job
         * @see Widget#getGUI() 
         */
        /*public<V> Future<V> invokeAsync(Runnable job, AsyncCompletionListener<V> listener)
        {
            if (job == null)
            {
                throw new ArgumentOutOfRangeException("job is null");
            }
            if (listener == null)
            {
                throw new ArgumentOutOfRangeException("listener is null");
            }
            return executorService.submit((Callable<V>)new AC<V>(null, job, listener));
        }*/

        public bool requestToolTip(Widget widget, int x, int y,
                Object content, Alignment alignment)
        {
            if (alignment == null)
            {
                throw new ArgumentOutOfRangeException("alignment is null");
            }
            if (widget == getWidgetUnderMouse())
            {
                setTooltip(x, y, widget, content, alignment);
                return true;
            }
            return false;
        }

        public MouseIdleListener getMouseIdleListener()
        {
            return mouseIdleListener;
        }

        public void setMouseIdleListener(MouseIdleListener mouseIdleListener)
        {
            this.mouseIdleListener = mouseIdleListener;
            callMouseIdleListener();
        }

        public int getMouseIdleTime()
        {
            return mouseIdleTime;
        }

        public void setMouseIdleTime(int mouseIdleTime)
        {
            if (mouseIdleTime < 1)
            {
                throw new ArgumentOutOfRangeException("mouseIdleTime < 1");
            }
            this.mouseIdleTime = mouseIdleTime;
        }

        public int getTooltipDelay()
        {
            return tooltipDelay;
        }

        /**
         * Sets the delay in MS before the tooltip is shown
         * @param tooltipDelay the delay in MS, must be &gt;= 1.
         */
        public void setTooltipDelay(int tooltipDelay)
        {
            if (tooltipDelay < 1)
            {
                throw new ArgumentOutOfRangeException("tooltipDelay");
            }
            this.tooltipDelay = tooltipDelay;
        }

        public int getTooltipReappearDelay()
        {
            return tooltipReappearDelay;
        }

        /**
         * Sets the time window in which a new tooltip is shown after the last
         * tooltip was closed before waiting for the tooltip delay.
         * @param tooltipReappearDelay the delay in MS - set to 0 to disable
         */
        public void setTooltipReappearDelay(int tooltipReappearDelay)
        {
            this.tooltipReappearDelay = tooltipReappearDelay;
        }

        public int getTooltipOffsetX()
        {
            return tooltipOffsetX;
        }

        public int getTooltipOffsetY()
        {
            return tooltipOffsetY;
        }

        /**
         * Sets the offset from the mouse position to display the tooltip
         * @param tooltipOffsetX the X offset
         * @param tooltipOffsetY the Y offset
         */
        public void setTooltipOffset(int tooltipOffsetX, int tooltipOffsetY)
        {
            this.tooltipOffsetX = tooltipOffsetX;
            this.tooltipOffsetY = tooltipOffsetY;
        }

        /**
         * Sets set offscreen rendering delegate on the tooltip window.
         * Can be null to disable offscreen rendering.
         * 
         * @param renderOffscreen the offscreen rendering delegate.
         * @see Widget#setRenderOffscreen(de.matthiasmann.twl.Widget.RenderOffscreen) 
         */
        public void setTooltipWindowRenderOffscreen(RenderOffscreen renderOffscreen)
        {
            tooltipWindow.setRenderOffscreen(renderOffscreen);
        }

        /**
         * Changes the theme name of the tooltip window and applies and calls {@link #reapplyTheme() }
         * 
         * @param theme the new theme path element
         * @see Widget#setTheme(java.lang.String) 
         */
        public void setTooltipWindowTheme(String theme)
        {
            tooltipWindow.setTheme(theme);
            tooltipWindow.reapplyTheme();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override bool setPosition(int x, int y)
        {
            throw new NotImplementedException();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override void insertChild(Widget child, int index)
        {
            throw new NotImplementedException();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override void removeAllChildren()
        {
            throw new NotImplementedException();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override Widget removeChild(int index)
        {
            throw new NotImplementedException();
        }

        /**
         * Does nothing
         */
        //@Override
        public override void adjustSize()
        {
        }

        //@Override
        protected override void layout()
        {
            layoutChildFullInnerArea(rootPane);
        }

        //@Override
        public override void validateLayout()
        {
            if (hasInvalidLayouts)
            {
                int MAX_ITERATIONS = 1000;
                int iterations = 0;
                while (hasInvalidLayouts && iterations < MAX_ITERATIONS)
                {
                    hasInvalidLayouts = false;
                    base.validateLayout();
                    iterations++;
                }
                List<Widget> widgetsInLoop = null;
                if (hasInvalidLayouts)
                {
                    widgetsInLoop = new List<Widget>();
                    collectLayoutLoop(widgetsInLoop);
                }
                DebugHook.getDebugHook().guiLayoutValidated(iterations, widgetsInLoop);
            }
        }

        /**
         * Sets the size of the GUI based on the OpenGL viewport.
         */
        public void setSize()
        {
            setSize(renderer.Width, renderer.Height);
        }

        /**
         * Polls inputs, updates layout and renders the GUI by calls the following method:<ol>
         * <li> {@link #setSize() }
         * <li> {@link #updateTime() }
         * <li> {@link #handleInput() }
         * <li> {@link #handleKeyRepeat() }
         * <li> {@link #handleTooltips() }
         * <li> {@link #updateTimers() }
         * <li> {@link #invokeRunables() }
         * <li> {@link #validateLayout() }
         * <li> {@link #draw() }
         * <li> {@link #setCursor() }
         * </ol>
         * 
         * This is the easiest method to use this GUI.
         * 
         * <p>When not using this method care must be taken to invoke the methods
         * in the right order. See the javadoc of the individual methods for details.</p>
         */
        public void update()
        {
            setSize();
            updateTime();
            handleInput();
            handleKeyRepeat();
            handleTooltips();
            updateTimers();
            invokeRunables();
            validateLayout();
            //draw();
            setCursor();
        }

        /**
         * When calls to updateTime where stopped then this method should be called
         * before calling updateTime again to prevent a large delta jump.
         * This allows the UI timer to be suspended.
         */
        public void resyncTimerAfterPause()
        {
            this.curTime = renderer.TimeMillis;
            this.deltaTime = 0;
        }

        /**
         * Updates the current time returned by {@code getCurrentTime} by calling
         * {@link Renderer.Renderer#getTimeMillis() } and computes the delta time since the last update.
         *
         * <p>This must be called exactly <b>once</b> per frame and befiore processing
         * input events or calling {@link #updateTimers() }. See {@link #update() }
         * for the sequence in which the methods of this class should be called.</p>
         * 
         * @see #getCurrentTime()
         * @see #getTimeMillis()
         */
        public void updateTime()
        {
            long newTime = renderer.TimeMillis;
            deltaTime = Math.Max(0, (int)(newTime - curTime));
            curTime = newTime;
        }

        /**
         * Updates all active timers with the delta time computed by {@code updateTime}.
         * 
         * <p>This method must be called exactly once after a call to {@code updateTime}.</p>
         * 
         * @see #updateTime() 
         */
        public void updateTimers()
        {
            for (int i = 0; i < activeTimers.Count;)
            {
                if (!activeTimers[i].tick(deltaTime))
                {
                    activeTimers.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /**
         * Invokes all queued {@code Runnable} objects.
         * 
         * @see #invokeLater(java.lang.Runnable) 
         */
        public void invokeRunables()
        {
            Runnable[] runnables = null;
            int count;
            lock(invokeLock) {
                count = invokeLaterQueueSize;
                if (count > 0)
                {
                    invokeLaterQueueSize = 0;
                    runnables = invokeLaterQueue;
                    invokeLaterQueue = invokeRunnables;
                    invokeRunnables = runnables;
                }
            }
            for (int i = 0; i < count;)
            {
                Runnable r = runnables[i];
                runnables[i++] = null;
                try
                {
                    r.run();
                }
                catch (Exception ex)
                {
                    Logger.GetLogger(typeof(GUI)).log(Level.SEVERE, "Exception in runnable", ex);
                }
            }
        }

        /**
         * Renders all visible widgets. Calls {@code startRendering} before and
         * {@code endRendering} after rendering all widgets.
         *
         * @see Renderer.Renderer#startRendering()
         * @see Renderer.Renderer#endRendering() 
         */
        public void draw()
        {
            if (renderer.StartRendering())
            {
                try
                {
                    drawWidget(this);

                    if (dragActive && boundDragPopup == null && lastMouseDownWidget != null)
                    {
                        lastMouseDownWidget.paintDragOverlay(this,
                                evt.mouseX, evt.mouseY, evt.modifier);
                    }
                }
                finally
                {
                    renderer.EndRendering();
                }
            }
        }

        /**
         * Sets the cursor from the widget under the mouse
         *
         * <p>If the widget is disabled or did not define a cursor then
         * it's parent widget is tried. If no cursor was found the default
         * OS cursor will be displayed.</p>
         * 
         * @see Renderer.Renderer#setCursor(de.matthiasmann.twl.renderer.MouseCursor) 
         * @see Widget#getMouseCursor(de.matthiasmann.twl.Event) 
         */
        public void setCursor()
        {
            evt.type = EventType.MOUSE_MOVED;
            Widget widget = getWidgetUnderMouse();
            MouseCursor cursor = null;
            while (widget != null)
            {
                if (widget.isEnabled())
                {
                    cursor = widget.getMouseCursor(evt);
                    if (cursor != null)
                    {
                        break;
                    }
                }
                widget = widget.getParent();
            }
            if (cursor == DefaultMouseCursor.OS_DEFAULT)
            {
                cursor = null;
            }
            renderer.SetCursor(cursor);
        }

        /**
         * Polls input by calling {@link Input#pollInput(de.matthiasmann.twl.GUI) }
         * if an input source was specified, otherwise it does nothing.
         * 
         * <p>If {@code pollInput} returned false then {@link #clearKeyboardState() }
         * and {@link #clearMouseState() } are called.</p>
         * 
         * <p>If you don't want to use polled input you can easily use a push model
         * for handling input. Just call the following methods:</p><ul>
         * <li>{@link #handleKey(int, char, bool) } for every keyboard event
         * <li>{@link #handleMouse(int, int, int, bool) } for every mouse event (buttons or move)
         * <li>{@link #handleMouseWheel(int) } for any mouse wheel event
         * </ul> These metods (including this one) needs to be called after {@link #updateTime() }
         */
        public void handleInput()
        {
            if (input != null && !input.PollInput(this))
            {
                clearKeyboardState();
                clearMouseState();
            }
        }

        /**
         * Mouse has moved / button was pressed or released.
         * 
         * @param mouseX the new mouse X coordinate
         * @param mouseY the new mouse Y coordinate
         * @param button the button that has been pressed/released or -1 if no button changed
         * @param pressed true if the button was pressed. Ignored if button is -1.
         * @return true if the event was handled by a widget
         */
        public bool handleMouse(int mouseX, int mouseY, int button, bool pressed)
        {
            mouseEventTime = curTime;
            tooltipEventTime = curTime;
            evt.mouseButton = button;

            // only the previously pressed mouse button
            int prevButtonState = evt.modifier & Event.MODIFIER_BUTTON;

            int buttonMask = 0;
            switch (button)
            {
                case Event.MOUSE_LBUTTON:
                    buttonMask = Event.MODIFIER_LBUTTON;
                    break;
                case Event.MOUSE_RBUTTON:
                    buttonMask = Event.MODIFIER_RBUTTON;
                    break;
                case Event.MOUSE_MBUTTON:
                    buttonMask = Event.MODIFIER_MBUTTON;
                    break;
            }
            evt.setModifier(buttonMask, pressed);
            bool wasPressed = (prevButtonState & buttonMask) != 0;

            if (buttonMask != 0)
            {
                renderer.SetMouseButton(button, pressed);
            }

            // don't send new mouse coords when still in drag area
            if (dragActive || prevButtonState == 0)
            {
                evt.mouseX = mouseX;
                evt.mouseY = mouseY;
            }
            else
            {
                evt.mouseX = mouseDownX;
                evt.mouseY = mouseDownY;
            }

            bool handled = dragActive;

            if (!dragActive)
            {
                if (!isInside(mouseX, mouseY))
                {
                    pressed = false;
                    mouseClickCount = 0;
                    if (wasInside)
                    {
                        sendMouseEvent(EventType.MOUSE_EXITED, null);
                        wasInside = false;
                    }
                }
                else if (!wasInside)
                {
                    wasInside = true;
                    if (sendMouseEvent(EventType.MOUSE_ENTERED, null) != null)
                    {
                        handled = true;
                    }
                }
            }

            if (mouseX != mouseLastX || mouseY != mouseLastY)
            {
                mouseLastX = mouseX;
                mouseLastY = mouseY;

                if (prevButtonState != 0 && !dragActive)
                {
                    if (Math.Abs(mouseX - mouseDownX) > DRAG_DIST ||
                        Math.Abs(mouseY - mouseDownY) > DRAG_DIST)
                    {
                        dragActive = true;
                        mouseClickCount = 0;
                        // close the tooltip - it may interface with dragging
                        hideTooltip();
                        hadOpenTooltip = false;
                        // grab the tooltip to prevent it from poping up while dragging
                        // the widget can still request a tooltip update
                        tooltipOwner = lastMouseDownWidget;
                    }
                }

                if (dragActive)
                {
                    if (boundDragPopup != null)
                    {
                        // a bound drag is converted to a mouse move
                        System.Diagnostics.Debug.Assert(getTopPane() == boundDragPopup);
                        sendMouseEvent(EventType.MOUSE_MOVED, null);
                    }
                    else if (lastMouseDownWidget != null)
                    {
                        // send MOUSE_DRAGGED only to the widget which received the MOUSE_BTNDOWN
                        sendMouseEvent(EventType.MOUSE_DRAGGED, lastMouseDownWidget);
                    }
                }
                else if (prevButtonState == 0)
                {
                    if (sendMouseEvent(EventType.MOUSE_MOVED, null) != null)
                    {
                        handled = true;
                    }
                }
            }

            if (buttonMask != 0 && pressed != wasPressed)
            {
                if (pressed)
                {
                    if (dragButton < 0)
                    {
                        mouseDownX = mouseX;
                        mouseDownY = mouseY;
                        dragButton = button;
                        lastMouseDownWidget = sendMouseEvent(EventType.MOUSE_BTNDOWN, null);
                    }
                    else if (lastMouseDownWidget != null && boundDragPopup == null)
                    {
                        // if another button is pressed while one button is already
                        // pressed then route the second button to the widget which
                        // received the first press
                        // but only when no bound drag is active
                        sendMouseEvent(EventType.MOUSE_BTNDOWN, lastMouseDownWidget);
                    }
                }
                else if (dragButton >= 0 && (boundDragPopup == null || evt.isMouseDragEnd()))
                {
                    // only send the last MOUSE_BTNUP event when a bound drag is active
                    if (boundDragPopup != null)
                    {
                        if (button == dragButton)
                        {
                            // for bound drag the MOUSE_BTNUP is first send to the current widget under the mouse
                            sendMouseEvent(EventType.MOUSE_BTNUP, getWidgetUnderMouse());
                        }
                    }
                    if (lastMouseDownWidget != null)
                    {
                        // send MOUSE_BTNUP only to the widget which received the MOUSE_BTNDOWN
                        sendMouseEvent(EventType.MOUSE_BTNUP, lastMouseDownWidget);
                    }
                }

                if (lastMouseDownWidget != null)
                {
                    handled = true;
                }

                if (button == Event.MOUSE_LBUTTON && !popupEventOccured)
                {
                    if (!pressed && !dragActive)
                    {
                        if (mouseClickCount == 0 ||
                                curTime - mouseClickedTime > DBLCLICK_TIME ||
                                lastMouseClickWidget != lastMouseDownWidget)
                        {
                            mouseClickedX = mouseX;
                            mouseClickedY = mouseY;
                            lastMouseClickWidget = lastMouseDownWidget;
                            mouseClickCount = 0;
                            mouseClickedTime = curTime;
                        }
                        if (Math.Abs(mouseX - mouseClickedX) < DRAG_DIST &&
                                Math.Abs(mouseY - mouseClickedY) < DRAG_DIST)
                        {
                            // ensure same click target as first
                            evt.mouseX = mouseClickedX;
                            evt.mouseY = mouseClickedY;
                            evt.mouseClickCount = ++mouseClickCount;
                            mouseClickedTime = curTime;
                            if (lastMouseClickWidget != null)
                            {
                                sendMouseEvent(EventType.MOUSE_CLICKED, lastMouseClickWidget);
                            }
                        }
                        else
                        {
                            lastMouseClickWidget = null;
                        }
                    }
                }
            }

            if (evt.isMouseDragEnd())
            {
                if (dragActive)
                {
                    dragActive = false;
                    sendMouseEvent(EventType.MOUSE_MOVED, null);
                }
                dragButton = -1;
                if (boundDragCallback != null)
                {
                    try
                    {
                        boundDragCallback.run();
                    }
                    catch (Exception ex)
                    {
                        Logger.GetLogger(typeof(GUI)).log(Level.SEVERE,
                                "Exception in bound drag callback", ex);
                    }
                    finally
                    {
                        boundDragCallback = null;
                        boundDragPopup = null;
                    }
                }
            }

            return handled;
        }

        /**
         * Clears current mouse button & drag state.
         *
         * Should be called when the Display is minimized or when mouse events are
         * handled outside of TWL.
         */
        public void clearMouseState()
        {
            evt.setModifier(Event.MODIFIER_LBUTTON, false);
            evt.setModifier(Event.MODIFIER_MBUTTON, false);
            evt.setModifier(Event.MODIFIER_RBUTTON, false);
            renderer.SetMouseButton(Event.MOUSE_LBUTTON, false);
            renderer.SetMouseButton(Event.MOUSE_MBUTTON, false);
            renderer.SetMouseButton(Event.MOUSE_RBUTTON, false);
            lastMouseClickWidget = null;
            mouseClickCount = 0;
            mouseClickedTime = curTime;
            boundDragPopup = null;
            boundDragCallback = null;
            if (dragActive)
            {
                dragActive = false;
                sendMouseEvent(EventType.MOUSE_MOVED, null);
            }
            dragButton = -1;
        }

        /**
         * Mouse wheel has been turned. Must be called after handleMouse.
         * 
         * @param wheelDelta the normalized wheel delta
         * @return true if the event was handled by a widget
         */
        public bool handleMouseWheel(int wheelDelta)
        {
            evt.mouseWheelDelta = wheelDelta;
            bool handled = sendMouseEvent(EventType.MOUSE_WHEEL,
                    dragActive ? lastMouseDownWidget : null) != null;
            evt.mouseWheelDelta = 0;
            return handled;
        }

        /**
         * A key was pressed or released. Keyboard events depend on the constants
         * of LWJGL's Keybaord class.
         *
         * Repeated key presses should be handled by {@code handleKeyRepeat} and not this
         * method so that the repeated flag is set correctly for the generated events.
         * 
         * @param keyCode the key code for this key or {@code Keyboard.KEY_NONE}
         * @param keyChar the unicode character resulting from this event or {@code Keyboard.CHAR_NONE}
         * @param pressed true if the key was pressed and false if it was released
         * @return true if the event was handled by a widget
         */
        public bool handleKey(int keyCode, char keyChar, bool pressed)
        {
            evt.keyCode = keyCode;
            evt.keyChar = keyChar;
            evt.keyRepeated = false;

            keyEventTime = curTime;
            if (evt.keyCode != Event.KEY_NONE || evt.keyChar != Event.CHAR_NONE)
            {
                evt.setModifiers(pressed);

                if (pressed)
                {
                    keyRepeatDelay = KEYREPEAT_INITIAL_DELAY;
                    return sendKeyEvent(EventType.KEY_PRESSED);
                }
                else
                {
                    keyRepeatDelay = NO_REPEAT;
                    return sendKeyEvent(EventType.KEY_RELEASED);
                }
            }
            else
            {
                keyRepeatDelay = NO_REPEAT;
            }

            return false;
        }

        /**
         * Clears current keyboard modifiers.
         *
         * Should be called when the Display is minimized or when keyboard events are
         * handled outside of TWL.
         */
        public void clearKeyboardState()
        {
            evt.modifier &= ~(Event.MODIFIER_ALT | Event.MODIFIER_CTRL | Event.MODIFIER_SHIFT | Event.MODIFIER_META);
            keyRepeatDelay = NO_REPEAT;

            evt.type = EventType.CLEAR_KEYBOARD_STATE;
            routePopupEvent(evt);
        }

        /**
         * Must be called after calling handleKey().
         *
         * This method checks the time since the last key event and causes a repeated
         * key press event to be generated.
         * 
         * @see #handleKey(int, char, bool) 
         */
        public void handleKeyRepeat()
        {
            if (keyRepeatDelay != NO_REPEAT)
            {
                long keyDeltaTime = curTime - keyEventTime;
                if (keyDeltaTime > keyRepeatDelay)
                {
                    keyEventTime = curTime;
                    keyRepeatDelay = KEYREPEAT_INTERVAL_DELAY;
                    evt.keyRepeated = true;
                    sendKeyEvent(EventType.KEY_PRESSED);  // refire last key event
                }
            }
        }

        /**
         * Must be called after calling handleMouse or handleMouseWheel.
         *
         * This method displays a tooltip if the widget under mouse has a tooltip
         * message and the mouse has not moved for a certain amount of time.
         * 
         * @see #handleMouse(int, int, int, bool) 
         * @see #handleMouseWheel(int)
         */
        public void handleTooltips()
        {
            Widget widgetUnderMouse = getWidgetUnderMouse();
            if (widgetUnderMouse != tooltipOwner)
            {
                if (widgetUnderMouse != null && (
                        ((curTime - tooltipEventTime) > tooltipDelay) ||
                        (hadOpenTooltip && (curTime - tooltipClosedTime) < tooltipReappearDelay)))
                {
                    setTooltip(
                            evt.mouseX + tooltipOffsetX,
                            evt.mouseY + tooltipOffsetY,
                            widgetUnderMouse,
                            widgetUnderMouse.getTooltipContentAt(evt.mouseX, evt.mouseY),
                            Alignment.BOTTOMLEFT);
                }
                else
                {
                    hideTooltip();
                }
            }

            bool mouseIdle = (curTime - mouseEventTime) > mouseIdleTime;
            if (mouseIdleState != mouseIdle)
            {
                mouseIdleState = mouseIdle;
                callMouseIdleListener();
            }
        }

        private Widget getTopPane()
        {
            // don't use potential overwritten methods
            return base.getChild(base.getNumChildren() - 3);
        }

        //@Override
        internal override Widget getWidgetUnderMouse()
        {
            return getTopPane().getWidgetUnderMouse();
        }

        private Widget sendMouseEvent(EventType type, Widget target)
        {
            System.Diagnostics.Debug.Assert(type.isMouseEvent);
            popupEventOccured = false;
            evt.type = type;
            evt.dragEvent = dragActive && (boundDragPopup == null);

            renderer.SetMousePosition(evt.mouseX, evt.mouseY);

            if (target != null)
            {
                if (target.isEnabled() || !isMouseAction(evt))
                {
                    target.handleEvent(target.translateMouseEvent(evt));
                }
                return target;
            }
            else
            {
                System.Diagnostics.Debug.Assert(!dragActive || boundDragPopup != null);
                Widget widget = null;
                if (activeInfoWindow != null)
                {
                    if (activeInfoWindow.isMouseInside(evt) && setMouseOverChild(activeInfoWindow, evt))
                    {
                        widget = activeInfoWindow;
                    }
                }
                if (widget == null)
                {
                    widget = getTopPane();
                    setMouseOverChild(widget, evt);
                }
                return widget.routeMouseEvent(evt);
            }
        }

        private static int FOCUS_KEY = Event.KEY_TAB;

        bool isFocusKey()
        {
            return evt.keyCode == FOCUS_KEY &&
                        ((evt.modifier & (Event.MODIFIER_CTRL | Event.MODIFIER_META | Event.MODIFIER_ALT)) == 0);
        }

        internal void setFocusKeyWidget(Widget widget)
        {
            if (focusKeyWidget == null && isFocusKey())
            {
                focusKeyWidget = widget;
            }
        }

        private bool sendKeyEvent(EventType type)
        {
            System.Diagnostics.Debug.Assert(type.isKeyEvent);
            popupEventOccured = false;
            focusKeyWidget = null;
            evt.type = type;
            evt.dragEvent = false;
            bool handled = getTopPane().handleEvent(evt);
            if (!handled && focusKeyWidget != null)
            {
                focusKeyWidget.handleFocusKeyEvent(evt);
                handled = true;
            }
            focusKeyWidget = null;  // allow GC
            return handled;
        }

        private void sendPopupEvent(EventType type)
        {
            System.Diagnostics.Debug.Assert(type == EventType.POPUP_OPENED || type == EventType.POPUP_CLOSED);
            popupEventOccured = false;
            evt.type = type;
            evt.dragEvent = false;
            try
            {
                getTopPane().routePopupEvent(evt);
            }
            catch (Exception ex)
            {
                Logger.GetLogger(typeof(GUI)).log(Level.SEVERE, "Exception in sendPopupEvent()", ex);
            }
        }

        protected internal void resendLastMouseMove()
        {
            if (!dragActive)
            {
                sendMouseEvent(EventType.MOUSE_MOVED, null);
            }
        }

        internal void openPopup(PopupWindow popup)
        {
            if (popup.getParent() == this)
            {
                closePopup(popup);
            }
            else if (popup.getParent() != null)
            {
                throw new ArgumentOutOfRangeException("popup must not be added anywhere");
            }
            hideTooltip();
            hadOpenTooltip = false;
            sendPopupEvent(EventType.POPUP_OPENED);
            base.insertChild(popup, getNumChildren() - 2);
            popup.getOwner().setOpenPopup(this, true);
            popupEventOccured = true;
            if (activeInfoWindow != null)
            {
                closeInfo(activeInfoWindow);
            }
        }

        internal void closePopup(PopupWindow popup)
        {
            if (boundDragPopup == popup)
            {
                boundDragPopup = null;
            }
            int idx = getChildIndex(popup);
            if (idx > 0)
            {
                base.removeChild(idx);
            }
            popup.getOwner().recalcOpenPopups(this);
            sendPopupEvent(EventType.POPUP_CLOSED);
            popupEventOccured = true;
            closeInfoFromWidget(popup);
            requestKeyboardFocus(getTopPane());
            resendLastMouseMove();
        }

        internal bool hasOpenPopups(Widget owner)
        {
            for (int i = getNumChildren() - 2; i-- > 1;)
            {
                PopupWindow popup = (PopupWindow)getChild(i);
                if (popup.getOwner() == owner)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isOwner(Widget owner, Widget widget)
        {
            while (owner != null && owner != widget)
            {
                owner = owner.getParent();
            }
            return owner == widget;
        }

        internal void closePopupFromWidgets(Widget widget)
        {
            for (int i = getNumChildren() - 2; i-- > 1;)
            {
                PopupWindow popup = (PopupWindow)getChild(i);
                if (isOwner(popup.getOwner(), widget))
                {
                    closePopup(popup);
                }
            }
        }

        void closeIfPopup(Widget widget)
        {
            if (widget is PopupWindow) {
                closePopup((PopupWindow)widget);
            }
        }

        internal bool bindDragEvent(PopupWindow popup, Runnable cb)
        {
            if (boundDragPopup == null && getTopPane() == popup && dragButton >= 0 && !isOwner(lastMouseDownWidget, popup))
            {
                dragActive = true;
                boundDragPopup = popup;
                boundDragCallback = cb;
                sendMouseEvent(EventType.MOUSE_MOVED, null);
                return true;
            }
            return false;
        }

        internal void widgetHidden(Widget widget)
        {
            closeIfPopup(widget);
            closePopupFromWidgets(widget);
            if (isOwner(tooltipOwner, widget))
            {
                hideTooltip();
                hadOpenTooltip = false;
            }
            closeInfoFromWidget(widget);
        }

        internal void widgetDisabled(Widget widget)
        {
            closeIfPopup(widget);
            closeInfoFromWidget(widget);
        }

        void closeInfoFromWidget(Widget widget)
        {
            if (activeInfoWindow != null)
            {
                if (activeInfoWindow == widget ||
                        isOwner(activeInfoWindow.getOwner(), widget))
                {
                    closeInfo(activeInfoWindow);
                }
            }
        }

        internal void openInfo(InfoWindow info)
        {
            int idx = getNumChildren() - 2;
            base.removeChild(idx);
            base.insertChild(info, idx);
            activeInfoWindow = info;
        }

        internal void closeInfo(InfoWindow info)
        {
            if (info == activeInfoWindow)
            {
                int idx = getNumChildren() - 2;
                base.removeChild(idx);
                base.insertChild(infoWindowPlaceholder, idx);
                activeInfoWindow = null;
                try
                {
                    info.infoWindowClosed();
                }
                catch (Exception ex)
                {
                    Logger.GetLogger(typeof(GUI)).log(Level.SEVERE, "Exception in infoWindowClosed()", ex);
                }
            }
        }

        //@Override
        public override bool requestKeyboardFocus()
        {
            // GUI always has the keyboard focus
            return true;
        }

        //@Override
        protected override bool requestKeyboardFocus(Widget child)
        {
            if (child != null)
            {
                if (child != getTopPane())
                {
                    return false;
                }
            }
            return base.requestKeyboardFocus(child);
        }

        internal void requestTooltipUpdate(Widget widget, bool resetToolTipTimer)
        {
            if (tooltipOwner == widget)
            {
                tooltipOwner = null;
                if (resetToolTipTimer)
                {
                    hideTooltip();
                    hadOpenTooltip = false;
                    tooltipEventTime = curTime;
                }
            }
        }

        private void hideTooltip()
        {
            if (tooltipWindow.isVisible())
            {
                tooltipClosedTime = curTime;
                hadOpenTooltip = true;
            }
            tooltipWindow.setVisible(false);
            tooltipOwner = null;

            // remove tooltip widget if it's not our label
            if (tooltipLabel.getParent() != tooltipWindow)
            {
                tooltipWindow.removeAllChildren();
            }
        }

        private void setTooltip(int x, int y, Widget widget, Object content, Alignment alignment)
        {
            if (content == null)
            {
                hideTooltip();
                return;
            }

            if (content is String) {
                String text = (String)content;
                if (text.Length == 0)
                {
                    hideTooltip();
                    return;
                }
                if (tooltipLabel.getParent() != tooltipWindow)
                {
                    tooltipWindow.removeAllChildren();
                    tooltipWindow.add(tooltipLabel);
                }
                tooltipLabel.setBackground(null);
                tooltipLabel.setText(text);
            } else if (content is Widget) {
                Widget tooltipWidget = (Widget)content;
                if (tooltipWidget.getParent() != null && tooltipWidget.getParent() != tooltipWindow)
                {
                    throw new ArgumentOutOfRangeException("Content widget must not be added to another widget");
                }
                tooltipWindow.removeAllChildren();
                tooltipWindow.add(tooltipWidget);
            } else
            {
                throw new ArgumentOutOfRangeException("Unsupported data type");
            }

            tooltipWindow.adjustSize();

            // some Widgets (esp TextArea) have complex sizing policy
            // give them a 2nd chance
            if (tooltipWindow.isLayoutInvalid())
            {
                tooltipWindow.adjustSize();
            }

            int ttWidth = tooltipWindow.getWidth();
            int ttHeight = tooltipWindow.getHeight();

            if (alignment == Alignment.TOP || alignment == Alignment.CENTER || alignment == Alignment.BOTTOM)
            {
                x -= ttWidth / 2;
            }
            else if (alignment == Alignment.TOPRIGHT || alignment == Alignment.RIGHT || alignment == Alignment.BOTTOMRIGHT)
            {
                x -= ttWidth;
            }

            if (alignment == Alignment.LEFT || alignment == Alignment.CENTER || alignment == Alignment.RIGHT)
            {
                y -= ttHeight / 2;
            }
            else if (alignment == Alignment.BOTTOMLEFT || alignment == Alignment.BOTTOM || alignment == Alignment.BOTTOMRIGHT)
            {
                y -= ttHeight;
            }

            if (x + ttWidth > getWidth())
            {
                x = getWidth() - ttWidth;
            }
            if (y + ttHeight > getHeight())
            {
                y = getHeight() - ttHeight;
            }
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }

            tooltipOwner = widget;
            tooltipWindow.setPosition(x, y);
            tooltipWindow.setVisible(true);
        }

        private void callMouseIdleListener()
        {
            if (mouseIdleListener != null)
            {
                if (mouseIdleState)
                {
                    mouseIdleListener.mouseEnterIdle();
                }
                else
                {
                    mouseIdleListener.mouseExitIdle();
                }
            }
        }

        private void growInvokeLaterQueue()
        {
            Runnable[] tmp = new Runnable[invokeLaterQueueSize * 2];
            Array.Copy(invokeLaterQueue, 0, tmp, 0, invokeLaterQueueSize);
            invokeLaterQueue = tmp;
        }

        class TooltipWindow : Container
        {
            public static StateKey STATE_FADE = StateKey.Get("fade");
            private int fadeInTime;

            //@Override
            protected override void applyTheme(ThemeInfo themeInfo)
            {
                base.applyTheme(themeInfo);
                fadeInTime = themeInfo.getParameter("fadeInTime", 0);
            }

            //@Override
            public override void setVisible(bool visible)
            {
                base.setVisible(visible);
                getAnimationState().resetAnimationTime(STATE_FADE);
            }

            //@Override
            protected override void paint(GUI gui)
            {
                int time = getAnimationState().GetAnimationTime(STATE_FADE);
                if (time < fadeInTime)
                {
                    float alpha = time / (float)fadeInTime;
                    gui.getRenderer().PushGlobalTintColor(1f, 1f, 1f, alpha);
                    try
                    {
                        base.paint(gui);
                    }
                    finally
                    {
                        gui.getRenderer().PopGlobalTintColor();
                    }
                }
                else
                {
                    base.paint(gui);
                }
            }
        }

        /*class AC<V> : Runnable
        {
            private Delegate jobC;
            private Runnable jobR;
            private AsyncCompletionListener<V> listener;
            private V result;
            private Exception exception;
            private GUI gui;

            AC(GUI gui, Delegate jobC, Runnable jobR, AsyncCompletionListener<V> listener)
            {
                this.jobC = jobC;
                this.jobR = jobR;
                this.listener = listener;
                this.gui = gui;
            }

            public V call()
            {
                try
                {
                    if (jobC != null)
                    {
                        result = (V) jobC.DynamicInvoke();
                    }
                    else
                    {
                        jobR.run();
                    }
                    this.gui.invokeLater(this);
                    return result;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    this.gui.invokeLater(this);
                    throw ex;
                }
            }

            public override void run()
            {
                if (exception != null)
                {
                    listener.failed(exception);
                }
                else
                {
                    listener.completed(result);
                }
            }
        }*/

        /*class TF
        {
            static int poolNumber = 1;
            int threadNumber = 1;
            String prefix;

            TF()
            {
                this.prefix = "GUI-" + poolNumber++ + "-invokeAsync-";
            }

            public Thread newThread(Runnable r)
            {
                Thread t = new Thread(new ThreadStart(r.run));
                t.Name = prefix + threadNumber++;
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal;
                return t;
            }
        }*/
    }
}
