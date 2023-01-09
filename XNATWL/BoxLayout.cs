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
            HORIZONTAL,
            VERTICAL
        };

        private Direction direction;
        private int spacing;
        private bool scroll;
        private Alignment alignment = Alignment.TOP;

        public BoxLayout() : this(Direction.HORIZONTAL)
        {
            
        }

        public BoxLayout(Direction direction)
        {
            this.direction = direction;
        }

        public int getSpacing()
        {
            return spacing;
        }

        public void setSpacing(int spacing)
        {
            if (this.spacing != spacing)
            {
                this.spacing = spacing;
                invalidateLayout();
            }
        }

        public bool isScroll()
        {
            return scroll;
        }

        public void setScroll(bool scroll)
        {
            if (this.scroll != scroll)
            {
                this.scroll = scroll;
                invalidateLayout();
            }
        }

        public Alignment getAlignment()
        {
            return alignment;
        }

        public void setAlignment(Alignment alignment)
        {
            if (alignment == null)
            {
                throw new NullReferenceException("alignment");
            }

            if (this.alignment != alignment)
            {
                this.alignment = alignment;
                invalidateLayout();
            }
        }

        public Direction getDirection()
        {
            return direction;
        }

        public void setDirection(Direction direction)
        {
            if (direction == null)
            {
                throw new NullReferenceException("direction");
            }
            if (this.direction != direction)
            {
                this.direction = direction;
                invalidateLayout();
            }
        }

        public override int getMinWidth()
        {
            int minWidth = (direction == Direction.HORIZONTAL)
                    ? computeMinWidthHorizontal(this, spacing)
                    : computeMinWidthVertical(this);
            return Math.Max(base.getMinWidth(), minWidth + getBorderHorizontal());
        }

        public override int getMinHeight()
        {
            int minHeight = (direction == Direction.HORIZONTAL)
                    ? computeMinHeightHorizontal(this)
                    : computeMinHeightVertical(this, spacing);
            return Math.Max(base.getMinHeight(), minHeight + getBorderVertical());
        }

        public override int getPreferredInnerWidth()
        {
            return (direction == Direction.HORIZONTAL)
                    ? computePreferredWidthHorizontal(this, spacing)
                    : computePreferredWidthVertical(this);
        }

        public override int getPreferredInnerHeight()
        {
            return (direction == Direction.HORIZONTAL)
                    ? computePreferredHeightHorizontal(this)
                    : computePreferredHeightVertical(this, spacing);
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            setSpacing(themeInfo.getParameter("spacing", 0));
            setAlignment((Alignment) themeInfo.getParameter("alignment", Alignment.TOP));
        }

        public static int computeMinWidthHorizontal(Widget container, int spacing)
        {
            int n = container.getNumChildren();
            int minWidth = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                minWidth += container.getChild(i).getMinWidth();
            }
            return minWidth;
        }

        public static int computeMinHeightHorizontal(Widget container)
        {
            int n = container.getNumChildren();
            int minHeight = 0;
            for (int i = 0; i < n; i++)
            {
                minHeight = Math.Max(minHeight, container.getChild(i).getMinHeight());
            }
            return minHeight;
        }

        public static int computePreferredWidthHorizontal(Widget container, int spacing)
        {
            int n = container.getNumChildren();
            int prefWidth = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                prefWidth += getPrefChildWidth(container.getChild(i));
            }
            return prefWidth;
        }

        public static int computePreferredHeightHorizontal(Widget container)
        {
            int n = container.getNumChildren();
            int prefHeight = 0;
            for (int i = 0; i < n; i++)
            {
                prefHeight = Math.Max(prefHeight, getPrefChildHeight(container.getChild(i)));
            }
            return prefHeight;
        }

        public static int computeMinWidthVertical(Widget container)
        {
            int n = container.getNumChildren();
            int minWidth = 0;
            for (int i = 0; i < n; i++)
            {
                minWidth = Math.Max(minWidth, container.getChild(i).getMinWidth());
            }
            return minWidth;
        }

        public static int computeMinHeightVertical(Widget container, int spacing)
        {
            int n = container.getNumChildren();
            int minHeight = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                minHeight += container.getChild(i).getMinHeight();
            }
            return minHeight;
        }

        public static int computePreferredWidthVertical(Widget container)
        {
            int n = container.getNumChildren();
            int prefWidth = 0;
            for (int i = 0; i < n; i++)
            {
                prefWidth = Math.Max(prefWidth, getPrefChildWidth(container.getChild(i)));
            }
            return prefWidth;
        }

        public static int computePreferredHeightVertical(Widget container, int spacing)
        {
            int n = container.getNumChildren();
            int prefHeight = Math.Max(0, n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                prefHeight += getPrefChildHeight(container.getChild(i));
            }
            return prefHeight;
        }

        public static void layoutHorizontal(Widget container, int spacing, Alignment alignment, bool scroll)
        {
            int numChildren = container.getNumChildren();
            int height = container.getInnerHeight();
            int x = container.getInnerX();
            int y = container.getInnerY();

            // 1: check if we need to scroll
            if (scroll)
            {
                int width = computePreferredWidthHorizontal(container, spacing);
                if (width > container.getInnerWidth())
                {
                    x -= width - container.getInnerWidth();
                }
            }

            // 2: position children
            for (int idx = 0; idx < numChildren; idx++)
            {
                Widget child = container.getChild(idx);
                int childWidth = getPrefChildWidth(child);
                int childHeight = (alignment == Alignment.FILL) ? height : getPrefChildHeight(child);
                int yoff = (height - childHeight) * alignment.getVPosition() / 2;
                child.setSize(childWidth, childHeight);
                child.setPosition(x, y + yoff);
                x += childWidth + spacing;
            }
        }

        public static void layoutVertical(Widget container, int spacing, Alignment alignment, bool scroll)
        {
            int numChildren = container.getNumChildren();
            int width = container.getInnerWidth();
            int x = container.getInnerX();
            int y = container.getInnerY();

            // 1: check if we need to scroll
            if (scroll)
            {
                int height = computePreferredHeightVertical(container, spacing);
                if (height > container.getInnerHeight())
                {
                    x -= height - container.getInnerHeight();
                }
            }

            // 2: position children
            for (int idx = 0; idx < numChildren; idx++)
            {
                Widget child = container.getChild(idx);
                int childWidth = (alignment == Alignment.FILL) ? width : getPrefChildWidth(child);
                int childHeight = getPrefChildHeight(child);
                int xoff = (width - childWidth) * alignment.getHPosition() / 2;
                child.setSize(childWidth, childHeight);
                child.setPosition(x + xoff, y);
                y += childHeight + spacing;
            }
        }

        protected override void layout()
        {
            if (getNumChildren() > 0)
            {
                if (direction == Direction.HORIZONTAL)
                {
                    layoutHorizontal(this, spacing, alignment, scroll);
                }
                else
                {
                    layoutVertical(this, spacing, alignment, scroll);
                }
            }
        }

        private static int getPrefChildWidth(Widget child)
        {
            return computeSize(child.getMinWidth(), child.getPreferredWidth(), child.getMaxWidth());
        }

        private static int getPrefChildHeight(Widget child)
        {
            return computeSize(child.getMinHeight(), child.getPreferredHeight(), child.getMaxHeight());
        }
    }
}
