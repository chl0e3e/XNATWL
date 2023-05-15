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

namespace XNATWL
{
    public class ScrollPane : Widget
    {

        public static StateKey STATE_DOWNARROW_ARMED = StateKey.Get("downArrowArmed");
        public static StateKey STATE_RIGHTARROW_ARMED = StateKey.Get("rightArrowArmed");
        public static StateKey STATE_HORIZONTAL_SCROLLBAR_VISIBLE = StateKey.Get("horizontalScrollbarVisible");
        public static StateKey STATE_VERTICAL_SCROLLBAR_VISIBLE = StateKey.Get("verticalScrollbarVisible");
        public static StateKey STATE_AUTO_SCROLL_UP = StateKey.Get("autoScrollUp");
        public static StateKey STATE_AUTO_SCROLL_DOWN = StateKey.Get("autoScrollDown");

        /**
         * Controls which axis of the scroll pane should be eFixed
         */
        public enum Fixed
        {
            /**
             * No axis is eFixed - the scroll pane may show 2 scroll bars
             */
            NONE,
            /**
             * The horizontal axis is eFixed - only a vertical scroll bar may be shown
             */
            HORIZONTAL,
            /**
             * The vertical axis is eFixed - only a horizontal scroll bar may be shown
             */
            VERTICAL
        }

        /**
         * Indicates that the content handles scrolling itself.
         *
         * This interfaces also allows for a larger scrollable size then
         * the Widget size limitations.
         *
         * The {@code ScrollPane} will set the size of content to the available
         * content area.
         */
        public interface Scrollable
        {
            /**
             * Called when the content is scrolled either by a call to
             * {@link ScrollPane#setScrollPositionX(int) },
             * {@link ScrollPane#setScrollPositionY(int) } or
             * through one of the scrollbars.
             *
             * @param scrollPosX the new horizontal scroll position. Always &gt;= 0.
             * @param scrollPosY the new vertical scroll position. Always &gt;= 0.
             */
            void SetScrollPosition(int scrollPosX, int scrollPosY);
        }

        /**
         * Custom auto scroll area checking. This is needed when the content
         * has column headers.
         */
        public interface AutoScrollable
        {
            /**
             * Returns the auto scroll direction for the specified mouse event.
             *
             * @param evt the mouse event which could trigger an auto scroll
             * @param autoScrollArea the size of the auto scroll area.
             *     This is a theme parameter of the {@link ScrollPane}
             * @return the auto scroll direction.
             *     -1 for upwards
             *      0 for no auto scrolling
             *     +1 for downwards
             * @see ScrollPane#checkAutoScroll(de.matthiasmann.twl.Event)
             */
            int GetAutoScrollDirection(Event evt, int autoScrollArea);
        }

        /**
         * Custom page sizes for page scrolling and scroll bar thumb sizing.
         * This is needed when the content has column or row headers.
         */
        public interface CustomPageSize
        {
            /**
             * Computes the horizontal page size based on the available width.
             * @param availableWidth the available width (the visible area)
             * @return the page size. Must be &gt; 0 and &lt;= availableWidth
             */
            int GetPageSizeX(int availableWidth);
            /**
             * Computes the vertical page size based on the available height.
             * @param availableHeight the available height (the visible area)
             * @return the page size. Must be &gt; 0 and &lt;= availableHeight
             */
            int GetPageSizeY(int availableHeight);
        }

        private static int AUTO_SCROLL_DELAY = 50;

        Scrollbar _scrollbarH;
        Scrollbar _scrollbarV;
        private Widget _contentArea;
        private DraggableButton _dragButton;
        private Widget _content;
        private Fixed _eFixed = Fixed.NONE;
        private Dimension _hScrollbarOffset = Dimension.ZERO;
        private Dimension _vScrollbarOffset = Dimension.ZERO;
        private Dimension _contentScrollbarSpacing = Dimension.ZERO;
        private bool _inLayout;
        private bool _expandContentSize;
        private bool _scrollbarsAlwaysVisible;
        private int _scrollbarsToggleFlags;
        private int _autoScrollArea;
        private int _autoScrollSpeed;
        private Timer _autoScrollTimer;
        private int _autoScrollDirection;

        public ScrollPane() : this(null)
        {

        }

        public ScrollPane(Widget content)
        {
            this._scrollbarH = new Scrollbar(Scrollbar.Orientation.Horizontal);
            this._scrollbarV = new Scrollbar(Scrollbar.Orientation.Vertical);
            this._contentArea = new Widget();

            //Runnable cb = new Runnable() {
            //    public void run() {
            //        scrollContent();
            //    }
            //};

            _scrollbarH.PositionChanged += ScrollbarH_PositionChanged;
            _scrollbarH.SetVisible(false);
            _scrollbarV.PositionChanged += ScrollbarH_PositionChanged;
            _scrollbarV.SetVisible(false);
            _contentArea.SetClip(true);
            _contentArea.SetTheme("");

            base.InsertChild(_contentArea, 0);
            base.InsertChild(_scrollbarH, 1);
            base.InsertChild(_scrollbarV, 2);
            SetContent(content);
            SetCanAcceptKeyboardFocus(true);
        }

        private void ScrollbarH_PositionChanged(object sender, ScrollbarChangedPositionEventArgs e)
        {
            ScrollContent();
        }

        public Fixed GetFixed()
        {
            return _eFixed;
        }

        /**
         * Controls if this scroll pane has a eFixed axis which will not show a scrollbar.
         * 
         * Default is {@link Fixed#NONE}
         *
         * @param eFixed the eFixed axis.
         */
        public void SetFixed(Fixed eFixed)
        {
            if (this._eFixed != eFixed)
            {
                this._eFixed = eFixed;
                InvalidateLayout();
            }
        }

        public Widget GetContent()
        {
            return _content;
        }

        /**
         * Sets the widget which should be scrolled.
         *
         * <p>The following interfaces change the behavior of the scroll pane when
         * they are implemented by the content:</p><ul>
         * <li>{@link Scrollable}</li>
         * <li>{@link AutoScrollable}</li>
         * <li>{@link CustomPageSize}</li>
         * </ul>
         * @param content the new scroll pane content
         */
        public void SetContent(Widget content)
        {
            if (this._content != null)
            {
                _contentArea.RemoveAllChildren();
                this._content = null;
            }
            if (content != null)
            {
                this._content = content;
                _contentArea.Add(content);
            }
        }

        public bool IsExpandContentSize()
        {
            return _expandContentSize;
        }

        /**
         * Control if the content size.
         *
         * If set to true then the content size will be the larger of it's preferred
         * size and the size of the content area.
         * If set to false then the content size will be it's preferred area.
         *
         * Default is false
         * 
         * @param expandContentSize true if the content should always cover the content area
         */
        public void SetExpandContentSize(bool expandContentSize)
        {
            if (this._expandContentSize != expandContentSize)
            {
                this._expandContentSize = expandContentSize;
                InvalidateLayoutLocally();
            }
        }

        /**
         * Forces a layout of the scroll pane content to update the ranges of the
         * scroll bars.
         * 
         * This method should be called after changes to the content which might
         * affect it's size and before computing a new scroll position.
         *
         * @see #scrollToAreaX(int, int, int)
         * @see #scrollToAreaY(int, int, int)
         */
        public void UpdateScrollbarSizes()
        {
            InvalidateLayoutLocally();
            ValidateLayout();
        }

        public int GetScrollPositionX()
        {
            return _scrollbarH.GetValue();
        }

        public int GetMaxScrollPosX()
        {
            return _scrollbarH.GetMaxValue();
        }

        public void SetScrollPositionX(int pos)
        {
            _scrollbarH.SetValue(pos);
        }

        /**
         * Tries to make the specified horizontal area completely visible. If it is
         * larger then the horizontal page size then it scrolls to the start of the area.
         *
         * @param start the position of the area
         * @param size size of the area
         * @param extra the extra space which should be visible around the area
         * @see Scrollbar#scrollToArea(int, int, int)
         */
        public void ScrollToAreaX(int start, int size, int extra)
        {
            _scrollbarH.ScrollToArea(start, size, extra);
        }

        public int GetScrollPositionY()
        {
            return _scrollbarV.GetValue();
        }

        public int GetMaxScrollPosY()
        {
            return _scrollbarV.GetMaxValue();
        }

        public void SetScrollPositionY(int pos)
        {
            _scrollbarV.SetValue(pos);
        }

        /**
         * Tries to make the specified vertical area completely visible. If it is
         * larger then the vertical page size then it scrolls to the start of the area.
         *
         * @param start the position of the area
         * @param size size of the area
         * @param extra the extra space which should be visible around the area
         * @see Scrollbar#scrollToArea(int, int, int)
         */
        public void ScrollToAreaY(int start, int size, int extra)
        {
            _scrollbarV.ScrollToArea(start, size, extra);
        }

        public int GetContentAreaWidth()
        {
            return _contentArea.GetWidth();
        }

        public int GetContentAreaHeight()
        {
            return _contentArea.GetHeight();
        }

        /**
         * Returns the horizontal scrollbar widget, be very careful with changes to it.
         * @return the horizontal scrollbar
         */
        public Scrollbar GetHorizontalScrollbar()
        {
            return _scrollbarH;
        }

        /**
         * Returns the vertical scrollbar widget, be very careful with changes to it.
         * @return the vertical scrollbar
         */
        public Scrollbar GetVerticalScrollbar()
        {
            return _scrollbarV;
        }

        /**
         * Creates a DragListener which can be used to drag the content of this ScrollPane around.
         * @return a DragListener to scroll this this ScrollPane.
         */
        public DraggableButton.DragListener CreateDragListener()
        {
            return null;
            /*return new DraggableButton.DragListener() {
                int startScrollX;
                int startScrollY;
                public void dragStarted() {
                    startScrollX = getScrollPositionX();
                    startScrollY = getScrollPositionY();
                }
                public void dragged(int deltaX, int deltaY) {
                    setScrollPositionX(startScrollX - deltaX);
                    setScrollPositionY(startScrollY - deltaY);
                }
                public void dragStopped() {
                }
            };*/
        }

        /**
         * Checks for an auto scroll event. This should be called when a drag & drop
         * operation is in progress and the drop target is inside a scroll pane.
         *
         * @param evt the mouse event which should be checked.
         * @return true if auto scrolling is started/active.
         * @see #stopAutoScroll()
         */
        public bool CheckAutoScroll(Event evt)
        {
            GUI gui = GetGUI();
            if (gui == null)
            {
                StopAutoScroll();
                return false;
            }

            _autoScrollDirection = GetAutoScrollDirection(evt);
            if (_autoScrollDirection == 0)
            {
                StopAutoScroll();
                return false;
            }

            SetAutoScrollMarker();

            if (_autoScrollTimer == null)
            {
                _autoScrollTimer = gui.CreateTimer();
                _autoScrollTimer.SetContinuous(true);
                _autoScrollTimer.SetDelay(AUTO_SCROLL_DELAY);
                //autoScrollTimer.setCallback(new Runnable() {
                //    public void run() {
                //        doAutoScroll();
                //    }
                //});
                DoAutoScroll();
            }
            _autoScrollTimer.Start();
            return true;
        }

        /**
         * Stops an activate auto scroll. This must be called when the drag & drop
         * operation is finished.
         *
         * @see #checkAutoScroll(de.matthiasmann.twl.Event)
         */
        public void StopAutoScroll()
        {
            if (_autoScrollTimer != null)
            {
                _autoScrollTimer.Stop();
            }
            _autoScrollDirection = 0;
            SetAutoScrollMarker();
        }

        /**
         * Returns the ScrollPane instance which has the specified widget as content.
         *
         * @param widget the widget to retrieve the containing ScrollPane for.
         * @return the ScrollPane or null if that widget is not directly in a ScrollPane.
         * @see #setContent(de.matthiasmann.twl.Widget)
         */
        public static ScrollPane GetContainingScrollPane(Widget widget)
        {
            Widget ca = widget.GetParent();
            if (ca != null)
            {
                Widget sp = ca.GetParent();
                if (sp is ScrollPane)
                {
                    ScrollPane scrollPane = (ScrollPane)sp;
                    System.Diagnostics.Debug.Assert(scrollPane.GetContent() == widget);
                    return scrollPane;
                }
            }
            return null;
        }

        public override int GetMinWidth()
        {
            int minWidth = base.GetMinWidth();
            int border = GetBorderHorizontal();
            //minWidth = Math.max(minWidth, scrollbarH.getMinWidth() + border);
            if (_eFixed == Fixed.HORIZONTAL && _content != null)
            {
                int sbWidth = _scrollbarV.IsVisible() ? _scrollbarV.GetMinWidth() : 0;
                minWidth = Math.Max(minWidth, _content.GetMinWidth() + border + sbWidth);
            }
            return minWidth;
        }

        public override int GetMinHeight()
        {
            int minHeight = base.GetMinHeight();
            int border = GetBorderVertical();
            //minHeight = Math.max(minHeight, scrollbarV.getMinHeight() + border);
            if (_eFixed == Fixed.VERTICAL && _content != null)
            {
                int sbHeight = _scrollbarH.IsVisible() ? _scrollbarH.GetMinHeight() : 0;
                minHeight = Math.Max(minHeight, _content.GetMinHeight() + border + sbHeight);
            }
            return minHeight;
        }

        public override int GetPreferredInnerWidth()
        {
            if (_content != null)
            {
                switch (_eFixed)
                {
                    case Fixed.HORIZONTAL:
                        int prefWidth = ComputeSize(
                                _content.GetMinWidth(),
                                _content.GetPreferredWidth(),
                                _content.GetMaxWidth());
                        if (_scrollbarV.IsVisible())
                        {
                            prefWidth += _scrollbarV.GetPreferredWidth();
                        }
                        return prefWidth;
                    case Fixed.VERTICAL:
                        return _content.GetPreferredWidth();
                }
            }
            return 0;
        }

        public override int GetPreferredInnerHeight()
        {
            if (_content != null)
            {
                switch (_eFixed)
                {
                    case Fixed.HORIZONTAL:
                        return _content.GetPreferredHeight();
                    case Fixed.VERTICAL:
                        int prefHeight = ComputeSize(
                                _content.GetMinHeight(),
                                _content.GetPreferredHeight(),
                                _content.GetMaxHeight());
                        if (_scrollbarH.IsVisible())
                        {
                            prefHeight += _scrollbarH.GetPreferredHeight();
                        }
                        return prefHeight;
                }
            }
            return 0;
        }

        public override void InsertChild(Widget child, int index)
        {
            throw new InvalidOperationException("use setContent");
        }

        public override void RemoveAllChildren()
        {
            throw new InvalidOperationException("use setContent");
        }

        public override Widget RemoveChild(int index)
        {
            throw new InvalidOperationException("use setContent");
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeScrollPane(themeInfo);
        }

        /**
         * The following theme parameters are required by the scroll pane:<table>
         * <tr><th>Parameter name</th><th>Type</th><th>Description</th></tr>
         * <tr><td>autoScrollArea</td><td>integer</td><td>The size of the auto scroll area</td></tr>
         * <tr><td>autoScrollSpeed</td><td>integer</td><td>The speed in pixels to scroll every 50 ms</td></tr>
         * <tr><td>hasDragButton</td><td>bool</td><td>If the dragButton should be shown or not</td></tr>
         * <tr><td>scrollbarsAlwaysVisible</td><td>bool</td><td>Show scrollbars always (true) or only when needed (false)</td></tr>
         * </table>
         * <br/>
         * The following optional parameters can be used to change the appearance of
         * the scroll pane:<table>
         * <tr><th>Parameter name</th><th>Type</th><th>Description</th></tr>
         * <tr><td>hscrollbarOffset</td><td>Dimension</td><td>Moves the horizontal scrollbar but does not
         *      change the available area for the scroll content.</td></tr>
         * <tr><td>vscrollbarOffset</td><td>Dimension</td><td>Moves the vertical scrollbar but does not
         *      change the available area for the scroll content.</td></tr>
         * <tr><td>contentScrollbarSpacing</td><td>Dimension</td><td>An optional spacing between
         *      the scrollbar and the content area. This is only applied when the corresponding
         *      scrollbar is visible. It should be &gt;= 0.</td></tr>
         * </table>
         *
         * @param themeInfo the theme info
         */
        class ThemeDragListener : DraggableButton.DragListener
        {
            private ScrollPane _scrollPane;
            public ThemeDragListener(ScrollPane scrollPane)
            {
                this._scrollPane = scrollPane;
            }
            public void DragStarted()
            {
                this._scrollPane._scrollbarH.ExternalDragStart();
                this._scrollPane._scrollbarV.ExternalDragStart();
            }
            public void Dragged(int deltaX, int deltaY)
            {
                this._scrollPane._scrollbarH.ExternalDragged(deltaX, deltaY);
                this._scrollPane._scrollbarV.ExternalDragged(deltaX, deltaY);
            }
            public void DragStopped()
            {
                this._scrollPane._scrollbarH.ExternalDragStopped();
                this._scrollPane._scrollbarV.ExternalDragStopped();
            }
        }

        protected void ApplyThemeScrollPane(ThemeInfo themeInfo)
        {
            _autoScrollArea = themeInfo.GetParameter("autoScrollArea", 5);
            _autoScrollSpeed = themeInfo.GetParameter("autoScrollSpeed", _autoScrollArea * 2);
            _hScrollbarOffset = themeInfo.GetParameterValue("hscrollbarOffset", false, typeof(Dimension), Dimension.ZERO);
            _vScrollbarOffset = themeInfo.GetParameterValue("vscrollbarOffset", false, typeof(Dimension), Dimension.ZERO);
            _contentScrollbarSpacing = themeInfo.GetParameterValue("contentScrollbarSpacing", false, typeof(Dimension), Dimension.ZERO);
            _scrollbarsAlwaysVisible = themeInfo.GetParameter("scrollbarsAlwaysVisible", false);

            bool hasDragButton = themeInfo.GetParameter("hasDragButton", false);
            if (hasDragButton && _dragButton == null)
            {
                _dragButton = new DraggableButton();
                _dragButton.SetTheme("dragButton");
                _dragButton.SetListener(new ThemeDragListener(this));
                base.InsertChild(_dragButton, 3);
            }
            else if (!hasDragButton && _dragButton != null)
            {
                System.Diagnostics.Debug.Assert(base.GetChild(3) == _dragButton);
                base.RemoveChild(3);
                _dragButton = null;
            }
        }

        protected int GetAutoScrollDirection(Event evt)
        {
            if (_content is AutoScrollable)
            {
                return ((AutoScrollable)_content).GetAutoScrollDirection(evt, _autoScrollArea);
            }
            if (_contentArea.IsMouseInside(evt))
            {
                int mouseY = evt.GetMouseY();
                int areaY = _contentArea.GetY();
                if ((mouseY - areaY) <= _autoScrollArea ||
                        (_contentArea.GetBottom() - mouseY) <= _autoScrollArea)
                {
                    // use a 2nd check to decide direction in case the autoScrollAreas overlap
                    if (mouseY < (areaY + _contentArea.GetHeight() / 2))
                    {
                        return -1;
                    }
                    else
                    {
                        return +1;
                    }
                }
            }
            return 0;
        }

        public override void ValidateLayout()
        {
            if (!_inLayout)
            {
                try
                {
                    _inLayout = true;
                    if (_content != null)
                    {
                        _content.ValidateLayout();
                    }
                    base.ValidateLayout();
                }
                finally
                {
                    _inLayout = false;
                }
            }
        }

        protected override void ChildInvalidateLayout(Widget child)
        {
            if (child == _contentArea)
            {
                // stop invalidate layout chain here when it comes from contentArea
                InvalidateLayoutLocally();
            }
            else
            {
                base.ChildInvalidateLayout(child);
            }
        }

        protected override void PaintWidget(GUI gui)
        {
            // clear flags - used to detect layout loops
            _scrollbarsToggleFlags = 0;
        }

        protected override void Layout()
        {
            if (_content != null)
            {
                int innerWidth = GetInnerWidth();
                int innerHeight = GetInnerHeight();
                int availWidth = innerWidth;
                int availHeight = innerHeight;
                innerWidth += _vScrollbarOffset.X;
                innerHeight += _hScrollbarOffset.Y;
                int scrollbarHX = _hScrollbarOffset.X;
                int scrollbarHY = innerHeight;
                int scrollbarVX = innerWidth;
                int scrollbarVY = _vScrollbarOffset.Y;
                int requiredWidth;
                int requiredHeight;
                bool repeat;
                bool visibleH = false;
                bool visibleV = false;

                switch (_eFixed)
                {
                    case Fixed.HORIZONTAL:
                        requiredWidth = availWidth;
                        requiredHeight = _content.GetPreferredHeight();
                        break;
                    case Fixed.VERTICAL:
                        requiredWidth = _content.GetPreferredWidth();
                        requiredHeight = availHeight;
                        break;
                    default:
                        requiredWidth = _content.GetPreferredWidth();
                        requiredHeight = _content.GetPreferredHeight();
                        break;
                }

                //System.out.println("required="+requiredWidth+","+requiredHeight+" avail="+availWidth+","+availHeight);

                int hScrollbarMax = 0;
                int vScrollbarMax = 0;

                // don't add scrollbars if we have zero size
                if (availWidth > 0 && availHeight > 0)
                {
                    do
                    {
                        repeat = false;

                        if (_eFixed != Fixed.HORIZONTAL)
                        {
                            hScrollbarMax = Math.Max(0, requiredWidth - availWidth);
                            if (hScrollbarMax > 0 || _scrollbarsAlwaysVisible || ((_scrollbarsToggleFlags & 3) == 3))
                            {
                                repeat |= !visibleH;
                                visibleH = true;
                                int prefHeight = _scrollbarH.GetPreferredHeight();
                                scrollbarHY = innerHeight - prefHeight;
                                availHeight = Math.Max(0, scrollbarHY - _contentScrollbarSpacing.Y);
                            }
                        }
                        else
                        {
                            hScrollbarMax = 0;
                            requiredWidth = availWidth;
                        }

                        if (_eFixed != Fixed.VERTICAL)
                        {
                            vScrollbarMax = Math.Max(0, requiredHeight - availHeight);
                            if (vScrollbarMax > 0 || _scrollbarsAlwaysVisible || ((_scrollbarsToggleFlags & 12) == 12))
                            {
                                repeat |= !visibleV;
                                visibleV = true;
                                int prefWidth = _scrollbarV.GetPreferredWidth();
                                scrollbarVX = innerWidth - prefWidth;
                                availWidth = Math.Max(0, scrollbarVX - _contentScrollbarSpacing.X);
                            }
                        }
                        else
                        {
                            vScrollbarMax = 0;
                            requiredHeight = availHeight;
                        }
                    } while (repeat);
                }

                // if a scrollbar visibility state has changed set it's flag to detect layout loops
                if (visibleH && !_scrollbarH.IsVisible())
                {
                    _scrollbarsToggleFlags |= 1;
                }
                if (!visibleH && _scrollbarH.IsVisible())
                {
                    _scrollbarsToggleFlags |= 2;
                }
                if (visibleV && !_scrollbarV.IsVisible())
                {
                    _scrollbarsToggleFlags |= 4;
                }
                if (!visibleV && _scrollbarV.IsVisible())
                {
                    _scrollbarsToggleFlags |= 8;
                }

                bool changedH = visibleH ^ _scrollbarH.IsVisible();
                bool changedV = visibleV ^ _scrollbarV.IsVisible();
                if (changedH || changedV)
                {
                    if ((changedH && _eFixed == Fixed.VERTICAL) ||
                       (changedV && _eFixed == Fixed.HORIZONTAL))
                    {
                        InvalidateLayout();
                    }
                    else
                    {
                        InvalidateLayoutLocally();
                    }
                }

                int pageSizeX, pageSizeY;
                if (_content is CustomPageSize)
                {
                    CustomPageSize customPageSize = (CustomPageSize)_content;
                    pageSizeX = customPageSize.GetPageSizeX(availWidth);
                    pageSizeY = customPageSize.GetPageSizeY(availHeight);
                }
                else
                {
                    pageSizeX = availWidth;
                    pageSizeY = availHeight;
                }

                _scrollbarH.SetVisible(visibleH);
                _scrollbarH.SetMinMaxValue(0, hScrollbarMax);
                _scrollbarH.SetSize(Math.Max(0, scrollbarVX - scrollbarHX), Math.Max(0, innerHeight - scrollbarHY));
                _scrollbarH.SetPosition(GetInnerX() + scrollbarHX, GetInnerY() + scrollbarHY);
                _scrollbarH.SetPageSize(Math.Max(1, pageSizeX));
                _scrollbarH.SetStepSize(Math.Max(1, pageSizeX / 10));

                _scrollbarV.SetVisible(visibleV);
                _scrollbarV.SetMinMaxValue(0, vScrollbarMax);
                _scrollbarV.SetSize(Math.Max(0, innerWidth - scrollbarVX), Math.Max(0, scrollbarHY - scrollbarVY));
                _scrollbarV.SetPosition(GetInnerX() + scrollbarVX, GetInnerY() + scrollbarVY);
                _scrollbarV.SetPageSize(Math.Max(1, pageSizeY));
                _scrollbarV.SetStepSize(Math.Max(1, pageSizeY / 10));

                if (_dragButton != null)
                {
                    _dragButton.SetVisible(visibleH && visibleV);
                    _dragButton.SetSize(Math.Max(0, innerWidth - scrollbarVX), Math.Max(0, innerHeight - scrollbarHY));
                    _dragButton.SetPosition(GetInnerX() + scrollbarVX, GetInnerY() + scrollbarHY);
                }

                _contentArea.SetPosition(GetInnerX(), GetInnerY());
                _contentArea.SetSize(availWidth, availHeight);
                if (_content is Scrollable)
                {
                    _content.SetPosition(_contentArea.GetX(), _contentArea.GetY());
                    _content.SetSize(availWidth, availHeight);
                }
                else if (_expandContentSize)
                {
                    _content.SetSize(Math.Max(availWidth, requiredWidth),
                            Math.Max(availHeight, requiredHeight));
                }
                else
                {
                    _content.SetSize(Math.Max(0, requiredWidth), Math.Max(0, requiredHeight));
                }

                AnimationState animationState = GetAnimationState();
                animationState.SetAnimationState(STATE_HORIZONTAL_SCROLLBAR_VISIBLE, visibleH);
                animationState.SetAnimationState(STATE_VERTICAL_SCROLLBAR_VISIBLE, visibleV);

                ScrollContent();
            }
            else
            {
                _scrollbarH.SetVisible(false);
                _scrollbarV.SetVisible(false);
            }
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.IsKeyEvent() && _content != null && _content.CanAcceptKeyboardFocus())
            {
                if (_content.HandleEvent(evt))
                {
                    _content.RequestKeyboardFocus();
                    return true;
                }
            }
            if (base.HandleEvent(evt))
            {
                return true;
            }

            if (evt.GetEventType() == EventType.KEY_PRESSED || evt.GetEventType() == EventType.KEY_RELEASED)
            {
                int keyCode = evt.GetKeyCode();
                if (keyCode == Event.KEY_LEFT ||
                        keyCode == Event.KEY_RIGHT)
                {
                    return _scrollbarH.HandleEvent(evt);
                }
                if (keyCode == Event.KEY_UP ||
                        keyCode == Event.KEY_DOWN ||
                        keyCode == Event.KEY_PRIOR ||
                        keyCode == Event.KEY_NEXT)
                {
                    return _scrollbarV.HandleEvent(evt);
                }
            }
            else if (evt.GetEventType() == EventType.MOUSE_WHEEL)
            {
                if (_scrollbarV.IsVisible())
                {
                    return _scrollbarV.HandleEvent(evt);
                }
                return false;
            }

            return evt.IsMouseEvent() && _contentArea.IsMouseInside(evt);
        }

        protected override void Paint(GUI gui)
        {
            if (_dragButton != null)
            {
                AnimationState animationState = _dragButton.GetAnimationState();
                animationState.SetAnimationState(STATE_DOWNARROW_ARMED, _scrollbarV.IsDownRightButtonArmed());
                animationState.SetAnimationState(STATE_RIGHTARROW_ARMED, _scrollbarH.IsDownRightButtonArmed());
            }
            base.Paint(gui);
        }

        void ScrollContent()
        {
            if (_content is Scrollable)
            {
                Scrollable scrollable = (Scrollable)_content;
                scrollable.SetScrollPosition(_scrollbarH.GetValue(), _scrollbarV.GetValue());
            }
            else
            {
                _content.SetPosition(
                        _contentArea.GetX() - _scrollbarH.GetValue(),
                        _contentArea.GetY() - _scrollbarV.GetValue());
            }
        }

        void SetAutoScrollMarker()
        {
            int scrollPos = _scrollbarV.GetValue();
            AnimationState animationState = GetAnimationState();
            animationState.SetAnimationState(STATE_AUTO_SCROLL_UP, _autoScrollDirection < 0 && scrollPos > 0);
            animationState.SetAnimationState(STATE_AUTO_SCROLL_DOWN, _autoScrollDirection > 0 && scrollPos < _scrollbarV.GetMaxValue());
        }

        void DoAutoScroll()
        {
            _scrollbarV.SetValue(_scrollbarV.GetValue() + _autoScrollDirection * _autoScrollSpeed);
            SetAutoScrollMarker();
        }
    }
}
