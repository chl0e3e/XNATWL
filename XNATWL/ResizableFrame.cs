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

namespace XNATWL
{
    public class ResizableFrame : Widget
    {
        public static StateKey STATE_FADE = StateKey.Get("fade");

        public enum ResizableAxis
        {
            NONE,
            HORIZONTAL,
            VERTICAL,
            BOTH
        }

        private enum DragMode
        {
            NONE,//("mouseCursor"),
            EDGE_LEFT,//("mouseCursor.left"),
            EDGE_TOP,//("mouseCursor.top"),
            EDGE_RIGHT,//("mouseCursor.right"),
            EDGE_BOTTOM,//("mouseCursor.bottom"),
            CORNER_TL,//("mouseCursor.top-left"),
            CORNER_TR,//("mouseCursor.top-right"),
            CORNER_BR,//("mouseCursor.bottom-right"),
            CORNER_BL,//("mouseCursor.bottom-left"),
            POSITION//("mouseCursor.all");
        }

        private static string DragModeName(DragMode dragMode)
        {
            switch(dragMode)
            {
                case DragMode.NONE:
                    return "mouseCursor";
                case DragMode.EDGE_LEFT:
                    return "mouseCursor.left";
                case DragMode.EDGE_TOP:
                    return "mouseCursor.top";
                case DragMode.EDGE_RIGHT:
                    return "mouseCursor.right";
                case DragMode.EDGE_BOTTOM:
                    return "mouseCursor.bottom";
                case DragMode.CORNER_TL:
                    return "mouseCursor.top-left";
                case DragMode.CORNER_TR:
                    return "mouseCursor.top-right";
                case DragMode.CORNER_BR:
                    return "mouseCursor.bottom-right";
                case DragMode.CORNER_BL:
                    return "mouseCursor.bottom-left";
                case DragMode.POSITION:
                    return "mouseCursor.all";
            }

            return "mouseCursor";
        }

        private static bool ResizableAxisAllowX(ResizableAxis rsAxis)
        {
            switch(rsAxis)
            {
                case ResizableAxis.NONE:
                    return false;
                case ResizableAxis.BOTH:
                    return true;
                case ResizableAxis.HORIZONTAL:
                    return true;
                case ResizableAxis.VERTICAL:
                    return false;
            }

            return false;
        }

        private static bool ResizableAxisAllowY(ResizableAxis rsAxis)
        {
            switch (rsAxis)
            {
                case ResizableAxis.NONE:
                    return false;
                case ResizableAxis.BOTH:
                    return true;
                case ResizableAxis.HORIZONTAL:
                    return false;
                case ResizableAxis.VERTICAL:
                    return true;
            }

            return false;
        }

        private String title;

        private MouseCursor[] cursors;
        private ResizableAxis resizableAxis = ResizableAxis.BOTH;
        private bool draggable = true;
        private bool backgroundDraggable;
        private DragMode dragMode = DragMode.NONE;
        private int dragStartX;
        private int dragStartY;
        private int dragInitialLeft;
        private int dragInitialTop;
        private int dragInitialRight;
        private int dragInitialBottom;

        private Color fadeColorInactive = Color.WHITE;
        private int fadeDurationActivate;
        private int fadeDurationDeactivate;
        private int fadeDurationShow;
        private int fadeDurationHide;

        private TextWidget titleWidget;
        private int titleAreaTop;
        private int titleAreaLeft;
        private int titleAreaRight;
        private int titleAreaBottom;

        private bool hasCloseButton;
        private Button closeButton;
        private int closeButtonX;
        private int closeButtonY;

        private bool hasResizeHandle;
        private Widget resizeHandle;
        private int resizeHandleX;
        private int resizeHandleY;
        private DragMode resizeHandleDragMode;

        public ResizableFrame()
        {
            title = "";
            cursors = new MouseCursor[Enum.GetValues(typeof(DragMode)).Length];
            setCanAcceptKeyboardFocus(true);
        }

        public String getTitle()
        {
            return title;
        }

        public void setTitle(String title)
        {
            this.title = title;
            if (titleWidget != null)
            {
                titleWidget.setCharSequence(title);
            }
        }

        public ResizableAxis getResizableAxis()
        {
            return resizableAxis;
        }

        public void setResizableAxis(ResizableAxis resizableAxis)
        {
            this.resizableAxis = resizableAxis;
            if (resizeHandle != null)
            {
                layoutResizeHandle();
            }
        }

        public bool isDraggable()
        {
            return draggable;
        }

        /**
         * Controls weather the ResizableFrame can be dragged via the title bar or
         * not, default is true.
         * 
         * <p>When set to false the resizing should also be disabled to present a
         * consistent behavior to the user.</p>
         * 
         * @param movable if dragging via the title bar is allowed - default is true.
         */
        public void setDraggable(bool movable)
        {
            this.draggable = movable;
        }

        public bool isBackgroundDraggable()
        {
            return backgroundDraggable;
        }

        /**
         * Controls weather the ResizableFrame can be dragged via the background
         * (eg space not occupied by any widget or a resizable edge), default is false.
         * 
         * <p>This works independent of {@link #setDraggable(bool) }.</p>
         * 
         * @param backgroundDraggable if dragging via the background is allowed - default is false.
         * @see #setDraggable(bool) 
         */
        public void setBackgroundDraggable(bool backgroundDraggable)
        {
            this.backgroundDraggable = backgroundDraggable;
        }

        public bool hasTitleBar()
        {
            return titleWidget != null && titleWidget.getParent() == this;
        }

        public event EventHandler<FrameClosedEventArgs> Closed;

        public void toggleCloseButton(bool use)
        {
            if (use)
            {
                if (closeButton == null)
                {
                    closeButton = new Button();
                    closeButton.setTheme("closeButton");
                    closeButton.setCanAcceptKeyboardFocus(false);
                    add(closeButton);
                    layoutCloseButton();
                }
                closeButton.setVisible(hasCloseButton);
                closeButton.Action += CloseButton_Action;
            }
            else
            {
                if (closeButton != null)
                {
                    closeButton.Action -= CloseButton_Action;
                    closeButton.setVisible(closeButton.hasCallbacks());
                }
            }
        }

        private void CloseButton_Action(object sender, Model.ButtonActionEventArgs e)
        {
            this.Closed.Invoke(this, new FrameClosedEventArgs());
        }

        public void removeCloseCallback(Runnable cb)
        {
        }

        public int getFadeDurationActivate()
        {
            return fadeDurationActivate;
        }

        public int getFadeDurationDeactivate()
        {
            return fadeDurationDeactivate;
        }

        public int getFadeDurationHide()
        {
            return fadeDurationHide;
        }

        public int getFadeDurationShow()
        {
            return fadeDurationShow;
        }

        //@Override
        public override void setVisible(bool visible)
        {
            if (visible)
            {
                TintAnimator tintAnimator = getTintAnimator();
                if ((tintAnimator != null && tintAnimator.HasTint()) || !base.isVisible())
                {
                    fadeTo(hasKeyboardFocus() ? Color.WHITE : fadeColorInactive, fadeDurationShow);
                }
            }
            else if (base.isVisible())
            {
                fadeToHide(fadeDurationHide);
            }
        }

        /**
         * Sets the visibility without triggering a fade
         * @param visible the new visibility flag
         * @see Widget#setVisible(bool)
         */
        public void setHardVisible(bool visible)
        {
            base.setVisible(visible);
        }

        protected void applyThemeResizableFrame(ThemeInfo themeInfo)
        {
            int i = 0;
            foreach (DragMode m in Enum.GetValues(typeof(DragMode)))
            {
                cursors[i] = themeInfo.GetMouseCursor(DragModeName(m));
                i++;
            }
            titleAreaTop = themeInfo.GetParameter("titleAreaTop", 0);
            titleAreaLeft = themeInfo.GetParameter("titleAreaLeft", 0);
            titleAreaRight = themeInfo.GetParameter("titleAreaRight", 0);
            titleAreaBottom = themeInfo.GetParameter("titleAreaBottom", 0);
            closeButtonX = themeInfo.GetParameter("closeButtonX", 0);
            closeButtonY = themeInfo.GetParameter("closeButtonY", 0);
            hasCloseButton = themeInfo.GetParameter("hasCloseButton", false);
            hasResizeHandle = themeInfo.GetParameter("hasResizeHandle", false);
            resizeHandleX = themeInfo.GetParameter("resizeHandleX", 0);
            resizeHandleY = themeInfo.GetParameter("resizeHandleY", 0);
            fadeColorInactive = themeInfo.GetParameter("fadeColorInactive", Color.WHITE);
            fadeDurationActivate = themeInfo.GetParameter("fadeDurationActivate", 0);
            fadeDurationDeactivate = themeInfo.GetParameter("fadeDurationDeactivate", 0);
            fadeDurationShow = themeInfo.GetParameter("fadeDurationShow", 0);
            fadeDurationHide = themeInfo.GetParameter("fadeDurationHide", 0);
            invalidateLayout();

            if (base.isVisible() && !hasKeyboardFocus() &&
                    (getTintAnimator() != null || !Color.WHITE.Equals(fadeColorInactive)))
            {
                fadeTo(fadeColorInactive, 0);
            }
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeResizableFrame(themeInfo);
        }

        //@Override
        protected override void updateTintAnimation()
        {
            TintAnimator tintAnimator = getTintAnimator();
            tintAnimator.Update();
            if (!tintAnimator.IsFadeActive() && tintAnimator.IsZeroAlpha())
            {
                setHardVisible(false);
            }
        }

        protected void fadeTo(Color color, int duration)
        {
            //System.out.println("Start fade to " + color + " over " + duration + " ms");
            allocateTint().FadeTo(color, duration);
            if (!base.isVisible() && color.Alpha != 0)
            {
                setHardVisible(true);
            }
        }

        protected void fadeToHide(int duration)
        {
            if (duration <= 0)
            {
                setHardVisible(false);
            }
            else
            {
                allocateTint().FadeToHide(duration);
            }
        }

        private TintAnimator allocateTint()
        {
            TintAnimator tintAnimator = getTintAnimator();
            if (tintAnimator == null)
            {
                tintAnimator = new TintAnimator(new TintAnimator.AnimationStateTimeSource(getAnimationState(), STATE_FADE));
                setTintAnimator(tintAnimator);
                if (!base.isVisible())
                {
                    // we start with TRANSPARENT when hidden
                    tintAnimator.FadeToHide(0);
                }
            }
            return tintAnimator;
        }

        protected bool isFrameElement(Widget widget)
        {
            return widget == titleWidget /*|| widget == closeButton*/ || widget == resizeHandle;
        }

        //@Override
        protected override void layout()
        {
            int minWidth = getMinWidth();
            int minHeight = getMinHeight();
            if (getWidth() < minWidth || getHeight() < minHeight)
            {
                int width = Math.Max(getWidth(), minWidth);
                int height = Math.Max(getHeight(), minHeight);
                if (getParent() != null)
                {
                    int x = Math.Min(getX(), getParent().getInnerRight() - width);
                    int y = Math.Min(getY(), getParent().getInnerBottom() - height);
                    setPosition(x, y);
                }
                setSize(width, height);
            }

            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    layoutChildFullInnerArea(child);
                }
            }

            layoutTitle();
            layoutCloseButton();
            layoutResizeHandle();
        }

        protected void layoutTitle()
        {
            int titleX = getTitleX(titleAreaLeft);
            int titleY = getTitleY(titleAreaTop);
            int titleWidth = Math.Max(0, getTitleX(titleAreaRight) - titleX);
            int titleHeight = Math.Max(0, getTitleY(titleAreaBottom) - titleY);

            if (titleAreaLeft != titleAreaRight && titleAreaTop != titleAreaBottom)
            {
                if (titleWidget == null)
                {
                    titleWidget = new TextWidget(getAnimationState());
                    titleWidget.setTheme("title");
                    //titleWidget.setMouseCursor(cursors[DragMode.POSITION.ordinal()]); // TODO: cursors
                    titleWidget.setCharSequence(title);
                    titleWidget.setClip(true);
                }

                if (titleWidget.getParent() == null)
                {
                    insertChild(titleWidget, 0);
                }

                titleWidget.setPosition(titleX, titleY);
                titleWidget.setSize(titleWidth, titleHeight);
            }
            else if (titleWidget != null && titleWidget.getParent() == this)
            {
                titleWidget.destroy();
                removeChild(titleWidget);
            }
        }

        protected void layoutCloseButton()
        {
            if (closeButton != null)
            {
                closeButton.adjustSize();
                closeButton.setPosition(
                        getTitleX(closeButtonX),
                        getTitleY(closeButtonY));
                closeButton.setVisible(closeButton.hasCallbacks() && hasCloseButton);
            }
        }

        protected void layoutResizeHandle()
        {
            if (hasResizeHandle && resizeHandle == null)
            {
                resizeHandle = new Widget(getAnimationState(), true);
                resizeHandle.setTheme("resizeHandle");
                base.insertChild(resizeHandle, 0);
            }
            if (resizeHandle != null)
            {
                if (resizeHandleX > 0)
                {
                    if (resizeHandleY > 0)
                    {
                        resizeHandleDragMode = DragMode.CORNER_TL;
                    }
                    else
                    {
                        resizeHandleDragMode = DragMode.CORNER_TR;
                    }
                }
                else if (resizeHandleY > 0)
                {
                    resizeHandleDragMode = DragMode.CORNER_BL;
                }
                else
                {
                    resizeHandleDragMode = DragMode.CORNER_BR;
                }

                resizeHandle.adjustSize();
                resizeHandle.setPosition(
                        getTitleX(resizeHandleX),
                        getTitleY(resizeHandleY));
                resizeHandle.setVisible(hasResizeHandle &&
                        resizableAxis == ResizableAxis.BOTH);
            }
            else
            {
                resizeHandleDragMode = DragMode.NONE;
            }
        }

        //@Override
        protected override void keyboardFocusGained()
        {
            fadeTo(Color.WHITE, fadeDurationActivate);
        }

        //@Override
        protected override void keyboardFocusLost()
        {
            if (!hasOpenPopups() && base.isVisible())
            {
                fadeTo(fadeColorInactive, fadeDurationDeactivate);
            }
        }

        //@Override
        public override int getMinWidth()
        {
            int minWidth = base.getMinWidth();
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    minWidth = Math.Max(minWidth, child.getMinWidth() + getBorderHorizontal());
                }
            }
            if (hasTitleBar() && titleAreaRight < 0)
            {
                minWidth = Math.Max(minWidth, titleWidget.getPreferredWidth() + titleAreaLeft - titleAreaRight);
            }
            return minWidth;
        }

        //@Override
        public override int getMinHeight()
        {
            int minHeight = base.getMinHeight();
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    minHeight = Math.Max(minHeight, child.getMinHeight() + getBorderVertical());
                }
            }
            return minHeight;
        }

        //@Override
        public override int getMaxWidth()
        {
            int maxWidth = base.getMaxWidth();
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    int aMaxWidth = child.getMaxWidth();
                    if (aMaxWidth > 0)
                    {
                        aMaxWidth += getBorderHorizontal();
                        if (maxWidth == 0 || aMaxWidth < maxWidth)
                        {
                            maxWidth = aMaxWidth;
                        }
                    }
                }
            }
            return maxWidth;
        }

        //@Override
        public override int getMaxHeight()
        {
            int maxHeight = base.getMaxHeight();
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    int aMaxHeight = child.getMaxHeight();
                    if (aMaxHeight > 0)
                    {
                        aMaxHeight += getBorderVertical();
                        if (maxHeight == 0 || aMaxHeight < maxHeight)
                        {
                            maxHeight = aMaxHeight;
                        }
                    }
                }
            }
            return maxHeight;
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            int prefWidth = 0;
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    prefWidth = Math.Max(prefWidth, child.getPreferredWidth());
                }
            }
            return prefWidth;
        }

        //@Override
        public override int getPreferredWidth()
        {
            int prefWidth = base.getPreferredWidth();
            if (hasTitleBar() && titleAreaRight < 0)
            {
                prefWidth = Math.Max(prefWidth, titleWidget.getPreferredWidth() + titleAreaLeft - titleAreaRight);
            }
            return prefWidth;
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            int prefHeight = 0;
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                if (!isFrameElement(child))
                {
                    prefHeight = Math.Max(prefHeight, child.getPreferredHeight());
                }
            }
            return prefHeight;
        }

        //@Override
        public override void adjustSize()
        {
            layoutTitle();
            base.adjustSize();
        }

        private int getTitleX(int offset)
        {
            return (offset < 0) ? getRight() + offset : getX() + offset;
        }

        private int getTitleY(int offset)
        {
            return (offset < 0) ? getBottom() + offset : getY() + offset;
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            bool isMouseExit = evt.getEventType() == EventType.MOUSE_EXITED;

            if (isMouseExit && resizeHandle != null && resizeHandle.isVisible())
            {
                resizeHandle.getAnimationState().setAnimationState(
                        TextWidget.STATE_HOVER, false);
            }

            if (dragMode != DragMode.NONE)
            {
                if (evt.isMouseDragEnd())
                {
                    dragMode = DragMode.NONE;
                }
                else if (evt.getEventType() == EventType.MOUSE_DRAGGED)
                {
                    handleMouseDrag(evt);
                }
                return true;
            }

            if (!isMouseExit && resizeHandle != null && resizeHandle.isVisible())
            {
                resizeHandle.getAnimationState().setAnimationState(
                        TextWidget.STATE_HOVER, resizeHandle.isMouseInside(evt));
            }

            if (!evt.isMouseDragEvent())
            {
                if (evt.getEventType() == EventType.MOUSE_BTNDOWN &&
                        evt.getMouseButton() == Event.MOUSE_LBUTTON &&
                        handleMouseDown(evt))
                {
                    return true;
                }
            }

            if (base.handleEvent(evt))
            {
                return true;
            }

            return evt.isMouseEvent();
        }

        //@Override
        public override MouseCursor getMouseCursor(Event evt)
        {
            DragMode cursorMode = dragMode;
            if (cursorMode == DragMode.NONE)
            {
                cursorMode = getDragMode(evt.getMouseX(), evt.getMouseY());
                if (cursorMode == DragMode.NONE)
                {
                    return getMouseCursor();
                }
            }

            DragMode[] dragModes = (DragMode[]) Enum.GetValues(typeof(DragMode));
            for (int i = 0; i < dragModes.Length; i++)
            {
                if (dragModes[i] == cursorMode)
                {
                    return cursors[i];
                }
            }
            return DefaultMouseCursor.OS_DEFAULT;
        }

        private DragMode getDragMode(int mx, int my)
        {
            bool left = mx < getInnerX();
            bool right = mx >= getInnerRight();

            bool top = my < getInnerY();
            bool bot = my >= getInnerBottom();

            if (hasTitleBar())
            {
                if (titleWidget.isInside(mx, my))
                {
                    if (draggable)
                    {
                        return DragMode.POSITION;
                    }
                    else
                    {
                        return DragMode.NONE;
                    }
                }
                top = my < titleWidget.getY();
            }

            if (closeButton != null && closeButton.isVisible() && closeButton.isInside(mx, my))
            {
                return DragMode.NONE;
            }

            if (resizableAxis == ResizableAxis.NONE)
            {
                if (backgroundDraggable)
                {
                    return DragMode.POSITION;
                }
                return DragMode.NONE;
            }

            if (resizeHandle != null && resizeHandle.isVisible() && resizeHandle.isInside(mx, my))
            {
                return resizeHandleDragMode;
            }

            if (!ResizableAxisAllowX(resizableAxis))
            {
                left = false;
                right = false;
            }
            if (!ResizableAxisAllowY(resizableAxis))
            {
                top = false;
                bot = false;  // TODO Resizablity
            }

            if (left)
            {
                if (top)
                {
                    return DragMode.CORNER_TL;
                }
                if (bot)
                {
                    return DragMode.CORNER_BL;
                }
                return DragMode.EDGE_LEFT;
            }
            if (right)
            {
                if (top)
                {
                    return DragMode.CORNER_TR;
                }
                if (bot)
                {
                    return DragMode.CORNER_BR;
                }
                return DragMode.EDGE_RIGHT;
            }
            if (top)
            {
                return DragMode.EDGE_TOP;
            }
            if (bot)
            {
                return DragMode.EDGE_BOTTOM;
            }
            if (backgroundDraggable)
            {
                return DragMode.POSITION;
            }
            return DragMode.NONE;
        }

        private bool handleMouseDown(Event evt)
        {
            int mx = evt.getMouseX();
            int my = evt.getMouseY();

            dragStartX = mx;
            dragStartY = my;
            dragInitialLeft = getX();
            dragInitialTop = getY();
            dragInitialRight = getRight();
            dragInitialBottom = getBottom();

            dragMode = getDragMode(mx, my);
            return dragMode != DragMode.NONE;
        }

        private void handleMouseDrag(Event evt)
        {
            int dx = evt.getMouseX() - dragStartX;
            int dy = evt.getMouseY() - dragStartY;

            int minWidth = getMinWidth();
            int minHeight = getMinHeight();
            int maxWidth = getMaxWidth();
            int maxHeight = getMaxHeight();

            // make sure max size is not smaller then min size
            if (maxWidth > 0 && maxWidth < minWidth)
            {
                maxWidth = minWidth;
            }
            if (maxHeight > 0 && maxHeight < minHeight)
            {
                maxHeight = minHeight;
            }

            int left = dragInitialLeft;
            int top = dragInitialTop;
            int right = dragInitialRight;
            int bottom = dragInitialBottom;

            switch (dragMode)
            {
                case DragMode.CORNER_BL:
                case DragMode.CORNER_TL:
                case DragMode.EDGE_LEFT:
                    left = Math.Min(left + dx, right - minWidth);
                    if (maxWidth > 0)
                    {
                        left = Math.Max(left, Math.Min(dragInitialLeft, right - maxWidth));
                    }
                    break;
                case DragMode.CORNER_BR:
                case DragMode.CORNER_TR:
                case DragMode.EDGE_RIGHT:
                    right = Math.Max(right + dx, left + minWidth);
                    if (maxWidth > 0)
                    {
                        right = Math.Min(right, Math.Max(dragInitialRight, left + maxWidth));
                    }
                    break;
                case DragMode.POSITION:
                    if (getParent() != null)
                    {
                        int minX = getParent().getInnerX();
                        int maxX = getParent().getInnerRight();
                        int width = dragInitialRight - dragInitialLeft;
                        left = Math.Max(minX, Math.Min(maxX - width, left + dx));
                        right = Math.Min(maxX, Math.Max(minX + width, right + dx));
                    }
                    else
                    {
                        left += dx;
                        right += dx;
                    }
                    break;
            }

            switch (dragMode)
            {
                case DragMode.CORNER_TL:
                case DragMode.CORNER_TR:
                case DragMode.EDGE_TOP:
                    top = Math.Min(top + dy, bottom - minHeight);
                    if (maxHeight > 0)
                    {
                        top = Math.Max(top, Math.Min(dragInitialTop, bottom - maxHeight));
                    }
                    break;
                case DragMode.CORNER_BL:
                case DragMode.CORNER_BR:
                case DragMode.EDGE_BOTTOM:
                    bottom = Math.Max(bottom + dy, top + minHeight);
                    if (maxHeight > 0)
                    {
                        bottom = Math.Min(bottom, Math.Max(dragInitialBottom, top + maxHeight));
                    }
                    break;
                case DragMode.POSITION:
                    if (getParent() != null)
                    {
                        int minY = getParent().getInnerY();
                        int maxY = getParent().getInnerBottom();
                        int height = dragInitialBottom - dragInitialTop;
                        top = Math.Max(minY, Math.Min(maxY - height, top + dy));
                        bottom = Math.Min(maxY, Math.Max(minY + height, bottom + dy));
                    }
                    else
                    {
                        top += dy;
                        bottom += dy;
                    }
                    break;
            }

            setArea(top, left, right, bottom);
        }

        private void setArea(int top, int left, int right, int bottom)
        {
            Widget p = getParent();
            if (p != null)
            {
                top = Math.Max(top, p.getInnerY());
                left = Math.Max(left, p.getInnerX());
                right = Math.Min(right, p.getInnerRight());
                bottom = Math.Min(bottom, p.getInnerBottom());
            }

            setPosition(left, top);
            setSize(Math.Max(getMinWidth(), right - left),
                    Math.Max(getMinHeight(), bottom - top));
        }
    }

    public class FrameClosedEventArgs
    {
    }
}
