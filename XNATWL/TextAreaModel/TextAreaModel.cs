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
using System.Collections;
using System.Collections.Generic;

namespace XNATWL.TextAreaModel
{
    public interface TextAreaModel : IEnumerable<Element>
    {
        event EventHandler<TextAreaChangedEventArgs> Changed;
    }

    public class TextAreaChangedEventArgs : EventArgs
    {
        public TextAreaChangedEventArgs()
        {

        }
    }

    public enum HAlignment
    {
        LEFT,
        RIGHT,
        CENTER,
        JUSTIFY
    }

    public enum Display
    {
        INLINE,
        BLOCK
    }

    public enum VAlignment
    {
        TOP,
        MIDDLE,
        BOTTOM,
        FILL
    }

    public enum Clear
    {
        NONE,
        LEFT,
        RIGHT,
        BOTH
    }

    public enum FloatPosition
    {
        NONE,
        LEFT,
        RIGHT
    }

    public abstract class Element
    {
        private Style _style;

        protected Element(Style style)
        {
            NotNull(style, "style");
            this._style = style;
        }

        /**
         * Returns the style associated with this element
         * @return the style associated with this element
         */
        public Style GetStyle()
        {
            return _style;
        }

        /**
         * Replaces the style associated with this element.
         * This method does not cause the model callback to be fired.
         *
         * @param style the new style. Must not be null.
         */
        public void SetStyle(Style style)
        {
            NotNull(style, "style");
            this._style = style;
        }

        public static void NotNull(Object o, String name)
        {
            if (o == null)
            {
                throw new NullReferenceException(name);
            }
        }
    }

    public class LineBreakElement : Element
    {
        public LineBreakElement(Style style) : base(style)
        {

        }
    }

    public class TextElement : Element
    {
        private String _text;

        public TextElement(Style style, String text) : base(style)
        {
            NotNull(text, "text");
            this._text = text;
        }

        /**
         * Returns ths text.
         * @return the text.
         */
        public String GetText()
        {
            return _text;
        }

        /**
         * Replaces the text of this element.
         * This method does not cause the model callback to be fired.
         *
         * @param text the new text. Must not be null.
         */
        public void SetText(String text)
        {
            NotNull(text, "text");
            this._text = text;
        }
    }

    public class ImageElement : Element
    {
        private String _imageName;
        private String _tooltip;

        public ImageElement(Style style, String imageName, String tooltip) : base(style)
        {
            this._imageName = imageName;
            this._tooltip = tooltip;
        }

        public ImageElement(Style style, String imageName) : this(style, imageName, null)
        {
            
        }

        /**
         * Returns the image name for this image element.
         * @return the image name for this image element.
         */
        public String GetImageName()
        {
            return _imageName;
        }

        /**
         * Returns the tooltip or null for this image.
         * @return the tooltip or null for this image. Can be null.
         */
        public String GetToolTip()
        {
            return _tooltip;
        }
    }

    public class WidgetElement : Element
    {
        private String _widgetName;
        private String _widgetParam;

        public WidgetElement(Style style, String widgetName, String widgetParam) : base(style)
        {
            this._widgetName = widgetName;
            this._widgetParam = widgetParam;
        }

        public String GetWidgetName()
        {
            return _widgetName;
        }

        public String GetWidgetParam()
        {
            return _widgetParam;
        }
    }

    public class ContainerElement : Element, ICollection<Element>
    {
        protected List<Element> _children;

        public int Count
        {
            get
            {
                return this._children.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ContainerElement(Style style) : base(style)
        {
            this._children = new List<Element>();
        }

        public Element ElementAt(int idx)
        {
            return this._children[idx];
        }

        public void Add(Element item)
        {
            _children.Add(item);
        }

        public void Clear()
        {
            _children.Clear();
        }

        public bool Contains(Element item)
        {
            return _children.Contains(item);
        }

        public void CopyTo(Element[] array, int arrayIndex)
        {
            _children.CopyTo(array, arrayIndex);
        }

        public bool Remove(Element item)
        {
            return _children.Remove(item);
        }

        public IEnumerator<Element> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
    }

    public class ParagraphElement : ContainerElement
    {
        public ParagraphElement(Style style) : base(style)
        {

        }
    }

    public class LinkElement : ContainerElement
    {
        private String href;

        public LinkElement(Style style, String href) : base(style)
        {
            this.href = href;
        }

        /**
         * Returns the href of the link.
         * @return the href of the link. Can be null.
         */
        public String GetHREF()
        {
            return href;
        }

        /**
         * Replaces the href of this link.
         * This method does not cause the model callback to be fired.
         *
         * @param href the new href of the link, can be null.
         */
        public void SetHREF(String href)
        {
            this.href = href;
        }
    }

    /**
     * A list item in an unordered list
     */
    public class ListElement : ContainerElement
    {
        public ListElement(Style style) : base(style)
        {

        }
    }

    /**
     * An ordered list. All contained elements are treated as list items.
     */
    public class OrderedListElement : ContainerElement
    {
        private int _start;

        public OrderedListElement(Style style, int start) : base(style)
        {
            this._start = start;
        }

        public int GetStart()
        {
            return _start;
        }
    }

    public class BlockElement : ContainerElement
    {
        public BlockElement(Style style) : base(style)
        {

        }
    }

    public class TableCellElement : ContainerElement
    {
        private int _colspan;

        public TableCellElement(Style style) : this(style, 1)
        {
            ;
        }

        public TableCellElement(Style style, int colspan) : base(style)
        {
            this._colspan = colspan;
        }

        public int GetColspan()
        {
            return _colspan;
        }
    }

    public class TableElement : Element
    {
        private int _numColumns;
        private int _numRows;
        private int _cellSpacing;
        private int _cellPadding;
        private TableCellElement[] _cells;
        private Style[] _rowStyles;

        public TableElement(Style style, int numColumns, int numRows, int cellSpacing, int cellPadding) : base(style)
        {
            if (numColumns < 0)
            {
                throw new ArgumentOutOfRangeException("numColumns");
            }
            if (numRows < 0)
            {
                throw new ArgumentOutOfRangeException("numRows");
            }

            this._numColumns = numColumns;
            this._numRows = numRows;
            this._cellSpacing = cellSpacing;
            this._cellPadding = cellPadding;
            this._cells = new TableCellElement[numRows * numColumns];
            this._rowStyles = new Style[numRows];
        }

        public int GetNumColumns()
        {
            return _numColumns;
        }

        public int GetNumRows()
        {
            return _numRows;
        }

        public int GetCellPadding()
        {
            return _cellPadding;
        }

        public int GetCellSpacing()
        {
            return _cellSpacing;
        }

        public TableCellElement GetCell(int row, int column)
        {
            if (column < 0 || column >= _numColumns)
            {
                throw new IndexOutOfRangeException("column");
            }

            if (row < 0 || row >= _numRows)
            {
                throw new IndexOutOfRangeException("row");
            }

            return _cells[row * _numColumns + column];
        }

        public Style GetRowStyle(int row)
        {
            return _rowStyles[row];
        }

        public void SetCell(int row, int column, TableCellElement cell)
        {
            if (column < 0 || column >= _numColumns)
            {
                throw new IndexOutOfRangeException("column");
            }

            if (row < 0 || row >= _numRows)
            {
                throw new IndexOutOfRangeException("row");
            }

            _cells[row * _numColumns + column] = cell;
        }

        public void SetRowStyle(int row, Style style)
        {
            _rowStyles[row] = style;
        }
    }
}
