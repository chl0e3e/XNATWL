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
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{

    public abstract class TableBase : Widget, ScrollPane.Scrollable, ScrollPane.AutoScrollable, ScrollPane.CustomPageSize
    {

        /**
         * IMPORTANT: Widgets implementing CellRenderer should not call
         * {@link Widget#invalidateLayout()} or {@link Widget#invalidateLayoutLocally()}
         * . This means they need to override {@link Widget#sizeChanged()}.
         */
        public interface CellRenderer
        {
            /**
             * Called when the CellRenderer is registered and a theme is applied.
             * @param themeInfo the theme object
             */
            void ApplyTheme(ThemeInfo themeInfo);

            /**
             * The theme name for this CellRenderer. Must be relative to the Table.
             * @return the theme name.
             */
            String GetTheme();

            /**
             * This method sets the row, column and the cell data.
             * It is called before any other cell related method is called.
             * @param row the table row
             * @param column the table column
             * @param data the cell data
             */
            void SetCellData(int row, int column, Object data);

            /**
             * Returns how many columns this cell spans. Must be >= 1.
             * Is called after setCellData.
             * @return the column span.
             * @see #setCellData(int, int, java.lang.Object)
             */
            int GetColumnSpan();

            /**
             * Returns the preferred cell height in variable row height mode.
             * It is not called at all in fixed row height mode.
             * @return the preferred cell height
             * @see #setCellData(int, int, java.lang.Object)
             * @see TableBase#setVaribleRowHeight(bool)
             */
            int GetPreferredHeight();

            /**
             * Returns the widget used to render the cell or null if no rendering
             * should happen. This widget should not be added to any widget. It
             * will be managed by the Table.
             * TableBase uses a stamping approch for cell rendering. This method
             * must not create a new widget each time.
             *
             * This method is responsible to call setPosition and setSize on the
             * returned widget.
             *
             * @param x the left edge of the cell
             * @param y the top edge of the cell
             * @param width the width of the cell
             * @param height the height of the cell
             * @param isSelected the selected state of this cell
             * @return the widget used for cell rendering or null.
             * @see #setCellData(int, int, java.lang.Object)
             */
            Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected);
        }

        public interface CellWidgetCreator : CellRenderer
        {
            Widget UpdateWidget(Widget existingWidget);
            void PositionWidget(Widget widget, int x, int y, int w, int h);
        }

        public interface KeyboardSearchHandler
        {
            /**
             * Update search with this key event
             *
             * @param evt the key event
             * @return true if the event was handled
             */
            bool HandleKeyEvent(Event evt);

            /**
             * Returns true if the search is active.
             * @return true if the search is active.
             */
            bool IsActive();

            /**
             * Called when the table position ot size has changed.
             */
            void UpdateInfoWindowPosition();
        }

        public interface DragListener
        {
            /**
             * Signals the start of the drag operation
             *
             * @param row the row where the drag started
             * @param col the column where the drag started
             * @param evt the mouse event which started the drag
             * @return true if the drag should start, false if it should be canceled
             */
            bool DragStarted(int row, int col, Event evt);

            /**
             * Mouse dragging in progress
             * @param evt the MOUSE_DRAGGED event
             * @return the mouse cursor to display
             */
            MouseCursor Dragged(Event evt);

            /**
             * Mouse dragging stopped
             * @param evt the event which stopped the mouse drag
             */
            void DragStopped(Event evt);

            /**
             * Called when the mouse drag is canceled (eg by pressing ESCAPE)
             */
            void DragCancelled();
        }


        public event EventHandler<TableDoubleClickEventArgs> DoubleClick;
        public event EventHandler<TableRightClickEventArgs> RightClick;
        public event EventHandler<TableColumnHeaderClickEventArgs> ColumnHeaderClick;

        public static StateKey STATE_FIRST_COLUMNHEADER = StateKey.Get("firstColumnHeader");
        public static StateKey STATE_LAST_COLUMNHEADER = StateKey.Get("lastColumnHeader");
        public static StateKey STATE_ROW_SELECTED = StateKey.Get("rowSelected");
        public static StateKey STATE_ROW_HOVER = StateKey.Get("rowHover");
        public static StateKey STATE_ROW_DROPTARGET = StateKey.Get("rowDropTarget");
        public static StateKey STATE_ROW_ODD = StateKey.Get("rowOdd");
        public static StateKey STATE_LEAD_ROW = StateKey.Get("leadRow");
        public static StateKey STATE_SELECTED = StateKey.Get("selected");
        public static StateKey STATE_SORT_ASCENDING = StateKey.Get("sortAscending");
        public static StateKey STATE_SORT_DESCENDING = StateKey.Get("sortDescending");

        private StringCellRenderer _stringCellRenderer;
        private RemoveCellWidgets _removeCellWidgetsFunction;
        private InsertCellWidgets _insertCellWidgetsFunction;
        private CellWidgetContainer _cellWidgetContainer;

        protected TypeMapping _cellRenderers;
        protected SparseGrid _widgetGrid;
        protected ColumnSizeSequence _columnModel;
        protected TableColumnHeaderModel _columnHeaderModel;
        protected SizeSequence _rowModel;
        protected bool _hasCellWidgetCreators;
        protected ColumnHeader[] _columnHeaders;
        protected CellRenderer[] _columnDefaultCellRenderer;
        protected TableSelectionManager _selectionManager;
        protected KeyboardSearchHandler _keyboardSearchHandler;
        protected DragListener _dragListener;

        protected Image _imageColumnDivider;
        protected Image _imageRowBackground;
        protected Image _imageRowOverlay;
        protected Image _imageRowDropMarker;
        protected ThemeInfo _tableBaseThemeInfo;
        protected int _columnHeaderHeight;
        protected int _columnDividerDraggableDistance;
        protected MouseCursor _columnResizeCursor;
        protected MouseCursor _normalCursor;
        protected MouseCursor _dragNotPossibleCursor;
        protected bool _ensureColumnHeaderMinWidth;

        protected int _numRows;
        protected int _numColumns;
        protected int _rowHeight = 32;
        protected int _defaultColumnWidth = 256;
        protected bool _bAutoSizeAllRows;
        protected bool _bUpdateAllCellWidgets;
        protected bool _bUpdateAllColumnWidth;

        protected int _scrollPosX;
        protected int _scrollPosY;

        protected int _firstVisibleRow;
        protected int _firstVisibleColumn;
        protected int _lastVisibleRow;
        protected int _lastVisibleColumn;
        protected bool _firstRowPartialVisible;
        protected bool _lastRowPartialVisible;

        protected int _dropMarkerRow = -1;
        protected bool _dropMarkerBeforeRow;

        protected static int LAST_MOUSE_Y_OUTSIDE = int.MinValue;

        protected int _lastMouseY = LAST_MOUSE_Y_OUTSIDE;
        protected int _lastMouseRow = -1;
        protected int _lastMouseColumn = -1;

        protected TableBase()
        {
            this._cellRenderers = new TypeMapping();
            this._stringCellRenderer = new StringCellRenderer();
            this._widgetGrid = new SparseGrid(32);
            this._removeCellWidgetsFunction = new RemoveCellWidgets(this);
            this._insertCellWidgetsFunction = new InsertCellWidgets(this);
            this._columnModel = new ColumnSizeSequence(this);
            this._columnDefaultCellRenderer = new CellRenderer[8];
            this._cellWidgetContainer = new CellWidgetContainer();

            base.InsertChild(_cellWidgetContainer, 0);
            SetCanAcceptKeyboardFocus(true);
        }

        public TableSelectionManager GetSelectionManager()
        {
            return _selectionManager;
        }

        public void SetSelectionManager(TableSelectionManager selectionManager)
        {
            if (this._selectionManager != selectionManager)
            {
                if (this._selectionManager != null)
                {
                    this._selectionManager.SetAssociatedTable(null);
                }
                this._selectionManager = selectionManager;
                if (this._selectionManager != null)
                {
                    this._selectionManager.SetAssociatedTable(this);
                }
            }
        }

        /**
         * Installs a multi row selection manager.
         *
         * @see TableRowSelectionManager
         * @see DefaultTableSelectionModel
         */
        public void SetDefaultSelectionManager()
        {
            SetSelectionManager(new TableRowSelectionManager());
        }

        public KeyboardSearchHandler GetKeyboardSearchHandler()
        {
            return _keyboardSearchHandler;
        }

        public void SetKeyboardSearchHandler(KeyboardSearchHandler keyboardSearchHandler)
        {
            this._keyboardSearchHandler = keyboardSearchHandler;
        }

        public DragListener GetDragListener()
        {
            return _dragListener;
        }

        public void SetDragListener(DragListener dragListener)
        {
            CancelDragging();
            this._dragListener = dragListener;
        }

        public bool IsDropMarkerBeforeRow()
        {
            return _dropMarkerBeforeRow;
        }

        public int GetDropMarkerRow()
        {
            return _dropMarkerRow;
        }

        public void SetDropMarker(int row, bool beforeRow)
        {
            if (row < 0 || row > _numRows)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (row == _numRows && !beforeRow)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            _dropMarkerRow = row;
            _dropMarkerBeforeRow = beforeRow;
        }

        public bool SetDropMarker(Event evt)
        {
            int mouseY = evt.GetMouseY();
            if (IsMouseInside(evt) && !IsMouseInColumnHeader(mouseY))
            {
                mouseY -= GetOffsetY();
                int row = GetRowFromPosition(mouseY);
                if (row >= 0 && row < _numRows)
                {
                    int rowStart = GetRowStartPosition(row);
                    int rowEnd = GetRowEndPosition(row);
                    int margin = (rowEnd - rowStart + 2) / 4;
                    if ((mouseY - rowStart) < margin)
                    {
                        SetDropMarker(row, true);
                    }
                    else if ((rowEnd - mouseY) < margin)
                    {
                        SetDropMarker(row + 1, true);
                    }
                    else
                    {
                        SetDropMarker(row, false);
                    }
                    return true;
                }
                else if (row == _numRows)
                {
                    SetDropMarker(row, true);
                    return true;
                }
            }
            return false;
        }

        public void ClearDropMarker()
        {
            _dropMarkerRow = -1;
        }

        public bool IsVariableRowHeight()
        {
            return _rowModel != null;
        }

        public void SetVariableRowHeight(bool variableRowHeight)
        {
            if (variableRowHeight && _rowModel == null)
            {
                _rowModel = new RowSizeSequence(this, _numRows);
                _bAutoSizeAllRows = true;
                InvalidateLayout();
            }
            else if (!variableRowHeight)
            {
                _rowModel = null;
            }
        }

        public int GetNumRows()
        {
            return _numRows;
        }

        public int GetNumColumns()
        {
            return _numColumns;
        }

        public int GetRowFromPosition(int y)
        {
            if (y >= 0)
            {
                if (_rowModel != null)
                {
                    return _rowModel.GetIndex(y);
                }
                return Math.Min(_numRows - 1, y / _rowHeight);
            }

            return -1;
        }

        public int GetRowStartPosition(int row)
        {
            CheckRowIndex(row);
            if (_rowModel != null)
            {
                return _rowModel.GetPosition(row);
            }
            else
            {
                return row * _rowHeight;
            }
        }

        public int GetRowHeight(int row)
        {
            CheckRowIndex(row);
            if (_rowModel != null)
            {
                return _rowModel.GetSize(row);
            }
            else
            {
                return _rowHeight;
            }
        }

        public int GetRowEndPosition(int row)
        {
            CheckRowIndex(row);
            if (_rowModel != null)
            {
                return _rowModel.GetPosition(row + 1);
            }
            else
            {
                return (row + 1) * _rowHeight;
            }
        }

        public int GetColumnFromPosition(int x)
        {
            if (x >= 0)
            {
                int column = _columnModel.GetIndex(x);
                return column;
            }
            return -1;
        }

        public int GetColumnStartPosition(int column)
        {
            CheckColumnIndex(column);
            return _columnModel.GetPosition(column);
        }

        public int GetColumnWidth(int column)
        {
            CheckColumnIndex(column);
            return _columnModel.GetSize(column);
        }

        public int GetColumnEndPosition(int column)
        {
            CheckColumnIndex(column);
            return _columnModel.GetPosition(column + 1);
        }

        public void SetColumnWidth(int column, int width)
        {
            CheckColumnIndex(column);
            _columnHeaders[column].SetColumnWidth(width);    // store passed width
            if (_columnModel.Update(column))
            {
                InvalidateLayout();
            }
        }

        public AnimationState GetColumnHeaderAnimationState(int column)
        {
            CheckColumnIndex(column);
            return _columnHeaders[column].GetAnimationState();
        }

        /**
         * Sets the sort order animation state for all column headers.
         * @param sortColumn This column gets sort order indicators, all other columns not
         * @param sortOrder Which sort order. Can be null to disable the indicators
         */
        public void SetColumnSortOrderAnimationState(int sortColumn, SortOrder sortOrder)
        {
            for (int column = 0; column < _numColumns; ++column)
            {
                AnimationState animState = _columnHeaders[column].GetAnimationState();
                animState.SetAnimationState(STATE_SORT_ASCENDING, (column == sortColumn) && (sortOrder == SortOrder.Ascending));
                animState.SetAnimationState(STATE_SORT_DESCENDING, (column == sortColumn) && (sortOrder == SortOrder.Descending));
            }
        }

        public void ScrollToRow(int row)
        {
            ScrollPane scrollPane = ScrollPane.GetContainingScrollPane(this);
            if (scrollPane != null && _numRows > 0)
            {
                scrollPane.ValidateLayout();
                int rowStart = GetRowStartPosition(row);
                int rowEnd = GetRowEndPosition(row);
                int height = rowEnd - rowStart;
                scrollPane.ScrollToAreaY(rowStart, height, height / 2);
            }
        }

        public int GetNumVisibleRows()
        {
            int rows = _lastVisibleRow - _firstVisibleRow;
            if (!_lastRowPartialVisible)
            {
                rows++;
            }
            return rows;
        }

        public override int GetMinHeight()
        {
            return Math.Max(base.GetMinHeight(), _columnHeaderHeight);
        }

        public override int GetPreferredInnerWidth()
        {
            if (GetInnerWidth() == 0)
            {
                return _columnModel.ComputePreferredWidth();
            }
            if (_bUpdateAllColumnWidth)
            {
                UpdateAllColumnWidth();
            }
            return (_numColumns > 0) ? GetColumnEndPosition(_numColumns - 1) : 0;
        }

        public override int GetPreferredInnerHeight()
        {
            if (_bAutoSizeAllRows)
            {
                AutoSizeAllRows();
            }
            return _columnHeaderHeight + 1 + // +1 for drop marker
                    ((_numRows > 0) ? GetRowEndPosition(_numRows - 1) : 0);
        }

        public void RegisterCellRenderer(Type dataClass, CellRenderer cellRenderer)
        {
            if (dataClass == null)
            {
                throw new NullReferenceException("dataClass");
            }
            _cellRenderers.SetByType(dataClass, cellRenderer);

            if (cellRenderer is CellWidgetCreator)
            {
                _hasCellWidgetCreators = true;
            }

            // only call it when we already have a theme
            if (_tableBaseThemeInfo != null)
            {
                ApplyCellRendererTheme(cellRenderer);
            }
        }

        public void SetScrollPosition(int scrollPosX, int scrollPosY)
        {
            if (this._scrollPosX != scrollPosX || this._scrollPosY != scrollPosY)
            {
                this._scrollPosX = scrollPosX;
                this._scrollPosY = scrollPosY;
                InvalidateLayoutLocally();
            }
        }

        public void AdjustScrollPosition(int row)
        {
            CheckRowIndex(row);
            ScrollPane scrollPane = ScrollPane.GetContainingScrollPane(this);
            int numVisibleRows = GetNumVisibleRows();
            if (numVisibleRows >= 1 && scrollPane != null)
            {
                if (row < _firstVisibleRow || (row == _firstVisibleRow && _firstRowPartialVisible))
                {
                    int pos = GetRowStartPosition(row);
                    scrollPane.SetScrollPositionY(pos);
                }
                else if (row > _lastVisibleRow || (row == _lastVisibleRow && _lastRowPartialVisible))
                {
                    int innerHeight = Math.Max(0, GetInnerHeight() - _columnHeaderHeight);
                    int pos = GetRowEndPosition(row);
                    pos = Math.Max(0, pos - innerHeight);
                    scrollPane.SetScrollPositionY(pos);
                }
            }
        }

        public int GetAutoScrollDirection(Event evt, int autoScrollArea)
        {
            int areaY = GetInnerY() + _columnHeaderHeight;
            int areaHeight = GetInnerHeight() - _columnHeaderHeight;
            int mouseY = evt.GetMouseY();
            if (mouseY >= areaY && mouseY < (areaY + areaHeight))
            {
                mouseY -= areaY;
                if ((mouseY <= autoScrollArea) || (areaHeight - mouseY) <= autoScrollArea)
                {
                    // do a 2nd check in case the auto scroll areas overlap
                    if (mouseY < areaHeight / 2)
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

        public int GetPageSizeX(int availableWidth)
        {
            return availableWidth;
        }

        public int GetPageSizeY(int availableHeight)
        {
            return availableHeight - _columnHeaderHeight;
        }

        public bool IsFixedWidthMode()
        {
            ScrollPane scrollPane = ScrollPane.GetContainingScrollPane(this);
            if (scrollPane != null)
            {
                if (scrollPane.GetFixed() != ScrollPane.Fixed.HORIZONTAL)
                {
                    return false;
                }
            }
            return true;
        }

        protected void CheckRowIndex(int row)
        {
            if (row < 0 || row >= _numRows)
            {
                throw new IndexOutOfRangeException("row");
            }
        }

        protected void CheckColumnIndex(int column)
        {
            if (column < 0 || column >= _numColumns)
            {
                throw new IndexOutOfRangeException("column");
            }
        }

        protected void CheckRowRange(int idx, int count)
        {
            if (idx < 0 || count < 0 || count > _numRows || idx > (_numRows - count))
            {
                throw new ArgumentOutOfRangeException("row");
            }
        }

        protected void CheckColumnRange(int idx, int count)
        {
            if (idx < 0 || count < 0 || count > _numColumns || idx > (_numColumns - count))
            {
                throw new ArgumentOutOfRangeException("column");
            }
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeTableBase(themeInfo);
            UpdateAll();
        }

        protected void ApplyThemeTableBase(ThemeInfo themeInfo)
        {
            this._tableBaseThemeInfo = themeInfo;
            this._imageColumnDivider = themeInfo.GetImage("columnDivider");
            this._imageRowBackground = themeInfo.GetImage("row.background");
            this._imageRowOverlay = themeInfo.GetImage("row.overlay");
            this._imageRowDropMarker = themeInfo.GetImage("row.dropmarker");
            this._rowHeight = themeInfo.GetParameter("rowHeight", 32);
            this._defaultColumnWidth = themeInfo.GetParameter("columnHeaderWidth", 256);
            this._columnHeaderHeight = themeInfo.GetParameter("columnHeaderHeight", 10);
            this._columnDividerDraggableDistance = themeInfo.GetParameter("columnDividerDragableDistance", 3);
            this._ensureColumnHeaderMinWidth = themeInfo.GetParameter("ensureColumnHeaderMinWidth", false);

            foreach (CellRenderer cellRenderer in _cellRenderers.GetUniqueValues())
            {
                ApplyCellRendererTheme(cellRenderer);
            }
            ApplyCellRendererTheme(_stringCellRenderer);
            _bUpdateAllColumnWidth = true;
        }

        protected override void ApplyThemeMouseCursor(ThemeInfo themeInfo)
        {
            this._columnResizeCursor = themeInfo.GetMouseCursor("columnResizeCursor");
            this._normalCursor = themeInfo.GetMouseCursor("mouseCursor");
            this._dragNotPossibleCursor = themeInfo.GetMouseCursor("dragNotPossibleCursor");
        }

        protected void ApplyCellRendererTheme(CellRenderer cellRenderer)
        {
            String childThemeName = cellRenderer.GetTheme();
            System.Diagnostics.Debug.Assert(!IsAbsoluteTheme(childThemeName));
            ThemeInfo childTheme = _tableBaseThemeInfo.GetChildTheme(childThemeName);
            if (childTheme != null)
            {
                cellRenderer.ApplyTheme(childTheme);
            }
        }

        public override void RemoveAllChildren()
        {
            throw new InvalidOperationException();
        }

        protected override void ChildAdded(Widget child)
        {
            // ignore
        }

        protected override void ChildRemoved(Widget exChild)
        {
            // ignore
        }

        protected int GetOffsetX()
        {
            return GetInnerX() - _scrollPosX;
        }

        protected int GetOffsetY()
        {
            return GetInnerY() - _scrollPosY + _columnHeaderHeight;
        }

        protected override void PositionChanged()
        {
            base.PositionChanged();
            if (_keyboardSearchHandler != null)
            {
                _keyboardSearchHandler.UpdateInfoWindowPosition();
            }
        }

        protected override void SizeChanged()
        {
            base.SizeChanged();
            if (IsFixedWidthMode())
            {
                _bUpdateAllColumnWidth = true;
            }
            if (_keyboardSearchHandler != null)
            {
                _keyboardSearchHandler.UpdateInfoWindowPosition();
            }
        }

        internal override Object GetTooltipContentAt(int mouseX, int mouseY)
        {
            // use cached row/column
            if (_lastMouseRow >= 0 && _lastMouseRow < GetNumRows() &&
                    _lastMouseColumn >= 0 && _lastMouseColumn < GetNumColumns())
            {
                Object tooltip = GetTooltipContentFromRow(_lastMouseRow, _lastMouseColumn);
                if (tooltip != null)
                {
                    return tooltip;
                }
            }

            return base.GetTooltipContentAt(mouseX, mouseY);
        }

        protected override void Layout()
        {
            int innerWidth = GetInnerWidth();
            int innerHeight = Math.Max(0, GetInnerHeight() - _columnHeaderHeight);

            _cellWidgetContainer.SetPosition(GetInnerX(), GetInnerY() + _columnHeaderHeight);
            _cellWidgetContainer.SetSize(innerWidth, innerHeight);

            if (_bUpdateAllColumnWidth)
            {
                UpdateAllColumnWidth();
            }
            if (_bAutoSizeAllRows)
            {
                AutoSizeAllRows();
            }
            if (_bUpdateAllCellWidgets)
            {
                UpdateAllCellWidgets();
            }

            int scrollEndX = _scrollPosX + innerWidth;
            int scrollEndY = _scrollPosY + innerHeight;

            int startRow = Math.Min(_numRows - 1, Math.Max(0, GetRowFromPosition(_scrollPosY)));
            int startColumn = Math.Min(_numColumns - 1, Math.Max(0, GetColumnFromPosition(_scrollPosX)));
            int endRow = Math.Min(_numRows - 1, Math.Max(startRow, GetRowFromPosition(scrollEndY)));
            int endColumn = Math.Min(_numColumns - 1, Math.Max(startColumn, GetColumnFromPosition(scrollEndX)));

            if (_numRows > 0)
            {
                _firstRowPartialVisible = GetRowStartPosition(startRow) < _scrollPosY;
                _lastRowPartialVisible = GetRowEndPosition(endRow) > scrollEndY;
            }
            else
            {
                _firstRowPartialVisible = false;
                _lastRowPartialVisible = false;
            }

            if (!_widgetGrid.IsEmpty())
            {
                if (startRow > _firstVisibleRow)
                {
                    _widgetGrid.Iterate(_firstVisibleRow, 0, startRow - 1, _numColumns, _removeCellWidgetsFunction);
                }
                if (endRow < _lastVisibleRow)
                {
                    _widgetGrid.Iterate(endRow + 1, 0, _lastVisibleRow, _numColumns, _removeCellWidgetsFunction);
                }

                _widgetGrid.Iterate(startRow, 0, endRow, _numColumns, _insertCellWidgetsFunction);
            }

            _firstVisibleRow = startRow;
            _firstVisibleColumn = startColumn;
            _lastVisibleRow = endRow;
            _lastVisibleColumn = endColumn;

            if (_numColumns > 0)
            {
                int offsetX = GetOffsetX();
                int colStartPos = GetColumnStartPosition(0);
                for (int i = 0; i < _numColumns; i++)
                {
                    int colEndPos = GetColumnEndPosition(i);
                    Widget w = _columnHeaders[i];
                    if (w != null)
                    {
                        System.Diagnostics.Debug.Assert(w.GetParent() == this);
                        w.SetPosition(offsetX + colStartPos +
                                _columnDividerDraggableDistance, GetInnerY());
                        w.SetSize(Math.Max(0, colEndPos - colStartPos -
                                2 * _columnDividerDraggableDistance), _columnHeaderHeight);
                        w.SetVisible(_columnHeaderHeight > 0);
                        AnimationState animationState = w.GetAnimationState();
                        animationState.SetAnimationState(STATE_FIRST_COLUMNHEADER, i == 0);
                        animationState.SetAnimationState(STATE_LAST_COLUMNHEADER, i == _numColumns - 1);
                    }
                    colStartPos = colEndPos;
                }
            }
        }

        protected override void PaintWidget(GUI gui)
        {
            if (_firstVisibleRow < 0 || _firstVisibleRow >= _numRows)
            {
                return;
            }

            int innerX = GetInnerX();
            int innerY = GetInnerY() + _columnHeaderHeight;
            int innerWidth = GetInnerWidth();
            int innerHeight = GetInnerHeight() - _columnHeaderHeight;
            int offsetX = GetOffsetX();
            int offsetY = GetOffsetY();
            Renderer.Renderer renderer = gui.GetRenderer();

            renderer.ClipEnter(innerX, innerY, innerWidth, innerHeight);
            try
            {
                AnimationState animState = GetAnimationState();
                int leadRow;
                int leadColumn;
                bool isCellSelection;

                if (_selectionManager != null)
                {
                    leadRow = _selectionManager.GetLeadRow();
                    leadColumn = _selectionManager.GetLeadColumn();
                    isCellSelection = _selectionManager.GetSelectionGranularity() ==
                            TableSelectionGranularity.Cells;
                }
                else
                {
                    leadRow = -1;
                    leadColumn = -1;
                    isCellSelection = false;
                }

                if (_imageRowBackground != null)
                {
                    PaintRowImage(_imageRowBackground, leadRow);
                }

                if (_imageColumnDivider != null)
                {
                    animState.SetAnimationState(STATE_ROW_SELECTED, false);
                    for (int col = _firstVisibleColumn; col <= _lastVisibleColumn; col++)
                    {
                        int colEndPos = GetColumnEndPosition(col);
                        int curX = offsetX + colEndPos;
                        _imageColumnDivider.Draw(animState, curX, innerY, 1, innerHeight);
                    }
                }

                int rowStartPos = GetRowStartPosition(_firstVisibleRow);
                for (int row = _firstVisibleRow; row <= _lastVisibleRow; row++)
                {
                    int rowEndPos = GetRowEndPosition(row);
                    int curRowHeight = rowEndPos - rowStartPos;
                    int curY = offsetY + rowStartPos;
                    TreeTableNode rowNode = GetNodeFromRow(row);
                    bool bIsRowSelected = !isCellSelection && IsRowSelected(row);

                    int colStartPos = GetColumnStartPosition(_firstVisibleColumn);
                    for (int col = _firstVisibleColumn; col <= _lastVisibleColumn;)
                    {
                        int colEndPos = GetColumnEndPosition(col);
                        CellRenderer cellRenderer = GetCellRenderer(row, col, rowNode);
                        bool bIsCellSelected = bIsRowSelected || IsCellSelected(row, col);

                        int curX = offsetX + colStartPos;
                        int colSpan = 1;

                        if (cellRenderer != null)
                        {
                            colSpan = cellRenderer.GetColumnSpan();
                            if (colSpan > 1)
                            {
                                colEndPos = GetColumnEndPosition(Math.Max(_numColumns - 1, col + colSpan - 1));
                            }

                            Widget cellRendererWidget = cellRenderer.GetCellRenderWidget(
                                    curX, curY, colEndPos - colStartPos, curRowHeight, bIsCellSelected);

                            if (cellRendererWidget != null)
                            {
                                if (cellRendererWidget.GetParent() != this)
                                {
                                    InsertCellRenderer(cellRendererWidget);
                                }
                                PaintChild(gui, cellRendererWidget);
                            }
                        }

                        col += Math.Max(1, colSpan);
                        colStartPos = colEndPos;
                    }

                    rowStartPos = rowEndPos;
                }

                if (_imageRowOverlay != null)
                {
                    PaintRowImage(_imageRowOverlay, leadRow);
                }

                if (_dropMarkerRow >= 0 && _dropMarkerBeforeRow && _imageRowDropMarker != null)
                {
                    int y = (_rowModel != null) ? _rowModel.GetPosition(_dropMarkerRow) : (_dropMarkerRow * _rowHeight);
                    _imageRowDropMarker.Draw(animState, GetOffsetX(), GetOffsetY() + y, _columnModel.GetEndPosition(), 1);
                }
            }
            finally
            {
                renderer.ClipLeave();
            }
        }

        private void PaintRowImage(Image img, int leadRow)
        {
            AnimationState animState = GetAnimationState();
            int x = GetOffsetX();
            int width = _columnModel.GetEndPosition();
            int offsetY = GetOffsetY();

            int rowStartPos = GetRowStartPosition(_firstVisibleRow);
            for (int row = _firstVisibleRow; row <= _lastVisibleRow; row++)
            {
                int rowEndPos = GetRowEndPosition(row);
                int curRowHeight = rowEndPos - rowStartPos;
                int curY = offsetY + rowStartPos;

                animState.SetAnimationState(STATE_ROW_SELECTED, IsRowSelected(row));
                animState.SetAnimationState(STATE_ROW_HOVER, _dragActive == DRAG_INACTIVE &&
                        _lastMouseY >= curY && _lastMouseY < (curY + curRowHeight));
                animState.SetAnimationState(STATE_LEAD_ROW, row == leadRow);
                animState.SetAnimationState(STATE_ROW_DROPTARGET, !_dropMarkerBeforeRow && row == _dropMarkerRow);
                animState.SetAnimationState(STATE_ROW_ODD, (row & 1) == 1);
                img.Draw(animState, x, curY, width, curRowHeight);

                rowStartPos = rowEndPos;
            }
        }

        protected void InsertCellRenderer(Widget widget)
        {
            int posX = widget.GetX();
            int posY = widget.GetY();
            widget.SetVisible(false);
            base.InsertChild(widget, base.GetNumChildren());
            widget.SetPosition(posX, posY);
        }

        public abstract TreeTableNode GetNodeFromRow(int row);
        public abstract Object GetCellData(int row, int column, TreeTableNode node);
        public abstract Object GetTooltipContentFromRow(int row, int column);

        protected bool IsRowSelected(int row)
        {
            if (_selectionManager != null)
            {
                return _selectionManager.IsRowSelected(row);
            }
            return false;
        }

        protected bool IsCellSelected(int row, int column)
        {
            if (_selectionManager != null)
            {
                return _selectionManager.IsCellSelected(row, column);
            }
            return false;
        }

        /**
         * Sets the default cell renderer for the specified column
         * The column numbers are not affected by model changes.
         * 
         * @param column the column, must eb &gt;= 0
         * @param cellRenderer the CellRenderer to use or null to restore the global default
         */
        public void SetColumnDefaultCellRenderer(int column, CellRenderer cellRenderer)
        {
            if (column >= _columnDefaultCellRenderer.Length)
            {
                CellRenderer[] tmp = new CellRenderer[Math.Max(column + 1, _numColumns)];
                Array.Copy(_columnDefaultCellRenderer, 0, tmp, 0, _columnDefaultCellRenderer.Length);
                _columnDefaultCellRenderer = tmp;
            }

            _columnDefaultCellRenderer[column] = cellRenderer;
        }

        /**
         * Returns the default cell renderer for the specified column
         * @param column the column, must eb &gt;= 0
         * @return the previously set CellRenderer or null if non was set
         */
        public CellRenderer GetColumnDefaultCellRenderer(int column)
        {
            if (column < _columnDefaultCellRenderer.Length)
            {
                return _columnDefaultCellRenderer[column];
            }
            return null;
        }

        protected virtual CellRenderer GetCellRendererNoDefault(Object data)
        {
            Type dataClass = data.GetType();
            return (CellRenderer) _cellRenderers.GetByType(dataClass);
        }

        protected CellRenderer GetDefaultCellRenderer(int col)
        {
            CellRenderer cellRenderer = GetColumnDefaultCellRenderer(col);
            if (cellRenderer == null)
            {
                cellRenderer = _stringCellRenderer;
            }
            return cellRenderer;
        }

        protected virtual CellRenderer GetCellRenderer(Object data, int col)
        {
            CellRenderer cellRenderer = GetCellRendererNoDefault(data);
            if (cellRenderer == null)
            {
                cellRenderer = GetDefaultCellRenderer(col);
            }
            return cellRenderer;
        }

        protected virtual CellRenderer GetCellRenderer(int row, int col, TreeTableNode node)
        {
            Object data = GetCellData(row, col, node);
            if (data != null)
            {
                CellRenderer cellRenderer = GetCellRenderer(data, col);
                cellRenderer.SetCellData(row, col, data);
                return cellRenderer;
            }
            return null;
        }

        protected int ComputeRowHeight(int row)
        {
            TreeTableNode rowNode = GetNodeFromRow(row);
            int height = 0;
            for (int column = 0; column < _numColumns; column++)
            {
                CellRenderer cellRenderer = GetCellRenderer(row, column, rowNode);
                if (cellRenderer != null)
                {
                    height = Math.Max(height, cellRenderer.GetPreferredHeight());
                    column += Math.Max(cellRenderer.GetColumnSpan() - 1, 0);
                }
            }
            return height;
        }

        protected int ClampColumnWidth(int width)
        {
            return Math.Max(2 * _columnDividerDraggableDistance + 1, width);
        }

        protected int ComputePreferredColumnWidth(int index)
        {
            return ClampColumnWidth(_columnHeaders[index].GetPreferredWidth());
        }

        protected bool AutoSizeRow(int row)
        {
            int height = ComputeRowHeight(row);
            return _rowModel.SetSize(row, height);
        }

        protected void AutoSizeAllRows()
        {
            if (_rowModel != null)
            {
                _rowModel.InitializeAll(_numRows);
            }
            _bAutoSizeAllRows = false;
        }

        protected void RemoveCellWidget(Widget widget)
        {
            int idx = _cellWidgetContainer.GetChildIndex(widget);
            if (idx >= 0)
            {
                _cellWidgetContainer.RemoveChild(idx);
            }
        }

        void InsertCellWidget(int row, int column, WidgetEntry widgetEntry)
        {
            CellWidgetCreator cwc = (CellWidgetCreator)GetCellRenderer(row, column, null);
            Widget widget = widgetEntry._widget;

            if (widget != null)
            {
                if (widget.GetParent() != _cellWidgetContainer)
                {
                    _cellWidgetContainer.InsertChild(widget, _cellWidgetContainer.GetNumChildren());
                }

                int x = GetColumnStartPosition(column);
                int w = GetColumnEndPosition(column) - x;
                int y = GetRowStartPosition(row);
                int h = GetRowEndPosition(row) - y;

                cwc.PositionWidget(widget, x + GetOffsetX(), y + GetOffsetY(), w, h);
            }
        }

        protected void UpdateCellWidget(int row, int column)
        {
            WidgetEntry we = (WidgetEntry)_widgetGrid.Get(row, column);
            Widget oldWidget = (we != null) ? we._widget : null;
            Widget newWidget = null;

            TreeTableNode rowNode = GetNodeFromRow(row);
            CellRenderer cellRenderer = GetCellRenderer(row, column, rowNode);
            if (cellRenderer is CellWidgetCreator)
            {
                CellWidgetCreator cellWidgetCreator = (CellWidgetCreator)cellRenderer;
                if (we != null && we._creator != cellWidgetCreator)
                {
                    // the cellWidgetCreator has changed for this cell
                    // discard the old widget
                    RemoveCellWidget(oldWidget);
                    oldWidget = null;
                }
                newWidget = cellWidgetCreator.UpdateWidget(oldWidget);
                if (newWidget != null)
                {
                    if (we == null)
                    {
                        we = new WidgetEntry();
                        _widgetGrid.Set(row, column, we);
                    }
                    we._widget = newWidget;
                    we._creator = cellWidgetCreator;
                }
            }

            if (newWidget == null && we != null)
            {
                _widgetGrid.Remove(row, column);
            }

            if (oldWidget != null && newWidget != oldWidget)
            {
                RemoveCellWidget(oldWidget);
            }
        }

        protected void UpdateAllCellWidgets()
        {
            if (!_widgetGrid.IsEmpty() || _hasCellWidgetCreators)
            {
                for (int row = 0; row < _numRows; row++)
                {
                    for (int col = 0; col < _numColumns; col++)
                    {
                        UpdateCellWidget(row, col);
                    }
                }
            }

            _bUpdateAllCellWidgets = false;
        }

        protected void RemoveAllCellWidgets()
        {
            _cellWidgetContainer.RemoveAllChildren();
        }

        protected DialogLayout.Gap GetColumnMPM(int column)
        {
            if (_tableBaseThemeInfo != null)
            {
                ParameterMap columnWidthMap = _tableBaseThemeInfo.GetParameterMap("columnWidths");
                Object obj = columnWidthMap.GetParameterValue(column.ToString(), false);
                if (obj is DialogLayout.Gap)
                {
                    return (DialogLayout.Gap)obj;
                }
                if (obj is Int32)
                {
                    return new DialogLayout.Gap((Int32)obj);
                }
            }
            return null;
        }

        protected ColumnHeader CreateColumnHeader(int column)
        {
            ColumnHeader btn = new ColumnHeader(this);
            btn.SetTheme("columnHeader");
            btn.SetCanAcceptKeyboardFocus(false);
            base.InsertChild(btn, base.GetNumChildren());
            return btn;
        }

        protected void UpdateColumnHeader(int column)
        {
            Button columnHeader = _columnHeaders[column];
            columnHeader.SetText(_columnHeaderModel.ColumnHeaderTextFor(column));
            StateKey[] states = _columnHeaderModel.ColumnHeaderStates;
            if (states.Length > 0)
            {
                AnimationState animationState = columnHeader.GetAnimationState();
                for (int i = 0; i < states.Length; i++)
                {
                    animationState.SetAnimationState(states[i],
                            _columnHeaderModel.ColumnHeaderStateFor(column, i));
                }
            }
        }

        protected virtual void UpdateColumnHeaderNumbers()
        {
            for (int i = 0; i < _columnHeaders.Length; i++)
            {
                _columnHeaders[i]._column = i;
            }
        }

        private void RemoveColumnHeaders(int column, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int idx = base.GetChildIndex(_columnHeaders[column + i]);
                if (idx >= 0)
                {
                    base.RemoveChild(idx);
                }
            }
        }

        protected bool IsMouseInColumnHeader(int y)
        {
            y -= GetInnerY();
            return y >= 0 && y < _columnHeaderHeight;
        }

        protected int GetColumnSeparatorUnderMouse(int x)
        {
            x -= GetOffsetX();
            x += _columnDividerDraggableDistance;
            int col = _columnModel.GetIndex(x);
            int dist = x - _columnModel.GetPosition(col);
            if (dist < 2 * _columnDividerDraggableDistance)
            {
                return col - 1;
            }
            return -1;
        }

        protected int GetRowUnderMouse(int y)
        {
            y -= GetOffsetY();
            int row = GetRowFromPosition(y);
            return row;
        }

        protected int GetColumnUnderMouse(int x)
        {
            x -= GetOffsetX();
            int col = _columnModel.GetIndex(x);
            return col;
        }

        public override bool HandleEvent(Event evt)
        {
            if (_dragActive != DRAG_INACTIVE)
            {
                return HandleDragEvent(evt);
            }

            if (evt.IsKeyEvent() &&
                    _keyboardSearchHandler != null &&
                    _keyboardSearchHandler.IsActive() &&
                    _keyboardSearchHandler.HandleKeyEvent(evt))
            {
                return true;
            }

            if (base.HandleEvent(evt))
            {
                return true;
            }

            if (evt.IsMouseEvent())
            {
                return HandleMouseEvent(evt);
            }

            if (evt.IsKeyEvent() &&
                    _keyboardSearchHandler != null &&
                    _keyboardSearchHandler.HandleKeyEvent(evt))
            {
                return true;
            }

            return false;
        }

        protected override bool HandleKeyStrokeAction(String action, Event evt)
        {
            if (!base.HandleKeyStrokeAction(action, evt))
            {
                if (_selectionManager == null)
                {
                    return false;
                }
                if (!_selectionManager.HandleKeyStrokeAction(action, evt))
                {
                    return false;
                }
            }
            // remove focus from childs
            RequestKeyboardFocus(null);
            return true;
        }

        protected const int DRAG_INACTIVE = 0;
        protected const int DRAG_COLUMN_HEADER = 1;
        protected const int DRAG_USER = 2;
        protected const int DRAG_IGNORE = 3;

        protected int _dragActive;
        protected int _dragColumn;
        protected int _dragStartX;
        protected int _dragStartColWidth;
        protected int _dragStartSumWidth;
        protected MouseCursor _dragCursor;

        protected void CancelDragging()
        {
            if (_dragActive == DRAG_USER)
            {
                if (_dragListener != null)
                {
                    _dragListener.DragCancelled();
                }
                _dragActive = DRAG_IGNORE;
            }
        }

        protected bool HandleDragEvent(Event evt)
        {
            if (evt.IsMouseEvent())
            {
                return HandleMouseEvent(evt);
            }

            if (evt.IsKeyPressedEvent() && evt.GetKeyCode() == Event.KEY_ESCAPE)
            {
                switch (_dragActive)
                {
                    case DRAG_USER:
                        CancelDragging();
                        break;
                    case DRAG_COLUMN_HEADER:
                        ColumnHeaderDragged(_dragStartColWidth);
                        _dragActive = DRAG_IGNORE;
                        break;
                }
                _dragCursor = null;
            }

            return true;
        }

        void MouseLeftTableArea()
        {
            _lastMouseY = LAST_MOUSE_Y_OUTSIDE;
            _lastMouseRow = -1;
            _lastMouseColumn = -1;
        }

        internal override Widget RouteMouseEvent(Event evt)
        {
            if (evt.GetEventType() == EventType.MOUSE_EXITED)
            {
                MouseLeftTableArea();
            }
            else
            {
                _lastMouseY = evt.GetMouseY();
            }

            if (_dragActive == DRAG_INACTIVE)
            {
                bool inHeader = IsMouseInColumnHeader(evt.GetMouseY());
                if (inHeader)
                {
                    if (_lastMouseRow != -1 || _lastMouseColumn != -1)
                    {
                        _lastMouseRow = -1;
                        _lastMouseColumn = -1;
                        ResetTooltip();
                    }
                }
                else
                {
                    int row = GetRowUnderMouse(evt.GetMouseY());
                    int column = GetColumnUnderMouse(evt.GetMouseX());

                    if (_lastMouseRow != row || _lastMouseColumn != column)
                    {
                        _lastMouseRow = row;
                        _lastMouseColumn = column;
                        ResetTooltip();
                    }
                }
            }

            return base.RouteMouseEvent(evt);
        }

        protected bool HandleMouseEvent(Event evt)
        {
            EventType evtType = evt.GetEventType();

            if (_dragActive != DRAG_INACTIVE)
            {
                switch (_dragActive)
                {
                    case DRAG_COLUMN_HEADER:
                        {
                            int innerWidth = GetInnerWidth();
                            if (_dragColumn >= 0 && innerWidth > 0)
                            {
                                int newWidth = ClampColumnWidth(evt.GetMouseX() - _dragStartX);
                                ColumnHeaderDragged(newWidth);
                            }
                            break;
                        }
                    case DRAG_USER:
                        {
                            _dragCursor = _dragListener.Dragged(evt);
                            if (evt.IsMouseDragEnd())
                            {
                                _dragListener.DragStopped(evt);
                            }
                            break;
                        }
                    case DRAG_IGNORE:
                        break;
                    default:
                        throw new Exception("Assertion error");
                }
                if (evt.IsMouseDragEnd())
                {
                    _dragActive = DRAG_INACTIVE;
                    _dragCursor = null;
                }
                return true;
            }

            bool inHeader = IsMouseInColumnHeader(evt.GetMouseY());
            if (inHeader)
            {
                int column = GetColumnSeparatorUnderMouse(evt.GetMouseX());
                bool fixedWidthMode = IsFixedWidthMode();

                // lastMouseRow and lastMouseColumn have been updated in routeMouseEvent()

                if (column >= 0 && (column < GetNumColumns() - 1 || !fixedWidthMode))
                {
                    if (evtType == EventType.MOUSE_BTNDOWN)
                    {
                        _dragStartColWidth = GetColumnWidth(column);
                        _dragColumn = column;
                        _dragStartX = evt.GetMouseX() - _dragStartColWidth;
                        if (fixedWidthMode)
                        {
                            for (int i = 0; i < _numColumns; ++i)
                            {
                                _columnHeaders[i].SetColumnWidth(GetColumnWidth(i));
                            }
                            _dragStartSumWidth = _dragStartColWidth + GetColumnWidth(column + 1);
                        }
                    }

                    if (evt.IsMouseDragEvent())
                    {
                        _dragActive = DRAG_COLUMN_HEADER;
                    }
                    return true;
                }
            }
            else
            {
                // lastMouseRow and lastMouseColumn have been updated in routeMouseEvent()
                int row = _lastMouseRow;
                int column = _lastMouseColumn;

                if (evt.IsMouseDragEvent())
                {
                    if (_dragListener != null && _dragListener.DragStarted(row, row, evt))
                    {
                        _dragCursor = _dragListener.Dragged(evt);
                        _dragActive = DRAG_USER;
                    }
                    else
                    {
                        _dragActive = DRAG_IGNORE;
                    }
                    return true;
                }

                if (_selectionManager != null)
                {
                    _selectionManager.HandleMouseEvent(row, column, evt);
                }

                if (evtType == EventType.MOUSE_CLICKED && evt.GetMouseClickCount() == 2)
                {
                    if (this.DoubleClick != null)
                    {
                        this.DoubleClick.Invoke(this, new TableDoubleClickEventArgs(row, column));
                    }
                }

                if (evtType == EventType.MOUSE_BTNUP && evt.GetMouseButton() == Event.MOUSE_RBUTTON)
                {
                    if (this.RightClick != null)
                    {
                        this.RightClick.Invoke(this, new TableRightClickEventArgs(row, column, evt));
                    }
                }
            }

            // let ScrollPane handle mouse wheel
            return evtType != EventType.MOUSE_WHEEL;
        }

        public override MouseCursor GetMouseCursor(Event evt)
        {
            switch (_dragActive)
            {
                case DRAG_COLUMN_HEADER:
                    return _columnResizeCursor;
                case DRAG_USER:
                    return _dragCursor;
                case DRAG_IGNORE:
                    return _dragNotPossibleCursor;
            }

            bool inHeader = IsMouseInColumnHeader(evt.GetMouseY());
            if (inHeader)
            {
                int column = GetColumnSeparatorUnderMouse(evt.GetMouseX());
                bool fixedWidthMode = IsFixedWidthMode();

                // lastMouseRow and lastMouseColumn have been updated in routeMouseEvent()

                if (column >= 0 && (column < GetNumColumns() - 1 || !fixedWidthMode))
                {
                    return _columnResizeCursor;
                }
            }

            return _normalCursor;
        }

        private void ColumnHeaderDragged(int newWidth)
        {
            if (IsFixedWidthMode())
            {
                System.Diagnostics.Debug.Assert(_dragColumn+1 < _numColumns);
                newWidth = Math.Min(newWidth, _dragStartSumWidth - 2 * _columnDividerDraggableDistance);
                _columnHeaders[_dragColumn].SetColumnWidth(newWidth);
                _columnHeaders[_dragColumn + 1].SetColumnWidth(_dragStartSumWidth - newWidth);
                _bUpdateAllColumnWidth = true;
                InvalidateLayout();
            }
            else
            {
                SetColumnWidth(_dragColumn, newWidth);
            }
        }

        protected virtual void ColumnHeaderClicked(int column)
        {
            if (this.ColumnHeaderClick != null)
            {
                this.ColumnHeaderClick.Invoke(this, new TableColumnHeaderClickEventArgs(column));
            }
        }

        protected void UpdateAllColumnWidth()
        {
            if (GetInnerWidth() > 0)
            {
                _columnModel.InitializeAll(_numColumns);
                _bUpdateAllColumnWidth = false;
            }
        }

        protected void UpdateAll()
        {
            if (!_widgetGrid.IsEmpty())
            {
                RemoveAllCellWidgets();
                _widgetGrid.Clear();
            }

            if (_rowModel != null)
            {
                _bAutoSizeAllRows = true;
            }

            _bUpdateAllCellWidgets = true;
            _bUpdateAllColumnWidth = true;
            InvalidateLayout();
        }

        protected void ModelAllChanged()
        {
            if (_columnHeaders != null)
            {
                RemoveColumnHeaders(0, _columnHeaders.Length);
            }

            _dropMarkerRow = -1;
            _columnHeaders = new ColumnHeader[_numColumns];
            for (int i = 0; i < _numColumns; i++)
            {
                _columnHeaders[i] = CreateColumnHeader(i);
                UpdateColumnHeader(i);
            }
            UpdateColumnHeaderNumbers();

            if (_selectionManager != null)
            {
                _selectionManager.ModelChanged();
            }

            UpdateAll();
        }

        protected void ModelRowChanged(int row)
        {
            if (_rowModel != null)
            {
                if (AutoSizeRow(row))
                {
                    InvalidateLayout();
                }
            }
            for (int col = 0; col < _numColumns; col++)
            {
                UpdateCellWidget(row, col);
            }
            InvalidateLayoutLocally();
        }

        protected void ModelRowsChanged(int idx, int count)
        {
            CheckRowRange(idx, count);
            bool rowHeightChanged = false;
            for (int i = 0; i < count; i++)
            {
                if (_rowModel != null)
                {
                    rowHeightChanged |= AutoSizeRow(idx + i);
                }
                for (int col = 0; col < _numColumns; col++)
                {
                    UpdateCellWidget(idx + i, col);
                }
            }
            InvalidateLayoutLocally();
            if (rowHeightChanged)
            {
                InvalidateLayout();
            }
        }

        protected void ModelCellChanged(int row, int column)
        {
            CheckRowIndex(row);
            CheckColumnIndex(column);
            if (_rowModel != null)
            {
                AutoSizeRow(row);
            }
            UpdateCellWidget(row, column);
            InvalidateLayout();
        }

        protected void ModelRowsInserted(int row, int count)
        {
            CheckRowRange(row, count);
            if (_rowModel != null)
            {
                _rowModel.Insert(row, count);
            }
            if (_dropMarkerRow > row || (_dropMarkerRow == row && _dropMarkerBeforeRow))
            {
                _dropMarkerRow += count;
            }
            if (!_widgetGrid.IsEmpty() || _hasCellWidgetCreators)
            {
                RemoveAllCellWidgets();
                _widgetGrid.InsertRows(row, count);

                for (int i = 0; i < count; i++)
                {
                    for (int col = 0; col < _numColumns; col++)
                    {
                        UpdateCellWidget(row + i, col);
                    }
                }
            }
            // invalidateLayout() before sp.setScrollPositionY() as this may cause a
            // call to invalidateLayoutLocally() which is redundant.
            InvalidateLayout();
            if (row < GetRowFromPosition(_scrollPosY))
            {
                ScrollPane sp = ScrollPane.GetContainingScrollPane(this);
                if (sp != null)
                {
                    int rowsStart = GetRowStartPosition(row);
                    int rowsEnd = GetRowEndPosition(row + count - 1);
                    sp.SetScrollPositionY(_scrollPosY + rowsEnd - rowsStart);
                }
            }
            if (_selectionManager != null)
            {
                _selectionManager.RowsInserted(row, count);
            }
        }

        protected void ModelRowsDeleted(int row, int count)
        {
            if (row + count <= GetRowFromPosition(_scrollPosY))
            {
                ScrollPane sp = ScrollPane.GetContainingScrollPane(this);
                if (sp != null)
                {
                    int rowsStart = GetRowStartPosition(row);
                    int rowsEnd = GetRowEndPosition(row + count - 1);
                    sp.SetScrollPositionY(_scrollPosY - rowsEnd + rowsStart);
                }
            }
            if (_rowModel != null)
            {
                _rowModel.Remove(row, count);
            }
            if (_dropMarkerRow >= row)
            {
                if (_dropMarkerRow < (row + count))
                {
                    _dropMarkerRow = -1;
                }
                else
                {
                    _dropMarkerRow -= count;
                }
            }
            if (!_widgetGrid.IsEmpty())
            {
                _widgetGrid.Iterate(row, 0, row + count - 1, _numColumns, _removeCellWidgetsFunction);
                _widgetGrid.RemoveRows(row, count);
            }
            if (_selectionManager != null)
            {
                _selectionManager.RowsDeleted(row, count);
            }
            InvalidateLayout();
        }

        protected void ModelColumnsInserted(int column, int count)
        {
            CheckColumnRange(column, count);
            ColumnHeader[] newColumnHeaders = new ColumnHeader[_numColumns];
            Array.Copy(_columnHeaders, 0, newColumnHeaders, 0, column);
            Array.Copy(_columnHeaders, column, newColumnHeaders, column + count,
                    _numColumns - (column + count));
            for (int i = 0; i < count; i++)
            {
                newColumnHeaders[column + i] = CreateColumnHeader(column + i);
            }
            _columnHeaders = newColumnHeaders;
            UpdateColumnHeaderNumbers();

            _columnModel.Insert(column, count);

            if (!_widgetGrid.IsEmpty() || _hasCellWidgetCreators)
            {
                RemoveAllCellWidgets();
                _widgetGrid.InsertColumns(column, count);

                for (int row = 0; row < _numRows; row++)
                {
                    for (int i = 0; i < count; i++)
                    {
                        UpdateCellWidget(row, column + i);
                    }
                }
            }
            if (column < GetColumnStartPosition(_scrollPosX))
            {
                ScrollPane sp = ScrollPane.GetContainingScrollPane(this);
                if (sp != null)
                {
                    int columnsStart = GetColumnStartPosition(column);
                    int columnsEnd = GetColumnEndPosition(column + count - 1);
                    sp.SetScrollPositionX(_scrollPosX + columnsEnd - columnsStart);
                }
            }
            InvalidateLayout();
        }

        protected void ModelColumnsDeleted(int column, int count)
        {
            if (column + count <= GetColumnStartPosition(_scrollPosX))
            {
                ScrollPane sp = ScrollPane.GetContainingScrollPane(this);
                if (sp != null)
                {
                    int columnsStart = GetColumnStartPosition(column);
                    int columnsEnd = GetColumnEndPosition(column + count - 1);
                    sp.SetScrollPositionY(_scrollPosX - columnsEnd + columnsStart);
                }
            }
            _columnModel.Remove(column, count);
            if (!_widgetGrid.IsEmpty())
            {
                _widgetGrid.Iterate(0, column, _numRows, column + count - 1, _removeCellWidgetsFunction);
                _widgetGrid.RemoveColumns(column, count);
            }

            RemoveColumnHeaders(column, count);

            ColumnHeader[] newColumnHeaders = new ColumnHeader[_numColumns];
            Array.Copy(_columnHeaders, 0, newColumnHeaders, 0, column);
            Array.Copy(_columnHeaders, column + count, newColumnHeaders, column, _numColumns - count);
            _columnHeaders = newColumnHeaders;
            UpdateColumnHeaderNumbers();

            InvalidateLayout();
        }

        protected void ModelColumnHeaderChanged(int column)
        {
            CheckColumnIndex(column);
            UpdateColumnHeader(column);
        }

        class RowSizeSequence : SizeSequence
        {
            private TableBase _tableBase;

            public RowSizeSequence(TableBase tableBase, int initialCapacity) : base(initialCapacity)
            {
                this._tableBase = tableBase;
            }

            protected internal override void InitializeSizes(int index, int count)
            {
                for (int i = 0; i < count; i++, index++)
                {
                    _table[index] = this._tableBase.ComputeRowHeight(index);
                }
            }
        }

        protected class ColumnSizeSequence : SizeSequence
        {
            private TableBase _tableBase;

            public ColumnSizeSequence(TableBase tableBase)
            {
                this._tableBase = tableBase;
            }

            protected internal override void InitializeSizes(int index, int count)
            {
                bool useSprings = this._tableBase.IsFixedWidthMode();
                if (!useSprings)
                {
                    int sum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int width = this._tableBase.ComputePreferredColumnWidth(index + i);
                        _table[index + i] = width;
                        sum += width;
                    }
                    useSprings = sum < this._tableBase.GetInnerWidth();
                }
                if (useSprings)
                {
                    ComputeColumnHeaderLayout();
                    for (int i = 0; i < count; i++)
                    {
                        _table[index + i] = this._tableBase.ClampColumnWidth(this._tableBase._columnHeaders[i]._springWidth);
                    }
                }
            }

            protected internal bool Update(int index)
            {
                int width;
                if (this._tableBase.IsFixedWidthMode())
                {
                    ComputeColumnHeaderLayout();
                    width = this._tableBase.ClampColumnWidth(this._tableBase._columnHeaders[index]._springWidth);
                }
                else
                {
                    width = this._tableBase.ComputePreferredColumnWidth(index);
                    if (this._tableBase._ensureColumnHeaderMinWidth)
                    {
                        width = Math.Max(width, this._tableBase._columnHeaders[index].GetMinWidth());
                    }
                }
                return SetSize(index, width);
            }

            void ComputeColumnHeaderLayout()
            {
                if (this._tableBase._columnHeaders != null)
                {
                    DialogLayout.SequentialGroup g = (DialogLayout.SequentialGroup)(new DialogLayout()).CreateSequentialGroup();
                    foreach (ColumnHeader h in this._tableBase._columnHeaders)
                    {
                        g.AddSpring(h._spring);
                    }
                    g.SetSize(DialogLayout.AXIS_X, 0, this._tableBase.GetInnerWidth());
                }
            }
            protected internal int ComputePreferredWidth()
            {
                int count = this._tableBase.GetNumColumns();
                if (!this._tableBase.IsFixedWidthMode())
                {
                    int sum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int width = this._tableBase.ComputePreferredColumnWidth(i);
                        sum += width;
                    }
                    return sum;
                }
                if (this._tableBase._columnHeaders != null)
                {
                    DialogLayout.SequentialGroup g = (DialogLayout.SequentialGroup)(new DialogLayout()).CreateSequentialGroup();
                    foreach (ColumnHeader h in this._tableBase._columnHeaders)
                    {
                        g.AddSpring(h._spring);
                    }
                    return g.GetPrefSize(DialogLayout.AXIS_X);
                }
                return 0;
            }
        }

        class RemoveCellWidgets : SparseGrid.GridFunction
        {
            private TableBase _tableBase;
            public RemoveCellWidgets(TableBase tableBase)
            {
                this._tableBase = tableBase;
            }
            public void Apply(int row, int column, SparseGrid.Entry e)
            {
                WidgetEntry widgetEntry = (WidgetEntry)e;
                Widget widget = widgetEntry._widget;
                if (widget != null)
                {
                    this._tableBase.RemoveCellWidget(widget);
                }
            }
        }

        class InsertCellWidgets : SparseGrid.GridFunction
        {
            private TableBase _tableBase;
            public InsertCellWidgets(TableBase tableBase)
            {
                this._tableBase = tableBase;
            }
            public void Apply(int row, int column, SparseGrid.Entry e)
            {
                this._tableBase.InsertCellWidget(row, column, (WidgetEntry)e);
            }
        }

        protected class ColumnHeader : Button
        {
            protected internal int _column;
            private int _columnWidth;
            protected internal int _springWidth;
            private TableBase _tableBase;
            protected internal DialogLayout.Spring _spring;

            public ColumnHeader(TableBase tableBase)
            {
                this._tableBase = tableBase;
                this.Action += (sender, e) =>
                {
                    this._tableBase.ColumnHeaderClicked(_column);
                };
                this._spring = new ColumnHeaderSpring(this);
            }

            class ColumnHeaderSpring : DialogLayout.Spring
            {
                public ColumnHeader columnHeader;
                public ColumnHeaderSpring(ColumnHeader columnHeader)
                {
                    this.columnHeader = columnHeader;
                }

                internal override int GetMinSize(int axis)
                {
                    return this.columnHeader._tableBase.ClampColumnWidth(this.columnHeader.GetMinWidth());
                }

                internal override int GetPrefSize(int axis)
                {
                    return this.columnHeader.GetPreferredWidth();
                }

                internal override int GetMaxSize(int axis)
                {
                    return this.columnHeader.GetMaxWidth();
                }

                internal override void SetSize(int axis, int pos, int size)
                {
                    this.columnHeader._springWidth = size;
                }
            }

            /*DialogLayout.Spring spring = new DialogLayout.Spring() {
                @Override
                int getMinSize(int axis) {
                    return clampColumnWidth(getMinWidth());
                }
                @Override
                int getPrefSize(int axis) {
                    return getPreferredWidth();
                }
                @Override
                int getMaxSize(int axis) {
                    return getMaxWidth();
                }
                @Override
                void setSize(int axis, int pos, int size) {
                    springWidth = size;
                }
            };*/

            public int GetColumnWidth()
            {
                return _columnWidth;
            }

            public void SetColumnWidth(int columnWidth)
            {
                this._columnWidth = columnWidth;
            }

            public override int GetPreferredWidth()
            {
                if (_columnWidth > 0)
                {
                    return _columnWidth;
                }
                DialogLayout.Gap mpm = this._tableBase.GetColumnMPM(_column);
                int prefWidth = (mpm != null) ? mpm.Preferred : this._tableBase._defaultColumnWidth;
                return Math.Max(prefWidth, base.GetPreferredWidth());
            }

            public override int GetMinWidth()
            {
                DialogLayout.Gap mpm = this._tableBase.GetColumnMPM(_column);
                int minWidth = (mpm != null) ? mpm.Min : 0;
                return Math.Max(minWidth, base.GetPreferredWidth());
            }

            public override int GetMaxWidth()
            {
                DialogLayout.Gap mpm = this._tableBase.GetColumnMPM(_column);
                int maxWidth = (mpm != null) ? mpm.Max : 32767;
                return maxWidth;
            }

            public override void AdjustSize()
            {
                // don't do anything
            }

            public override bool HandleEvent(Event evt)
            {
                if (evt.IsMouseEventNoWheel())
                {
                    this._tableBase.MouseLeftTableArea();
                }
                return base.HandleEvent(evt);
            }

            protected override void PaintWidget(GUI gui)
            {
                Renderer.Renderer renderer = gui.GetRenderer();
                renderer.ClipEnter(GetX(), GetY(), GetWidth(), GetHeight());
                try
                {
                    PaintLabelText(GetAnimationState());
                }
                finally
                {
                    renderer.ClipLeave();
                }
            }

            public void Run()
            {
                this._tableBase.ColumnHeaderClicked(_column);
            }
        }

        class WidgetEntry : SparseGrid.Entry
        {
            protected internal Widget _widget;
            protected internal CellWidgetCreator _creator;
        }

        protected internal class CellWidgetContainer : Widget
        {
            protected internal CellWidgetContainer()
            {
                SetTheme("");
                SetClip(true);
            }

            protected override void ChildInvalidateLayout(Widget child)
            {
                // always ignore
            }

            protected override void SizeChanged()
            {
                // always ignore
            }

            protected override void ChildAdded(Widget child)
            {
                // always ignore
            }

            protected override void ChildRemoved(Widget exChild)
            {
                // always ignore
            }

            protected override void AllChildrenRemoved()
            {
                // always ignore
            }
        }

        public class StringCellRenderer : TextWidget, CellRenderer
        {
            public StringCellRenderer()
            {
                SetCache(false);
                SetClip(true);
            }

            public virtual void SetCellData(int row, int column, Object data)
            {
                SetCharSequence(data.ToString());
            }

            public virtual int GetColumnSpan()
            {
                return 1;
            }

            protected override void SizeChanged()
            {
                // this method is overriden to prevent Widget.sizeChanged() from
                // calling invalidateLayout().
                // StringCellRenderer is used as a stamp and does not participate
                // in layouts - so invalidating the layout would lead to many
                // or even constant relayouts and bad performance
            }

            public Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                SetPosition(x, y);
                SetSize(width, height);
                GetAnimationState().SetAnimationState(STATE_SELECTED, isSelected);
                return this;
            }

            void CellRenderer.ApplyTheme(ThemeInfo themeInfo)
            {
                base.ApplyTheme(themeInfo);
            }
        }
    }

    public class TableColumnHeaderClickEventArgs
    {
        public readonly int Column;

        public TableColumnHeaderClickEventArgs(int column)
        {
            this.Column = column;
        }
    }

    public class TableRightClickEventArgs
    {
        public readonly int Row;
        public readonly int Column;
        public readonly Event Event;

        public TableRightClickEventArgs(int row, int column, Event evt)
        {
            this.Row = row;
            this.Column = column;
            this.Event = evt;
        }
    }

    public class TableDoubleClickEventArgs : EventArgs
    {
        public readonly int Row;
        public readonly int Column;

        public TableDoubleClickEventArgs(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }
    }
}
