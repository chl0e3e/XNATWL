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

namespace XNATWL
{
    public class BoxLayout : Widget
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        };

        private Direction _direction;
        private int _spacing;
        private bool _scroll;
        private Alignment _alignment = Alignment.TOP;

        public BoxLayout() : this(Direction.Horizontal)
        {
            
        }

        public BoxLayout(Direction direction)
        {
            this._direction = direction;
        }

        public int GetSpacing()
        {
            return _spacing;
        }

        public void SetSpacing(int spacing)
        {
            if (this._spacing != spacing)
            {
                this._spacing = spacing;
                InvalidateLayout();
            }
        }

        public bool IsScroll()
        {
            return _scroll;
        }

        public void SetScroll(bool scroll)
        {
            if (this._scroll != scroll)
            {
                this._scroll = scroll;
                InvalidateLayout();
            }
        }

        public Alignment GetAlignment()
        {
            return _alignment;
        }

        public void SetAlignment(Alignment alignment)
        {
            if (alignment == null)
            {
                throw new NullReferenceException("alignment");
            }

            if (this._alignment != alignment)
            {
                this._alignment = alignment;
                InvalidateLayout();
            }
        }

        public Direction GetDirection()
        {
            return _direction;
        }

        public void SetDirection(Direction direction)
        {
            if (this._direction != direction)
            {
                this._direction = direction;
                InvalidateLayout();
            }
        }

        public override int GetMinWidth()
        {
            int minWidth = (_direction == Direction.Horizontal)
                    ? ComputeMinWidthHorizontal(this, _spacing)
                    : ComputeMinWidthVertical(this);
            return Math.Max(base.GetMinWidth(), minWidth + GetBorderHorizontal());
        }

        public override int GetMinHeight()
        {
            int minHeight = (_direction == Direction.Horizontal)
                    ? ComputeMinHeightHorizontal(this)
                    : ComputeMinHeightVertical(this, _spacing);
            return Math.Max(base.GetMinHeight(), minHeight + GetBorderVertical());
        }

        public override int GetPreferredInnerWidth()
        {
            return (_direction == Direction.Horizontal)
                    ? ComputePreferredWidthHorizontal(this, _spacing)
                    : ComputePreferredWidthVertical(this);
        }

        public override int GetPreferredInnerHeight()
        {
            return (_direction == Direction.Horizontal)
                    ? ComputePreferredHeightHorizontal(this)
                    : ComputePreferredHeightVertical(this, _spacing);
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            SetSpacing(themeInfo.GetParameter("spacing", 0));
            SetAlignment((Alignment) themeInfo.GetParameter("alignment", Alignment.TOP));
        }

        public static int ComputeMinWidthHorizontal(Widget container, int spacing)
        {
            int n = container.GetNumChildren();
            int minWidth = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                minWidth += container.GetChild(i).GetMinWidth();
            }
            return minWidth;
        }

        public static int ComputeMinHeightHorizontal(Widget container)
        {
            int n = container.GetNumChildren();
            int minHeight = 0;
            for (int i = 0; i < n; i++)
            {
                minHeight = Math.Max(minHeight, container.GetChild(i).GetMinHeight());
            }
            return minHeight;
        }

        public static int ComputePreferredWidthHorizontal(Widget container, int spacing)
        {
            int n = container.GetNumChildren();
            int prefWidth = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                prefWidth += GetPrefChildWidth(container.GetChild(i));
            }
            return prefWidth;
        }

        public static int ComputePreferredHeightHorizontal(Widget container)
        {
            int n = container.GetNumChildren();
            int prefHeight = 0;
            for (int i = 0; i < n; i++)
            {
                prefHeight = Math.Max(prefHeight, GetPrefChildHeight(container.GetChild(i)));
            }
            return prefHeight;
        }

        public static int ComputeMinWidthVertical(Widget container)
        {
            int n = container.GetNumChildren();
            int minWidth = 0;
            for (int i = 0; i < n; i++)
            {
                minWidth = Math.Max(minWidth, container.GetChild(i).GetMinWidth());
            }
            return minWidth;
        }

        public static int ComputeMinHeightVertical(Widget container, int spacing)
        {
            int n = container.GetNumChildren();
            int minHeight = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                minHeight += container.GetChild(i).GetMinHeight();
            }
            return minHeight;
        }

        public static int ComputePreferredWidthVertical(Widget container)
        {
            int n = container.GetNumChildren();
            int prefWidth = 0;
            for (int i = 0; i < n; i++)
            {
                prefWidth = Math.Max(prefWidth, GetPrefChildWidth(container.GetChild(i)));
            }
            return prefWidth;
        }

        public static int ComputePreferredHeightVertical(Widget container, int spacing)
        {
            int n = container.GetNumChildren();
            int prefHeight = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                prefHeight += GetPrefChildHeight(container.GetChild(i));
            }
            return prefHeight;
        }

        public static void LayoutHorizontal(Widget container, int spacing, Alignment alignment, bool scroll)
        {
            int numChildren = container.GetNumChildren();
            int height = container.GetInnerHeight();
            int x = container.GetInnerX();
            int y = container.GetInnerY();

            // 1: check if we need to scroll
            if (scroll)
            {
                int width = ComputePreferredWidthHorizontal(container, spacing);
                if (width > container.GetInnerWidth())
                {
                    x -= width - container.GetInnerWidth();
                }
            }

            // 2: position children
            for (int idx = 0; idx < numChildren; idx++)
            {
                Widget child = container.GetChild(idx);
                int childWidth = GetPrefChildWidth(child);
                int childHeight = (alignment == Alignment.FILL) ? height : GetPrefChildHeight(child);
                int yoff = (height - childHeight) * alignment.GetVPosition() / 2;
                child.SetSize(childWidth, childHeight);
                child.SetPosition(x, y + yoff);
                x += childWidth + spacing;
            }
        }

        public static void LayoutVertical(Widget container, int spacing, Alignment alignment, bool scroll)
        {
            int numChildren = container.GetNumChildren();
            int width = container.GetInnerWidth();
            int x = container.GetInnerX();
            int y = container.GetInnerY();

            // 1: check if we need to scroll
            if (scroll)
            {
                int height = ComputePreferredHeightVertical(container, spacing);
                if (height > container.GetInnerHeight())
                {
                    x -= height - container.GetInnerHeight();
                }
            }

            // 2: position children
            for (int idx = 0; idx < numChildren; idx++)
            {
                Widget child = container.GetChild(idx);
                int childWidth = (alignment == Alignment.FILL) ? width : GetPrefChildWidth(child);
                int childHeight = GetPrefChildHeight(child);
                int xoff = (width - childWidth) * alignment.GetHPosition() / 2;
                child.SetSize(childWidth, childHeight);
                child.SetPosition(x + xoff, y);
                y += childHeight + spacing;
            }
        }

        protected override void Layout()
        {
            if (GetNumChildren() > 0)
            {
                if (_direction == Direction.Horizontal)
                {
                    LayoutHorizontal(this, _spacing, _alignment, _scroll);
                }
                else
                {
                    LayoutVertical(this, _spacing, _alignment, _scroll);
                }
            }
        }

        private static int GetPrefChildWidth(Widget child)
        {
            return ComputeSize(child.GetMinWidth(), child.GetPreferredWidth(), child.GetMaxWidth());
        }

        private static int GetPrefChildHeight(Widget child)
        {
            return ComputeSize(child.GetMinHeight(), child.GetPreferredHeight(), child.GetMaxHeight());
        }
    }
}
