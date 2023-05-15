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
            None,
            Horizontal,
            Vertical,
            Both
        }

        private enum DragMode
        {
            None,//("mouseCursor"),
            EdgeLeft,//("mouseCursor.left"),
            EdgeTop,//("mouseCursor.top"),
            EdgeRight,//("mouseCursor.right"),
            EdgeBottom,//("mouseCursor.bottom"),
            CornerTopLeft,//("mouseCursor.top-left"),
            CornerTopRight,//("mouseCursor.top-right"),
            CornerBottomRight,//("mouseCursor.bottom-right"),
            CornerBottomLeft,//("mouseCursor.bottom-left"),
            Position//("mouseCursor.all");
        }

        private static string DragModeName(DragMode dragMode)
        {
            switch(dragMode)
            {
                case DragMode.None:
                    return "mouseCursor";
                case DragMode.EdgeLeft:
                    return "mouseCursor.left";
                case DragMode.EdgeTop:
                    return "mouseCursor.top";
                case DragMode.EdgeRight:
                    return "mouseCursor.right";
                case DragMode.EdgeBottom:
                    return "mouseCursor.bottom";
                case DragMode.CornerTopLeft:
                    return "mouseCursor.top-left";
                case DragMode.CornerTopRight:
                    return "mouseCursor.top-right";
                case DragMode.CornerBottomRight:
                    return "mouseCursor.bottom-right";
                case DragMode.CornerBottomLeft:
                    return "mouseCursor.bottom-left";
                case DragMode.Position:
                    return "mouseCursor.all";
            }

            return "mouseCursor";
        }

        private static bool ResizableAxisAllowX(ResizableAxis rsAxis)
        {
            switch(rsAxis)
            {
                case ResizableAxis.None:
                    return false;
                case ResizableAxis.Both:
                    return true;
                case ResizableAxis.Horizontal:
                    return true;
                case ResizableAxis.Vertical:
                    return false;
            }

            return false;
        }

        private static bool ResizableAxisAllowY(ResizableAxis rsAxis)
        {
            switch (rsAxis)
            {
                case ResizableAxis.None:
                    return false;
                case ResizableAxis.Both:
                    return true;
                case ResizableAxis.Horizontal:
                    return false;
                case ResizableAxis.Vertical:
                    return true;
            }

            return false;
        }

        private String _title;

        private MouseCursor[] _cursors;
        private ResizableAxis _resizableAxis = ResizableAxis.Both;
        private bool _draggable = true;
        private bool _backgroundDraggable;
        private DragMode _dragMode = DragMode.None;
        private int _dragStartX;
        private int _dragStartY;
        private int _dragInitialLeft;
        private int _dragInitialTop;
        private int _dragInitialRight;
        private int _dragInitialBottom;

        private Color _fadeColorInactive = Color.WHITE;
        private int _fadeDurationActivate;
        private int _fadeDurationDeactivate;
        private int _fadeDurationShow;
        private int _fadeDurationHide;

        private TextWidget _titleWidget;
        private int _titleAreaTop;
        private int _titleAreaLeft;
        private int _titleAreaRight;
        private int _titleAreaBottom;

        private bool _hasCloseButton;
        private Button _closeButton;
        private int _closeButtonX;
        private int _closeButtonY;

        private bool _hasResizeHandle;
        private Widget _resizeHandle;
        private int _resizeHandleX;
        private int _resizeHandleY;
        private DragMode _resizeHandleDragMode;

        public ResizableFrame()
        {
            _title = "";
            _cursors = new MouseCursor[Enum.GetValues(typeof(DragMode)).Length];
            SetCanAcceptKeyboardFocus(true);
        }

        public String GetTitle()
        {
            return _title;
        }

        public void SetTitle(String title)
        {
            this._title = title;
            if (_titleWidget != null)
            {
                _titleWidget.SetCharSequence(title);
            }
        }

        public ResizableAxis GetResizableAxis()
        {
            return _resizableAxis;
        }

        public void SetResizableAxis(ResizableAxis resizableAxis)
        {
            this._resizableAxis = resizableAxis;
            if (_resizeHandle != null)
            {
                LayoutResizeHandle();
            }
        }

        public bool IsDraggable()
        {
            return _draggable;
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
        public void SetDraggable(bool movable)
        {
            this._draggable = movable;
        }

        public bool IsBackgroundDraggable()
        {
            return _backgroundDraggable;
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
        public void SetBackgroundDraggable(bool backgroundDraggable)
        {
            this._backgroundDraggable = backgroundDraggable;
        }

        public bool HasTitleBar()
        {
            return _titleWidget != null && _titleWidget.GetParent() == this;
        }

        public event EventHandler<FrameClosedEventArgs> Closed;

        public void ToggleCloseButton(bool use)
        {
            if (use)
            {
                if (_closeButton == null)
                {
                    _closeButton = new Button();
                    _closeButton.SetTheme("closeButton");
                    _closeButton.SetCanAcceptKeyboardFocus(false);
                    Add(_closeButton);
                    LayoutCloseButton();
                }
                _closeButton.SetVisible(_hasCloseButton);
                _closeButton.Action += CloseButton_Action;
            }
            else
            {
                if (_closeButton != null)
                {
                    _closeButton.Action -= CloseButton_Action;
                    _closeButton.SetVisible(_closeButton.HasCallbacks());
                }
            }
        }

        private void CloseButton_Action(object sender, Model.ButtonActionEventArgs e)
        {
            this.Closed.Invoke(this, new FrameClosedEventArgs());
        }

        public void RemoveCloseCallback(Runnable cb)
        {
        }

        public int GetFadeDurationActivate()
        {
            return _fadeDurationActivate;
        }

        public int GetFadeDurationDeactivate()
        {
            return _fadeDurationDeactivate;
        }

        public int GetFadeDurationHide()
        {
            return _fadeDurationHide;
        }

        public int GetFadeDurationShow()
        {
            return _fadeDurationShow;
        }

        public override void SetVisible(bool visible)
        {
            if (visible)
            {
                TintAnimator tintAnimator = GetTintAnimator();
                if ((tintAnimator != null && tintAnimator.HasTint()) || !base.IsVisible())
                {
                    FadeTo(HasKeyboardFocus() ? Color.WHITE : _fadeColorInactive, _fadeDurationShow);
                }
            }
            else if (base.IsVisible())
            {
                FadeToHide(_fadeDurationHide);
            }
        }

        /**
         * Sets the visibility without triggering a fade
         * @param visible the new visibility flag
         * @see Widget#setVisible(bool)
         */
        public void SetHardVisible(bool visible)
        {
            base.SetVisible(visible);
        }

        protected void ApplyThemeResizableFrame(ThemeInfo themeInfo)
        {
            int i = 0;
            foreach (DragMode m in Enum.GetValues(typeof(DragMode)))
            {
                _cursors[i] = themeInfo.GetMouseCursor(DragModeName(m));
                i++;
            }
            _titleAreaTop = themeInfo.GetParameter("titleAreaTop", 0);
            _titleAreaLeft = themeInfo.GetParameter("titleAreaLeft", 0);
            _titleAreaRight = themeInfo.GetParameter("titleAreaRight", 0);
            _titleAreaBottom = themeInfo.GetParameter("titleAreaBottom", 0);
            _closeButtonX = themeInfo.GetParameter("closeButtonX", 0);
            _closeButtonY = themeInfo.GetParameter("closeButtonY", 0);
            _hasCloseButton = themeInfo.GetParameter("hasCloseButton", false);
            _hasResizeHandle = themeInfo.GetParameter("hasResizeHandle", false);
            _resizeHandleX = themeInfo.GetParameter("resizeHandleX", 0);
            _resizeHandleY = themeInfo.GetParameter("resizeHandleY", 0);
            _fadeColorInactive = themeInfo.GetParameter("fadeColorInactive", Color.WHITE);
            _fadeDurationActivate = themeInfo.GetParameter("fadeDurationActivate", 0);
            _fadeDurationDeactivate = themeInfo.GetParameter("fadeDurationDeactivate", 0);
            _fadeDurationShow = themeInfo.GetParameter("fadeDurationShow", 0);
            _fadeDurationHide = themeInfo.GetParameter("fadeDurationHide", 0);
            InvalidateLayout();

            if (base.IsVisible() && !HasKeyboardFocus() &&
                    (GetTintAnimator() != null || !Color.WHITE.Equals(_fadeColorInactive)))
            {
                FadeTo(_fadeColorInactive, 0);
            }
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeResizableFrame(themeInfo);
        }

        protected override void UpdateTintAnimation()
        {
            TintAnimator tintAnimator = GetTintAnimator();
            tintAnimator.Update();
            if (!tintAnimator.IsFadeActive() && tintAnimator.IsZeroAlpha())
            {
                SetHardVisible(false);
            }
        }

        protected void FadeTo(Color color, int duration)
        {
            //System.out.println("Start fade to " + color + " over " + duration + " ms");
            AllocateTint().FadeTo(color, duration);
            if (!base.IsVisible() && color.Alpha != 0)
            {
                SetHardVisible(true);
            }
        }

        protected void FadeToHide(int duration)
        {
            if (duration <= 0)
            {
                SetHardVisible(false);
            }
            else
            {
                AllocateTint().FadeToHide(duration);
            }
        }

        private TintAnimator AllocateTint()
        {
            TintAnimator tintAnimator = GetTintAnimator();
            if (tintAnimator == null)
            {
                tintAnimator = new TintAnimator(new TintAnimator.AnimationStateTimeSource(GetAnimationState(), STATE_FADE));
                GetTintAnimator(tintAnimator);
                if (!base.IsVisible())
                {
                    // we start with TRANSPARENT when hidden
                    tintAnimator.FadeToHide(0);
                }
            }
            return tintAnimator;
        }

        protected bool IsFrameElement(Widget widget)
        {
            return widget == _titleWidget /*|| widget == closeButton*/ || widget == _resizeHandle;
        }

        protected override void Layout()
        {
            int minWidth = GetMinWidth();
            int minHeight = GetMinHeight();
            if (GetWidth() < minWidth || GetHeight() < minHeight)
            {
                int width = Math.Max(GetWidth(), minWidth);
                int height = Math.Max(GetHeight(), minHeight);
                if (GetParent() != null)
                {
                    int x = Math.Min(GetX(), GetParent().GetInnerRight() - width);
                    int y = Math.Min(GetY(), GetParent().GetInnerBottom() - height);
                    SetPosition(x, y);
                }
                SetSize(width, height);
            }

            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    LayoutChildFullInnerArea(child);
                }
            }

            LayoutTitle();
            LayoutCloseButton();
            LayoutResizeHandle();
        }

        protected void LayoutTitle()
        {
            int titleX = GetTitleX(_titleAreaLeft);
            int titleY = GetTitleY(_titleAreaTop);
            int titleWidth = Math.Max(0, GetTitleX(_titleAreaRight) - titleX);
            int titleHeight = Math.Max(0, GetTitleY(_titleAreaBottom) - titleY);

            if (_titleAreaLeft != _titleAreaRight && _titleAreaTop != _titleAreaBottom)
            {
                if (_titleWidget == null)
                {
                    _titleWidget = new TextWidget(GetAnimationState());
                    _titleWidget.SetTheme("title");
                    //titleWidget.setMouseCursor(cursors[DragMode.POSITION.ordinal()]); // TODO: cursors
                    _titleWidget.SetCharSequence(_title);
                    _titleWidget.SetClip(true);
                }

                if (_titleWidget.GetParent() == null)
                {
                    InsertChild(_titleWidget, 0);
                }

                _titleWidget.SetPosition(titleX, titleY);
                _titleWidget.SetSize(titleWidth, titleHeight);
            }
            else if (_titleWidget != null && _titleWidget.GetParent() == this)
            {
                _titleWidget.Destroy();
                RemoveChild(_titleWidget);
            }
        }

        protected void LayoutCloseButton()
        {
            if (_closeButton != null)
            {
                _closeButton.AdjustSize();
                _closeButton.SetPosition(
                        GetTitleX(_closeButtonX),
                        GetTitleY(_closeButtonY));
                _closeButton.SetVisible(_closeButton.HasCallbacks() && _hasCloseButton);
            }
        }

        protected void LayoutResizeHandle()
        {
            if (_hasResizeHandle && _resizeHandle == null)
            {
                _resizeHandle = new Widget(GetAnimationState(), true);
                _resizeHandle.SetTheme("resizeHandle");
                base.InsertChild(_resizeHandle, 0);
            }
            if (_resizeHandle != null)
            {
                if (_resizeHandleX > 0)
                {
                    if (_resizeHandleY > 0)
                    {
                        _resizeHandleDragMode = DragMode.CornerTopLeft;
                    }
                    else
                    {
                        _resizeHandleDragMode = DragMode.CornerTopRight;
                    }
                }
                else if (_resizeHandleY > 0)
                {
                    _resizeHandleDragMode = DragMode.CornerBottomLeft;
                }
                else
                {
                    _resizeHandleDragMode = DragMode.CornerBottomRight;
                }

                _resizeHandle.AdjustSize();
                _resizeHandle.SetPosition(
                        GetTitleX(_resizeHandleX),
                        GetTitleY(_resizeHandleY));
                _resizeHandle.SetVisible(_hasResizeHandle &&
                        _resizableAxis == ResizableAxis.Both);
            }
            else
            {
                _resizeHandleDragMode = DragMode.None;
            }
        }

        protected override void KeyboardFocusGained()
        {
            FadeTo(Color.WHITE, _fadeDurationActivate);
        }

        protected override void KeyboardFocusLost()
        {
            if (!HasOpenPopups() && base.IsVisible())
            {
                FadeTo(_fadeColorInactive, _fadeDurationDeactivate);
            }
        }

        public override int GetMinWidth()
        {
            int minWidth = base.GetMinWidth();
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    minWidth = Math.Max(minWidth, child.GetMinWidth() + GetBorderHorizontal());
                }
            }
            if (HasTitleBar() && _titleAreaRight < 0)
            {
                minWidth = Math.Max(minWidth, _titleWidget.GetPreferredWidth() + _titleAreaLeft - _titleAreaRight);
            }
            return minWidth;
        }

        public override int GetMinHeight()
        {
            int minHeight = base.GetMinHeight();
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    minHeight = Math.Max(minHeight, child.GetMinHeight() + GetBorderVertical());
                }
            }
            return minHeight;
        }

        public override int GetMaxWidth()
        {
            int maxWidth = base.GetMaxWidth();
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    int aMaxWidth = child.GetMaxWidth();
                    if (aMaxWidth > 0)
                    {
                        aMaxWidth += GetBorderHorizontal();
                        if (maxWidth == 0 || aMaxWidth < maxWidth)
                        {
                            maxWidth = aMaxWidth;
                        }
                    }
                }
            }
            return maxWidth;
        }

        public override int GetMaxHeight()
        {
            int maxHeight = base.GetMaxHeight();
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    int aMaxHeight = child.GetMaxHeight();
                    if (aMaxHeight > 0)
                    {
                        aMaxHeight += GetBorderVertical();
                        if (maxHeight == 0 || aMaxHeight < maxHeight)
                        {
                            maxHeight = aMaxHeight;
                        }
                    }
                }
            }
            return maxHeight;
        }

        public override int GetPreferredInnerWidth()
        {
            int prefWidth = 0;
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    prefWidth = Math.Max(prefWidth, child.GetPreferredWidth());
                }
            }
            return prefWidth;
        }

        public override int GetPreferredWidth()
        {
            int prefWidth = base.GetPreferredWidth();
            if (HasTitleBar() && _titleAreaRight < 0)
            {
                prefWidth = Math.Max(prefWidth, _titleWidget.GetPreferredWidth() + _titleAreaLeft - _titleAreaRight);
            }
            return prefWidth;
        }

        public override int GetPreferredInnerHeight()
        {
            int prefHeight = 0;
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                if (!IsFrameElement(child))
                {
                    prefHeight = Math.Max(prefHeight, child.GetPreferredHeight());
                }
            }
            return prefHeight;
        }

        public override void AdjustSize()
        {
            LayoutTitle();
            base.AdjustSize();
        }

        private int GetTitleX(int offset)
        {
            return (offset < 0) ? GetRight() + offset : GetX() + offset;
        }

        private int GetTitleY(int offset)
        {
            return (offset < 0) ? GetBottom() + offset : GetY() + offset;
        }

        public override bool HandleEvent(Event evt)
        {
            bool isMouseExit = evt.GetEventType() == EventType.MOUSE_EXITED;

            if (isMouseExit && _resizeHandle != null && _resizeHandle.IsVisible())
            {
                _resizeHandle.GetAnimationState().SetAnimationState(
                        TextWidget.STATE_HOVER, false);
            }

            if (_dragMode != DragMode.None)
            {
                if (evt.IsMouseDragEnd())
                {
                    _dragMode = DragMode.None;
                }
                else if (evt.GetEventType() == EventType.MOUSE_DRAGGED)
                {
                    HandleMouseDrag(evt);
                }
                return true;
            }

            if (!isMouseExit && _resizeHandle != null && _resizeHandle.IsVisible())
            {
                _resizeHandle.GetAnimationState().SetAnimationState(
                        TextWidget.STATE_HOVER, _resizeHandle.IsMouseInside(evt));
            }

            if (!evt.IsMouseDragEvent())
            {
                if (evt.GetEventType() == EventType.MOUSE_BTNDOWN &&
                        evt.GetMouseButton() == Event.MOUSE_LBUTTON &&
                        HandleMouseDown(evt))
                {
                    return true;
                }
            }

            if (base.HandleEvent(evt))
            {
                return true;
            }

            return evt.IsMouseEvent();
        }

        public override MouseCursor GetMouseCursor(Event evt)
        {
            DragMode cursorMode = _dragMode;
            if (cursorMode == DragMode.None)
            {
                cursorMode = GetDragMode(evt.GetMouseX(), evt.GetMouseY());
                if (cursorMode == DragMode.None)
                {
                    return GetMouseCursor();
                }
            }

            DragMode[] dragModes = (DragMode[]) Enum.GetValues(typeof(DragMode));
            for (int i = 0; i < dragModes.Length; i++)
            {
                if (dragModes[i] == cursorMode)
                {
                    return _cursors[i];
                }
            }
            return DefaultMouseCursor.OS_DEFAULT;
        }

        private DragMode GetDragMode(int mx, int my)
        {
            bool left = mx < GetInnerX();
            bool right = mx >= GetInnerRight();

            bool top = my < GetInnerY();
            bool bot = my >= GetInnerBottom();

            if (HasTitleBar())
            {
                if (_titleWidget.IsInside(mx, my))
                {
                    if (_draggable)
                    {
                        return DragMode.Position;
                    }
                    else
                    {
                        return DragMode.None;
                    }
                }
                top = my < _titleWidget.GetY();
            }

            if (_closeButton != null && _closeButton.IsVisible() && _closeButton.IsInside(mx, my))
            {
                return DragMode.None;
            }

            if (_resizableAxis == ResizableAxis.None)
            {
                if (_backgroundDraggable)
                {
                    return DragMode.Position;
                }
                return DragMode.None;
            }

            if (_resizeHandle != null && _resizeHandle.IsVisible() && _resizeHandle.IsInside(mx, my))
            {
                return _resizeHandleDragMode;
            }

            if (!ResizableAxisAllowX(_resizableAxis))
            {
                left = false;
                right = false;
            }
            if (!ResizableAxisAllowY(_resizableAxis))
            {
                top = false;
                bot = false;  // TODO Resizablity
            }

            if (left)
            {
                if (top)
                {
                    return DragMode.CornerTopLeft;
                }
                if (bot)
                {
                    return DragMode.CornerBottomLeft;
                }
                return DragMode.EdgeLeft;
            }
            if (right)
            {
                if (top)
                {
                    return DragMode.CornerTopRight;
                }
                if (bot)
                {
                    return DragMode.CornerBottomRight;
                }
                return DragMode.EdgeRight;
            }
            if (top)
            {
                return DragMode.EdgeTop;
            }
            if (bot)
            {
                return DragMode.EdgeBottom;
            }
            if (_backgroundDraggable)
            {
                return DragMode.Position;
            }
            return DragMode.None;
        }

        private bool HandleMouseDown(Event evt)
        {
            int mx = evt.GetMouseX();
            int my = evt.GetMouseY();

            _dragStartX = mx;
            _dragStartY = my;
            _dragInitialLeft = GetX();
            _dragInitialTop = GetY();
            _dragInitialRight = GetRight();
            _dragInitialBottom = GetBottom();

            _dragMode = GetDragMode(mx, my);
            return _dragMode != DragMode.None;
        }

        private void HandleMouseDrag(Event evt)
        {
            int dx = evt.GetMouseX() - _dragStartX;
            int dy = evt.GetMouseY() - _dragStartY;

            int minWidth = GetMinWidth();
            int minHeight = GetMinHeight();
            int maxWidth = GetMaxWidth();
            int maxHeight = GetMaxHeight();

            // make sure max size is not smaller then min size
            if (maxWidth > 0 && maxWidth < minWidth)
            {
                maxWidth = minWidth;
            }
            if (maxHeight > 0 && maxHeight < minHeight)
            {
                maxHeight = minHeight;
            }

            int left = _dragInitialLeft;
            int top = _dragInitialTop;
            int right = _dragInitialRight;
            int bottom = _dragInitialBottom;

            switch (_dragMode)
            {
                case DragMode.CornerBottomLeft:
                case DragMode.CornerTopLeft:
                case DragMode.EdgeLeft:
                    left = Math.Min(left + dx, right - minWidth);
                    if (maxWidth > 0)
                    {
                        left = Math.Max(left, Math.Min(_dragInitialLeft, right - maxWidth));
                    }
                    break;
                case DragMode.CornerBottomRight:
                case DragMode.CornerTopRight:
                case DragMode.EdgeRight:
                    right = Math.Max(right + dx, left + minWidth);
                    if (maxWidth > 0)
                    {
                        right = Math.Min(right, Math.Max(_dragInitialRight, left + maxWidth));
                    }
                    break;
                case DragMode.Position:
                    if (GetParent() != null)
                    {
                        int minX = GetParent().GetInnerX();
                        int maxX = GetParent().GetInnerRight();
                        int width = _dragInitialRight - _dragInitialLeft;
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

            switch (_dragMode)
            {
                case DragMode.CornerTopLeft:
                case DragMode.CornerTopRight:
                case DragMode.EdgeTop:
                    top = Math.Min(top + dy, bottom - minHeight);
                    if (maxHeight > 0)
                    {
                        top = Math.Max(top, Math.Min(_dragInitialTop, bottom - maxHeight));
                    }
                    break;
                case DragMode.CornerBottomLeft:
                case DragMode.CornerBottomRight:
                case DragMode.EdgeBottom:
                    bottom = Math.Max(bottom + dy, top + minHeight);
                    if (maxHeight > 0)
                    {
                        bottom = Math.Min(bottom, Math.Max(_dragInitialBottom, top + maxHeight));
                    }
                    break;
                case DragMode.Position:
                    if (GetParent() != null)
                    {
                        int minY = GetParent().GetInnerY();
                        int maxY = GetParent().GetInnerBottom();
                        int height = _dragInitialBottom - _dragInitialTop;
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

            SetArea(top, left, right, bottom);
        }

        private void SetArea(int top, int left, int right, int bottom)
        {
            Widget p = GetParent();
            if (p != null)
            {
                top = Math.Max(top, p.GetInnerY());
                left = Math.Max(left, p.GetInnerX());
                right = Math.Min(right, p.GetInnerRight());
                bottom = Math.Min(bottom, p.GetInnerBottom());
            }

            SetPosition(left, top);
            SetSize(Math.Max(GetMinWidth(), right - left),
                    Math.Max(GetMinHeight(), bottom - top));
        }
    }

    public class FrameClosedEventArgs
    {
    }
}
