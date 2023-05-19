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
    /// <summary>
    /// Generic model for a text area
    /// </summary>
    public interface TextAreaModel : IEnumerable<Element>
    {
        event EventHandler<TextAreaChangedEventArgs> Changed;
    }

    /// <summary>
    /// Fired when a TextArea has changed
    /// </summary>
    public class TextAreaChangedEventArgs : EventArgs
    {
        public TextAreaChangedEventArgs()
        {

        }
    }

    /// <summary>
    /// '???' CSS attribute enum
    /// </summary>
    public enum HAlignment
    {
        LEFT,
        RIGHT,
        CENTER,
        JUSTIFY
    }

    /// <summary>
    /// 'display' CSS attribute enum
    /// </summary>
    public enum Display
    {
        INLINE,
        BLOCK
    }

    /// <summary>
    /// 'vertical-align' CSS attribute enum
    /// </summary>
    public enum VAlignment
    {
        TOP,
        MIDDLE,
        BOTTOM,
        FILL
    }

    /// <summary>
    /// 'clear' CSS attribute enum
    /// </summary>
    public enum Clear
    {
        NONE,
        LEFT,
        RIGHT,
        BOTH
    }

    /// <summary>
    /// 'float' CSS attribute enum
    /// </summary>
    public enum FloatPosition
    {
        NONE,
        LEFT,
        RIGHT
    }

    /// <summary>
    /// Generic text area element
    /// </summary>
    public abstract class Element
    {
        private Style _style;

        /// <summary>
        /// Create a generic element given a <see cref="Style"/>
        /// </summary>
        /// <param name="style">Styling variables</param>
        protected Element(Style style)
        {
            NotNull(style, "style");
            this._style = style;
        }

        /// <summary>
        /// The style associated with this element
        /// </summary>
        public Style Style
        {
            get
            {
                return _style;
            }

            set
            {
                NotNull(value, "style");
                this._style = value;
            }
        }

        /// <summary>
        /// Throws an exception if the value is null
        /// </summary>
        /// <param name="o">Object testing</param>
        /// <param name="name">Name to throw with</param>
        /// <exception cref="NullReferenceException">Assertion thrown exception</exception>
        public static void NotNull(Object o, String name)
        {
            if (o == null)
            {
                throw new NullReferenceException(name);
            }
        }
    }

    /// <summary>
    /// Element representing a line break
    /// </summary>
    public class LineBreakElement : Element
    {
        /// <summary>
        /// Create a line break element given a <see cref="Style"/>
        /// </summary>
        /// <param name="style">Styling variables</param>
        public LineBreakElement(Style style) : base(style)
        {

        }
    }

    /// <summary>
    /// Element representing text
    /// </summary>
    public class TextElement : Element
    {
        private String _text;

        /// <summary>
        /// Create a text element given a <see cref="Style"/> and some <paramref name="text"/>
        /// </summary>
        /// <param name="style">Styling variables</param>
        /// <param name="text">Text</param>
        public TextElement(Style style, String text) : base(style)
        {
            NotNull(text, "text");
            this._text = text;
        }

        /// <summary>
        /// The text this element draws
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                NotNull(value, "text");
                this._text = value;
            }
        }
    }

    /// <summary>
    /// Element representing an image
    /// </summary>
    public class ImageElement : Element
    {
        private String _imageName;
        private String _tooltip;

        /// <summary>
        /// Create an image element given a <see cref="Style"/>, an <paramref name="imageName"/> and a <paramref name="tooltip"/>
        /// </summary>
        /// <param name="style">Styling variables</param>
        /// <param name="imageName">Name of the image</param>
        /// <param name="tooltip">Tooltip to display for image</param>
        public ImageElement(Style style, String imageName, String tooltip) : base(style)
        {
            this._imageName = imageName;
            this._tooltip = tooltip;
        }

        /// <summary>
        /// Create an image element given a <see cref="Style"/>, an <paramref name="imageName"/> and a blank tooltip
        /// </summary>
        /// <param name="style">Styling variables</param>
        /// <param name="imageName">Name of the image</param>
        public ImageElement(Style style, String imageName) : this(style, imageName, null)
        {
            
        }

        /// <summary>
        /// the image name for this image element.
        /// </summary>
        public String ImageName => _imageName;

        /// <summary>
        /// the tooltip or null for this image
        /// </summary>
        public String ToolTip => _tooltip;
    }

    /// <summary>
    /// An element whcih is a widget in place
    /// </summary>
    public class WidgetElement : Element
    {
        private String _widgetName;
        private String _widgetParam;

        /// <summary>
        /// Create an element in the textarea occupied by another Widget
        /// </summary>
        /// <param name="style">Styling variables</param>
        /// <param name="widgetName">Name of widget</param>
        /// <param name="widgetParam">Widget parameter</param>
        public WidgetElement(Style style, String widgetName, String widgetParam) : base(style)
        {
            this._widgetName = widgetName;
            this._widgetParam = widgetParam;
        }

        /// <summary>
        /// Widget name
        /// </summary>
        public String WidgetName => _widgetName;

        /// <summary>
        /// Widget parameter
        /// </summary>
        public String WidgetParam => _widgetParam;
    }

    /// <summary>
    /// Element which acts as a container for other elements
    /// </summary>
    public class ContainerElement : Element, ICollection<Element>
    {
        protected List<Element> _children;

        /// <summary>
        /// Number of children in <see cref="ContainerElement"/>
        /// </summary>
        public int Count
        {
            get
            {
                return this._children.Count;
            }
        }

        /// <summary>
        /// Whether or not the container is now read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Create a <see cref="ContainerElement"/> while regarding styles
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        public ContainerElement(Style style) : base(style)
        {
            this._children = new List<Element>();
        }

        /// <summary>
        /// Get an <see cref="Element"/> in the container given an index
        /// </summary>
        /// <param name="idx">Index of <see cref="Element"/></param>
        /// <returns>Element</returns>
        public Element this[int idx]
        {
            get
            {
                return this._children[idx];
            }
        }

        /// <summary>
        /// Add an <see cref="Element"/> to the container
        /// </summary>
        /// <param name="item">New <see cref="Element"/></param>
        public void Add(Element item)
        {
            _children.Add(item);
        }

        /// <summary>
        /// Clear the container
        /// </summary>
        public void Clear()
        {
            _children.Clear();
        }

        /// <summary>
        /// Check if container contains given <see cref="Element"/>
        /// </summary>
        /// <param name="item">given <see cref="Element"/></param>
        /// <returns><strong>true</strong> if <see cref="Element"/> is in container</returns>
        public bool Contains(Element item)
        {
            return _children.Contains(item);
        }

        /// <summary>
        /// Copy <see cref="Element"/> array into container
        /// </summary>
        /// <param name="array"><see cref="Element"/> array</param>
        /// <param name="arrayIndex">Starting index of <see cref="Element"/> array</param>
        public void CopyTo(Element[] array, int arrayIndex)
        {
            _children.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove <see cref="Element"/> from container
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns><strong>true</strong> if removed</returns>
        public bool Remove(Element item)
        {
            return _children.Remove(item);
        }

        /// <summary>
        /// Enumerator for children of container
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<Element> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        /// <summary>
        /// Enumerator for children of container
        /// </summary>
        /// <returns>Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
    }

    /// <summary>
    /// Element representing a paragraph container
    /// </summary>
    public class ParagraphElement : ContainerElement
    {
        /// <summary>
        /// Create a new paragraph based on a <see cref="ContainerElement"/>
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        public ParagraphElement(Style style) : base(style)
        {

        }
    }

    /// <summary>
    /// Element representing a hyperlink
    /// </summary>
    public class LinkElement : ContainerElement
    {
        private String href;

        /// <summary>
        /// Create a <see cref="LinkElement"/> pointing to a given <paramref name="href"/>
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        /// <param name="href">Hyperlink address</param>
        public LinkElement(Style style, String href) : base(style)
        {
            this.href = href;
        }

        /// <summary>
        /// Link HREF
        /// </summary>
        public String HREF { get => href; set => this.href = value; }
    }

    /// <summary>
    /// A list item in an unordered list
    /// </summary>
    public class ListElement : ContainerElement
    {
        /// <summary>
        /// Create a new list element based on a <see cref="ContainerElement"/>
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        public ListElement(Style style) : base(style)
        {

        }
    }

    /// <summary>
    /// An ordered list. All contained elements are treated as list items.
    /// </summary>
    public class OrderedListElement : ContainerElement
    {
        private int _start;

        /// <summary>
        /// Create a new ordered list element based on a <see cref="ContainerElement"/> with a given start index
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        public OrderedListElement(Style style, int start) : base(style)
        {
            this._start = start;
        }

        /// <summary>
        /// Start index of ordered list item
        /// </summary>
        public int Start => _start;
    }

    /// <summary>
    /// Element representing a block
    /// </summary>
    public class BlockElement : ContainerElement
    {
        /// <summary>
        /// Create a new block element based on a <see cref="ContainerElement"/>
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        public BlockElement(Style style) : base(style)
        {

        }
    }

    /// <summary>
    /// Element representing a table cell
    /// </summary>
    public class TableCellElement : ContainerElement
    {
        private int _colspan;

        /// <summary>
        /// Create a new table cell based on a <see cref="ContainerElement"/>
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        public TableCellElement(Style style) : this(style, 1)
        {
            ;
        }

        /// <summary>
        /// Create a new table cell based on a <see cref="ContainerElement"/> with a given column span integer
        /// </summary>
        /// <param name="style">Styles to use on container</param>
        /// <param name="colspan">column span integer</param>
        public TableCellElement(Style style, int colspan) : base(style)
        {
            this._colspan = colspan;
        }

        /// <summary>
        /// Column span in characters
        /// </summary>
        public int Colspan => _colspan;
    }

    /// <summary>
    /// Element representing a table
    /// </summary>
    public class TableElement : Element
    {
        private int _numColumns;
        private int _numRows;
        private int _cellSpacing;
        private int _cellPadding;
        private TableCellElement[] _cells;
        private Style[] _rowStyles;

        /// <summary>
        /// Create a table element in the text area
        /// </summary>
        /// <param name="style">Styles to use</param>
        /// <param name="numColumns">Number of columns</param>
        /// <param name="numRows">Number of rows</param>
        /// <param name="cellSpacing">Spacing between cells</param>
        /// <param name="cellPadding">Padding between cells</param>
        /// <exception cref="ArgumentOutOfRangeException">Invalid number of rows or columns</exception>
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

        /// <summary>
        /// Number of columns
        /// </summary>
        public int NumColumns => _numColumns;

        /// <summary>
        /// Number of rows
        /// </summary>
        public int NumRows => _numRows;

        /// <summary>
        /// Padding between cells
        /// </summary>
        public int CellPadding => _cellPadding;

        /// <summary>
        /// Spacing between cells
        /// </summary>
        public int CellSpacing => _cellSpacing;

        /// <summary>
        /// Get cell element given <paramref name="column"/> and <paramref name="row"/>
        /// </summary>
        /// <param name="row">Row number</param>
        /// <param name="column">Column number</param>
        /// <returns><see cref="TableCellElement"/></returns>
        /// <exception cref="IndexOutOfRangeException">Row/column out of range</exception>
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

        /// <summary>
        /// Get <see cref="Style"/> for a <paramref name="row"/>
        /// </summary>
        /// <param name="row">Row number</param>
        /// <returns>Style object for the row</returns>
        public Style GetRowStyle(int row)
        {
            return _rowStyles[row];
        }

        /// <summary>
        /// Set cell element <paramref name="column"/>, <paramref name="row"/> and the <see cref="TableCellElement"/>
        /// </summary>
        /// <param name="row">Row number</param>
        /// <param name="column">Column number</param>
        /// <param name="cell"><see cref="TableCellElement"/> object</param>
        /// <exception cref="IndexOutOfRangeException">Row/column out of range</exception>
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

        /// <summary>
        /// Set <see cref="Style"/> for a <paramref name="row"/>
        /// </summary>
        /// <param name="row">Row number</param>
        /// <param name="style">Style object for the row</param>
        public void SetRowStyle(int row, Style style)
        {
            _rowStyles[row] = style;
        }
    }
}
