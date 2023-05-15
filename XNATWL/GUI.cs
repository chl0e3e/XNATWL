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
using Microsoft.Xna.Framework;

namespace XNATWL
{
    public class GUI : Widget
    {
        public interface MouseIdleListener
        {
            void MouseEnterIdle();
            void MouseExitIdle();
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
            void Completed(V result);

            /**
             * The job has failed with an exception
             *
             * @param ex the exception thrown by the async job
             */
            void Failed(Exception ex);
        }

        private static int DRAG_DIST = 3;
        private static int DBLCLICK_TIME = 500;   // ms
        private static int KEYREPEAT_INITIAL_DELAY = 250; // ms
        private static int KEYREPEAT_INTERVAL_DELAY = 1000 / 30;    // ms
        private static int NO_REPEAT = 0;

        private int _tooltipOffsetX = 0;
        private int _tooltipOffsetY = 0;
        private int _tooltipDelay = 1000;  // 1 sec in ms
        private int _tooltipReappearDelay = 100;

        private Renderer.Renderer _renderer;
        private Input.Input _input;

        internal long _curTime;
        private int _deltaTime;

        private Widget _rootPane;
        internal bool _hasInvalidLayouts;

        Event _evt;
        private bool _wasInside;
        private bool _dragActive;
        private int _mouseClickCount;
        private int _dragButton = -1;
        private int _mouseDownX;
        private int _mouseDownY;
        private int _mouseLastX;
        private int _mouseLastY;
        private int _mouseClickedX;
        private int _mouseClickedY;
        private long _mouseEventTime;
        private long _tooltipEventTime;
        private long _mouseClickedTime;
        private long _keyEventTime;
        private int _keyRepeatDelay;
        private bool _popupEventOccurred;
        private Widget _lastMouseDownWidget;
        private Widget _lastMouseClickWidget;
        private PopupWindow _boundDragPopup;
        private Runnable _boundDragCallback;
        private Widget _focusKeyWidget;

        private int _mouseIdleTime = 60;
        private bool _mouseIdleState;
        private MouseIdleListener _mouseIdleListener;

        private InfoWindow _activeInfoWindow;
        private Widget _infoWindowPlaceholder;

        private TooltipWindow _tooltipWindow;
        private Label _tooltipLabel;
        private Widget _tooltipOwner;
        private bool _hadOpenTooltip;
        private long _tooltipClosedTime;

        internal List<Timer> _activeTimers;
        //ExecutorService executorService;

        private Object _invokeLock;
        private Runnable[] _invokeLaterQueue;
        private int _invokeLaterQueueSize;
        private Runnable[] _invokeRunnables;

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
            _rootPane.SetTheme("");
            _rootPane.SetFocusKeyEnabled(false);
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

            this._guiInstance = this;
            this._renderer = renderer;
            this._input = input;
            this._evt = new Event();
            this._rootPane = rootPane;
            this._rootPane.SetFocusKeyEnabled(false);

            this._infoWindowPlaceholder = new Widget();
            this._infoWindowPlaceholder.SetTheme("");

            this._tooltipLabel = new Label();
            this._tooltipWindow = new TooltipWindow();
            this._tooltipWindow.SetVisible(false);

            this._activeTimers = new List<Timer>();
            //this.executorService = Executors.newSingleThreadExecutor(new TF());    // thread creatation is lazy
            this._invokeLock = new Object();
            this._invokeLaterQueue = new Runnable[16];
            this._invokeRunnables = new Runnable[16];

            SetTheme("");
            SetFocusKeyEnabled(false);
            SetSize();

            // insert rootPane (user provided class) last incase it invokes methods
            // which access GUI state (like requestKeyboardFocus) in overridable
            // methods (like afterAddToGUI)
            base.InsertChild(_infoWindowPlaceholder, 0);
            base.InsertChild(_tooltipWindow, 1);
            base.InsertChild(rootPane, 0);

            ResyncTimerAfterPause();
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
        public override void ApplyTheme(ThemeManager themeManager)
        {
            if (themeManager == null)
            {
                throw new ArgumentOutOfRangeException("themeManager is null");
            }

            base.ApplyTheme(themeManager);
        }

        public Widget GetRootPane()
        {
            return _rootPane;
        }

        public void SetRootPane(Widget rootPane)
        {
            if (rootPane == null)
            {
                throw new ArgumentOutOfRangeException("rootPane is null");
            }
            this._rootPane = rootPane;
            base.RemoveChild(0);
            base.InsertChild(rootPane, 0);
        }

        public Renderer.Renderer GetRenderer()
        {
            return _renderer;
        }

        public Input.Input GetInput()
        {
            return _input;
        }

        public MouseSensitiveRectangle CreateMouseSenitiveRectangle()
        {
            return new GUIMouseSensitiveRectangle(this);
        }

        class GUIMouseSensitiveRectangle : MouseSensitiveRectangle
        {
            private GUI _gui;

            public GUIMouseSensitiveRectangle(GUI gui) : base()
            {
                this._gui = gui;
            }

            public override bool IsMouseOver()
            {
                return IsInside(this._gui._evt._mouseX, this._gui._evt._mouseY);
            }
        }

        /**
         * Creates a new UI timer.
         * @return new Timer(this)
         */
        public Timer CreateTimer()
        {
            return new Timer(this);
        }

        /**
         * Returns the current UI time in milliseconds.
         * This time is updated via {@link #updateTime() }
         *
         * @return the current UI time in milliseconds.
         */
        public long GetCurrentTime()
        {
            return _curTime;
        }

        /**
         * Returns the delta time to the previous frame in milliseconds.
         * This time is updated via {@link #updateTime() }
         * 
         * @return the delta time
         */
        public int GetCurrentDeltaTime()
        {
            return _deltaTime;
        }

        /**
         * Queues a Runnable to be executed in the GUI main loop.
         * This method is thread safe.
         * 
         * @param runnable  the Runnable to execute
         * @see Widget#getGUI()
         */
        public void InvokeLater(Runnable runnable)
        {
            if (runnable == null)
            {
                throw new ArgumentOutOfRangeException("runnable is null");
            }

            lock (_invokeLock)
            {
                if (_invokeLaterQueueSize == _invokeLaterQueue.Length)
                {
                    GrowInvokeLaterQueue();
                }
                _invokeLaterQueue[_invokeLaterQueueSize++] = runnable;
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

        public bool RequestToolTip(Widget widget, int x, int y,
                Object content, Alignment alignment)
        {
            if (alignment == null)
            {
                throw new ArgumentOutOfRangeException("alignment is null");
            }

            if (widget == GetWidgetUnderMouse())
            {
                SetTooltip(x, y, widget, content, alignment);
                return true;
            }

            return false;
        }

        public MouseIdleListener GetMouseIdleListener()
        {
            return _mouseIdleListener;
        }

        public void SetMouseIdleListener(MouseIdleListener mouseIdleListener)
        {
            this._mouseIdleListener = mouseIdleListener;
            CallMouseIdleListener();
        }

        public int GetMouseIdleTime()
        {
            return _mouseIdleTime;
        }

        public void SetMouseIdleTime(int mouseIdleTime)
        {
            if (mouseIdleTime < 1)
            {
                throw new ArgumentOutOfRangeException("mouseIdleTime < 1");
            }
            this._mouseIdleTime = mouseIdleTime;
        }

        public int GetTooltipDelay()
        {
            return _tooltipDelay;
        }

        /**
         * Sets the delay in MS before the tooltip is shown
         * @param tooltipDelay the delay in MS, must be &gt;= 1.
         */
        public void SetTooltipDelay(int tooltipDelay)
        {
            if (tooltipDelay < 1)
            {
                throw new ArgumentOutOfRangeException("tooltipDelay");
            }
            this._tooltipDelay = tooltipDelay;
        }

        public int GetTooltipReappearDelay()
        {
            return _tooltipReappearDelay;
        }

        /**
         * Sets the time window in which a new tooltip is shown after the last
         * tooltip was closed before waiting for the tooltip delay.
         * @param tooltipReappearDelay the delay in MS - set to 0 to disable
         */
        public void SetTooltipReappearDelay(int tooltipReappearDelay)
        {
            this._tooltipReappearDelay = tooltipReappearDelay;
        }

        public int GetTooltipOffsetX()
        {
            return _tooltipOffsetX;
        }

        public int GetTooltipOffsetY()
        {
            return _tooltipOffsetY;
        }

        /**
         * Sets the offset from the mouse position to display the tooltip
         * @param tooltipOffsetX the X offset
         * @param tooltipOffsetY the Y offset
         */
        public void SetTooltipOffset(int tooltipOffsetX, int tooltipOffsetY)
        {
            this._tooltipOffsetX = tooltipOffsetX;
            this._tooltipOffsetY = tooltipOffsetY;
        }

        /**
         * Sets set offscreen rendering delegate on the tooltip window.
         * Can be null to disable offscreen rendering.
         * 
         * @param renderOffscreen the offscreen rendering delegate.
         * @see Widget#setRenderOffscreen(de.matthiasmann.twl.Widget.RenderOffscreen) 
         */
        public void SetTooltipWindowRenderOffscreen(RenderOffscreen renderOffscreen)
        {
            _tooltipWindow.SetRenderOffscreen(renderOffscreen);
        }

        /**
         * Changes the theme name of the tooltip window and applies and calls {@link #reapplyTheme() }
         * 
         * @param theme the new theme path element
         * @see Widget#setTheme(java.lang.String) 
         */
        public void SetTooltipWindowTheme(String theme)
        {
            _tooltipWindow.SetTheme(theme);
            _tooltipWindow.ReapplyTheme();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override bool SetPosition(int x, int y)
        {
            throw new NotImplementedException();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override void InsertChild(Widget child, int index)
        {
            throw new NotImplementedException();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override void RemoveAllChildren()
        {
            throw new NotImplementedException();
        }

        /**
         * Throws UnsupportedOperationException
         * @throws UnsupportedOperationException always
         */
        //@Override
        public override Widget RemoveChild(int index)
        {
            throw new NotImplementedException();
        }

        /**
         * Does nothing
         */
        //@Override
        public override void AdjustSize()
        {
        }

        //@Override
        protected override void Layout()
        {
            LayoutChildFullInnerArea(_rootPane);
        }

        //@Override
        public override void ValidateLayout()
        {
            if (_hasInvalidLayouts)
            {
                int MAX_ITERATIONS = 1000;
                int iterations = 0;
                while (_hasInvalidLayouts && iterations < MAX_ITERATIONS)
                {
                    _hasInvalidLayouts = false;
                    base.ValidateLayout();
                    iterations++;
                }
                List<Widget> widgetsInLoop = null;
                if (_hasInvalidLayouts)
                {
                    widgetsInLoop = new List<Widget>();
                    CollectLayoutLoop(widgetsInLoop);
                }
                DebugHook.getDebugHook().GuiLayoutValidated(iterations, widgetsInLoop);
            }
        }

        /**
         * Sets the size of the GUI based on the OpenGL viewport.
         */
        public void SetSize()
        {
            SetSize(_renderer.Width, _renderer.Height);
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
        public void Update(GameTime gameTime)
        {
            SetSize();
            UpdateTime(gameTime);
            HandleInput();
            HandleKeyRepeat();
            HandleTooltips();
            UpdateTimers();
            InvokeRunables();
            ValidateLayout();
            //draw();
            SetCursor();
        }

        /**
         * When calls to updateTime where stopped then this method should be called
         * before calling updateTime again to prevent a large delta jump.
         * This allows the UI timer to be suspended.
         */
        public void ResyncTimerAfterPause()
        {
            this._curTime = _renderer.TimeMillis;
            this._deltaTime = 0;
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
        public void UpdateTime(GameTime gameTime)
        {
            long newTime = (long)gameTime.TotalGameTime.TotalMilliseconds;
            _deltaTime = Math.Max(0, (int)(newTime - _curTime));
            _curTime = newTime;
        }

        /**
         * Updates all active timers with the delta time computed by {@code updateTime}.
         * 
         * <p>This method must be called exactly once after a call to {@code updateTime}.</p>
         * 
         * @see #updateTime() 
         */
        public void UpdateTimers()
        {
            for (int i = 0; i < _activeTimers.Count;)
            {
                if (!_activeTimers[i].RunOneTick(_deltaTime))
                {
                    _activeTimers.RemoveAt(i);
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
        public void InvokeRunables()
        {
            Runnable[] runnables = null;
            int count;
            lock (_invokeLock)
            {
                count = _invokeLaterQueueSize;
                if (count > 0)
                {
                    _invokeLaterQueueSize = 0;
                    runnables = _invokeLaterQueue;
                    _invokeLaterQueue = _invokeRunnables;
                    _invokeRunnables = runnables;
                }
            }
            for (int i = 0; i < count;)
            {
                Runnable r = runnables[i];
                runnables[i++] = null;
                try
                {
                    r.Run();
                }
                catch (Exception ex)
                {
                    Logger.GetLogger(typeof(GUI)).Log(Level.SEVERE, "Exception in runnable", ex);
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
        public void Draw()
        {
            if (_renderer.StartRendering())
            {
                try
                {
                    DrawWidget(this);

                    if (_dragActive && _boundDragPopup == null && _lastMouseDownWidget != null)
                    {
                        _lastMouseDownWidget.PaintDragOverlay(this,
                                _evt._mouseX, _evt._mouseY, _evt._modifier);
                    }
                }
                finally
                {
                    _renderer.EndRendering();
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
        public void SetCursor()
        {
            _evt._type = EventType.MOUSE_MOVED;
            Widget widget = GetWidgetUnderMouse();
            MouseCursor cursor = null;
            while (widget != null)
            {
                if (widget.IsEnabled())
                {
                    cursor = widget.GetMouseCursor(_evt);
                    if (cursor != null)
                    {
                        break;
                    }
                }
                widget = widget.GetParent();
            }

            if (cursor == DefaultMouseCursor.OS_DEFAULT)
            {
                cursor = null;
            }

            _renderer.SetCursor(cursor);
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
        public void HandleInput()
        {
            if (_input != null && !_input.PollInput(this))
            {
                ClearKeyboardState();
                ClearMouseState();
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
        public bool HandleMouse(int mouseX, int mouseY, int button, bool pressed)
        {
            _mouseEventTime = _curTime;
            _tooltipEventTime = _curTime;
            _evt._mouseButton = button;

            // only the previously pressed mouse button
            int prevButtonState = _evt._modifier & Event.MODIFIER_BUTTON;

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
            _evt.SetModifier(buttonMask, pressed);
            bool wasPressed = (prevButtonState & buttonMask) != 0;

            if (buttonMask != 0)
            {
                _renderer.SetMouseButton(button, pressed);
            }

            // don't send new mouse coords when still in drag area
            if (_dragActive || prevButtonState == 0)
            {
                _evt._mouseX = mouseX;
                _evt._mouseY = mouseY;
            }
            else
            {
                _evt._mouseX = _mouseDownX;
                _evt._mouseY = _mouseDownY;
            }

            bool handled = _dragActive;

            if (!_dragActive)
            {
                if (!IsInside(mouseX, mouseY))
                {
                    pressed = false;
                    _mouseClickCount = 0;
                    if (_wasInside)
                    {
                        SendMouseEvent(EventType.MOUSE_EXITED, null);
                        _wasInside = false;
                    }
                }
                else if (!_wasInside)
                {
                    _wasInside = true;
                    if (SendMouseEvent(EventType.MOUSE_ENTERED, null) != null)
                    {
                        handled = true;
                    }
                }
            }

            if (mouseX != _mouseLastX || mouseY != _mouseLastY)
            {
                _mouseLastX = mouseX;
                _mouseLastY = mouseY;

                if (prevButtonState != 0 && !_dragActive)
                {
                    if (Math.Abs(mouseX - _mouseDownX) > DRAG_DIST ||
                        Math.Abs(mouseY - _mouseDownY) > DRAG_DIST)
                    {
                        _dragActive = true;
                        _mouseClickCount = 0;
                        // close the tooltip - it may interface with dragging
                        HideTooltip();
                        _hadOpenTooltip = false;
                        // grab the tooltip to prevent it from poping up while dragging
                        // the widget can still request a tooltip update
                        _tooltipOwner = _lastMouseDownWidget;
                    }
                }

                if (_dragActive)
                {
                    if (_boundDragPopup != null)
                    {
                        // a bound drag is converted to a mouse move
                        System.Diagnostics.Debug.Assert(GetTopPane() == _boundDragPopup);
                        SendMouseEvent(EventType.MOUSE_MOVED, null);
                    }
                    else if (_lastMouseDownWidget != null)
                    {
                        // send MOUSE_DRAGGED only to the widget which received the MOUSE_BTNDOWN
                        SendMouseEvent(EventType.MOUSE_DRAGGED, _lastMouseDownWidget);
                    }
                }
                else if (prevButtonState == 0)
                {
                    if (SendMouseEvent(EventType.MOUSE_MOVED, null) != null)
                    {
                        handled = true;
                    }
                }
            }

            if (buttonMask != 0 && pressed != wasPressed)
            {
                if (pressed)
                {
                    if (_dragButton < 0)
                    {
                        _mouseDownX = mouseX;
                        _mouseDownY = mouseY;
                        _dragButton = button;
                        _lastMouseDownWidget = SendMouseEvent(EventType.MOUSE_BTNDOWN, null);
                    }
                    else if (_lastMouseDownWidget != null && _boundDragPopup == null)
                    {
                        // if another button is pressed while one button is already
                        // pressed then route the second button to the widget which
                        // received the first press
                        // but only when no bound drag is active
                        SendMouseEvent(EventType.MOUSE_BTNDOWN, _lastMouseDownWidget);
                    }
                }
                else if (_dragButton >= 0 && (_boundDragPopup == null || _evt.IsMouseDragEnd()))
                {
                    // only send the last MOUSE_BTNUP event when a bound drag is active
                    if (_boundDragPopup != null)
                    {
                        if (button == _dragButton)
                        {
                            // for bound drag the MOUSE_BTNUP is first send to the current widget under the mouse
                            SendMouseEvent(EventType.MOUSE_BTNUP, GetWidgetUnderMouse());
                        }
                    }
                    if (_lastMouseDownWidget != null)
                    {
                        // send MOUSE_BTNUP only to the widget which received the MOUSE_BTNDOWN
                        SendMouseEvent(EventType.MOUSE_BTNUP, _lastMouseDownWidget);
                    }
                }

                if (_lastMouseDownWidget != null)
                {
                    handled = true;
                }

                if (button == Event.MOUSE_LBUTTON && !_popupEventOccurred)
                {
                    if (!pressed && !_dragActive)
                    {
                        if (_mouseClickCount == 0 ||
                                _curTime - _mouseClickedTime > DBLCLICK_TIME ||
                                _lastMouseClickWidget != _lastMouseDownWidget)
                        {
                            _mouseClickedX = mouseX;
                            _mouseClickedY = mouseY;
                            _lastMouseClickWidget = _lastMouseDownWidget;
                            _mouseClickCount = 0;
                            _mouseClickedTime = _curTime;
                        }
                        if (Math.Abs(mouseX - _mouseClickedX) < DRAG_DIST &&
                                Math.Abs(mouseY - _mouseClickedY) < DRAG_DIST)
                        {
                            // ensure same click target as first
                            _evt._mouseX = _mouseClickedX;
                            _evt._mouseY = _mouseClickedY;
                            _evt._mouseClickCount = ++_mouseClickCount;
                            _mouseClickedTime = _curTime;
                            if (_lastMouseClickWidget != null)
                            {
                                SendMouseEvent(EventType.MOUSE_CLICKED, _lastMouseClickWidget);
                            }
                        }
                        else
                        {
                            _lastMouseClickWidget = null;
                        }
                    }
                }
            }

            if (_evt.IsMouseDragEnd())
            {
                if (_dragActive)
                {
                    _dragActive = false;
                    SendMouseEvent(EventType.MOUSE_MOVED, null);
                }
                _dragButton = -1;
                if (_boundDragCallback != null)
                {
                    try
                    {
                        _boundDragCallback.Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.GetLogger(typeof(GUI)).Log(Level.SEVERE,
                                "Exception in bound drag callback", ex);
                    }
                    finally
                    {
                        _boundDragCallback = null;
                        _boundDragPopup = null;
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
        public void ClearMouseState()
        {
            _evt.SetModifier(Event.MODIFIER_LBUTTON, false);
            _evt.SetModifier(Event.MODIFIER_MBUTTON, false);
            _evt.SetModifier(Event.MODIFIER_RBUTTON, false);
            _renderer.SetMouseButton(Event.MOUSE_LBUTTON, false);
            _renderer.SetMouseButton(Event.MOUSE_MBUTTON, false);
            _renderer.SetMouseButton(Event.MOUSE_RBUTTON, false);
            _lastMouseClickWidget = null;
            _mouseClickCount = 0;
            _mouseClickedTime = _curTime;
            _boundDragPopup = null;
            _boundDragCallback = null;
            if (_dragActive)
            {
                _dragActive = false;
                SendMouseEvent(EventType.MOUSE_MOVED, null);
            }
            _dragButton = -1;
        }

        /**
         * Mouse wheel has been turned. Must be called after handleMouse.
         * 
         * @param wheelDelta the normalized wheel delta
         * @return true if the event was handled by a widget
         */
        public bool HandleMouseWheel(int wheelDelta)
        {
            _evt._mouseWheelDelta = wheelDelta;
            bool handled = SendMouseEvent(EventType.MOUSE_WHEEL,
                    _dragActive ? _lastMouseDownWidget : null) != null;
            _evt._mouseWheelDelta = 0;
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
        public bool HandleKey(int keyCode, char keyChar, bool pressed)
        {
            _evt._keyCode = keyCode;
            _evt._keyChar = keyChar;
            _evt._keyRepeated = false;

            _keyEventTime = _curTime;
            if (_evt._keyCode != Event.KEY_NONE || _evt._keyChar != Event.CHAR_NONE)
            {
                _evt.SetModifiers(pressed);

                if (pressed)
                {
                    _keyRepeatDelay = KEYREPEAT_INITIAL_DELAY;
                    return SendKeyEvent(EventType.KEY_PRESSED);
                }
                else
                {
                    _keyRepeatDelay = NO_REPEAT;
                    return SendKeyEvent(EventType.KEY_RELEASED);
                }
            }
            else
            {
                _keyRepeatDelay = NO_REPEAT;
            }

            return false;
        }

        /**
         * Clears current keyboard modifiers.
         *
         * Should be called when the Display is minimized or when keyboard events are
         * handled outside of TWL.
         */
        public void ClearKeyboardState()
        {
            _evt._modifier &= ~(Event.MODIFIER_ALT | Event.MODIFIER_CTRL | Event.MODIFIER_SHIFT | Event.MODIFIER_META);
            _keyRepeatDelay = NO_REPEAT;

            _evt._type = EventType.CLEAR_KEYBOARD_STATE;
            RoutePopupEvent(_evt);
        }

        /**
         * Must be called after calling handleKey().
         *
         * This method checks the time since the last key event and causes a repeated
         * key press event to be generated.
         * 
         * @see #handleKey(int, char, bool) 
         */
        public void HandleKeyRepeat()
        {
            if (_keyRepeatDelay != NO_REPEAT)
            {
                long keyDeltaTime = _curTime - _keyEventTime;
                if (keyDeltaTime > _keyRepeatDelay)
                {
                    _keyEventTime = _curTime;
                    _keyRepeatDelay = KEYREPEAT_INTERVAL_DELAY;
                    _evt._keyRepeated = true;
                    SendKeyEvent(EventType.KEY_PRESSED);  // refire last key event
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
        public void HandleTooltips()
        {
            Widget widgetUnderMouse = GetWidgetUnderMouse();
            if (widgetUnderMouse != _tooltipOwner)
            {
                if (widgetUnderMouse != null && (
                        ((_curTime - _tooltipEventTime) > _tooltipDelay) ||
                        (_hadOpenTooltip && (_curTime - _tooltipClosedTime) < _tooltipReappearDelay)))
                {
                    SetTooltip(
                            _evt._mouseX + _tooltipOffsetX,
                            _evt._mouseY + _tooltipOffsetY,
                            widgetUnderMouse,
                            widgetUnderMouse.GetTooltipContentAt(_evt._mouseX, _evt._mouseY),
                            Alignment.BOTTOMLEFT);
                }
                else
                {
                    HideTooltip();
                }
            }

            bool mouseIdle = (_curTime - _mouseEventTime) > _mouseIdleTime;
            if (_mouseIdleState != mouseIdle)
            {
                _mouseIdleState = mouseIdle;
                CallMouseIdleListener();
            }
        }

        private Widget GetTopPane()
        {
            // don't use potential overwritten methods
            return base.GetChild(base.GetNumChildren() - 3);
        }

        //@Override
        internal override Widget GetWidgetUnderMouse()
        {
            return GetTopPane().GetWidgetUnderMouse();
        }

        private Widget SendMouseEvent(EventType type, Widget target)
        {
            System.Diagnostics.Debug.Assert(type.IsMouseEvent);
            _popupEventOccurred = false;
            _evt._type = type;
            _evt._dragEvent = _dragActive && (_boundDragPopup == null);

            _renderer.SetMousePosition(_evt._mouseX, _evt._mouseY);

            if (target != null)
            {
                if (target.IsEnabled() || !IsMouseAction(_evt))
                {
                    target.HandleEvent(target.TranslateMouseEvent(_evt));
                }
                return target;
            }
            else
            {
                System.Diagnostics.Debug.Assert(!_dragActive || _boundDragPopup != null);
                Widget widget = null;
                if (_activeInfoWindow != null)
                {
                    if (_activeInfoWindow.IsMouseInside(_evt) && SetMouseOverChild(_activeInfoWindow, _evt))
                    {
                        widget = _activeInfoWindow;
                    }
                }
                if (widget == null)
                {
                    widget = GetTopPane();
                    SetMouseOverChild(widget, _evt);
                }
                return widget.RouteMouseEvent(_evt);
            }
        }

        private static int FOCUS_KEY = Event.KEY_TAB;

        bool IsFocusKey()
        {
            return _evt._keyCode == FOCUS_KEY &&
                        ((_evt._modifier & (Event.MODIFIER_CTRL | Event.MODIFIER_META | Event.MODIFIER_ALT)) == 0);
        }

        internal void SetFocusKeyWidget(Widget widget)
        {
            if (_focusKeyWidget == null && IsFocusKey())
            {
                _focusKeyWidget = widget;
            }
        }

        private bool SendKeyEvent(EventType type)
        {
            System.Diagnostics.Debug.Assert(type.IsKeyEvent);
            _popupEventOccurred = false;
            _focusKeyWidget = null;
            _evt._type = type;
            _evt._dragEvent = false;
            bool handled = GetTopPane().HandleEvent(_evt);
            if (!handled && _focusKeyWidget != null)
            {
                _focusKeyWidget.HandleFocusKeyEvent(_evt);
                handled = true;
            }
            _focusKeyWidget = null;  // allow GC
            return handled;
        }

        private void SendPopupEvent(EventType type)
        {
            System.Diagnostics.Debug.Assert(type == EventType.POPUP_OPENED || type == EventType.POPUP_CLOSED);
            _popupEventOccurred = false;
            _evt._type = type;
            _evt._dragEvent = false;
            try
            {
                GetTopPane().RoutePopupEvent(_evt);
            }
            catch (Exception ex)
            {
                Logger.GetLogger(typeof(GUI)).Log(Level.SEVERE, "Exception in sendPopupEvent()", ex);
            }
        }

        protected internal void ResendLastMouseMove()
        {
            if (!_dragActive)
            {
                SendMouseEvent(EventType.MOUSE_MOVED, null);
            }
        }

        internal void OpenPopup(PopupWindow popup)
        {
            if (popup.GetParent() == this)
            {
                ClosePopup(popup);
            }
            else if (popup.GetParent() != null)
            {
                throw new ArgumentOutOfRangeException("popup must not be added anywhere");
            }
            HideTooltip();
            _hadOpenTooltip = false;
            SendPopupEvent(EventType.POPUP_OPENED);
            base.InsertChild(popup, GetNumChildren() - 2);
            popup.GetOwner().SetOpenPopup(this, true);
            _popupEventOccurred = true;
            if (_activeInfoWindow != null)
            {
                CloseInfo(_activeInfoWindow);
            }
        }

        internal void ClosePopup(PopupWindow popup)
        {
            if (_boundDragPopup == popup)
            {
                _boundDragPopup = null;
            }
            int idx = GetChildIndex(popup);
            if (idx > 0)
            {
                base.RemoveChild(idx);
            }
            popup.GetOwner().RecalcOpenPopups(this);
            SendPopupEvent(EventType.POPUP_CLOSED);
            _popupEventOccurred = true;
            CloseInfoFromWidget(popup);
            RequestKeyboardFocus(GetTopPane());
            ResendLastMouseMove();
        }

        internal bool HasOpenPopups(Widget owner)
        {
            for (int i = GetNumChildren() - 2; i-- > 1;)
            {
                PopupWindow popup = (PopupWindow)GetChild(i);
                if (popup.GetOwner() == owner)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOwner(Widget owner, Widget widget)
        {
            while (owner != null && owner != widget)
            {
                owner = owner.GetParent();
            }
            return owner == widget;
        }

        internal void ClosePopupFromWidgets(Widget widget)
        {
            for (int i = GetNumChildren() - 2; i-- > 1;)
            {
                PopupWindow popup = (PopupWindow)GetChild(i);
                if (IsOwner(popup.GetOwner(), widget))
                {
                    ClosePopup(popup);
                }
            }
        }

        void CloseIfPopup(Widget widget)
        {
            if (widget is PopupWindow)
            {
                ClosePopup((PopupWindow)widget);
            }
        }

        internal bool BindDragEvent(PopupWindow popup, Runnable cb)
        {
            if (_boundDragPopup == null && GetTopPane() == popup && _dragButton >= 0 && !IsOwner(_lastMouseDownWidget, popup))
            {
                _dragActive = true;
                _boundDragPopup = popup;
                _boundDragCallback = cb;
                SendMouseEvent(EventType.MOUSE_MOVED, null);
                return true;
            }
            return false;
        }

        internal void WidgetHidden(Widget widget)
        {
            CloseIfPopup(widget);
            ClosePopupFromWidgets(widget);
            if (IsOwner(_tooltipOwner, widget))
            {
                HideTooltip();
                _hadOpenTooltip = false;
            }
            CloseInfoFromWidget(widget);
        }

        internal void WidgetDisabled(Widget widget)
        {
            CloseIfPopup(widget);
            CloseInfoFromWidget(widget);
        }

        void CloseInfoFromWidget(Widget widget)
        {
            if (_activeInfoWindow != null)
            {
                if (_activeInfoWindow == widget ||
                        IsOwner(_activeInfoWindow.GetOwner(), widget))
                {
                    CloseInfo(_activeInfoWindow);
                }
            }
        }

        internal void OpenInfo(InfoWindow info)
        {
            int idx = GetNumChildren() - 2;
            base.RemoveChild(idx);
            base.InsertChild(info, idx);
            _activeInfoWindow = info;
        }

        internal void CloseInfo(InfoWindow info)
        {
            if (info == _activeInfoWindow)
            {
                int idx = GetNumChildren() - 2;
                base.RemoveChild(idx);
                base.InsertChild(_infoWindowPlaceholder, idx);
                _activeInfoWindow = null;
                try
                {
                    info.InfoWindowClosed();
                }
                catch (Exception ex)
                {
                    Logger.GetLogger(typeof(GUI)).Log(Level.SEVERE, "Exception in infoWindowClosed()", ex);
                }
            }
        }

        //@Override
        public override bool RequestKeyboardFocus()
        {
            // GUI always has the keyboard focus
            return true;
        }

        //@Override
        protected override bool RequestKeyboardFocus(Widget child)
        {
            if (child != null)
            {
                if (child != GetTopPane())
                {
                    return false;
                }
            }
            return base.RequestKeyboardFocus(child);
        }

        internal void RequestTooltipUpdate(Widget widget, bool resetToolTipTimer)
        {
            if (_tooltipOwner == widget)
            {
                _tooltipOwner = null;
                if (resetToolTipTimer)
                {
                    HideTooltip();
                    _hadOpenTooltip = false;
                    _tooltipEventTime = _curTime;
                }
            }
        }

        private void HideTooltip()
        {
            if (_tooltipWindow.IsVisible())
            {
                _tooltipClosedTime = _curTime;
                _hadOpenTooltip = true;
            }
            _tooltipWindow.SetVisible(false);
            _tooltipOwner = null;

            // remove tooltip widget if it's not our label
            if (_tooltipLabel.GetParent() != _tooltipWindow)
            {
                _tooltipWindow.RemoveAllChildren();
            }
        }

        private void SetTooltip(int x, int y, Widget widget, Object content, Alignment alignment)
        {
            if (content == null)
            {
                HideTooltip();
                return;
            }

            if (content is String)
            {
                String text = (String)content;
                if (text.Length == 0)
                {
                    HideTooltip();
                    return;
                }
                if (_tooltipLabel.GetParent() != _tooltipWindow)
                {
                    _tooltipWindow.RemoveAllChildren();
                    _tooltipWindow.Add(_tooltipLabel);
                }
                _tooltipLabel.SetBackground(null);
                _tooltipLabel.SetText(text);
            }
            else if (content is Widget)
            {
                Widget tooltipWidget = (Widget)content;
                if (tooltipWidget.GetParent() != null && tooltipWidget.GetParent() != _tooltipWindow)
                {
                    throw new ArgumentOutOfRangeException("Content widget must not be added to another widget");
                }
                _tooltipWindow.RemoveAllChildren();
                _tooltipWindow.Add(tooltipWidget);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unsupported data type");
            }

            _tooltipWindow.AdjustSize();

            // some Widgets (esp TextArea) have complex sizing policy
            // give them a 2nd chance
            if (_tooltipWindow.IsLayoutInvalid())
            {
                _tooltipWindow.AdjustSize();
            }

            int ttWidth = _tooltipWindow.GetWidth();
            int ttHeight = _tooltipWindow.GetHeight();

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

            if (x + ttWidth > GetWidth())
            {
                x = GetWidth() - ttWidth;
            }
            if (y + ttHeight > GetHeight())
            {
                y = GetHeight() - ttHeight;
            }
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }

            _tooltipOwner = widget;
            _tooltipWindow.SetPosition(x, y);
            _tooltipWindow.SetVisible(true);
        }

        private void CallMouseIdleListener()
        {
            if (_mouseIdleListener != null)
            {
                if (_mouseIdleState)
                {
                    _mouseIdleListener.MouseEnterIdle();
                }
                else
                {
                    _mouseIdleListener.MouseExitIdle();
                }
            }
        }

        private void GrowInvokeLaterQueue()
        {
            Runnable[] tmp = new Runnable[_invokeLaterQueueSize * 2];
            Array.Copy(_invokeLaterQueue, 0, tmp, 0, _invokeLaterQueueSize);
            _invokeLaterQueue = tmp;
        }

        class TooltipWindow : Container
        {
            public static StateKey STATE_FADE = StateKey.Get("fade");
            private int _fadeInTime;

            //@Override
            protected override void ApplyTheme(ThemeInfo themeInfo)
            {
                base.ApplyTheme(themeInfo);
                _fadeInTime = themeInfo.GetParameter("fadeInTime", 0);
            }

            //@Override
            public override void SetVisible(bool visible)
            {
                base.SetVisible(visible);
                GetAnimationState().ResetAnimationTime(STATE_FADE);
            }

            //@Override
            protected override void Paint(GUI gui)
            {
                int time = GetAnimationState().GetAnimationTime(STATE_FADE);
                if (time < _fadeInTime)
                {
                    float alpha = time / (float)_fadeInTime;
                    gui.GetRenderer().PushGlobalTintColor(1f, 1f, 1f, alpha);
                    try
                    {
                        base.Paint(gui);
                    }
                    finally
                    {
                        gui.GetRenderer().PopGlobalTintColor();
                    }
                }
                else
                {
                    base.Paint(gui);
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
