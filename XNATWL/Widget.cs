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
using System.Text;
using System.Threading;
using static XNATWL.Utils.Logger;
using XNATWL.Renderer;
using XNATWL.Theme;
using XNATWL.Utils;
using System.Collections.ObjectModel;
using XNATWL.Property;

namespace XNATWL
{
    public class Widget
    {
        public static StateKey STATE_KEYBOARD_FOCUS = StateKey.Get("keyboardFocus");
        public static StateKey STATE_HAS_OPEN_POPUPS = StateKey.Get("hasOpenPopups");
        public static StateKey STATE_HAS_FOCUSED_CHILD = StateKey.Get("hasFocusedChild");
        public static StateKey STATE_DISABLED = StateKey.Get("disabled");

        public static bool DEBUG_LAYOUT_GROUPS = true;

        private static int LAYOUT_INVALID_LOCAL = 1;
        private static int LAYOUT_INVALID_GLOBAL = 3;

        private Widget _parent;
        private int _posX;
        private int _posY;
        private int _width;
        private int _height;
        private int _layoutInvalid;
        private bool _clip;
        private bool _visible = true;
        private bool _hasOpenPopup;
        private bool _enabled = true;
        private bool _locallyEnabled = true;
        private String _theme;
        private ThemeManager _themeManager;
        private Image _background;
        private Image _overlay;
        private Object _tooltipContent;
        private Object _themeTooltipContent;
        private InputMap _inputMap;
        private ActionMap _actionMap;
        private TintAnimator _tintAnimator;
        private PropertyChangeSupport _propertyChangeSupport;
        internal volatile GUI _guiInstance;
        private OffscreenSurface _offscreenSurface;
        private RenderOffscreen _renderOffscreen;

        private AnimationState _animState;
        private bool _sharedAnimState;

        private short _borderLeft;
        private short _borderTop;
        private short _borderRight;
        private short _borderBottom;

        private short _minWidth;
        private short _minHeight;
        private short _maxWidth;
        private short _maxHeight;

        private short _offscreenExtraLeft;
        private short _offscreenExtraTop;
        private short _offscreenExtraRight;
        private short _offscreenExtraBottom;

        private List<Widget> _children;
        private Widget _lastChildMouseOver;
        private Widget _focusChild;
        private MouseCursor _mouseCursor;
        private FocusGainedCause _focusGainedCause;

        private bool _focusKeyEnabled = true;
        private bool _bCanAcceptKeyboardFocus;
        private bool _depthFocusTraversal = true;

        /**
         * Stores the state of the current focus transfer:
         * null                     no focus transfer active
         * Widget[]{ null }         transfer is active, but no previous focused widget
         * Widget[]{ prevWidget }   transfer is active, prevWidget was focused
         */
        private static ThreadLocal<Widget[]> focusTransferInfo = new ThreadLocal<Widget[]>();

        /**
         * Creates a Widget with it's own animation state
         * 
         * <p>The initial theme name is the lower case version of the simple class
         * name of the concrete subclass - or in pseudo code:</p>
         * <pre>{@code getClass().getSimpleName().toLowerCase() }</pre>
         * 
         * @see #setTheme(java.lang.String) 
         */
        public Widget() : this(null, false)
        {
            
        }

        /**
         * Creates a Widget with a shared animation state
         * 
         * <p>The initial theme name is the lower case version of the simple class
         * name of the concrete subclass - or in pseudo code:</p>
         * <pre>{@code getClass().getSimpleName().toLowerCase() }</pre>
         *
         * @param animState the animation state to share, can be null
         * @see #setTheme(java.lang.String) 
         */
        public Widget(AnimationState animState) : this(animState, false)
        {

        }

        /**
         * Creates a Widget with a shared or inherited animation state
         * 
         * <p>The initial theme name is the lower case version of the simple class
         * name of the concrete subclass - or in pseudo code:</p>
         * <pre>{@code getClass().getSimpleName().toLowerCase() }</pre>
         *
         * @param animState the animation state to share or inherit, can be null
         * @param inherit true if the animation state should be inherited, false for sharing
         * @see AnimationState#AnimationState(de.matthiasmann.twl.AnimationState) 
         * @see #setTheme(java.lang.String) 
         */
        public Widget(AnimationState animState, bool inherit)
        {
            // determine the default theme name from the class name of this instance
            // eg class Label => "label"
            Type clazz = this.GetType();
            do
            {
                _theme = clazz.Name.ToLower();
                int genericIDStart = _theme.IndexOf('`');
                if (genericIDStart != -1)
                {
                    _theme = _theme.Substring(0, genericIDStart);
                }
                clazz = clazz.GetType().BaseType;
            } while (_theme.Length == 0 && clazz != null);

            if (animState == null || inherit)
            {
                this._animState = new AnimationState(animState);
                this._sharedAnimState = false;
            }
            else
            {
                this._animState = animState;
                this._sharedAnimState = true;
            }
        }

        /**
         * Add a PropertyChangeListener for all properties.
         *
         * @param listener The PropertyChangeListener to be added
         * @see PropertyChangeSupport#addPropertyChangeListener(java.beans.PropertyChangeListener)
         */
        public void AddPropertyChangeListener(PropertyChangeListener listener)
        {
            CreatePropertyChangeSupport().AddPropertyChangeListener(listener);
        }

        /**
         * Add a PropertyChangeListener for a specific property.
         *
         * @param propertyName The name of the property to listen on
         * @param listener The PropertyChangeListener to be added
         * @see PropertyChangeSupport#addPropertyChangeListener(java.lang.String, java.beans.PropertyChangeListener) 
         */
        public void AddPropertyChangeListener(String propertyName, PropertyChangeListener listener)
        {
            CreatePropertyChangeSupport().AddPropertyChangeListener(propertyName, listener);
        }

        /**
         * Remove a PropertyChangeListener.
         *
         * @param listener The PropertyChangeListener to be removed
         * @see PropertyChangeSupport#removePropertyChangeListener(java.beans.PropertyChangeListener) 
         */
        public void RemovePropertyChangeListener(PropertyChangeListener listener)
        {
            if (_propertyChangeSupport != null)
            {
                _propertyChangeSupport.RemovePropertyChangeListener(listener);
            }
        }

        /**
         * Remove a PropertyChangeListener.
         *
         * @param propertyName The name of the property that was listened on
         * @param listener The PropertyChangeListener to be removed
         * @see PropertyChangeSupport#removePropertyChangeListener(java.lang.String, java.beans.PropertyChangeListener) 
         */
        public void RemovePropertyChangeListener(String propertyName, PropertyChangeListener listener)
        {
            if (_propertyChangeSupport != null)
            {
                _propertyChangeSupport.RemovePropertyChangeListener(propertyName, listener);
            }
        }

        /**
         * Checks whether this widget or atleast one of it's children
         * owns an open popup.
         * @return true if atleast own open popup is owned (indirectly) by this widget.
         */
        public bool HasOpenPopups()
        {
            return _hasOpenPopup;
        }

        /**
         * Returns the parent of this widget or null if it is the tree root.
         * All coordinates are relative to the root of the widget tree.
         * @return the parent of this widget or null if it is the tree root
         */
        public Widget GetParent()
        {
            return _parent;
        }

        /**
         * Returns the root of this widget tree.
         * All coordinates are relative to the root of the widget tree.
         * @return the root of this widget tree
         */
        public Widget GetRootWidget()
        {
            Widget w = this;
            Widget p;
            while ((p = w._parent) != null)
            {
                w = p;
            }
            return w;
        }

        /**
         * Returns the GUI root of this widget tree if it has one.<p>
         *
         * Once a widget is added (indirectly) to a GUI object it will be part of
         * that GUI tree.<p>
         *
         * This method is thread safe.<p>
         *
         * Repeated calls may not return the same result. Use it like this:
         * <pre>
         * GUI gui = getGUI();
         * if(gui != null) {
         *     gui.invokeLater(....);
         * }
         * </pre>
         *
         * @return the GUI root or null if the root is not a GUI instance.
         * @see #afterAddToGUI(de.matthiasmann.twl.GUI)
         * @see #beforeRemoveFromGUI(de.matthiasmann.twl.GUI)
         */
        public GUI GetGUI()
        {
            return _guiInstance;
        }

        /**
         * Returns the current visibility flag of this widget.
         * This does not check if the widget is clipped or buried behind another widget.
         * @return the current visibility flag of this widget
         */
        public bool IsVisible()
        {
            return _visible;
        }

        /**
         * Changes the visibility flag of this widget.
         * Widgets are by default visible.
         * Invisible widgets don't receive paint() or handleEvent() calls
         * @param visible the new visibility flag
         */
        public virtual void SetVisible(bool visible)
        {
            if (this._visible != visible)
            {
                this._visible = visible;
                if (!visible)
                {
                    GUI gui = GetGUI();
                    if (gui != null)
                    {
                        gui.WidgetHidden(this);
                    }
                    if (_parent != null)
                    {
                        _parent.ChildHidden(this);
                    }
                }
                if (_parent != null)
                {
                    _parent.ChildVisibilityChanged(this);
                }
            }
        }

        /**
         * Returns the local enabled state of this widget.
         * 
         * If one of it's parents is disabled then this widget will also be
         * disabled even when it's local enabled state is true.
         *
         * @return the local enabled state.
         * @see #isEnabled()
         * @see #setEnabled(bool)
         */
        public bool IsLocallyEnabled()
        {
            return _locallyEnabled;
        }

        /**
         * Checks if this widget and all it's parents are enabled.
         * If one of it's parents is disabled then it will return false.
         * 
         * This is the effective enabled state which is also represented as
         * animation state with inverse polarity {@code STATE_DISABLED}
         *
         * If a widget is disabled it will not receive keyboard or mouse events
         * except {@code MOUSE_ENTERED} and {@code MOUSE_EXITED}
         *
         * @return the effective enabled state
         * @see #isEnabled()
         * @see #setEnabled(bool)
         */
        public bool IsEnabled()
        {
            return _enabled;
        }

        /**
         * Sets the local enabled state of that widget. The effective enabled state
         * of the widget is the effective enabled state of it's parent and it's
         * local enabled state.
         *
         * The effective enabled state is exposed as animation state but with
         * inverse polarity as {@code STATE_DISABLED}.
         *
         * On disabling the keyboard focus will be removed.
         *
         * If a widget is disabled it will not receive keyboard or mouse events
         * except {@code MOUSE_ENTERED} and {@code MOUSE_EXITED}
         *
         * @param enabled true if the widget should be locally enabled
         * @see #isEnabled()
         * @see #isLocallyEnabled()
         */
        public virtual void SetEnabled(bool enabled)
        {
            if (this._locallyEnabled != enabled)
            {
                this._locallyEnabled = enabled;
                FirePropertyChange("locallyEnabled", !enabled, enabled);
                RecursivelyEnabledChanged(GetGUI(),
                        (_parent != null) ? _parent._enabled : true);
            }
        }

        /**
         * Returns the absolute X coordinate of widget in it's tree
         *
         * This property can be bound and fires PropertyChangeEvent
         *
         * @return the absolute X coordinate of widget in it's tree
         * @see #addPropertyChangeListener(java.beans.PropertyChangeListener)
         * @see #addPropertyChangeListener(java.lang.String, java.beans.PropertyChangeListener)
         */
        public int GetX()
        {
            return _posX;
        }


        /**
         * Returns the absolute Y coordinate of widget in it's tree
         *
         * This property can be bound and fires PropertyChangeEvent
         *
         * @return the absolute Y coordinate of widget in it's tree
         * @see #addPropertyChangeListener(java.beans.PropertyChangeListener)
         * @see #addPropertyChangeListener(java.lang.String, java.beans.PropertyChangeListener)
         */
        public int GetY()
        {
            return _posY;
        }

        /**
         * Returns the width of this widget
         *
         * This property can be bound and fires PropertyChangeEvent
         *
         * @return the width of this widget
         * @see #addPropertyChangeListener(java.beans.PropertyChangeListener)
         * @see #addPropertyChangeListener(java.lang.String, java.beans.PropertyChangeListener)
         */
        public int GetWidth()
        {
            return _width;
        }

        /**
         * Returns the height of this widget
         *
         * This property can be bound and fires PropertyChangeEvent
         *
         * @return the height of this widget
         * @see #addPropertyChangeListener(java.beans.PropertyChangeListener)
         * @see #addPropertyChangeListener(java.lang.String, java.beans.PropertyChangeListener) 
         */
        public int GetHeight()
        {
            return _height;
        }

        /**
         * Returns the right X coordinate of this widget
         * @return getX() + getWidth()
         */
        public int GetRight()
        {
            return _posX + _width;
        }

        /**
         * Returns the bottom Y coordinate of this widget
         * @return getY() + getHeight()
         */
        public int GetBottom()
        {
            return _posY + _height;
        }

        /**
         * The inner X position takes the left border into account
         * @return getX() + getBorderLeft()
         */
        public int GetInnerX()
        {
            return _posX + _borderLeft;
        }

        /**
         * The inner Y position takes the top border into account
         * @return getY() + getBorderTop()
         */
        public int GetInnerY()
        {
            return _posY + _borderTop;
        }

        /**
         * The inner width takes the left and right border into account.
         * @return the inner width - never negative
         */
        public int GetInnerWidth()
        {
            return Math.Max(0, _width - _borderLeft - _borderRight);
        }

        /**
         * The inner height takes the top and bottom border into account.
         * @return the inner height - never negative
         */
        public int GetInnerHeight()
        {
            return Math.Max(0, _height - _borderTop - _borderBottom);
        }

        /**
         * Returns the right X coordinate while taking the right border into account.
         * @return getInnerX() + getInnerWidth()
         */
        public int GetInnerRight()
        {
            return _posX + Math.Max(_borderLeft, _width - _borderRight);
        }

        /**
         * Returns the bottom Y coordinate while taking the bottom border into account.
         * @return getInnerY() + getInnerHeight()
         */
        public int GetInnerBottom()
        {
            return _posY + Math.Max(_borderTop, _height - _borderBottom);
        }

        /**
         * Checks if the given absolute (to this widget's tree) coordinates are inside this widget.
         * 
         * @param x the X coordinate to test
         * @param y the Y coordinate to test
         * @return true if it was inside
         */
        public virtual bool IsInside(int x, int y)
        {
            return (x >= _posX) && (y >= _posY) && (x < _posX + _width) && (y < _posY + _height);
        }

        /**
         * Changes the position of this widget.
         * 
         * <p>When the position has changed then<ul>
         * <li>The positions of all children are updated</li>
         * <li>{@link #positionChanged()} is called</li>
         * <li>{@link PropertyChangeEvent} are fired for "x" and "y"</li>
         * </ul></p>
         *
         * <p>This method should only be called from within the layout() method of the
         * parent. Otherwise it could lead to bad interaction with theming and result
         * in a wrong position after the theme has been applied.</p>
         *
         * <p>NOTE: Position is absolute in the widget's tree.</p>
         *
         * @param x The new x position, can be negative
         * @param y The new y position, can be negative
         * @return true if the position was changed, false if new position == old position
         * @see #layout()
         */
        public virtual bool SetPosition(int x, int y)
        {
            return SetPositionImpl(x, y);
        }

        /** 
         * Changes the size of this widget.
         * Zero size is allowed but not negative.
         * Size is not checked against parent widgets.
         * 
         * When the size has changed then
         * - the parent widget's childChangedSize is called
         * - sizeChanged is called
         * - PropertyChangeEvent are fired for "width" and "height"
         *
         * This method should only be called from within the layout() method of the
         * parent. Otherwise it could lead to bad interaction with theming and result
         * in a wrong size after the theme has been applied.
         *
         * @param width The new width (including border)
         * @param height The new height (including border)
         * @return true if the size was changed, false if new size == old size
         * @throws java.lang.ArgumentOutOfRangeException if the size is negative
         * @see #sizeChanged()
         * @see #layout()
         */
        public virtual bool SetSize(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException("negative size");
            }
            int oldWidth = this._width;
            int oldHeight = this._height;
            if (oldWidth != width || oldHeight != height)
            {
                this._width = width;
                this._height = height;

                SizeChanged();

                if (_propertyChangeSupport != null)
                {
                    FirePropertyChange("width", oldWidth, width);
                    FirePropertyChange("height", oldHeight, height);
                }
                return true;
            }
            return false;
        }

        /** 
         * Changes the inner size of this widget.
         * Calls setSize after adding the border width/height.
         * 
         * @param width The new width (exclusive border)
         * @param height The new height (exclusive border)
         * @return true if the size was changed, false if new size == old size
         * @see #setSize(int,int)
         */
        public bool SetInnerSize(int width, int height)
        {
            return SetSize(width + _borderLeft + _borderRight, height + _borderTop + _borderBottom);
        }

        public short GetBorderTop()
        {
            return _borderTop;
        }

        public short GetBorderLeft()
        {
            return _borderLeft;
        }

        public short GetBorderBottom()
        {
            return _borderBottom;
        }

        public short GetBorderRight()
        {
            return _borderRight;
        }

        public int GetBorderHorizontal()
        {
            return _borderLeft + _borderRight;
        }

        public int GetBorderVertical()
        {
            return _borderTop + _borderBottom;
        }

        /**
         * Sets a border for this widget.
         * @param top the top border
         * @param left the left border
         * @param bottom the bottom  border
         * @param right the right border
         * @return true if the border values have changed
         * @throws ArgumentOutOfRangeException if any of the parameters is negative.
         */
        public bool SetBorderSize(int top, int left, int bottom, int right)
        {
            if (top < 0 || left < 0 || bottom < 0 || right < 0)
            {
                throw new ArgumentOutOfRangeException("negative border size");
            }
            if (this._borderTop != top || this._borderBottom != bottom ||
                    this._borderLeft != left || this._borderRight != right)
            {
                int innerWidth = GetInnerWidth();
                int innerHeight = GetInnerHeight();
                int deltaLeft = left - this._borderLeft;
                int deltaTop = top - this._borderTop;
                this._borderLeft = (short)left;
                this._borderTop = (short)top;
                this._borderRight = (short)right;
                this._borderBottom = (short)bottom;

                // first adjust child position
                if (_children != null && (deltaLeft != 0 || deltaTop != 0))
                {
                    for (int i = 0, n = _children.Count; i < n; i++)
                    {
                        AdjustChildPosition(_children[i], deltaLeft, deltaTop);
                    }
                }

                // now change size
                SetInnerSize(innerWidth, innerHeight);
                borderChanged();
                return true;
            }
            return false;
        }

        /**
         * Sets a border for this widget.
         * @param horizontal the border width for left and right
         * @param vertical the border height for top and bottom
         * @return true if the border values have changed
         * @throws ArgumentOutOfRangeException if horizontal or vertical is negative.
         */
        public bool SetBorderSize(int horizontal, int vertical)
        {
            return SetBorderSize(vertical, horizontal, vertical, horizontal);
        }

        /**
         * Sets a uniform border for this widget.
         * @param border the border width/height on all edges
         * @return true if the border values have changed
         * @throws ArgumentOutOfRangeException if border is negative.
         */
        public bool SetBorderSize(int border)
        {
            return SetBorderSize(border, border, border, border);
        }

        /**
         * Sets the border width for this widget.
         * @param border the border object or null for no border
         * @return true if the border values have changed
         */
        public bool SetBorderSize(Border border)
        {
            if (border == null)
            {
                return SetBorderSize(0, 0, 0, 0);
            }
            else
            {
                return SetBorderSize(border.BorderTop, border.BorderLeft,
                                        border.BorderBottom, border.BorderRight);
            }
        }

        public short GetOffscreenExtraTop()
        {
            return _offscreenExtraTop;
        }

        public short GetOffscreenExtraLeft()
        {
            return _offscreenExtraLeft;
        }

        public short GetOffscreenExtraBottom()
        {
            return _offscreenExtraBottom;
        }

        public short GetOffscreenExtraRight()
        {
            return _offscreenExtraRight;
        }

        /**
         * Sets the offscreen rendering extra area for this widget.
         * @param top the extra area on top
         * @param left the extra area on left
         * @param bottom the extra area on bottom
         * @param right the extra area on right
         * @throws ArgumentOutOfRangeException if any of the parameters is negative.
         * @see #setRenderOffscreen(de.matthiasmann.twl.Widget.RenderOffscreen) 
         */
        public void SetOffscreenExtra(int top, int left, int bottom, int right)
        {
            if (top < 0 || left < 0 || bottom < 0 || right < 0)
            {
                throw new ArgumentOutOfRangeException("negative offscreen extra size");
            }
            this._offscreenExtraTop = (short)top;
            this._offscreenExtraLeft = (short)left;
            this._offscreenExtraBottom = (short)bottom;
            this._offscreenExtraRight = (short)right;
        }

        /**
         * Sets the offscreen rendering extra area for this widget.
         * @param offscreenExtra the border object or null for no extra area
         * @throws ArgumentOutOfRangeException if any of the values is negative.
         * @see #setRenderOffscreen(de.matthiasmann.twl.Widget.RenderOffscreen) 
         */
        public void SetOffscreenExtra(Border offscreenExtra)
        {
            if (offscreenExtra == null)
            {
                SetOffscreenExtra(0, 0, 0, 0);
            }
            else
            {
                SetOffscreenExtra(offscreenExtra.BorderTop, offscreenExtra.BorderLeft,
                        offscreenExtra.BorderBottom, offscreenExtra.BorderRight);
            }
        }

        /**
         * Returns the minimum width of the widget.
         * Layout manager will allocate atleast the minimum width to a widget even
         * when the container is not big enough.
         *
         * The default implementation will not return values smaller then the
         * current border width.
         *
         * @return the minimum width of the widget
         */
        public virtual int GetMinWidth()
        {
            return Math.Max(_minWidth, _borderLeft + _borderRight);
        }

        /**
         * Returns the minimum height of the widget.
         * Layout manager will allocate atleast the minimum height to a widget even
         * when the container is not big enough.
         *
         * The default implementation will not return values smaller then the
         * current border width.
         *
         * @return the minimum height of the widget
         */
        public virtual int GetMinHeight()
        {
            return Math.Max(_minHeight, _borderTop + _borderBottom);
        }

        /**
         * Sets the minimum size of the widget. This size includes the border.
         *
         * <p>The minimum size is set via the theme in {@link #applyThemeMinSize(de.matthiasmann.twl.ThemeInfo)}</p>
         *
         * @param width the minimum width
         * @param height the minimum height
         * @see #getMinWidth()
         * @see #getMinHeight()
         * @throws ArgumentOutOfRangeException when width or height is negative
         */
        public virtual void SetMinSize(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException("negative size");
            }
            _minWidth = (short)Math.Min(width, short.MaxValue);
            _minHeight = (short)Math.Min(height, short.MaxValue);
        }

        /**
         * Computes the preferred inner width (the size of the widget without the border)
         *
         * The default implementation uses the current position of the children.
         *
         * It is highly recommended to override this method as the default implementation
         * lead to unstable layouts.
         *
         * The default behavior might change in the future to provide a better default
         * behavior.
         *
         * @return the preferred inner width
         */
        public virtual int GetPreferredInnerWidth()
        {
            int right = GetInnerX();
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    Widget child = _children[i];
                    right = Math.Max(right, child.GetRight());
                }
            }
            return right - GetInnerX();
        }

        /**
         * Returns the preferred width based on it's children and preferred inner width.
         *
         * Subclasses can overwrite this method to compute the preferred size differently.
         *
         * @return the preferred width.
         * @see #getPreferredInnerWidth()
         */
        public virtual int GetPreferredWidth()
        {
            int prefWidth = _borderLeft + _borderRight + GetPreferredInnerWidth();
            Image bg = GetBackground();
            if (bg != null)
            {
                prefWidth = Math.Max(prefWidth, bg.Width);
            }
            return Math.Max(_minWidth, prefWidth);
        }

        /**
         * Computes the preferred inner height (the size of the widget without the border)
         *
         * The default implementation uses the current position of the children.
         *
         * It is highly recommended to override this method as the default implementation
         * lead to unstable layouts.
         *
         * The default behavior might change in the future to provide a better default
         * behavior.
         *
         * @return the preferred inner height
         */
        public virtual int GetPreferredInnerHeight()
        {
            int bottom = GetInnerY();
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    Widget child = _children[i];
                    bottom = Math.Max(bottom, child.GetBottom());
                }
            }
            return bottom - GetInnerY();
        }

        /**
         * Returns the preferred height.
         *
         * This method determines the preferred height based on it's children.
         * Subclasses can overwrite this method to compute the preferred size differently.
         *
         * @return the preferred height.
         * @see #getPreferredInnerHeight() 
         */
        public virtual int GetPreferredHeight()
        {
            int prefHeight = _borderTop + _borderBottom + GetPreferredInnerHeight();
            Image bg = GetBackground();
            if (bg != null)
            {
                prefHeight = Math.Max(prefHeight, bg.Height);
            }
            return Math.Max(_minHeight, prefHeight);
        }

        /**
         * Returns the maximum width of the widget.
         *
         * A maximum of 0 means that the widgets wants it's preferred size and no
         * extra space from layout.
         * A value &gt; 0 is used for widgets which can expand to cover available
         * area to that maximum.
         *
         * @return the maximum width
         */
        public virtual int GetMaxWidth()
        {
            return _maxWidth;
        }

        /**
         * Returns the maximum height of the widget.
         *
         * A maximum of 0 means that the widgets wants it's preferred size and no
         * extra space from layout.
         * A value &gt; 0 is used for widgets which can expand to cover available
         * area to that maximum.
         *
         * @return the maximum height
         */
        public virtual int GetMaxHeight()
        {
            return _maxHeight;
        }

        /**
         * Sets the maximum size of the widget.
         * A value of 0 means no expansion, use {@link Short#MAX_VALUE} for unbounded expansion.
         *
         * <p>The maximum size is set via the theme in {@link #applyThemeMaxSize(de.matthiasmann.twl.ThemeInfo)}</p>
         * 
         * @param width the maximum width
         * @param height the maximum height
         * @see #getMaxWidth()
         * @see #getMaxHeight()
         * @throws ArgumentOutOfRangeException when width or height is negative
         */
        public virtual void SetMaxSize(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException("negative size");
            }
            _maxWidth = (short)Math.Min(width, short.MaxValue);
            _maxHeight = (short)Math.Min(height, short.MaxValue);
        }

        /**
         * A helper method to compute the size of a widget based on min, max and
         * preferred size.
         *
         * If max size is &gt; 0 then the preferred size is limited to max.
         *
         * @param min the minimum size of the widget
         * @param preferred the preferred size of the widget
         *                  or the available space where the widget is fitted into
         * @param max the maximum size of the widget
         * @return Math.Max(min, (max > 0) ? Math.Min(preferred, max) : preferred)
         */
        public static int ComputeSize(int min, int preferred, int max)
        {
            if (max > 0)
            {
                preferred = Math.Min(preferred, max);
            }
            return Math.Max(min, preferred);
        }

        /**
         * Auto adjust the size of this widget based on it's preferred size.
         * 
         * Subclasses can provide more functionality
         */
        public virtual void AdjustSize()
        {
            /*
            System.out.println(this+" minSize="+getMinWidth()+","+getMinHeight()+
                    " prefSize="+getPreferredWidth()+","+getPreferredHeight()+
                    " maxSize="+getMaxWidth()+","+getMaxHeight());
             * */
            SetSize(ComputeSize(GetMinWidth(), GetPreferredWidth(), GetMaxWidth()),
                    ComputeSize(GetMinHeight(), GetPreferredHeight(), GetMaxHeight()));
            ValidateLayout();
        }

        /**
         * Called when something has changed which affected the layout of this widget.
         *
         * The default implementation calls invalidateLayoutLocally() followed by childInvalidateLayout()
         *
         * Called by the default implementation of borderChanged.
         *
         * @see #invalidateLayoutLocally()
         * @see #borderChanged()
         */
        public virtual void InvalidateLayout()
        {
            if (_layoutInvalid < LAYOUT_INVALID_GLOBAL)
            {
                InvalidateLayoutLocally();
                if (_parent != null)
                {
                    _layoutInvalid = LAYOUT_INVALID_GLOBAL;
                    _parent.ChildInvalidateLayout(this);
                }
            }
        }

        /**
         * Calls layout() if the layout is marked invalid.
         * @see #invalidateLayout()
         * @see #layout()
         */
        public virtual void ValidateLayout()
        {
            if (_layoutInvalid != 0)
            {
                /* Reset the flag first so that widgets like TextArea can invalidate
                 * their layout from inside layout()
                 */
                _layoutInvalid = 0;
                Layout();
            }
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    _children[i].ValidateLayout();
                }
            }
        }

        /**
         * Returns the current theme name of this widget.
         * The default theme name is the lower case simple class name of this widget.
         * @return the current theme name of this widget
         */
        public String GetTheme()
        {
            return _theme;
        }

        /**
         * Changes the theme name of this widget - DOES NOT call reapplyTheme()
         *
         * <p>If the theme name is empty then this widget won't receive theme data
         * and is not included in the theme path, but it's children are still
         * themed.</p>
         *
         * <p>A theme name must not contain spaces or '*'.
         * A '/' is only allowed as first character to indicate an absolute theme path.
         * A '.' is only allowed for absolute theme paths.</p>
         * 
         * @param theme The new theme path element
         * @throws java.lang.NullPointerException if theme is null
         * @throws java.lang.ArgumentOutOfRangeException if the theme name is invalid
         * @see GUI#applyTheme(ThemeManager)
         * @see #reapplyTheme()
         * @see #getThemePath()
         * @see #isAbsoluteTheme(java.lang.String)
         */
        public void SetTheme(String theme)
        {
            if (theme == null)
            {
                throw new ArgumentOutOfRangeException("theme is null");
            }
            if (theme.Length > 0)
            {
                int slashIdx = theme.LastIndexOf('/');
                if (slashIdx > 0)
                {
                    throw new ArgumentOutOfRangeException("'/' is only allowed as first character in theme name");
                }
                if (slashIdx < 0)
                {
                    if (theme.IndexOf('.') >= 0)
                    {
                        throw new ArgumentOutOfRangeException("'.' is only allowed for absolute theme paths");
                    }
                }
                else if (theme.Length == 1)
                {
                    throw new ArgumentOutOfRangeException("'/' requires a theme path");
                }
                for (int i = 0, n = theme.Length; i < n; i++)
                {
                    char ch = theme[i];
                    if (Char.IsControl(ch) || ch == '*')
                    {
                        throw new ArgumentOutOfRangeException("invalid character '" + TextUtil.ToPrintableString(ch) + "' in theme name");
                    }
                }
            }
            this._theme = theme;
        }

        /**
         * Returns this widget's theme path by concatenating the theme names
         * from all parents separated by '.'.
         *
         * If a parent theme is empty then it will be omitted from the theme path.
         *
         * The theme path will start with the first absolute theme starting from
         * this widget up to the GUI.
         *
         * @return the effective theme path - can be empty
         */
        public String GetThemePath()
        {
            return GetThemePath(0).ToString();
        }

        /**
         * Returns true if paint() is clipped to this widget.
         * @return true if paint() is clipped to this widget
         */
        public bool IsClip()
        {
            return _clip;
        }

        /**
         * Sets whether paint() must be clipped to this Widget or not.
         *
         * Clipping is performed for the whole widget and all it's children.
         * The clip area is the outer area of the widget (it does include the border).
         *
         * If the widget theme has effects which extend outside of the widget (like
         * shadow or glow) then clipping will also clip the this effect. A work
         * around is to not apply clipping to the widget itself but to a child
         * which will act as a clip container - this child may not need a theme.
         *
         * @param clip true if clipping must be used - default is false
         **/
        public void SetClip(bool clip)
        {
            this._clip = clip;
        }

        /**
         * Returns if this widget will handle the FOCUS_KEY.
         * @return if this widget will handle the FOCUS_KEY.
         */
        public bool IsFocusKeyEnabled()
        {
            return _focusKeyEnabled;
        }

        /**
         * Controls the handling of the FOCUS_KEY.
         * <p>The default is true.</p>
         * <p>When enabled the focus key (TAB) will cycle through all (indirect)
         * children which can receive keyboard focus. The order is defined
         * by {@link #getKeyboardFocusOrder() }.</p>
         * @param focusKeyEnabled if true this widget will handle the focus key.
         */
        public void SetFocusKeyEnabled(bool focusKeyEnabled)
        {
            this._focusKeyEnabled = focusKeyEnabled;
        }

        /**
         * Returns the current background image or null.
         * @return the current background image or null
         * @see #paintBackground(de.matthiasmann.twl.GUI)
         */
        public Image GetBackground()
        {
            return _background;
        }

        /**
         * Sets the background image that should be drawn before drawing this widget
         * @param background the new background image - can be null
         * @see #paintBackground(de.matthiasmann.twl.GUI)
         */
        public void SetBackground(Image background)
        {
            this._background = background;
        }

        /**
         * Returns the current overlay image or null.
         * @return the current overlay image or null.
         * @see #paintOverlay(de.matthiasmann.twl.GUI)
         */
        public Image GetOverlay()
        {
            return _overlay;
        }

        /**
         * Sets the overlay image that should be drawn after drawing the children
         * @param overlay the new overlay image - can be null
         * @see #paintOverlay(de.matthiasmann.twl.GUI)
         */
        public void SetOverlay(Image overlay)
        {
            this._overlay = overlay;
        }

        /**
         * Returns the mouse cursor which should be used for the given
         * mouse coordinates and modifiers.
         * 
         * The default implementation calls {@link #getMouseCursor() }
         * 
         * @param evt only {@link Event#getMouseX() }, {@link Event#getMouseY() } and {@link Event#getModifiers() } are valid.
         * @return the mouse cursor or null when no mouse cursor is defined for this widget
         */
        public virtual MouseCursor GetMouseCursor(Event evt)
        {
            return GetMouseCursor();
        }

        public virtual MouseCursor GetMouseCursor()
        {
            return _mouseCursor;
        }

        public void SetMouseCursor(MouseCursor mouseCursor)
        {
            this._mouseCursor = mouseCursor;
        }

        /**
         * Returns the number of children in this widget.
         * @return the number of children in this widget
         */
        public int GetNumChildren()
        {
            if (_children != null)
            {
                return _children.Count;
            }
            return 0;
        }

        /**
         * Returns the child at the given index
         * @param index
         * @return the child widget
         * @throws java.lang.IndexOutOfRangeException if the index is invalid
         */
        public Widget GetChild(int index)
        {
            if (_children != null)
            {
                return _children[index];
            }
            throw new IndexOutOfRangeException();
        }

        /**
         * Adds a new child at the end of this widget.
         * This call is equal to <code>insertChild(child, getNumChildren())</code>
         *
         * @param child the child that should be added
         * @throws java.lang.NullPointerException if child is null
         * @throws java.lang.ArgumentOutOfRangeException if the child is already in a tree
         * @see #insertChild(de.matthiasmann.twl.Widget, int)
         * @see #getNumChildren()
         */
        public virtual void Add(Widget child)
        {
            InsertChild(child, GetNumChildren());
        }

        /**
         * Inserts a new child into this widget.
         * The position of the child is treated as relative to this widget and adjusted.
         * If a theme was applied to this widget then this theme is also applied to the new child.
         * 
         * @param child the child that should be inserted
         * @param index the index where it should be inserted
         * @throws java.lang.IndexOutOfRangeException if the index is invalid
         * @throws java.lang.NullPointerException if child is null
         * @throws java.lang.ArgumentOutOfRangeException if the child is already in a tree
         */
        public virtual void InsertChild(Widget child, int index)
        {
            if (child == null)
            {
                throw new ArgumentOutOfRangeException("child is null");
            }
            if (child == this)
            {
                throw new ArgumentOutOfRangeException("can't add to self");
            }
            if (child._parent != null)
            {
                throw new ArgumentOutOfRangeException("child widget already in tree");
            }
            if (_children == null)
            {
                _children = new List<Widget>();
            }
            if (index < 0 || index > _children.Count)
            {
                throw new IndexOutOfRangeException();
            }
            child.SetParent(this);  // can throw exception - see PopupWindow
            _children.Insert(index, child);
            GUI gui = GetGUI();
            if (gui != null)
            {
                child.RecursivelySetGUI(gui);
            }
            AdjustChildPosition(child, _posX + _borderLeft, _posY + _borderTop);
            child.RecursivelyEnabledChanged(null, _enabled);
            if (gui != null)
            {
                child.RecursivelyAddToGUI(gui);
            }
            if (_themeManager != null)
            {
                child.ApplyTheme(_themeManager);
            }
            try
            {
                ChildAdded(child);
            }
            catch (Exception ex)
            {
                GetLogger().Log(Level.SEVERE, "Exception in childAdded()", ex);
            }
            // A newly added child can't have open popups
            // because it needs a GUI for this - and it had no parent up to now
        }

        /**
         * Returns the index of the specified child in this widget.
         * Uses object identity for comparing.
         * @param child the child which index should be returned
         * @return the index of the child or -1 if it was not found
         */
        public int GetChildIndex(Widget child)
        {
            if (_children != null)
            {
                // can't use children.indexOf(child) as this uses equals()
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    if (_children[i] == child)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /**
         * Removes the specified child from this widget.
         * Uses object identity for comparing.
         * @param child the child that should be removed.
         * @return true if the child was found and removed.
         */
        public virtual bool RemoveChild(Widget child)
        {
            int idx = GetChildIndex(child);
            if (idx >= 0)
            {
                RemoveChild(idx);
                return true;
            }
            return false;
        }

        /**
         * Removes the specified child from this widget.
         * The position of the removed child is changed to the relative
         * position to this widget.
         * Calls invalidateLayout after removing the child.
         * 
         * @param index the index of the child
         * @return the removed widget
         * @throws java.lang.IndexOutOfRangeException if the index is invalid
         * @see #invalidateLayout()
         */
        public virtual Widget RemoveChild(int index)
        {
            if (_children != null)
            {
                Widget child = _children[index];
                _children.RemoveAt(index);
                UnparentChild(child);
                if (_lastChildMouseOver == child)
                {
                    _lastChildMouseOver = null;
                }
                if (_focusChild == child)
                {
                    _focusChild = null;
                }
                ChildRemoved(child);
                return child;
            }
            throw new IndexOutOfRangeException();
        }

        /**
         * Removes all children of this widget.
         * The position of the all removed children is changed to the relative
         * position to this widget.
         * Calls allChildrenRemoved after removing all children.
         * 
         * @see #allChildrenRemoved()
         */
        public virtual void RemoveAllChildren()
        {
            if (_children != null)
            {
                _focusChild = null;
                _lastChildMouseOver = null;
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    Widget child = _children[i];
                    UnparentChild(child);
                }
                _children.Clear(); // we expect that new children will be added - so keep list
                if (_hasOpenPopup)
                {
                    GUI gui = GetGUI();
                    System.Diagnostics.Debug.Assert(gui != null);
                    RecalcOpenPopups(gui);
                }
                AllChildrenRemoved();
            }
        }

        /**
         * Clean up GL resources. When overwritten then super method must be called.
         */
        public virtual void Destroy()
        {
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    _children[i].Destroy();
                }
            }
            if (_offscreenSurface != null)
            {
                _offscreenSurface.Dispose();
                _offscreenSurface = null;
            }
        }

        public bool CanAcceptKeyboardFocus()
        {
            return _bCanAcceptKeyboardFocus;
        }

        public void SetCanAcceptKeyboardFocus(bool bCanAcceptKeyboardFocus)
        {
            this._bCanAcceptKeyboardFocus = bCanAcceptKeyboardFocus;
        }

        public bool IsDepthFocusTraversal()
        {
            return _depthFocusTraversal;
        }

        public void SetDepthFocusTraversal(bool depthFocusTraversal)
        {
            this._depthFocusTraversal = depthFocusTraversal;
        }

        /**
         * Requests that the keyboard focus is transfered to this widget.
         *
         * <p>Use with care - users don't expect focus changes while working with the UI</p>
         *
         * <p>Focus transfer only works when the widget is added to the GUI tree.
         * See {@link #getGUI()}.</p>
         * 
         * @return true if keyboard focus was transfered to this widget.
         */
        public virtual bool RequestKeyboardFocus()
        {
            if (_parent != null && _visible)
            {
                if (_parent._focusChild == this)
                {
                    return true;
                }

                bool clear = FocusTransferStart();
                try
                {
                    return _parent.RequestKeyboardFocus(this);
                }
                finally
                {
                    FocusTransferClear(clear);
                }
            }
            return false;
        }

        /**
         * If this widget currently has the keyboard focus, then the keyboard focus is removed.
         * The focus will be transferred to the parent widget.
         */
        public void GiveupKeyboardFocus()
        {
            if (_parent != null && _parent._focusChild == this)
            {
                _parent.RequestKeyboardFocus(null);
            }
        }

        /**
         * Checks if this widget has the keyboard focus
         * @return true if this widget has the keyboard focus
         */
        public bool HasKeyboardFocus()
        {
            if (_parent != null)
            {
                return _parent._focusChild == this;
            }
            return false;
        }

        public bool FocusNextChild()
        {
            return MoveFocus(true, +1);
        }

        public bool FocusPrevChild()
        {
            return MoveFocus(true, -1);
        }

        public bool FocusFirstChild()
        {
            return MoveFocus(false, +1);
        }

        public bool FocusLastChild()
        {
            return MoveFocus(false, -1);
        }

        /**
         * Returns the animation state object.
         * @return the animation state object.
         */
        public AnimationState GetAnimationState()
        {
            return _animState;
        }

        /**
         * Returns true if the animation state of this widget is shared with
         * another widget.
         * 
         * A widget with a shared animation state should normally not modify
         * the animation state itself. How a shared animation state is used
         * depends on the widgets.
         * 
         * @return true if it is shared
         * @see #Widget(de.matthiasmann.twl.AnimationState) 
         */
        public bool HasSharedAnimationState()
        {
            return _sharedAnimState;
        }

        /**
         * Returns the current tine animation object or null if none was set
         * @return the current tine animation object or null if none was set
         */
        public TintAnimator GetTintAnimator()
        {
            return _tintAnimator;
        }

        /**
         * Sets the tint animation object. Can be null to disable tinting.
         * @param tintAnimator the new tint animation object
         */
        public void GetTintAnimator(TintAnimator tintAnimator)
        {
            this._tintAnimator = tintAnimator;
        }

        /**
         * Returns the currently active offscreen rendering delegate or null if none was set
         * @return the currently active offscreen rendering delegate or null if none was set
         */
        public RenderOffscreen GetRenderOffscreen()
        {
            return _renderOffscreen;
        }

        /**
         * Sets set offscreen rendering delegate. Can be null to disable offscreen rendering.
         * @param renderOffscreen the offscreen rendering delegate.
         */
        public void SetRenderOffscreen(RenderOffscreen renderOffscreen)
        {
            this._renderOffscreen = renderOffscreen;
        }


        /**
         * Returns the currently set tooltip content.
         * @return the currently set tooltip content. Can be null.
         */
        public virtual Object GetTooltipContent()
        {
            return _tooltipContent;
        }

        /**
         * Changes the tooltip context. If the tooltip is currently active then
         * it's refreshed with the new content.
         *
         * @param tooltipContent the new tooltip content.
         * @see #updateTooltip()
         * @see #getTooltipContent()
         */
        public virtual void SetTooltipContent(Object tooltipContent)
        {
            this._tooltipContent = tooltipContent;
            UpdateTooltip();
        }

        /**
         * Returns the current input map.
         * @return the current input map or null.
         */
        public InputMap GetInputMap()
        {
            return _inputMap;
        }

        /**
         * Sets the input map for key strokes.
         * 
         * @param inputMap the input map or null.
         * @see #handleKeyStrokeAction(java.lang.String, de.matthiasmann.twl.Event)
         */
        public void SetInputMap(InputMap inputMap)
        {
            this._inputMap = inputMap;
        }

        /**
         * Returns the current action map. If no action map has been set then
         * {@code null} is returned.
         * @return the current action map or null.
         */
        public ActionMap GetActionMap()
        {
            return _actionMap;
        }

        /**
         * Returns the current action map. If no action map has been set then
         * a new one is created and set (setActionMap is not called).
         * @return the current action map (or the new action map).
         */
        public ActionMap GetOrCreateActionMap()
        {
            if (_actionMap == null)
            {
                _actionMap = new ActionMap();
            }
            return _actionMap;
        }

        /**
         * Installs an action map for this widget.
         * @param actionMap the new action map or null.
         */
        public void SetActionMap(ActionMap actionMap)
        {
            this._actionMap = actionMap;
        }

        /**
         * Returns the visible widget at the specified location.
         * Use this method to locate drag&drop tragets.
         *
         * Subclasses can overwrite this method hide implementation details.
         * 
         * @param x the x coordinate
         * @param y the y coordinate
         * @return the widget at that location.
         */
        public virtual Widget GetWidgetAt(int x, int y)
        {
            Widget child = GetChildAt(x, y);
            if (child != null)
            {
                return child.GetWidgetAt(x, y);
            }
            return this;
        }

        //
        // start of API for derived widgets
        //

        /**
         * Apply the given theme.
         * 
         * This method also calls invalidateLayout()
         * 
         * @param themeInfo The theme info for this widget
         */
        protected virtual void ApplyTheme(ThemeInfo themeInfo)
        {
            ApplyThemeBackground(themeInfo);
            ApplyThemeOverlay(themeInfo);
            ApplyThemeBorder(themeInfo);
            ApplyThemeOffscreenExtra(themeInfo);
            ApplyThemeMinSize(themeInfo);
            ApplyThemeMaxSize(themeInfo);
            ApplyThemeMouseCursor(themeInfo);
            ApplyThemeInputMap(themeInfo);
            ApplyThemeTooltip(themeInfo);
            InvalidateLayout();
        }

        protected void ApplyThemeBackground(ThemeInfo themeInfo)
        {
            SetBackground(themeInfo.GetImage("background"));
        }

        protected void ApplyThemeOverlay(ThemeInfo themeInfo)
        {
            SetOverlay(themeInfo.GetImage("overlay"));
        }

        protected void ApplyThemeBorder(ThemeInfo themeInfo)
        {
            SetBorderSize((Border) themeInfo.GetParameterValue("border", false, typeof(Border)));
        }

        protected void ApplyThemeOffscreenExtra(ThemeInfo themeInfo)
        {
            SetOffscreenExtra((Border) themeInfo.GetParameterValue("offscreenExtra", false, typeof(Border)));
        }

        protected void ApplyThemeMinSize(ThemeInfo themeInfo)
        {
            SetMinSize(
                    themeInfo.GetParameter("minWidth", 0),
                    themeInfo.GetParameter("minHeight", 0));
        }

        protected void ApplyThemeMaxSize(ThemeInfo themeInfo)
        {
            SetMaxSize(
                    themeInfo.GetParameter("maxWidth", short.MaxValue),
                    themeInfo.GetParameter("maxHeight", short.MaxValue));
        }

        protected virtual void ApplyThemeMouseCursor(ThemeInfo themeInfo)
        {
            SetMouseCursor(themeInfo.GetMouseCursor("mouseCursor"));
        }

        protected void ApplyThemeInputMap(ThemeInfo themeInfo)
        {
            SetInputMap((InputMap) themeInfo.GetParameterValue("inputMap", false, typeof(InputMap)));
        }

        protected void ApplyThemeTooltip(ThemeInfo themeInfo)
        {
            _themeTooltipContent = themeInfo.GetParameterValue("tooltip", false);
            if (_tooltipContent == null)
            {
                UpdateTooltip();
            }
        }

        protected Object GetThemeTooltipContent()
        {
            return _themeTooltipContent;
        }

        /**
         * Automatic tooltip support.
         *
         * This function is called when the mouse is idle over the widget for a certain time.
         *
         * The default implementation returns the result from {@code getTooltipContent}
         * if it is non null, otherwise the result from {@code getThemeTooltipContent}
         * is returned.
         *
         * This method is not called if the tooltip is already open and the mouse is
         * moved but does not leave this widget. If the tooltip depends on the mouse
         * position then {@code updateTooltip} must be called from {@code handleEvent}.
         *
         * @param mouseX the mouse X coordinate
         * @param mouseY the mouse Y coordinate
         * @return the tooltip message or null if no tooltip is specified.
         * @see #updateTooltip()
         */
        internal virtual Object GetTooltipContentAt(int mouseX, int mouseY)
        {
            Object content = GetTooltipContent();
            if (content == null)
            {
                content = GetThemeTooltipContent();
            }
            return content;
        }

        /**
         * Called by setTooltipContent and applyThemeTooltip.
         * If this widget currently has an open tooltip then this tooltip is updated
         * to show the new content.
         *
         * @see #getTooltipContent()
         */
        protected void UpdateTooltip()
        {
            GUI gui = GetGUI();
            if (gui != null)
            {
                gui.RequestTooltipUpdate(this, false);
            }
        }

        /**
         * If this widget currently has an open tooltip then this tooltip is reset
         * and the tooltip timer is restarted.
         *
         * @see #getTooltipContent()
         */
        protected void ResetTooltip()
        {
            GUI gui = GetGUI();
            if (gui != null)
            {
                gui.RequestTooltipUpdate(this, true);
            }
        }

        /**
         * Installs an action mapping for the given action in the current action map.
         * If no action map is set then a new one will be created.
         *
         * The mapping will invoke a public method on {@code this} widget.
         *
         * This is equal to calling {@code addActionMapping} on {@code ActionMap} with
         * {@code this} as target and {@code ActionMap.FLAG_ON_PRESSED} as flags.
         *
         * @param action the action name
         * @param methodName the method name to invoke on this widget
         * @param params optional parameters which can be passed to the method
         * @see #getActionMap()
         * @see ActionMap#addMapping(java.lang.String, java.lang.Object, java.lang.reflect.Method, java.lang.Object[], int)
         * @see #getInputMap()
         */
        protected void AddActionMapping(String action, String methodName, params Object[] parameters)
        {
            GetOrCreateActionMap().AddMapping(action, this, methodName, parameters, ActionMap.FLAG_ON_PRESSED);
        }

        /**
         * If the widget changed some internal state which may
         * require different theme information then this function
         * can be used to reapply the current theme.
         */
        public void ReapplyTheme()
        {
            if (_themeManager != null)
            {
                ApplyTheme(_themeManager);
            }
        }

        /**
         * Checks whether the mouse is inside the widget or not.
         * <p>Calls {@link #isInside(int, int)} with the mouse coordinates.</p>
         *
         * @param evt the mouse event
         * @return true if the widgets wants to claim this mouse event
         */
        public virtual bool IsMouseInside(Event evt)
        {
            return IsInside(evt.GetMouseX(), evt.GetMouseY());
        }

        /**
         * Called when an event occurred that this widget could be interested in.
         *
         * <p>The default implementation handles only keyboard events and delegates
         * them to the child widget which has keyboard focus.
         * If focusKey handling is enabled then this widget cycles the keyboard
         * focus through it's children.
         * If the key was not consumed by a child or focusKey and an inputMap is
         * specified then the event is translated by the InputMap and
         * <code>handleKeyStrokeAction</code> is called when a mapping was found.</p>
         *
         * <p>If the widget wants to receive mouse events then it must return true
         * for all mouse events except for MOUSE_WHEEL (which is optional) event.
         * Otherwise the following mouse event are not send. Before mouse movement
         * or button events are send a MOUSE_ENTERED event is send first.</p>
         * 
         * @param evt The event - do not store this object - it may be reused
         * @return true if the widget handled this event
         * @see #setFocusKeyEnabled(bool)
         * @see #handleKeyStrokeAction(java.lang.String, de.matthiasmann.twl.Event)
         * @see #setInputMap(de.matthiasmann.twl.InputMap)
         */
        public virtual bool HandleEvent(Event evt)
        {
            if (evt.IsKeyEvent())
            {
                return HandleKeyEvent(evt);
            }
            return false;
        }

        /**
         * Called when a key stroke was found in the inputMap.
         *
         * @param action the action associated with the key stroke
         * @param event the event which caused the action
         * @return true if the action was handled
         * @see #setInputMap(de.matthiasmann.twl.InputMap) 
         */
        protected virtual bool HandleKeyStrokeAction(String action, Event evt)
        {
            if (_actionMap != null)
            {
                return _actionMap.Invoke(action, evt);
            }
            return false;
        }

        /**
         * Moves the child at index from to index to. This will shift the position
         * of all children in between.
         * 
         * @param from the index of the child that should be moved
         * @param to the new index for the child at from
         * @throws java.lang.IndexOutOfRangeException if from or to are invalid
         */
        protected void MoveChild(int from, int to)
        {
            if (_children == null)
            {
                throw new IndexOutOfRangeException();
            }
            if (to < 0 || to >= _children.Count)
            {
                throw new IndexOutOfRangeException("to");
            }
            if (from < 0 || from >= _children.Count)
            {
                throw new IndexOutOfRangeException("from");
            }
            Widget child = _children[from];
            _children.RemoveAt(from);
            _children.Insert(to, child);
        }

        /**
         * A child requests keyboard focus.
         * Default implementation will grant keyboard focus and
         * request itself keyboard focus.
         *
         * @param child The child that wants keyboard focus
         * @return true if the child received the focus.
         */
        protected virtual bool RequestKeyboardFocus(Widget child)
        {
            if (child != null && child._parent != this)
            {
                throw new ArgumentOutOfRangeException("not a direct child");
            }
            if (_focusChild != child)
            {
                if (child == null)
                {
                    RecursivelyChildFocusLost(_focusChild);
                    _focusChild = null;
                    KeyboardFocusChildChanged(null);
                }
                else
                {
                    bool clear = FocusTransferStart();
                    try
                    {
                        // first request focus for ourself
                        {
                            FocusGainedCause savedCause = _focusGainedCause;
                            if (savedCause == null)
                            {
                                _focusGainedCause = FocusGainedCause.ChildFocused;
                            }
                            try
                            {
                                if (!RequestKeyboardFocus())
                                {
                                    return false;
                                }
                            }
                            finally
                            {
                                _focusGainedCause = savedCause;
                            }
                        }

                        // second change focused child
                        RecursivelyChildFocusLost(_focusChild);
                        _focusChild = child;
                        KeyboardFocusChildChanged(child);
                        if (!child._sharedAnimState)
                        {
                            child._animState.SetAnimationState(STATE_KEYBOARD_FOCUS, true);
                        }

                        // last inform the child widget why it gained keyboard focus
                        FocusGainedCause cause = child._focusGainedCause;
                        Widget[] fti = focusTransferInfo.Value;
                        child.KeyboardFocusGained(
                                (cause != null) ? cause : FocusGainedCause.Manual,
                                (fti != null) ? fti[0] : null);
                    }
                    finally
                    {
                        FocusTransferClear(clear);
                    }
                }
            }
            if (!_sharedAnimState)
            {
                _animState.SetAnimationState(STATE_HAS_FOCUSED_CHILD, _focusChild != null);
            }
            return _focusChild != null;
        }

        /**
         * Called when this widget is removed from the GUI tree.
         * After this call getGUI() will return null.
         * 
         * @param gui the GUI object - same as getGUI()
         * @see #getGUI()
         */
        protected virtual void BeforeRemoveFromGUI(GUI gui)
        {
        }

        /**
         * Called after this widget has been added to a GUI tree.
         * 
         * @param gui the GUI object - same as getGUI()
         * @see #getGUI()
         */
        protected virtual void AfterAddToGUI(GUI gui)
        {
        }

        /**
         * Called when the layoutInvalid flag is set.
         *
         * The default implementation does nothing.
         */
        protected virtual void Layout()
        {
        }

        /**
         * Called when the position of this widget was changed.
         * The default implementation does nothing.
         * 
         * Child positions are already updated to retain the absolute
         * coordinate system. This has the side effect of firing child's
         * positionChanged before the parent's.
         */
        protected virtual void PositionChanged()
        {
        }

        /**
         * Called when the size of this widget has changed.
         * The default implementation calls invalidateLayoutLocally. As size changes
         * are normally the result of the parent's layout() function.
         * 
         * @see #invalidateLayoutLocally()
         */
        protected virtual void SizeChanged()
        {
            InvalidateLayoutLocally();
        }

        /**
         * Called when the border size has changed.
         * The default implementation calls invalidateLayout.
         * 
         * @see #invalidateLayout()
         */
        protected virtual void borderChanged()
        {
            InvalidateLayout();
        }

        /**
         * Called when the layout of a child has been invalidated.
         * The default implementation calls invalidateLayout.
         *
         * @param child the child which was invalidated
         * @see #invalidateLayout()
         */
        protected virtual void ChildInvalidateLayout(Widget child)
        {
            InvalidateLayout();
        }

        /**
         * A new child has been added.
         * The default implementation calls invalidateLayout.
         *
         * @param child the new child
         * @see #invalidateLayout()
         */
        protected virtual void ChildAdded(Widget child)
        {
            System.Diagnostics.Debug.WriteLine("Child added !!! " + child.GetType().FullName + " - " + child.GetThemePath());
            InvalidateLayout();
        }

        /**
         * A child has been removed.
         * The default implementation calls invalidateLayout.
         * 
         * @param exChild the removed widget - no longer a child
         * @see #invalidateLayout()
         */
        protected virtual void ChildRemoved(Widget exChild)
        {
            InvalidateLayout();
        }

        /**
         * All children have been removed.
         * This is called by {@code removeAllChildren} instead of {@code childRemoved}.
         * 
         * The default implementation calls invalidateLayout.
         * 
         * @see #invalidateLayout()
         */
        protected virtual void AllChildrenRemoved()
        {
            InvalidateLayout();
        }

        /**
         * Called when the visibility state of a child was changed.
         * The default implementation does nothing.
         * 
         * @param child the child which changed it's visibility state
         * @see #setVisible(bool) 
         */
        protected virtual void ChildVisibilityChanged(Widget child)
        {
        }

        /**
         * The current keyboard focus child has changed.
         * The default implementation does nothing.
         * 
         * @param child The child which has now the keyboard focus in this hierachy level or null
         */
        protected virtual void KeyboardFocusChildChanged(Widget child)
        {
        }

        /**
         * Called when this widget has lost the keyboard focus.
         * The default implementation does nothing.
         */
        protected virtual void KeyboardFocusLost()
        {
        }

        /**
         * Called when this widget has gained the keyboard focus.
         * The default implementation does nothing.
         *
         * @see #keyboardFocusGained(de.matthiasmann.twl.FocusGainedCause, de.matthiasmann.twl.Widget) 
         */
        protected virtual void KeyboardFocusGained()
        {
        }

        /**
         * Called when this widget has gained the keyboard focus.
         * The default implementation calls {@link #keyboardFocusGained() }
         *
         * @param cause the cause for the this focus transfer
         * @param previousWidget the widget which previously had the keyboard focus - can be null.
         */
        protected virtual void KeyboardFocusGained(FocusGainedCause cause, Widget previousWidget)
        {
            // System.out.println(this + " " + cause + " " + previousWidget);
            KeyboardFocusGained();
        }

        /**
         * This method is called when this widget has been disabled,
         * either directly or one of it's parents.
         *
         * <p>The default implementation does nothing.</p>
         */
        internal virtual void WidgetDisabled()
        {
        }

        /**
         * Paints this widget and it's children.
         * <p>A subclass should overwrite paintWidget() instead of this function.</p>
         * 
         * <p>The default implementation calls the following method in order:</p><ol>
         * <li>{@link #paintBackground(de.matthiasmann.twl.GUI)}</li>
         * <li>{@link #paintWidget(de.matthiasmann.twl.GUI)}</li>
         * <li>{@link #paintChildren(de.matthiasmann.twl.GUI)}</li>
         * <li>{@link #paintOverlay(de.matthiasmann.twl.GUI)}</li></ol>
         *
         * @param gui the GUI object
         */
        protected virtual void Paint(GUI gui)
        {
            PaintBackground(gui);
            PaintWidget(gui);
            PaintChildren(gui);
            PaintOverlay(gui);
        }

        /**
         * Called by {@link #paint(de.matthiasmann.twl.GUI)} after painting the
         * background and before painting all children.
         * 
         * <p>This should be overwritten instead of {@code paint} if normal themeable
         * painting is desired by the subclass.</p>
         * 
         * <p>The default implementation does nothing.</p>
         * 
         * @param gui the GUI object - it's the same as getGUI()
         */
        protected virtual void PaintWidget(GUI gui)
        {
        }

        /**
         * Paint the background image of this widget.
         * @param gui the GUI object
         * @see #paint(de.matthiasmann.twl.GUI) 
         */
        protected void PaintBackground(GUI gui)
        {
            Image bgImage = GetBackground();
            if (bgImage != null)
            {
                bgImage.Draw(GetAnimationState(), _posX, _posY, _width, _height);
            }
        }

        /**
         * Paints the overlay image of this widget.
         * @param gui the GUI object
         * @see #paint(de.matthiasmann.twl.GUI) 
         */
        protected virtual void PaintOverlay(GUI gui)
        {
            Image ovImage = GetOverlay();
            if (ovImage != null)
            {
                ovImage.Draw(GetAnimationState(), _posX, _posY, _width, _height);
            }
        }

        /**
         * Paints all children in index order. Invisible children are skipped.
         * @param gui the GUI object
         * @see #paint(de.matthiasmann.twl.GUI) 
         */
        protected void PaintChildren(GUI gui)
        {
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    Widget child = _children[i];
                    if (child._visible)
                    {
                        child.DrawWidget(gui);
                    }
                }
            }
        }

        /**
         * Paints a specified child. Does not check for visibility.
         *
         * @param gui the GUI object
         * @param child the child Widget
         */
        protected void PaintChild(GUI gui, Widget child)
        {
            if (child._parent != this)
            {
                throw new ArgumentOutOfRangeException("can only render direct children");
            }
            child.DrawWidget(gui);
        }

        /**
         * Called after all other widgets have been rendered when a drag operation is in progress.
         * The mouse position can be outsife of this widget
         * 
         * @param gui the GUI object
         * @param mouseX the current mouse X position
         * @param mouseY the current mouse Y position
         * @param modifier the current active modifiers - see {@link Event#getModifiers() }
         */
        internal void PaintDragOverlay(GUI gui, int mouseX, int mouseY, int modifier)
        {
        }

        /**
         * Invalidates only the layout of this widget. Does not invalidate the layout of the parent.
         * Should only be used for things like scrolling.
         *
         * This method is called by sizeChanged()
         * 
         * @see #sizeChanged()
         */
        protected void InvalidateLayoutLocally()
        {
            if (_layoutInvalid < LAYOUT_INVALID_LOCAL)
            {
                _layoutInvalid = LAYOUT_INVALID_LOCAL;
                GUI gui = GetGUI();
                if (gui != null)
                {
                    gui._hasInvalidLayouts = true;
                }
            }
        }

        /**
         * Sets size and position of a child widget so that it consumes the complete
         * inner area.
         *
         * @param child A child widget
         */
        protected internal void LayoutChildFullInnerArea(Widget child)
        {
            if (child._parent != this)
            {
                throw new ArgumentOutOfRangeException("can only layout direct children");
            }
            child.SetPosition(GetInnerX(), GetInnerY());
            child.SetSize(GetInnerWidth(), GetInnerHeight());
        }

        /**
         * Sets size and position of all child widgets so that they all consumes the
         * complete inner area. If there is more then one child then they will overlap.
         */
        protected void LayoutChildrenFullInnerArea()
        {
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    LayoutChildFullInnerArea(_children[i]);
                }
            }
        }

        /**
         * Returns all children of this widget in their focus travel order.
         * <p>The returned list is only iterated and not stored.</p>
         * <p>The default implementation just returns an unmodifable view of
         * the internal children list.</p>
         * @return a read only collection with all children in focus order.
         */
        protected ICollection<Widget> GetKeyboardFocusOrder()
        {
            if (_children == null)
            {
                return new List<Widget>();
            }
            return new ReadOnlyCollection<Widget>(_children);
        }

        private int CollectFocusOrderList(List<Widget> list)
        {
            int idx = -1;
            foreach (Widget child in GetKeyboardFocusOrder())
            {
                if (child._visible && child.IsEnabled())
                {
                    if (child._bCanAcceptKeyboardFocus)
                    {
                        if (child == _focusChild)
                        {
                            idx = list.Count;
                        }
                        list.Add(child);
                    }
                    if (child._depthFocusTraversal)
                    {
                        int subIdx = child.CollectFocusOrderList(list);
                        if (subIdx != -1)
                        {
                            idx = subIdx;
                        }
                    }
                }
            }
            return idx;
        }

        private bool MoveFocus(bool relative, int dir)
        {
            List<Widget> focusList = new List<Widget>();
            int curIndex = CollectFocusOrderList(focusList);
            if (focusList.Count == 0)
            {
                return false;
            }
            if (dir < 0)
            {
                if (!relative || --curIndex < 0)
                {
                    curIndex = focusList.Count - 1;
                }
            }
            else if (!relative || ++curIndex >= focusList.Count)
            {
                curIndex = 0;
            }
            Widget widget = focusList[curIndex];
            try
            {
                widget._focusGainedCause = FocusGainedCause.FocusKey;
                widget.RequestKeyboardFocus(null);
                widget.RequestKeyboardFocus();
            }
            finally
            {
                widget._focusGainedCause = FocusGainedCause.None;
            }
            return true;
        }

        private bool FocusTransferStart()
        {
            Widget[] fti = focusTransferInfo.Value;
            if (fti == null)
            {
                Widget root = GetRootWidget();
                Widget w = root;
                while (w._focusChild != null)
                {
                    w = w._focusChild;
                }
                if (w == root)
                {
                    w = null;
                }
                focusTransferInfo.Value = new Widget[] { w };
                return true;
            }
            return false;
        }

        private void FocusTransferClear(bool clear)
        {
            if (clear)
            {
                focusTransferInfo.Value = null;
            }
        }

        /**
         * Returns the visible child widget which is at the specified coordinate.
         *
         * @param x the x coordinate
         * @param y the y coordinate
         * @return the child widget at that location or null if there is no visible child.
         * @see #getX()
         * @see #getY()
         */
        protected Widget GetChildAt(int x, int y)
        {
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    Widget child = _children[i];
                    if (child._visible && child.IsInside(x, y))
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        /**
         * Updates the tint animation when a fade is active.
         * 
         * Can be overridden to do additional things like hide the widget
         * after the end of the animation.
         */
        protected virtual void UpdateTintAnimation()
        {
            _tintAnimator.Update();
        }

        /**
         * Fire an existing PropertyChangeEvent to any registered listeners.
         *
         * @param evt The PropertyChangeEvent object
         * @see PropertyChangeSupport#firePropertyChange(java.beans.PropertyChangeEvent)
         */
        protected void FirePropertyChange(PropertyChangeEvent evt)
        {
            if (_propertyChangeSupport != null)
            {
                _propertyChangeSupport.FirePropertyChange(evt);
            }
        }

        /**
         * Report a bound property update to any registered listeners.
         *
         * @param propertyName The programmatic name of the property that was changed
         * @param oldValue The old value of the property
         * @param newValue The new value of the property
         * @see PropertyChangeSupport#firePropertyChange(java.lang.String, bool, bool)
         */
        protected void FirePropertyChange(String propertyName, bool oldValue, bool newValue)
        {
            if (_propertyChangeSupport != null)
            {
                _propertyChangeSupport.FirePropertyChange(propertyName, oldValue, newValue);
            }
        }

        /**
         * Report a bound property update to any registered listeners.
         *
         * @param propertyName The programmatic name of the property that was changed
         * @param oldValue The old value of the property
         * @param newValue The new value of the property
         * @see PropertyChangeSupport#firePropertyChange(java.lang.String, int, int) 
         */
        protected void FirePropertyChange(String propertyName, int oldValue, int newValue)
        {
            if (_propertyChangeSupport != null)
            {
                _propertyChangeSupport.FirePropertyChange(propertyName, oldValue, newValue);
            }
        }

        /**
         * Report a bound property update to any registered listeners.
         *
         * @param propertyName The programmatic name of the property that was changed
         * @param oldValue The old value of the property
         * @param newValue The new value of the property
         * @see PropertyChangeSupport#firePropertyChange(java.lang.String, java.lang.Object, java.lang.Object)
         */
        protected void FirePropertyChange(String propertyName, Object oldValue, Object newValue)
        {
            if (_propertyChangeSupport != null)
            {
                _propertyChangeSupport.FirePropertyChange(propertyName, oldValue, newValue);
            }
        }

        //
        // start of internal stuff
        //

        internal virtual void SetParent(Widget parent)
        {
            this._parent = parent;
        }

        private void UnparentChild(Widget child)
        {
            GUI gui = GetGUI();
            if (child._hasOpenPopup)
            {
                System.Diagnostics.Debug.Assert(gui != null);
                gui.ClosePopupFromWidgets(child);
            }
            RecursivelyChildFocusLost(child);
            if (gui != null)
            {
                child.RecursivelyRemoveFromGUI(gui);
            }
            child.RecursivelyClearGUI(gui);
            child._parent = null;
            try
            {
                child.Destroy();
            }
            catch (Exception ex)
            {
                GetLogger().Log(Level.SEVERE, "Exception in destroy()", ex);
            }
            AdjustChildPosition(child, -_posX, -_posY);
            child.RecursivelyEnabledChanged(null, child._locallyEnabled);
        }

        private void RecursivelySetGUI(GUI gui)
        {
            System.Diagnostics.Debug.Assert(_guiInstance == null);
            _guiInstance = gui;
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    _children[i].RecursivelySetGUI(gui);
                }
            }
        }

        private void RecursivelyAddToGUI(GUI gui)
        {
            System.Diagnostics.Debug.Assert(_guiInstance == gui);
            if (_layoutInvalid != 0)
            {
                gui._hasInvalidLayouts = true;
            }
            if (!_sharedAnimState)
            {
                _animState.SetGUI(gui);
            }
            try
            {
                AfterAddToGUI(gui);
            }
            catch (Exception ex)
            {
                GetLogger().Log(Level.SEVERE, "Exception in afterAddToGUI()", ex);
            }
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    _children[i].RecursivelyAddToGUI(gui);
                }
            }
        }

        private void RecursivelyClearGUI(GUI gui)
        {
            System.Diagnostics.Debug.Assert( _guiInstance == gui);
            _guiInstance = null;
            _themeManager = null;
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    _children[i].RecursivelyClearGUI(gui);
                }
            }
        }

        private void RecursivelyRemoveFromGUI(GUI gui)
        {
            System.Diagnostics.Debug.Assert(_guiInstance == gui);
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    _children[i].RecursivelyRemoveFromGUI(gui);
                }
            }
            _focusChild = null;
            if (!_sharedAnimState)
            {
                _animState.SetGUI(null);
            }
            try
            {
                BeforeRemoveFromGUI(gui);
            }
            catch (Exception ex)
            {
                GetLogger().Log(Level.SEVERE, "Exception in beforeRemoveFromGUI()", ex);
            }
        }

        private void RecursivelyChildFocusLost(Widget w)
        {
            while (w != null)
            {
                Widget next = w._focusChild;
                if (!w._sharedAnimState)
                {
                    w._animState.SetAnimationState(STATE_KEYBOARD_FOCUS, false);
                }
                try
                {
                    w.KeyboardFocusLost();
                }
                catch (Exception ex)
                {
                    GetLogger().Log(Level.SEVERE, "Exception in keyboardFocusLost()", ex);
                }
                w._focusChild = null;
                w = next;
            }
        }

        private void RecursivelyEnabledChanged(GUI gui, bool enabled)
        {
            enabled &= _locallyEnabled;
            if (this._enabled != enabled)
            {
                this._enabled = enabled;
                if (!_sharedAnimState)
                {
                    GetAnimationState().SetAnimationState(STATE_DISABLED, !enabled);
                }
                if (!enabled)
                {
                    if (gui != null)
                    {
                        gui.WidgetDisabled(this);
                    }
                    try
                    {
                        WidgetDisabled();
                    }
                    catch (Exception ex)
                    {
                        GetLogger().Log(Level.SEVERE, "Exception in widgetDisabled()", ex);
                    }
                    try
                    {
                        GiveupKeyboardFocus();
                    }
                    catch (Exception ex)
                    {
                        GetLogger().Log(Level.SEVERE, "Exception in giveupKeyboardFocus()", ex);
                    }
                }
                try
                {
                    FirePropertyChange("enabled", !enabled, enabled);
                }
                catch (Exception ex)
                {
                    GetLogger().Log(Level.SEVERE, "Exception in firePropertyChange(\"enabled\")", ex);
                }
                if (_children != null)
                {
                    for (int i = _children.Count; i-- > 0;)
                    {
                        Widget child = _children[i];
                        child.RecursivelyEnabledChanged(gui, enabled);
                    }
                }
            }
        }

        private void ChildHidden(Widget child)
        {
            if (_focusChild == child)
            {
                RecursivelyChildFocusLost(_focusChild);
                _focusChild = null;
            }
            if (_lastChildMouseOver == child)
            {
                _lastChildMouseOver = null;
            }
        }

        internal void SetOpenPopup(GUI gui, bool hasOpenPopup)
        {
            if (this._hasOpenPopup != hasOpenPopup)
            {
                this._hasOpenPopup = hasOpenPopup;
                if (!_sharedAnimState)
                {
                    GetAnimationState().SetAnimationState(STATE_HAS_OPEN_POPUPS, hasOpenPopup);
                }
                if (_parent != null)
                {
                    if (hasOpenPopup)
                    {
                        _parent.SetOpenPopup(gui, true);
                    }
                    else
                    {
                        _parent.RecalcOpenPopups(gui);
                    }
                }
            }
        }

        internal void RecalcOpenPopups(GUI gui)
        {
            // 1) check self
            if (gui.HasOpenPopups(this))
            {
                SetOpenPopup(gui, true);
                return;
            }
            // 2) check children (don't compute, just check the flag)
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    if (_children[i]._hasOpenPopup)
                    {
                        SetOpenPopup(gui, true);
                        return;
                    }
                }
            }
            SetOpenPopup(gui, false);
        }

        internal bool IsLayoutInvalid()
        {
            return _layoutInvalid != 0;
        }

        internal void DrawWidget(GUI gui)
        {
            if (_renderOffscreen != null)
            {
                DrawWidgetOffscreen(gui);
                return;
            }
            if (_tintAnimator != null && _tintAnimator.HasTint())
            {
                DrawWidgetTint(gui);
                return;
            }
            if (_clip)
            {
                DrawWidgetClip(gui);
                return;
            }
            Paint(gui);
        }

        private void DrawWidgetTint(GUI gui)
        {
            if (_tintAnimator.IsFadeActive())
            {
                UpdateTintAnimation();
            }
            Renderer.Renderer renderer = gui.GetRenderer();
            _tintAnimator.PaintWithTint(renderer);
            try
            {
                if (_clip)
                {
                    DrawWidgetClip(gui);
                }
                else
                {
                    Paint(gui);
                }
            }
            finally
            {
                renderer.PopGlobalTintColor();
            }
        }

        private void DrawWidgetClip(GUI gui)
        {
            Renderer.Renderer renderer = gui.GetRenderer();
            renderer.ClipEnter(_posX, _posY, _width, _height);
            try
            {
                Paint(gui);
            }
            finally
            {
                renderer.ClipLeave();
            }
        }

        private void DrawWidgetOffscreen(GUI gui)
        {
            RenderOffscreen ro = this._renderOffscreen;
            Renderer.Renderer renderer = gui.GetRenderer();
            OffscreenRenderer offscreenRenderer = renderer.OffscreenRenderer;
            if (offscreenRenderer != null)
            {
                int extraTop = _offscreenExtraTop;
                int extraLeft = _offscreenExtraLeft;
                int extraRight = _offscreenExtraRight;
                int extraBottom = _offscreenExtraBottom;
                int[] effectExtra = ro.GetEffectExtraArea(this);
                if (effectExtra != null)
                {
                    extraTop += effectExtra[0];
                    extraLeft += effectExtra[1];
                    extraRight += effectExtra[2];
                    extraBottom += effectExtra[3];
                }
                if (_offscreenSurface != null && !ro.NeedPainting(gui, _parent, _offscreenSurface))
                {
                    ro.PaintOffscreenSurface(gui, this, _offscreenSurface);
                    return;
                }
                _offscreenSurface = offscreenRenderer.StartOffscreenRendering(
                        this, _offscreenSurface, _posX - extraLeft, _posY - extraTop,
                        _width + extraLeft + extraRight, _height + extraTop + extraBottom);
                if (_offscreenSurface != null)
                {
                    try
                    {
                        if (_tintAnimator != null && _tintAnimator.HasTint())
                        {
                            DrawWidgetTint(gui);
                        }
                        else
                        {
                            Paint(gui);
                        }
                    }
                    finally
                    {
                        offscreenRenderer.EndOffscreenRendering();
                    }
                    ro.PaintOffscreenSurface(gui, this, _offscreenSurface);
                    return;
                }
            }
            _renderOffscreen = null;
            ro.OffscreenRenderingFailed(this);
            DrawWidget(gui);
        }

        internal virtual Widget GetWidgetUnderMouse()
        {
            if (!_visible)
            {
                return null;
            }
            Widget w = this;
            while (w._lastChildMouseOver != null && w._visible)
            {
                w = w._lastChildMouseOver;
            }
            return w;
        }

        private static void AdjustChildPosition(Widget child, int deltaX, int deltaY)
        {
            child.SetPositionImpl(child._posX + deltaX, child._posY + deltaY);
        }

        bool SetPositionImpl(int x, int y)
        {
            int deltaX = x - _posX;
            int deltaY = y - _posY;
            if (deltaX != 0 || deltaY != 0)
            {
                this._posX = x;
                this._posY = y;

                if (_children != null)
                {
                    for (int i = 0, n = _children.Count; i < n; i++)
                    {
                        AdjustChildPosition(_children[i], deltaX, deltaY);
                    }
                }

                PositionChanged();

                if (_propertyChangeSupport != null)
                {
                    FirePropertyChange("x", x - deltaX, x);
                    FirePropertyChange("y", y - deltaY, y);
                }
                return true;
            }
            return false;
        }

        public virtual void ApplyTheme(ThemeManager themeManager)
        {
            this._themeManager = themeManager;

            String themePath = GetThemePath();
            System.Diagnostics.Debug.WriteLine("Widget@applyTheme with ThemeManager : " + this.GetType().FullName + " '" + themePath + "'");
            if (themePath.Length == 0)
            {
                if (_children != null)
                {
                    for (int i = 0, n = _children.Count; i < n; i++)
                    {
                        _children[i].ApplyTheme(themeManager);
                    }
                }
                return;
            }

            DebugHook hook = DebugHook.getDebugHook();
            hook.BeforeApplyTheme(this);

            ThemeInfo themeInfo = null;
            try
            {
                themeInfo = themeManager.FindThemeInfo(themePath);
                if (themeInfo != null && _theme.Length > 0)
                {
                    try
                    {
                        ApplyTheme(themeInfo);
                    }
                    catch (Exception ex)
                    {
                        GetLogger().Log(Level.SEVERE, "Exception in applyTheme()", ex);
                    }
                }
            }
            finally
            {
                hook.AfterApplyTheme(this);
            }

            ApplyThemeToChildren(themeManager, themeInfo, hook);
        }

        /**
         * Checks if the given theme name is absolute or relative to it's parent.
         * An absolute theme name starts with a '/'.
         * 
         * @param theme the theme name or path.
         * @return true if the theme is absolute.
         */
        public static bool IsAbsoluteTheme(String theme)
        {
            return theme.Length > 1 && theme[0] == '/';
        }

        private void ApplyThemeImpl(ThemeManager themeManager, ThemeInfo themeInfo, DebugHook hook)
        {
            System.Diagnostics.Debug.WriteLine("Widget@applyThemeImpl with ThemeManager : " + this.GetType().FullName + " '" + this._theme + "'");
            this._themeManager = themeManager;
            if (_theme.Length > 0)
            {
                hook.BeforeApplyTheme(this);
                try
                {
                    if (IsAbsoluteTheme(_theme))
                    {
                        themeInfo = themeManager.FindThemeInfo(_theme.Substring(1));
                    }
                    else
                    {
                        themeInfo = themeInfo.GetChildTheme(_theme);
                    }
                    if (themeInfo != null)
                    {
                        try
                        {
                            ApplyTheme(themeInfo);
                        }
                        catch (Exception ex)
                        {
                            GetLogger().Log(Level.SEVERE, "Exception in applyTheme()", ex);
                        }
                    }
                }
                finally
                {
                    hook.AfterApplyTheme(this);
                }
            }
            ApplyThemeToChildren(themeManager, themeInfo, hook);
        }

        private void ApplyThemeToChildren(ThemeManager themeManager, ThemeInfo themeInfo, DebugHook hook)
        {
            System.Diagnostics.Debug.WriteLine("Widget@applyThemeToChildren with ThemeManager : " + this._theme);
            if (_children != null && themeInfo != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    Widget child = _children[i];
                    child.ApplyThemeImpl(themeManager, themeInfo, hook);
                }
            }
        }

        private StringBuilder GetThemePath(int length)
        {
            StringBuilder sb;
            length += _theme.Length;
            bool abs = IsAbsoluteTheme(_theme);
            if (_parent != null && !abs)
            {
                sb = _parent.GetThemePath(length + 1);
                if (_theme.Length > 0 && sb.Length > 0)
                {
                    sb.Append('.');
                }
            }
            else
            {
                sb = new StringBuilder(length);
            }
            if (abs)
            {
                return sb.Append(_theme.Substring(1));
            }
            return sb.Append(_theme);
        }

        internal Event TranslateMouseEvent(Event evt)
        {
            if (_renderOffscreen is OffscreenMouseAdjustments)
            {
                int[] newXY = ((OffscreenMouseAdjustments)_renderOffscreen).AdjustMouseCoordinates(this, evt);
                evt = evt.CreateSubEvent(newXY[0], newXY[1]);
            }
            return evt;
        }

        internal virtual Widget RouteMouseEvent(Event evt)
        {
            System.Diagnostics.Debug.Assert(!evt.IsMouseDragEvent());
            evt = TranslateMouseEvent(evt);
            if (_children != null)
            {
                for (int i = _children.Count; i-- > 0;)
                {
                    Widget child = _children[i];
                    if (child._visible && child.IsMouseInside(evt))
                    {
                        // we send the real event only only if we can transfer the mouse "focus" to this child
                        if (SetMouseOverChild(child, evt))
                        {
                            if (evt.GetEventType() == EventType.MOUSE_ENTERED ||
                                    evt.GetEventType() == EventType.MOUSE_EXITED)
                            {
                                return child;
                            }
                            Widget result = child.RouteMouseEvent(evt);
                            if (result != null)
                            {
                                // need to check if the focus was transfered to this child or its descendants
                                // if not we need to transfer focus on mouse click here
                                // this can happen if we click on a widget which doesn't want the keyboard focus itself
                                if (evt.GetEventType() == EventType.MOUSE_BTNDOWN && _focusChild != child)
                                {
                                    try
                                    {
                                        child._focusGainedCause = FocusGainedCause.MouseBtnDown;
                                        if (child.IsEnabled() && child.CanAcceptKeyboardFocus())
                                        {
                                            RequestKeyboardFocus(child);
                                        }
                                    }
                                    finally
                                    {
                                        child._focusGainedCause = FocusGainedCause.None;
                                    }
                                }
                                return result;
                            }
                            // widget no longer wants mouse events
                        }
                        // found a widget - but it doesn't want mouse events
                        // so assumes it's "invisible" for the mouse
                    }
                }
            }

            // the following code is only executed for the widget which received
            // the click event. That's why we can call {@code requestKeyboardFocus(null)}
            if (evt.GetEventType() == EventType.MOUSE_BTNDOWN && IsEnabled() && CanAcceptKeyboardFocus())
            {
                try
                {
                    _focusGainedCause = FocusGainedCause.MouseBtnDown;
                    if (_focusChild == null)
                    {
                        RequestKeyboardFocus();
                    }
                    else
                    {
                        RequestKeyboardFocus(null);
                    }
                }
                finally
                {
                    _focusGainedCause = FocusGainedCause.None;
                }
            }
            if (evt.GetEventType() != EventType.MOUSE_WHEEL)
            {
                // no child has mouse over
                SetMouseOverChild(null, evt);
            }
            if (!IsEnabled() && IsMouseAction(evt))
            {
                return this;
            }
            if (HandleEvent(evt))
            {
                return this;
            }
            return null;
        }

        internal static bool IsMouseAction(Event evt)
        {
            EventType type = evt.GetEventType();
            return type == EventType.MOUSE_BTNDOWN ||
                    type == EventType.MOUSE_BTNUP ||
                    type == EventType.MOUSE_CLICKED ||
                    type == EventType.MOUSE_DRAGGED;
        }

        internal void RoutePopupEvent(Event evt)
        {
            HandleEvent(evt);
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    _children[i].RoutePopupEvent(evt);
                }
            }
        }

        static bool GetSafeBooleanProperty(String name)
        {
            try
            {
                return bool.Parse(name);
            }
            catch (FormatException ex)
            {
                return false;
            }
        }

        private static bool WARN_ON_UNHANDLED_ACTION = GetSafeBooleanProperty("warnOnUnhandledAction");

        private bool HandleKeyEvent(Event evt)
        {
            if (_children != null)
            {
                if (_focusKeyEnabled && _guiInstance != null)
                {
                    _guiInstance.SetFocusKeyWidget(this);
                }
                if (_focusChild != null && _focusChild.IsVisible())
                {
                    if (_focusChild.HandleEvent(evt))
                    {
                        return true;
                    }
                }
            }
            if (_inputMap != null)
            {
                String action = _inputMap.MapEvent(evt);
                if (action != null)
                {
                    if (HandleKeyStrokeAction(action, evt))
                    {
                        return true;
                    }
                    if (WARN_ON_UNHANDLED_ACTION)
                    {
                        Logger.GetLogger(this.GetType()).Log(Level.WARNING, "Unhandled action '" + action + "' for class '" + this.GetType().FullName + "'");
                    }
                }
            }
            return false;
        }

        internal void HandleFocusKeyEvent(Event evt)
        {
            if (evt.IsKeyPressedEvent())
            {
                if ((evt.GetModifiers() & Event.MODIFIER_SHIFT) != 0)
                {
                    FocusPrevChild();
                }
                else
                {
                    FocusNextChild();
                }
            }
        }

        internal bool SetMouseOverChild(Widget child, Event evt)
        {
            if (_lastChildMouseOver != child)
            {
                if (child != null)
                {
                    Widget result = child.RouteMouseEvent(evt.CreateSubEvent(EventType.MOUSE_ENTERED));
                    if (result == null)
                    {
                        // this child widget doesn't want mouse events
                        return false;
                    }
                }
                if (_lastChildMouseOver != null)
                {
                    _lastChildMouseOver.RouteMouseEvent(evt.CreateSubEvent(EventType.MOUSE_EXITED));
                }
                _lastChildMouseOver = child;
            }
            return true;
        }

        internal void CollectLayoutLoop(List<Widget> result)
        {
            if (_layoutInvalid != 0)
            {
                result.Add(this);
            }
            if (_children != null)
            {
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    _children[i].CollectLayoutLoop(result);
                }
            }
        }

        private PropertyChangeSupport CreatePropertyChangeSupport()
        {
            if (_propertyChangeSupport == null)
            {
                _propertyChangeSupport = new PropertyChangeSupport(this);
            }
            return _propertyChangeSupport;
        }

        private Logger GetLogger()
        {
            return Logger.GetLogger(typeof(Widget));
        }

        /**
         * When this interface is installed in a Widget then the widget tries to
         * render into an offscreen surface.
         */
        public interface RenderOffscreen
        {
            /**
             * This method is called after the widget has been sucessfully rendered
             * into an offscreen surface.
             * 
             * @param gui the GUI instance
             * @param widget the widget
             * @param surface the resulting offscreen surface
             */
            void PaintOffscreenSurface(GUI gui, Widget widget, OffscreenSurface surface);

            /**
             * Called when {@link OffscreenRenderer#startOffscreenRendering(de.matthiasmann.twl.renderer.OffscreenSurface, int, int, int, int) }
             * failed.
             * At the moment this method is called the RenderOffscreen instance has
             * already been removed from the widget.
             * @param widget the widget
             */
            void OffscreenRenderingFailed(Widget widget);

            /**
             * Returns the extra area around the widget needed for the effect.
             * <p>All returned values must be &gt;= 0.</p>
             * 
             * <p>The returned object can be reused on the next call and should not
             * be stored by the caller.</p>
             * 
             * @param widget the widget
             * @return the extra area in {@code top, left, right, bottom} order or null
             */
            int[] GetEffectExtraArea(Widget widget);

            /**
             * Called before offscreen rendering is started.
             * 
             * <p>NOTE: when this function returns false none of the paint methods
             * of that widget are called which might effect some widgets.</p>
             * 
             * <p>If you are unsure it is always safer to return true.</p>
             * 
             * @param gui the GUI instance
             * @param widget the widget
             * @param surface the previous offscreen surface - never null
             * @return true if the surface needs to be updated, false if no new rendering should be done
             */
            bool NeedPainting(GUI gui, Widget widget, OffscreenSurface surface);
        }

        public interface OffscreenMouseAdjustments : RenderOffscreen
        {

            /**
             * Called when mouse events are routed for the widget.
             * 
             * <p>All mouse coordinates in TWL are absolute.</p>
             * 
             * <p>The returned object can be reused on the next call and should not
             * be stored by the caller.</p>
             * 
             * @param widget the widget
             * @param evt the mouse event
             * @return the new mouse coordinates in {@code x, y} order
             */
            int[] AdjustMouseCoordinates(Widget widget, Event evt);
        }
    }
}
