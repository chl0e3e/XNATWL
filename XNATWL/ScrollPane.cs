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
            void setScrollPosition(int scrollPosX, int scrollPosY);
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
            int getAutoScrollDirection(Event evt, int autoScrollArea);
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
            int getPageSizeX(int availableWidth);
            /**
             * Computes the vertical page size based on the available height.
             * @param availableHeight the available height (the visible area)
             * @return the page size. Must be &gt; 0 and &lt;= availableHeight
             */
            int getPageSizeY(int availableHeight);
        }

        private static int AUTO_SCROLL_DELAY = 50;

        Scrollbar scrollbarH;
        Scrollbar scrollbarV;
        private Widget contentArea;
        private DraggableButton dragButton;
        private Widget content;
        private Fixed eFixed = Fixed.NONE;
        private Dimension hscrollbarOffset = Dimension.ZERO;
        private Dimension vscrollbarOffset = Dimension.ZERO;
        private Dimension contentScrollbarSpacing = Dimension.ZERO;
        private bool inLayout;
        private bool expandContentSize;
        private bool scrollbarsAlwaysVisible;
        private int scrollbarsToggleFlags;
        private int autoScrollArea;
        private int autoScrollSpeed;
        private Timer autoScrollTimer;
        private int autoScrollDirection;

        public ScrollPane() : this(null)
        {

        }

        public ScrollPane(Widget content)
        {
            this.scrollbarH = new Scrollbar(Scrollbar.Orientation.HORIZONTAL);
            this.scrollbarV = new Scrollbar(Scrollbar.Orientation.VERTICAL);
            this.contentArea = new Widget();

            //Runnable cb = new Runnable() {
            //    public void run() {
            //        scrollContent();
            //    }
            //};

            scrollbarH.PositionChanged += ScrollbarH_PositionChanged;
            scrollbarH.setVisible(false);
            scrollbarV.PositionChanged += ScrollbarH_PositionChanged;
            scrollbarV.setVisible(false);
            contentArea.setClip(true);
            contentArea.setTheme("");

            base.insertChild(contentArea, 0);
            base.insertChild(scrollbarH, 1);
            base.insertChild(scrollbarV, 2);
            setContent(content);
            setCanAcceptKeyboardFocus(true);
        }

        private void ScrollbarH_PositionChanged(object sender, ScrollbarChangedPositionEventArgs e)
        {
            scrollContent();
        }

        public Fixed getFixed()
        {
            return eFixed;
        }

        /**
         * Controls if this scroll pane has a eFixed axis which will not show a scrollbar.
         * 
         * Default is {@link Fixed#NONE}
         *
         * @param eFixed the eFixed axis.
         */
        public void setFixed(Fixed eFixed)
        {
            if (eFixed == null)
            {
                throw new NullReferenceException("eFixed");
            }
            if (this.eFixed != eFixed)
            {
                this.eFixed = eFixed;
                invalidateLayout();
            }
        }

        public Widget getContent()
        {
            return content;
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
        public void setContent(Widget content)
        {
            if (this.content != null)
            {
                contentArea.removeAllChildren();
                this.content = null;
            }
            if (content != null)
            {
                this.content = content;
                contentArea.add(content);
            }
        }

        public bool isExpandContentSize()
        {
            return expandContentSize;
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
        public void setExpandContentSize(bool expandContentSize)
        {
            if (this.expandContentSize != expandContentSize)
            {
                this.expandContentSize = expandContentSize;
                invalidateLayoutLocally();
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
        public void updateScrollbarSizes()
        {
            invalidateLayoutLocally();
            validateLayout();
        }

        public int getScrollPositionX()
        {
            return scrollbarH.getValue();
        }

        public int getMaxScrollPosX()
        {
            return scrollbarH.getMaxValue();
        }

        public void setScrollPositionX(int pos)
        {
            scrollbarH.setValue(pos);
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
        public void scrollToAreaX(int start, int size, int extra)
        {
            scrollbarH.scrollToArea(start, size, extra);
        }

        public int getScrollPositionY()
        {
            return scrollbarV.getValue();
        }

        public int getMaxScrollPosY()
        {
            return scrollbarV.getMaxValue();
        }

        public void setScrollPositionY(int pos)
        {
            scrollbarV.setValue(pos);
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
        public void scrollToAreaY(int start, int size, int extra)
        {
            scrollbarV.scrollToArea(start, size, extra);
        }

        public int getContentAreaWidth()
        {
            return contentArea.getWidth();
        }

        public int getContentAreaHeight()
        {
            return contentArea.getHeight();
        }

        /**
         * Returns the horizontal scrollbar widget, be very careful with changes to it.
         * @return the horizontal scrollbar
         */
        public Scrollbar getHorizontalScrollbar()
        {
            return scrollbarH;
        }

        /**
         * Returns the vertical scrollbar widget, be very careful with changes to it.
         * @return the vertical scrollbar
         */
        public Scrollbar getVerticalScrollbar()
        {
            return scrollbarV;
        }

        /**
         * Creates a DragListener which can be used to drag the content of this ScrollPane around.
         * @return a DragListener to scroll this this ScrollPane.
         */
        public DraggableButton.DragListener createDragListener()
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
        public bool checkAutoScroll(Event evt)
        {
            GUI gui = getGUI();
            if (gui == null)
            {
                stopAutoScroll();
                return false;
            }

            autoScrollDirection = getAutoScrollDirection(evt);
            if (autoScrollDirection == 0)
            {
                stopAutoScroll();
                return false;
            }

            setAutoScrollMarker();

            if (autoScrollTimer == null)
            {
                autoScrollTimer = gui.createTimer();
                autoScrollTimer.setContinuous(true);
                autoScrollTimer.setDelay(AUTO_SCROLL_DELAY);
                //autoScrollTimer.setCallback(new Runnable() {
                //    public void run() {
                //        doAutoScroll();
                //    }
                //});
                doAutoScroll();
            }
            autoScrollTimer.start();
            return true;
        }

        /**
         * Stops an activate auto scroll. This must be called when the drag & drop
         * operation is finished.
         *
         * @see #checkAutoScroll(de.matthiasmann.twl.Event)
         */
        public void stopAutoScroll()
        {
            if (autoScrollTimer != null)
            {
                autoScrollTimer.stop();
            }
            autoScrollDirection = 0;
            setAutoScrollMarker();
        }

        /**
         * Returns the ScrollPane instance which has the specified widget as content.
         *
         * @param widget the widget to retrieve the containing ScrollPane for.
         * @return the ScrollPane or null if that widget is not directly in a ScrollPane.
         * @see #setContent(de.matthiasmann.twl.Widget)
         */
        public static ScrollPane getContainingScrollPane(Widget widget)
        {
            Widget ca = widget.getParent();
            if (ca != null)
            {
                Widget sp = ca.getParent();
                if (sp is ScrollPane)
                {
                    ScrollPane scrollPane = (ScrollPane)sp;
                    System.Diagnostics.Debug.Assert(scrollPane.getContent() == widget);
                    return scrollPane;
                }
            }
            return null;
        }

        //@Override
        public override int getMinWidth()
        {
            int minWidth = base.getMinWidth();
            int border = getBorderHorizontal();
            //minWidth = Math.max(minWidth, scrollbarH.getMinWidth() + border);
            if (eFixed == Fixed.HORIZONTAL && content != null)
            {
                int sbWidth = scrollbarV.isVisible() ? scrollbarV.getMinWidth() : 0;
                minWidth = Math.Max(minWidth, content.getMinWidth() + border + sbWidth);
            }
            return minWidth;
        }

        //@Override
        public override int getMinHeight()
        {
            int minHeight = base.getMinHeight();
            int border = getBorderVertical();
            //minHeight = Math.max(minHeight, scrollbarV.getMinHeight() + border);
            if (eFixed == Fixed.VERTICAL && content != null)
            {
                int sbHeight = scrollbarH.isVisible() ? scrollbarH.getMinHeight() : 0;
                minHeight = Math.Max(minHeight, content.getMinHeight() + border + sbHeight);
            }
            return minHeight;
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            if (content != null)
            {
                switch (eFixed)
                {
                    case Fixed.HORIZONTAL:
                        int prefWidth = computeSize(
                                content.getMinWidth(),
                                content.getPreferredWidth(),
                                content.getMaxWidth());
                        if (scrollbarV.isVisible())
                        {
                            prefWidth += scrollbarV.getPreferredWidth();
                        }
                        return prefWidth;
                    case Fixed.VERTICAL:
                        return content.getPreferredWidth();
                }
            }
            return 0;
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            if (content != null)
            {
                switch (eFixed)
                {
                    case Fixed.HORIZONTAL:
                        return content.getPreferredHeight();
                    case Fixed.VERTICAL:
                        int prefHeight = computeSize(
                                content.getMinHeight(),
                                content.getPreferredHeight(),
                                content.getMaxHeight());
                        if (scrollbarH.isVisible())
                        {
                            prefHeight += scrollbarH.getPreferredHeight();
                        }
                        return prefHeight;
                }
            }
            return 0;
        }

        //@Override
        public override void insertChild(Widget child, int index)
        {
            throw new InvalidOperationException("use setContent");
        }

        //@Override
        public override void removeAllChildren()
        {
            throw new InvalidOperationException("use setContent");
        }

        //@Override
        public override Widget removeChild(int index)
        {
            throw new InvalidOperationException("use setContent");
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeScrollPane(themeInfo);
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
            public void dragStarted()
            {
                this._scrollPane.scrollbarH.externalDragStart();
                this._scrollPane.scrollbarV.externalDragStart();
            }
            public void dragged(int deltaX, int deltaY)
            {
                this._scrollPane.scrollbarH.externalDragged(deltaX, deltaY);
                this._scrollPane.scrollbarV.externalDragged(deltaX, deltaY);
            }
            public void dragStopped()
            {
                this._scrollPane.scrollbarH.externalDragStopped();
                this._scrollPane.scrollbarV.externalDragStopped();
            }
        }

        protected void applyThemeScrollPane(ThemeInfo themeInfo)
        {
            autoScrollArea = themeInfo.getParameter("autoScrollArea", 5);
            autoScrollSpeed = themeInfo.getParameter("autoScrollSpeed", autoScrollArea * 2);
            hscrollbarOffset = themeInfo.getParameterValue("hscrollbarOffset", false, typeof(Dimension), Dimension.ZERO);
            vscrollbarOffset = themeInfo.getParameterValue("vscrollbarOffset", false, typeof(Dimension), Dimension.ZERO);
            contentScrollbarSpacing = themeInfo.getParameterValue("contentScrollbarSpacing", false, typeof(Dimension), Dimension.ZERO);
            scrollbarsAlwaysVisible = themeInfo.getParameter("scrollbarsAlwaysVisible", false);

            bool hasDragButton = themeInfo.getParameter("hasDragButton", false);
            if (hasDragButton && dragButton == null)
            {
                dragButton = new DraggableButton();
                dragButton.setTheme("dragButton");
                dragButton.setListener(new ThemeDragListener(this));
                base.insertChild(dragButton, 3);
            }
            else if (!hasDragButton && dragButton != null)
            {
                System.Diagnostics.Debug.Assert(base.getChild(3) == dragButton);
                base.removeChild(3);
                dragButton = null;
            }
        }

        protected int getAutoScrollDirection(Event evt)
        {
            if (content is AutoScrollable)
            {
                return ((AutoScrollable)content).getAutoScrollDirection(evt, autoScrollArea);
            }
            if (contentArea.isMouseInside(evt))
            {
                int mouseY = evt.getMouseY();
                int areaY = contentArea.getY();
                if ((mouseY - areaY) <= autoScrollArea ||
                        (contentArea.getBottom() - mouseY) <= autoScrollArea)
                {
                    // use a 2nd check to decide direction in case the autoScrollAreas overlap
                    if (mouseY < (areaY + contentArea.getHeight() / 2))
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

        //@Override
        public override void validateLayout()
        {
            if (!inLayout)
            {
                try
                {
                    inLayout = true;
                    if (content != null)
                    {
                        content.validateLayout();
                    }
                    base.validateLayout();
                }
                finally
                {
                    inLayout = false;
                }
            }
        }

        //@Override
        protected override void childInvalidateLayout(Widget child)
        {
            if (child == contentArea)
            {
                // stop invalidate layout chain here when it comes from contentArea
                invalidateLayoutLocally();
            }
            else
            {
                base.childInvalidateLayout(child);
            }
        }

        //@Override
        protected override void paintWidget(GUI gui)
        {
            // clear flags - used to detect layout loops
            scrollbarsToggleFlags = 0;
        }

        //@Override
        protected override void layout()
        {
            if (content != null)
            {
                int innerWidth = getInnerWidth();
                int innerHeight = getInnerHeight();
                int availWidth = innerWidth;
                int availHeight = innerHeight;
                innerWidth += vscrollbarOffset.X;
                innerHeight += hscrollbarOffset.Y;
                int scrollbarHX = hscrollbarOffset.X;
                int scrollbarHY = innerHeight;
                int scrollbarVX = innerWidth;
                int scrollbarVY = vscrollbarOffset.Y;
                int requiredWidth;
                int requiredHeight;
                bool repeat;
                bool visibleH = false;
                bool visibleV = false;

                switch (eFixed)
                {
                    case Fixed.HORIZONTAL:
                        requiredWidth = availWidth;
                        requiredHeight = content.getPreferredHeight();
                        break;
                    case Fixed.VERTICAL:
                        requiredWidth = content.getPreferredWidth();
                        requiredHeight = availHeight;
                        break;
                    default:
                        requiredWidth = content.getPreferredWidth();
                        requiredHeight = content.getPreferredHeight();
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

                        if (eFixed != Fixed.HORIZONTAL)
                        {
                            hScrollbarMax = Math.Max(0, requiredWidth - availWidth);
                            if (hScrollbarMax > 0 || scrollbarsAlwaysVisible || ((scrollbarsToggleFlags & 3) == 3))
                            {
                                repeat |= !visibleH;
                                visibleH = true;
                                int prefHeight = scrollbarH.getPreferredHeight();
                                scrollbarHY = innerHeight - prefHeight;
                                availHeight = Math.Max(0, scrollbarHY - contentScrollbarSpacing.Y);
                            }
                        }
                        else
                        {
                            hScrollbarMax = 0;
                            requiredWidth = availWidth;
                        }

                        if (eFixed != Fixed.VERTICAL)
                        {
                            vScrollbarMax = Math.Max(0, requiredHeight - availHeight);
                            if (vScrollbarMax > 0 || scrollbarsAlwaysVisible || ((scrollbarsToggleFlags & 12) == 12))
                            {
                                repeat |= !visibleV;
                                visibleV = true;
                                int prefWidth = scrollbarV.getPreferredWidth();
                                scrollbarVX = innerWidth - prefWidth;
                                availWidth = Math.Max(0, scrollbarVX - contentScrollbarSpacing.X);
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
                if (visibleH && !scrollbarH.isVisible())
                {
                    scrollbarsToggleFlags |= 1;
                }
                if (!visibleH && scrollbarH.isVisible())
                {
                    scrollbarsToggleFlags |= 2;
                }
                if (visibleV && !scrollbarV.isVisible())
                {
                    scrollbarsToggleFlags |= 4;
                }
                if (!visibleV && scrollbarV.isVisible())
                {
                    scrollbarsToggleFlags |= 8;
                }

                bool changedH = visibleH ^ scrollbarH.isVisible();
                bool changedV = visibleV ^ scrollbarV.isVisible();
                if (changedH || changedV)
                {
                    if ((changedH && eFixed == Fixed.VERTICAL) ||
                       (changedV && eFixed == Fixed.HORIZONTAL))
                    {
                        invalidateLayout();
                    }
                    else
                    {
                        invalidateLayoutLocally();
                    }
                }

                int pageSizeX, pageSizeY;
                if (content is CustomPageSize)
                {
                    CustomPageSize customPageSize = (CustomPageSize)content;
                    pageSizeX = customPageSize.getPageSizeX(availWidth);
                    pageSizeY = customPageSize.getPageSizeY(availHeight);
                }
                else
                {
                    pageSizeX = availWidth;
                    pageSizeY = availHeight;
                }

                scrollbarH.setVisible(visibleH);
                scrollbarH.setMinMaxValue(0, hScrollbarMax);
                scrollbarH.setSize(Math.Max(0, scrollbarVX - scrollbarHX), Math.Max(0, innerHeight - scrollbarHY));
                scrollbarH.setPosition(getInnerX() + scrollbarHX, getInnerY() + scrollbarHY);
                scrollbarH.setPageSize(Math.Max(1, pageSizeX));
                scrollbarH.setStepSize(Math.Max(1, pageSizeX / 10));

                scrollbarV.setVisible(visibleV);
                scrollbarV.setMinMaxValue(0, vScrollbarMax);
                scrollbarV.setSize(Math.Max(0, innerWidth - scrollbarVX), Math.Max(0, scrollbarHY - scrollbarVY));
                scrollbarV.setPosition(getInnerX() + scrollbarVX, getInnerY() + scrollbarVY);
                scrollbarV.setPageSize(Math.Max(1, pageSizeY));
                scrollbarV.setStepSize(Math.Max(1, pageSizeY / 10));

                if (dragButton != null)
                {
                    dragButton.setVisible(visibleH && visibleV);
                    dragButton.setSize(Math.Max(0, innerWidth - scrollbarVX), Math.Max(0, innerHeight - scrollbarHY));
                    dragButton.setPosition(getInnerX() + scrollbarVX, getInnerY() + scrollbarHY);
                }

                contentArea.setPosition(getInnerX(), getInnerY());
                contentArea.setSize(availWidth, availHeight);
                if (content is Scrollable)
                {
                    content.setPosition(contentArea.getX(), contentArea.getY());
                    content.setSize(availWidth, availHeight);
                }
                else if (expandContentSize)
                {
                    content.setSize(Math.Max(availWidth, requiredWidth),
                            Math.Max(availHeight, requiredHeight));
                }
                else
                {
                    content.setSize(Math.Max(0, requiredWidth), Math.Max(0, requiredHeight));
                }

                AnimationState animationState = getAnimationState();
                animationState.setAnimationState(STATE_HORIZONTAL_SCROLLBAR_VISIBLE, visibleH);
                animationState.setAnimationState(STATE_VERTICAL_SCROLLBAR_VISIBLE, visibleV);

                scrollContent();
            }
            else
            {
                scrollbarH.setVisible(false);
                scrollbarV.setVisible(false);
            }
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (evt.isKeyEvent() && content != null && content.canAcceptKeyboardFocus())
            {
                if (content.handleEvent(evt))
                {
                    content.requestKeyboardFocus();
                    return true;
                }
            }
            if (base.handleEvent(evt))
            {
                return true;
            }

            if (evt.getEventType() == EventType.KEY_PRESSED || evt.getEventType() == EventType.KEY_RELEASED)
            {
                int keyCode = evt.getKeyCode();
                if (keyCode == Event.KEY_LEFT ||
                        keyCode == Event.KEY_RIGHT)
                {
                    return scrollbarH.handleEvent(evt);
                }
                if (keyCode == Event.KEY_UP ||
                        keyCode == Event.KEY_DOWN ||
                        keyCode == Event.KEY_PRIOR ||
                        keyCode == Event.KEY_NEXT)
                {
                    return scrollbarV.handleEvent(evt);
                }
            }
            else if (evt.getEventType() == EventType.MOUSE_WHEEL)
            {
                if (scrollbarV.isVisible())
                {
                    return scrollbarV.handleEvent(evt);
                }
                return false;
            }

            return evt.isMouseEvent() && contentArea.isMouseInside(evt);
        }

        //@Override
        protected override void paint(GUI gui)
        {
            if (dragButton != null)
            {
                AnimationState animationState = dragButton.getAnimationState();
                animationState.setAnimationState(STATE_DOWNARROW_ARMED, scrollbarV.isDownRightButtonArmed());
                animationState.setAnimationState(STATE_RIGHTARROW_ARMED, scrollbarH.isDownRightButtonArmed());
            }
            base.paint(gui);
        }

        void scrollContent()
        {
            if (content is Scrollable)
            {
                Scrollable scrollable = (Scrollable)content;
                scrollable.setScrollPosition(scrollbarH.getValue(), scrollbarV.getValue());
            }
            else
            {
                content.setPosition(
                        contentArea.getX() - scrollbarH.getValue(),
                        contentArea.getY() - scrollbarV.getValue());
            }
        }

        void setAutoScrollMarker()
        {
            int scrollPos = scrollbarV.getValue();
            AnimationState animationState = getAnimationState();
            animationState.setAnimationState(STATE_AUTO_SCROLL_UP, autoScrollDirection < 0 && scrollPos > 0);
            animationState.setAnimationState(STATE_AUTO_SCROLL_DOWN, autoScrollDirection > 0 && scrollPos < scrollbarV.getMaxValue());
        }

        void doAutoScroll()
        {
            scrollbarV.setValue(scrollbarV.getValue() + autoScrollDirection * autoScrollSpeed);
            setAutoScrollMarker();
        }
    }
}
