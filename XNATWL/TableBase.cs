using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            void applyTheme(ThemeInfo themeInfo);

            /**
             * The theme name for this CellRenderer. Must be relative to the Table.
             * @return the theme name.
             */
            String getTheme();

            /**
             * This method sets the row, column and the cell data.
             * It is called before any other cell related method is called.
             * @param row the table row
             * @param column the table column
             * @param data the cell data
             */
            void setCellData(int row, int column, Object data);

            /**
             * Returns how many columns this cell spans. Must be >= 1.
             * Is called after setCellData.
             * @return the column span.
             * @see #setCellData(int, int, java.lang.Object)
             */
            int getColumnSpan();

            /**
             * Returns the preferred cell height in variable row height mode.
             * It is not called at all in fixed row height mode.
             * @return the preferred cell height
             * @see #setCellData(int, int, java.lang.Object)
             * @see TableBase#setVaribleRowHeight(bool)
             */
            int getPreferredHeight();

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
            Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected);
        }

        public interface CellWidgetCreator : CellRenderer
        {
            Widget updateWidget(Widget existingWidget);
            void positionWidget(Widget widget, int x, int y, int w, int h);
        }

        public interface KeyboardSearchHandler
        {
            /**
             * Update search with this key event
             *
             * @param evt the key event
             * @return true if the event was handled
             */
            bool handleKeyEvent(Event evt);

            /**
             * Returns true if the search is active.
             * @return true if the search is active.
             */
            bool isActive();

            /**
             * Called when the table position ot size has changed.
             */
            void updateInfoWindowPosition();
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
            bool dragStarted(int row, int col, Event evt);

            /**
             * Mouse dragging in progress
             * @param evt the MOUSE_DRAGGED event
             * @return the mouse cursor to display
             */
            MouseCursor dragged(Event evt);

            /**
             * Mouse dragging stopped
             * @param evt the event which stopped the mouse drag
             */
            void dragStopped(Event evt);

            /**
             * Called when the mouse drag is canceled (eg by pressing ESCAPE)
             */
            void dragCanceled();
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

        private StringCellRenderer stringCellRenderer;
        private RemoveCellWidgets removeCellWidgetsFunction;
        private InsertCellWidgets insertCellWidgetsFunction;
        private CellWidgetContainer cellWidgetContainer;

        protected TypeMapping cellRenderers;
        protected SparseGrid widgetGrid;
        protected ColumnSizeSequence columnModel;
        protected TableColumnHeaderModel columnHeaderModel;
        protected SizeSequence rowModel;
        protected bool hasCellWidgetCreators;
        protected ColumnHeader[] columnHeaders;
        protected CellRenderer[] columnDefaultCellRenderer;
        protected TableSelectionManager selectionManager;
        protected KeyboardSearchHandler keyboardSearchHandler;
        protected DragListener dragListener;

        protected Image imageColumnDivider;
        protected Image imageRowBackground;
        protected Image imageRowOverlay;
        protected Image imageRowDropMarker;
        protected ThemeInfo tableBaseThemeInfo;
        protected int columnHeaderHeight;
        protected int columnDividerDragableDistance;
        protected MouseCursor columnResizeCursor;
        protected MouseCursor normalCursor;
        protected MouseCursor dragNotPossibleCursor;
        protected bool ensureColumnHeaderMinWidth;

        protected int numRows;
        protected int numColumns;
        protected int rowHeight = 32;
        protected int defaultColumnWidth = 256;
        protected bool bAutoSizeAllRows;
        protected bool bUpdateAllCellWidgets;
        protected bool bUpdateAllColumnWidth;

        protected int scrollPosX;
        protected int scrollPosY;

        protected int firstVisibleRow;
        protected int firstVisibleColumn;
        protected int lastVisibleRow;
        protected int lastVisibleColumn;
        protected bool firstRowPartialVisible;
        protected bool lastRowPartialVisible;

        protected int dropMarkerRow = -1;
        protected bool dropMarkerBeforeRow;

        protected static int LAST_MOUSE_Y_OUTSIDE = int.MinValue;

        protected int lastMouseY = LAST_MOUSE_Y_OUTSIDE;
        protected int lastMouseRow = -1;
        protected int lastMouseColumn = -1;

        protected TableBase()
        {
            this.cellRenderers = new TypeMapping();
            this.stringCellRenderer = new StringCellRenderer();
            this.widgetGrid = new SparseGrid(32);
            this.removeCellWidgetsFunction = new RemoveCellWidgets(this);
            this.insertCellWidgetsFunction = new InsertCellWidgets(this);
            this.columnModel = new ColumnSizeSequence(this);
            this.columnDefaultCellRenderer = new CellRenderer[8];
            this.cellWidgetContainer = new CellWidgetContainer();

            base.insertChild(cellWidgetContainer, 0);
            setCanAcceptKeyboardFocus(true);
        }

        public TableSelectionManager getSelectionManager()
        {
            return selectionManager;
        }

        public void setSelectionManager(TableSelectionManager selectionManager)
        {
            if (this.selectionManager != selectionManager)
            {
                if (this.selectionManager != null)
                {
                    this.selectionManager.setAssociatedTable(null);
                }
                this.selectionManager = selectionManager;
                if (this.selectionManager != null)
                {
                    this.selectionManager.setAssociatedTable(this);
                }
            }
        }

        /**
         * Installs a multi row selection manager.
         *
         * @see TableRowSelectionManager
         * @see DefaultTableSelectionModel
         */
        public void setDefaultSelectionManager()
        {
            setSelectionManager(new TableRowSelectionManager());
        }

        public KeyboardSearchHandler getKeyboardSearchHandler()
        {
            return keyboardSearchHandler;
        }

        public void setKeyboardSearchHandler(KeyboardSearchHandler keyboardSearchHandler)
        {
            this.keyboardSearchHandler = keyboardSearchHandler;
        }

        public DragListener getDragListener()
        {
            return dragListener;
        }

        public void setDragListener(DragListener dragListener)
        {
            cancelDragging();
            this.dragListener = dragListener;
        }

        public bool isDropMarkerBeforeRow()
        {
            return dropMarkerBeforeRow;
        }

        public int getDropMarkerRow()
        {
            return dropMarkerRow;
        }

        public void setDropMarker(int row, bool beforeRow)
        {
            if (row < 0 || row > numRows)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (row == numRows && !beforeRow)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            dropMarkerRow = row;
            dropMarkerBeforeRow = beforeRow;
        }

        public bool setDropMarker(Event evt)
        {
            int mouseY = evt.getMouseY();
            if (isMouseInside(evt) && !isMouseInColumnHeader(mouseY))
            {
                mouseY -= getOffsetY();
                int row = getRowFromPosition(mouseY);
                if (row >= 0 && row < numRows)
                {
                    int rowStart = getRowStartPosition(row);
                    int rowEnd = getRowEndPosition(row);
                    int margin = (rowEnd - rowStart + 2) / 4;
                    if ((mouseY - rowStart) < margin)
                    {
                        setDropMarker(row, true);
                    }
                    else if ((rowEnd - mouseY) < margin)
                    {
                        setDropMarker(row + 1, true);
                    }
                    else
                    {
                        setDropMarker(row, false);
                    }
                    return true;
                }
                else if (row == numRows)
                {
                    setDropMarker(row, true);
                    return true;
                }
            }
            return false;
        }

        public void clearDropMarker()
        {
            dropMarkerRow = -1;
        }

        public bool isVariableRowHeight()
        {
            return rowModel != null;
        }

        public void setVaribleRowHeight(bool varibleRowHeight)
        {
            if (varibleRowHeight && rowModel == null)
            {
                rowModel = new RowSizeSequence(this, numRows);
                bAutoSizeAllRows = true;
                invalidateLayout();
            }
            else if (!varibleRowHeight)
            {
                rowModel = null;
            }
        }

        public int getNumRows()
        {
            return numRows;
        }

        public int getNumColumns()
        {
            return numColumns;
        }

        public int getRowFromPosition(int y)
        {
            if (y >= 0)
            {
                if (rowModel != null)
                {
                    return rowModel.getIndex(y);
                }
                return Math.Min(numRows - 1, y / rowHeight);
            }
            return -1;
        }

        public int getRowStartPosition(int row)
        {
            checkRowIndex(row);
            if (rowModel != null)
            {
                return rowModel.getPosition(row);
            }
            else
            {
                return row * rowHeight;
            }
        }

        public int getRowHeight(int row)
        {
            checkRowIndex(row);
            if (rowModel != null)
            {
                return rowModel.getSize(row);
            }
            else
            {
                return rowHeight;
            }
        }

        public int getRowEndPosition(int row)
        {
            checkRowIndex(row);
            if (rowModel != null)
            {
                return rowModel.getPosition(row + 1);
            }
            else
            {
                return (row + 1) * rowHeight;
            }
        }

        public int getColumnFromPosition(int x)
        {
            if (x >= 0)
            {
                int column = columnModel.getIndex(x);
                return column;
            }
            return -1;
        }

        public int getColumnStartPosition(int column)
        {
            checkColumnIndex(column);
            return columnModel.getPosition(column);
        }

        public int getColumnWidth(int column)
        {
            checkColumnIndex(column);
            return columnModel.getSize(column);
        }

        public int getColumnEndPosition(int column)
        {
            checkColumnIndex(column);
            return columnModel.getPosition(column + 1);
        }

        public void setColumnWidth(int column, int width)
        {
            checkColumnIndex(column);
            columnHeaders[column].setColumnWidth(width);    // store passed width
            if (columnModel.update(column))
            {
                invalidateLayout();
            }
        }

        public AnimationState getColumnHeaderAnimationState(int column)
        {
            checkColumnIndex(column);
            return columnHeaders[column].getAnimationState();
        }

        /**
         * Sets the sort order animation state for all column headers.
         * @param sortColumn This column gets sort order indicators, all other columns not
         * @param sortOrder Which sort order. Can be null to disable the indicators
         */
        public void setColumnSortOrderAnimationState(int sortColumn, SortOrder sortOrder)
        {
            for (int column = 0; column < numColumns; ++column)
            {
                AnimationState animState = columnHeaders[column].getAnimationState();
                animState.setAnimationState(STATE_SORT_ASCENDING, (column == sortColumn) && (sortOrder == SortOrder.ASCENDING));
                animState.setAnimationState(STATE_SORT_DESCENDING, (column == sortColumn) && (sortOrder == SortOrder.DESCENDING));
            }
        }

        public void scrollToRow(int row)
        {
            ScrollPane scrollPane = ScrollPane.getContainingScrollPane(this);
            if (scrollPane != null && numRows > 0)
            {
                scrollPane.validateLayout();
                int rowStart = getRowStartPosition(row);
                int rowEnd = getRowEndPosition(row);
                int height = rowEnd - rowStart;
                scrollPane.scrollToAreaY(rowStart, height, height / 2);
            }
        }

        public int getNumVisibleRows()
        {
            int rows = lastVisibleRow - firstVisibleRow;
            if (!lastRowPartialVisible)
            {
                rows++;
            }
            return rows;
        }

        public override int getMinHeight()
        {
            return Math.Max(base.getMinHeight(), columnHeaderHeight);
        }

        public override int getPreferredInnerWidth()
        {
            if (getInnerWidth() == 0)
            {
                return columnModel.computePreferredWidth();
            }
            if (bUpdateAllColumnWidth)
            {
                updateAllColumnWidth();
            }
            return (numColumns > 0) ? getColumnEndPosition(numColumns - 1) : 0;
        }

        public override int getPreferredInnerHeight()
        {
            if (bAutoSizeAllRows)
            {
                autoSizeAllRows();
            }
            return columnHeaderHeight + 1 + // +1 for drop marker
                    ((numRows > 0) ? getRowEndPosition(numRows - 1) : 0);
        }

        public void registerCellRenderer(Type dataClass, CellRenderer cellRenderer)
        {
            if (dataClass == null)
            {
                throw new NullReferenceException("dataClass");
            }
            cellRenderers.SetByType(dataClass, cellRenderer);

            if (cellRenderer is CellWidgetCreator)
            {
                hasCellWidgetCreators = true;
            }

            // only call it when we already have a theme
            if (tableBaseThemeInfo != null)
            {
                applyCellRendererTheme(cellRenderer);
            }
        }

        public void setScrollPosition(int scrollPosX, int scrollPosY)
        {
            if (this.scrollPosX != scrollPosX || this.scrollPosY != scrollPosY)
            {
                this.scrollPosX = scrollPosX;
                this.scrollPosY = scrollPosY;
                invalidateLayoutLocally();
            }
        }

        public void adjustScrollPosition(int row)
        {
            checkRowIndex(row);
            ScrollPane scrollPane = ScrollPane.getContainingScrollPane(this);
            int numVisibleRows = getNumVisibleRows();
            if (numVisibleRows >= 1 && scrollPane != null)
            {
                if (row < firstVisibleRow || (row == firstVisibleRow && firstRowPartialVisible))
                {
                    int pos = getRowStartPosition(row);
                    scrollPane.setScrollPositionY(pos);
                }
                else if (row > lastVisibleRow || (row == lastVisibleRow && lastRowPartialVisible))
                {
                    int innerHeight = Math.Max(0, getInnerHeight() - columnHeaderHeight);
                    int pos = getRowEndPosition(row);
                    pos = Math.Max(0, pos - innerHeight);
                    scrollPane.setScrollPositionY(pos);
                }
            }
        }

        public int getAutoScrollDirection(Event evt, int autoScrollArea)
        {
            int areaY = getInnerY() + columnHeaderHeight;
            int areaHeight = getInnerHeight() - columnHeaderHeight;
            int mouseY = evt.getMouseY();
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

        public int getPageSizeX(int availableWidth)
        {
            return availableWidth;
        }

        public int getPageSizeY(int availableHeight)
        {
            return availableHeight - columnHeaderHeight;
        }

        public bool isFixedWidthMode()
        {
            ScrollPane scrollPane = ScrollPane.getContainingScrollPane(this);
            if (scrollPane != null)
            {
                if (scrollPane.getFixed() != ScrollPane.Fixed.HORIZONTAL)
                {
                    return false;
                }
            }
            return true;
        }

        protected void checkRowIndex(int row)
        {
            if (row < 0 || row >= numRows)
            {
                throw new IndexOutOfRangeException("row");
            }
        }

        protected void checkColumnIndex(int column)
        {
            if (column < 0 || column >= numColumns)
            {
                throw new IndexOutOfRangeException("column");
            }
        }

        protected void checkRowRange(int idx, int count)
        {
            if (idx < 0 || count < 0 || count > numRows || idx > (numRows - count))
            {
                throw new ArgumentOutOfRangeException("row");
            }
        }

        protected void checkColumnRange(int idx, int count)
        {
            if (idx < 0 || count < 0 || count > numColumns || idx > (numColumns - count))
            {
                throw new ArgumentOutOfRangeException("column");
            }
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeTableBase(themeInfo);
            updateAll();
        }

        protected void applyThemeTableBase(ThemeInfo themeInfo)
        {
            this.tableBaseThemeInfo = themeInfo;
            this.imageColumnDivider = themeInfo.getImage("columnDivider");
            this.imageRowBackground = themeInfo.getImage("row.background");
            this.imageRowOverlay = themeInfo.getImage("row.overlay");
            this.imageRowDropMarker = themeInfo.getImage("row.dropmarker");
            this.rowHeight = themeInfo.getParameter("rowHeight", 32);
            this.defaultColumnWidth = themeInfo.getParameter("columnHeaderWidth", 256);
            this.columnHeaderHeight = themeInfo.getParameter("columnHeaderHeight", 10);
            this.columnDividerDragableDistance = themeInfo.getParameter("columnDividerDragableDistance", 3);
            this.ensureColumnHeaderMinWidth = themeInfo.getParameter("ensureColumnHeaderMinWidth", false);

            foreach (CellRenderer cellRenderer in cellRenderers.getUniqueValues())
            {
                applyCellRendererTheme(cellRenderer);
            }
            applyCellRendererTheme(stringCellRenderer);
            bUpdateAllColumnWidth = true;
        }

        protected override void applyThemeMouseCursor(ThemeInfo themeInfo)
        {
            this.columnResizeCursor = themeInfo.getMouseCursor("columnResizeCursor");
            this.normalCursor = themeInfo.getMouseCursor("mouseCursor");
            this.dragNotPossibleCursor = themeInfo.getMouseCursor("dragNotPossibleCursor");
        }

        protected void applyCellRendererTheme(CellRenderer cellRenderer)
        {
            String childThemeName = cellRenderer.getTheme();
            System.Diagnostics.Debug.Assert(!isAbsoluteTheme(childThemeName));
            ThemeInfo childTheme = tableBaseThemeInfo.getChildTheme(childThemeName);
            if (childTheme != null)
            {
                cellRenderer.applyTheme(childTheme);
            }
        }

        public override void removeAllChildren()
        {
            throw new InvalidOperationException();
        }

        protected override void childAdded(Widget child)
        {
            // ignore
        }

        protected override void childRemoved(Widget exChild)
        {
            // ignore
        }

        protected int getOffsetX()
        {
            return getInnerX() - scrollPosX;
        }

        protected int getOffsetY()
        {
            return getInnerY() - scrollPosY + columnHeaderHeight;
        }

        protected override void positionChanged()
        {
            base.positionChanged();
            if (keyboardSearchHandler != null)
            {
                keyboardSearchHandler.updateInfoWindowPosition();
            }
        }

        protected override void sizeChanged()
        {
            base.sizeChanged();
            if (isFixedWidthMode())
            {
                bUpdateAllColumnWidth = true;
            }
            if (keyboardSearchHandler != null)
            {
                keyboardSearchHandler.updateInfoWindowPosition();
            }
        }

        internal override Object getTooltipContentAt(int mouseX, int mouseY)
        {
            // use cached row/column
            if (lastMouseRow >= 0 && lastMouseRow < getNumRows() &&
                    lastMouseColumn >= 0 && lastMouseColumn < getNumColumns())
            {
                Object tooltip = getTooltipContentFromRow(lastMouseRow, lastMouseColumn);
                if (tooltip != null)
                {
                    return tooltip;
                }
            }
            return base.getTooltipContentAt(mouseX, mouseY);
        }

        protected override void layout()
        {
            int innerWidth = getInnerWidth();
            int innerHeight = Math.Max(0, getInnerHeight() - columnHeaderHeight);

            cellWidgetContainer.setPosition(getInnerX(), getInnerY() + columnHeaderHeight);
            cellWidgetContainer.setSize(innerWidth, innerHeight);

            if (bUpdateAllColumnWidth)
            {
                updateAllColumnWidth();
            }
            if (bAutoSizeAllRows)
            {
                autoSizeAllRows();
            }
            if (bUpdateAllCellWidgets)
            {
                updateAllCellWidgets();
            }

            int scrollEndX = scrollPosX + innerWidth;
            int scrollEndY = scrollPosY + innerHeight;

            int startRow = Math.Min(numRows - 1, Math.Max(0, getRowFromPosition(scrollPosY)));
            int startColumn = Math.Min(numColumns - 1, Math.Max(0, getColumnFromPosition(scrollPosX)));
            int endRow = Math.Min(numRows - 1, Math.Max(startRow, getRowFromPosition(scrollEndY)));
            int endColumn = Math.Min(numColumns - 1, Math.Max(startColumn, getColumnFromPosition(scrollEndX)));

            if (numRows > 0)
            {
                firstRowPartialVisible = getRowStartPosition(startRow) < scrollPosY;
                lastRowPartialVisible = getRowEndPosition(endRow) > scrollEndY;
            }
            else
            {
                firstRowPartialVisible = false;
                lastRowPartialVisible = false;
            }

            if (!widgetGrid.isEmpty())
            {
                if (startRow > firstVisibleRow)
                {
                    widgetGrid.iterate(firstVisibleRow, 0, startRow - 1, numColumns, removeCellWidgetsFunction);
                }
                if (endRow < lastVisibleRow)
                {
                    widgetGrid.iterate(endRow + 1, 0, lastVisibleRow, numColumns, removeCellWidgetsFunction);
                }

                widgetGrid.iterate(startRow, 0, endRow, numColumns, insertCellWidgetsFunction);
            }

            firstVisibleRow = startRow;
            firstVisibleColumn = startColumn;
            lastVisibleRow = endRow;
            lastVisibleColumn = endColumn;

            if (numColumns > 0)
            {
                int offsetX = getOffsetX();
                int colStartPos = getColumnStartPosition(0);
                for (int i = 0; i < numColumns; i++)
                {
                    int colEndPos = getColumnEndPosition(i);
                    Widget w = columnHeaders[i];
                    if (w != null)
                    {
                        System.Diagnostics.Debug.Assert(w.getParent() == this);
                        w.setPosition(offsetX + colStartPos +
                                columnDividerDragableDistance, getInnerY());
                        w.setSize(Math.Max(0, colEndPos - colStartPos -
                                2 * columnDividerDragableDistance), columnHeaderHeight);
                        w.setVisible(columnHeaderHeight > 0);
                        AnimationState animationState = w.getAnimationState();
                        animationState.setAnimationState(STATE_FIRST_COLUMNHEADER, i == 0);
                        animationState.setAnimationState(STATE_LAST_COLUMNHEADER, i == numColumns - 1);
                    }
                    colStartPos = colEndPos;
                }
            }
        }

        protected override void paintWidget(GUI gui)
        {
            if (firstVisibleRow < 0 || firstVisibleRow >= numRows)
            {
                return;
            }

            int innerX = getInnerX();
            int innerY = getInnerY() + columnHeaderHeight;
            int innerWidth = getInnerWidth();
            int innerHeight = getInnerHeight() - columnHeaderHeight;
            int offsetX = getOffsetX();
            int offsetY = getOffsetY();
            Renderer.Renderer renderer = gui.getRenderer();

            renderer.ClipEnter(innerX, innerY, innerWidth, innerHeight);
            try
            {
                AnimationState animState = getAnimationState();
                int leadRow;
                int leadColumn;
                bool isCellSelection;

                if (selectionManager != null)
                {
                    leadRow = selectionManager.getLeadRow();
                    leadColumn = selectionManager.getLeadColumn();
                    isCellSelection = selectionManager.getSelectionGranularity() ==
                            TableSelectionGranularity.CELLS;
                }
                else
                {
                    leadRow = -1;
                    leadColumn = -1;
                    isCellSelection = false;
                }

                if (imageRowBackground != null)
                {
                    paintRowImage(imageRowBackground, leadRow);
                }

                if (imageColumnDivider != null)
                {
                    animState.setAnimationState(STATE_ROW_SELECTED, false);
                    for (int col = firstVisibleColumn; col <= lastVisibleColumn; col++)
                    {
                        int colEndPos = getColumnEndPosition(col);
                        int curX = offsetX + colEndPos;
                        imageColumnDivider.Draw(animState, curX, innerY, 1, innerHeight);
                    }
                }

                int rowStartPos = getRowStartPosition(firstVisibleRow);
                for (int row = firstVisibleRow; row <= lastVisibleRow; row++)
                {
                    int rowEndPos = getRowEndPosition(row);
                    int curRowHeight = rowEndPos - rowStartPos;
                    int curY = offsetY + rowStartPos;
                    TreeTableNode rowNode = getNodeFromRow(row);
                    bool bIsRowSelected = !isCellSelection && isRowSelected(row);

                    int colStartPos = getColumnStartPosition(firstVisibleColumn);
                    for (int col = firstVisibleColumn; col <= lastVisibleColumn;)
                    {
                        int colEndPos = getColumnEndPosition(col);
                        CellRenderer cellRenderer = getCellRenderer(row, col, rowNode);
                        bool bIsCellSelected = bIsRowSelected || isCellSelected(row, col);

                        int curX = offsetX + colStartPos;
                        int colSpan = 1;

                        if (cellRenderer != null)
                        {
                            colSpan = cellRenderer.getColumnSpan();
                            if (colSpan > 1)
                            {
                                colEndPos = getColumnEndPosition(Math.Max(numColumns - 1, col + colSpan - 1));
                            }

                            Widget cellRendererWidget = cellRenderer.getCellRenderWidget(
                                    curX, curY, colEndPos - colStartPos, curRowHeight, bIsCellSelected);

                            if (cellRendererWidget != null)
                            {
                                if (cellRendererWidget.getParent() != this)
                                {
                                    insertCellRenderer(cellRendererWidget);
                                }
                                paintChild(gui, cellRendererWidget);
                            }
                        }

                        col += Math.Max(1, colSpan);
                        colStartPos = colEndPos;
                    }

                    rowStartPos = rowEndPos;
                }

                if (imageRowOverlay != null)
                {
                    paintRowImage(imageRowOverlay, leadRow);
                }

                if (dropMarkerRow >= 0 && dropMarkerBeforeRow && imageRowDropMarker != null)
                {
                    int y = (rowModel != null) ? rowModel.getPosition(dropMarkerRow) : (dropMarkerRow * rowHeight);
                    imageRowDropMarker.Draw(animState, getOffsetX(), getOffsetY() + y, columnModel.getEndPosition(), 1);
                }
            }
            finally
            {
                renderer.ClipLeave();
            }
        }

        private void paintRowImage(Image img, int leadRow)
        {
            AnimationState animState = getAnimationState();
            int x = getOffsetX();
            int width = columnModel.getEndPosition();
            int offsetY = getOffsetY();

            int rowStartPos = getRowStartPosition(firstVisibleRow);
            for (int row = firstVisibleRow; row <= lastVisibleRow; row++)
            {
                int rowEndPos = getRowEndPosition(row);
                int curRowHeight = rowEndPos - rowStartPos;
                int curY = offsetY + rowStartPos;

                animState.setAnimationState(STATE_ROW_SELECTED, isRowSelected(row));
                animState.setAnimationState(STATE_ROW_HOVER, dragActive == DRAG_INACTIVE &&
                        lastMouseY >= curY && lastMouseY < (curY + curRowHeight));
                animState.setAnimationState(STATE_LEAD_ROW, row == leadRow);
                animState.setAnimationState(STATE_ROW_DROPTARGET, !dropMarkerBeforeRow && row == dropMarkerRow);
                animState.setAnimationState(STATE_ROW_ODD, (row & 1) == 1);
                img.Draw(animState, x, curY, width, curRowHeight);

                rowStartPos = rowEndPos;
            }
        }

        protected void insertCellRenderer(Widget widget)
        {
            int posX = widget.getX();
            int posY = widget.getY();
            widget.setVisible(false);
            base.insertChild(widget, base.getNumChildren());
            widget.setPosition(posX, posY);
        }

        public abstract TreeTableNode getNodeFromRow(int row);
        public abstract Object getCellData(int row, int column, TreeTableNode node);
        public abstract Object getTooltipContentFromRow(int row, int column);

        protected bool isRowSelected(int row)
        {
            if (selectionManager != null)
            {
                return selectionManager.isRowSelected(row);
            }
            return false;
        }

        protected bool isCellSelected(int row, int column)
        {
            if (selectionManager != null)
            {
                return selectionManager.isCellSelected(row, column);
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
        public void setColumnDefaultCellRenderer(int column, CellRenderer cellRenderer)
        {
            if (column >= columnDefaultCellRenderer.Length)
            {
                CellRenderer[] tmp = new CellRenderer[Math.Max(column + 1, numColumns)];
                Array.Copy(columnDefaultCellRenderer, 0, tmp, 0, columnDefaultCellRenderer.Length);
                columnDefaultCellRenderer = tmp;
            }

            columnDefaultCellRenderer[column] = cellRenderer;
        }

        /**
         * Returns the default cell renderer for the specified column
         * @param column the column, must eb &gt;= 0
         * @return the previously set CellRenderer or null if non was set
         */
        public CellRenderer getColumnDefaultCellRenderer(int column)
        {
            if (column < columnDefaultCellRenderer.Length)
            {
                return columnDefaultCellRenderer[column];
            }
            return null;
        }

        protected virtual CellRenderer getCellRendererNoDefault(Object data)
        {
            Type dataClass = data.GetType();
            return (CellRenderer) cellRenderers.GetByType(dataClass);
        }

        protected CellRenderer getDefaultCellRenderer(int col)
        {
            CellRenderer cellRenderer = getColumnDefaultCellRenderer(col);
            if (cellRenderer == null)
            {
                cellRenderer = stringCellRenderer;
            }
            return cellRenderer;
        }

        protected virtual CellRenderer getCellRenderer(Object data, int col)
        {
            CellRenderer cellRenderer = getCellRendererNoDefault(data);
            if (cellRenderer == null)
            {
                cellRenderer = getDefaultCellRenderer(col);
            }
            return cellRenderer;
        }

        protected virtual CellRenderer getCellRenderer(int row, int col, TreeTableNode node)
        {
            Object data = getCellData(row, col, node);
            if (data != null)
            {
                CellRenderer cellRenderer = getCellRenderer(data, col);
                cellRenderer.setCellData(row, col, data);
                return cellRenderer;
            }
            return null;
        }

        protected int computeRowHeight(int row)
        {
            TreeTableNode rowNode = getNodeFromRow(row);
            int height = 0;
            for (int column = 0; column < numColumns; column++)
            {
                CellRenderer cellRenderer = getCellRenderer(row, column, rowNode);
                if (cellRenderer != null)
                {
                    height = Math.Max(height, cellRenderer.getPreferredHeight());
                    column += Math.Max(cellRenderer.getColumnSpan() - 1, 0);
                }
            }
            return height;
        }

        protected int clampColumnWidth(int width)
        {
            return Math.Max(2 * columnDividerDragableDistance + 1, width);
        }

        protected int computePreferredColumnWidth(int index)
        {
            return clampColumnWidth(columnHeaders[index].getPreferredWidth());
        }

        protected bool autoSizeRow(int row)
        {
            int height = computeRowHeight(row);
            return rowModel.setSize(row, height);
        }

        protected void autoSizeAllRows()
        {
            if (rowModel != null)
            {
                rowModel.initializeAll(numRows);
            }
            bAutoSizeAllRows = false;
        }

        protected void removeCellWidget(Widget widget)
        {
            int idx = cellWidgetContainer.getChildIndex(widget);
            if (idx >= 0)
            {
                cellWidgetContainer.removeChild(idx);
            }
        }

        void insertCellWidget(int row, int column, WidgetEntry widgetEntry)
        {
            CellWidgetCreator cwc = (CellWidgetCreator)getCellRenderer(row, column, null);
            Widget widget = widgetEntry.widget;

            if (widget != null)
            {
                if (widget.getParent() != cellWidgetContainer)
                {
                    cellWidgetContainer.insertChild(widget, cellWidgetContainer.getNumChildren());
                }

                int x = getColumnStartPosition(column);
                int w = getColumnEndPosition(column) - x;
                int y = getRowStartPosition(row);
                int h = getRowEndPosition(row) - y;

                cwc.positionWidget(widget, x + getOffsetX(), y + getOffsetY(), w, h);
            }
        }

        protected void updateCellWidget(int row, int column)
        {
            WidgetEntry we = (WidgetEntry)widgetGrid.get(row, column);
            Widget oldWidget = (we != null) ? we.widget : null;
            Widget newWidget = null;

            TreeTableNode rowNode = getNodeFromRow(row);
            CellRenderer cellRenderer = getCellRenderer(row, column, rowNode);
            if (cellRenderer is CellWidgetCreator)
            {
                CellWidgetCreator cellWidgetCreator = (CellWidgetCreator)cellRenderer;
                if (we != null && we.creator != cellWidgetCreator)
                {
                    // the cellWidgetCreator has changed for this cell
                    // discard the old widget
                    removeCellWidget(oldWidget);
                    oldWidget = null;
                }
                newWidget = cellWidgetCreator.updateWidget(oldWidget);
                if (newWidget != null)
                {
                    if (we == null)
                    {
                        we = new WidgetEntry();
                        widgetGrid.set(row, column, we);
                    }
                    we.widget = newWidget;
                    we.creator = cellWidgetCreator;
                }
            }

            if (newWidget == null && we != null)
            {
                widgetGrid.remove(row, column);
            }

            if (oldWidget != null && newWidget != oldWidget)
            {
                removeCellWidget(oldWidget);
            }
        }

        protected void updateAllCellWidgets()
        {
            if (!widgetGrid.isEmpty() || hasCellWidgetCreators)
            {
                for (int row = 0; row < numRows; row++)
                {
                    for (int col = 0; col < numColumns; col++)
                    {
                        updateCellWidget(row, col);
                    }
                }
            }

            bUpdateAllCellWidgets = false;
        }

        protected void removeAllCellWidgets()
        {
            cellWidgetContainer.removeAllChildren();
        }

        protected DialogLayout.Gap getColumnMPM(int column)
        {
            if (tableBaseThemeInfo != null)
            {
                ParameterMap columnWidthMap = tableBaseThemeInfo.getParameterMap("columnWidths");
                Object obj = columnWidthMap.getParameterValue(column.ToString(), false);
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

        protected ColumnHeader createColumnHeader(int column)
        {
            ColumnHeader btn = new ColumnHeader(this);
            btn.setTheme("columnHeader");
            btn.setCanAcceptKeyboardFocus(false);
            base.insertChild(btn, base.getNumChildren());
            return btn;
        }

        protected void updateColumnHeader(int column)
        {
            Button columnHeader = columnHeaders[column];
            columnHeader.setText(columnHeaderModel.ColumnHeaderTextFor(column));
            StateKey[] states = columnHeaderModel.ColumnHeaderStates;
            if (states.Length > 0)
            {
                AnimationState animationState = columnHeader.getAnimationState();
                for (int i = 0; i < states.Length; i++)
                {
                    animationState.setAnimationState(states[i],
                            columnHeaderModel.ColumnHeaderStateFor(column, i));
                }
            }
        }

        protected virtual void updateColumnHeaderNumbers()
        {
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                columnHeaders[i].column = i;
            }
        }

        private void removeColumnHeaders(int column, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int idx = base.getChildIndex(columnHeaders[column + i]);
                if (idx >= 0)
                {
                    base.removeChild(idx);
                }
            }
        }

        protected bool isMouseInColumnHeader(int y)
        {
            y -= getInnerY();
            return y >= 0 && y < columnHeaderHeight;
        }

        protected int getColumnSeparatorUnderMouse(int x)
        {
            x -= getOffsetX();
            x += columnDividerDragableDistance;
            int col = columnModel.getIndex(x);
            int dist = x - columnModel.getPosition(col);
            if (dist < 2 * columnDividerDragableDistance)
            {
                return col - 1;
            }
            return -1;
        }

        protected int getRowUnderMouse(int y)
        {
            y -= getOffsetY();
            int row = getRowFromPosition(y);
            return row;
        }

        protected int getColumnUnderMouse(int x)
        {
            x -= getOffsetX();
            int col = columnModel.getIndex(x);
            return col;
        }

        public override bool handleEvent(Event evt)
        {
            if (dragActive != DRAG_INACTIVE)
            {
                return handleDragEvent(evt);
            }

            if (evt.isKeyEvent() &&
                    keyboardSearchHandler != null &&
                    keyboardSearchHandler.isActive() &&
                    keyboardSearchHandler.handleKeyEvent(evt))
            {
                return true;
            }

            if (base.handleEvent(evt))
            {
                return true;
            }

            if (evt.isMouseEvent())
            {
                return handleMouseEvent(evt);
            }

            if (evt.isKeyEvent() &&
                    keyboardSearchHandler != null &&
                    keyboardSearchHandler.handleKeyEvent(evt))
            {
                return true;
            }

            return false;
        }

        protected override bool handleKeyStrokeAction(String action, Event evt)
        {
            if (!base.handleKeyStrokeAction(action, evt))
            {
                if (selectionManager == null)
                {
                    return false;
                }
                if (!selectionManager.handleKeyStrokeAction(action, evt))
                {
                    return false;
                }
            }
            // remove focus from childs
            requestKeyboardFocus(null);
            return true;
        }

        protected const int DRAG_INACTIVE = 0;
        protected const int DRAG_COLUMN_HEADER = 1;
        protected const int DRAG_USER = 2;
        protected const int DRAG_IGNORE = 3;

        protected int dragActive;
        protected int dragColumn;
        protected int dragStartX;
        protected int dragStartColWidth;
        protected int dragStartSumWidth;
        protected MouseCursor dragCursor;

        protected void cancelDragging()
        {
            if (dragActive == DRAG_USER)
            {
                if (dragListener != null)
                {
                    dragListener.dragCanceled();
                }
                dragActive = DRAG_IGNORE;
            }
        }

        protected bool handleDragEvent(Event evt)
        {
            if (evt.isMouseEvent())
            {
                return handleMouseEvent(evt);
            }

            if (evt.isKeyPressedEvent() && evt.getKeyCode() == Event.KEY_ESCAPE)
            {
                switch (dragActive)
                {
                    case DRAG_USER:
                        cancelDragging();
                        break;
                    case DRAG_COLUMN_HEADER:
                        columnHeaderDragged(dragStartColWidth);
                        dragActive = DRAG_IGNORE;
                        break;
                }
                dragCursor = null;
            }

            return true;
        }

        void mouseLeftTableArea()
        {
            lastMouseY = LAST_MOUSE_Y_OUTSIDE;
            lastMouseRow = -1;
            lastMouseColumn = -1;
        }

        internal override Widget routeMouseEvent(Event evt)
        {
            if (evt.getEventType() == Event.EventType.MOUSE_EXITED)
            {
                mouseLeftTableArea();
            }
            else
            {
                lastMouseY = evt.getMouseY();
            }

            if (dragActive == DRAG_INACTIVE)
            {
                bool inHeader = isMouseInColumnHeader(evt.getMouseY());
                if (inHeader)
                {
                    if (lastMouseRow != -1 || lastMouseColumn != -1)
                    {
                        lastMouseRow = -1;
                        lastMouseColumn = -1;
                        resetTooltip();
                    }
                }
                else
                {
                    int row = getRowUnderMouse(evt.getMouseY());
                    int column = getColumnUnderMouse(evt.getMouseX());

                    if (lastMouseRow != row || lastMouseColumn != column)
                    {
                        lastMouseRow = row;
                        lastMouseColumn = column;
                        resetTooltip();
                    }
                }
            }

            return base.routeMouseEvent(evt);
        }

        protected bool handleMouseEvent(Event evt)
        {
            Event.EventType evtType = evt.getEventType();

            if (dragActive != DRAG_INACTIVE)
            {
                switch (dragActive)
                {
                    case DRAG_COLUMN_HEADER:
                        {
                            int innerWidth = getInnerWidth();
                            if (dragColumn >= 0 && innerWidth > 0)
                            {
                                int newWidth = clampColumnWidth(evt.getMouseX() - dragStartX);
                                columnHeaderDragged(newWidth);
                            }
                            break;
                        }
                    case DRAG_USER:
                        {
                            dragCursor = dragListener.dragged(evt);
                            if (evt.isMouseDragEnd())
                            {
                                dragListener.dragStopped(evt);
                            }
                            break;
                        }
                    case DRAG_IGNORE:
                        break;
                    default:
                        throw new Exception("Assertion error");
                }
                if (evt.isMouseDragEnd())
                {
                    dragActive = DRAG_INACTIVE;
                    dragCursor = null;
                }
                return true;
            }

            bool inHeader = isMouseInColumnHeader(evt.getMouseY());
            if (inHeader)
            {
                int column = getColumnSeparatorUnderMouse(evt.getMouseX());
                bool fixedWidthMode = isFixedWidthMode();

                // lastMouseRow and lastMouseColumn have been updated in routeMouseEvent()

                if (column >= 0 && (column < getNumColumns() - 1 || !fixedWidthMode))
                {
                    if (evtType == Event.EventType.MOUSE_BTNDOWN)
                    {
                        dragStartColWidth = getColumnWidth(column);
                        dragColumn = column;
                        dragStartX = evt.getMouseX() - dragStartColWidth;
                        if (fixedWidthMode)
                        {
                            for (int i = 0; i < numColumns; ++i)
                            {
                                columnHeaders[i].setColumnWidth(getColumnWidth(i));
                            }
                            dragStartSumWidth = dragStartColWidth + getColumnWidth(column + 1);
                        }
                    }

                    if (evt.isMouseDragEvent())
                    {
                        dragActive = DRAG_COLUMN_HEADER;
                    }
                    return true;
                }
            }
            else
            {
                // lastMouseRow and lastMouseColumn have been updated in routeMouseEvent()
                int row = lastMouseRow;
                int column = lastMouseColumn;

                if (evt.isMouseDragEvent())
                {
                    if (dragListener != null && dragListener.dragStarted(row, row, evt))
                    {
                        dragCursor = dragListener.dragged(evt);
                        dragActive = DRAG_USER;
                    }
                    else
                    {
                        dragActive = DRAG_IGNORE;
                    }
                    return true;
                }

                if (selectionManager != null)
                {
                    selectionManager.handleMouseEvent(row, column, evt);
                }

                if (evtType == Event.EventType.MOUSE_CLICKED && evt.getMouseClickCount() == 2)
                {
                    if (this.DoubleClick != null)
                    {
                        this.DoubleClick.Invoke(this, new TableDoubleClickEventArgs(row, column));
                    }
                }

                if (evtType == Event.EventType.MOUSE_BTNUP && evt.getMouseButton() == Event.MOUSE_RBUTTON)
                {
                    if (this.RightClick != null)
                    {
                        this.RightClick.Invoke(this, new TableRightClickEventArgs(row, column, evt));
                    }
                }
            }

            // let ScrollPane handle mouse wheel
            return evtType != Event.EventType.MOUSE_WHEEL;
        }

        public override MouseCursor getMouseCursor(Event evt)
        {
            switch (dragActive)
            {
                case DRAG_COLUMN_HEADER:
                    return columnResizeCursor;
                case DRAG_USER:
                    return dragCursor;
                case DRAG_IGNORE:
                    return dragNotPossibleCursor;
            }

            bool inHeader = isMouseInColumnHeader(evt.getMouseY());
            if (inHeader)
            {
                int column = getColumnSeparatorUnderMouse(evt.getMouseX());
                bool fixedWidthMode = isFixedWidthMode();

                // lastMouseRow and lastMouseColumn have been updated in routeMouseEvent()

                if (column >= 0 && (column < getNumColumns() - 1 || !fixedWidthMode))
                {
                    return columnResizeCursor;
                }
            }

            return normalCursor;
        }

        private void columnHeaderDragged(int newWidth)
        {
            if (isFixedWidthMode())
            {
                System.Diagnostics.Debug.Assert(dragColumn+1 < numColumns);
                newWidth = Math.Min(newWidth, dragStartSumWidth - 2 * columnDividerDragableDistance);
                columnHeaders[dragColumn].setColumnWidth(newWidth);
                columnHeaders[dragColumn + 1].setColumnWidth(dragStartSumWidth - newWidth);
                bUpdateAllColumnWidth = true;
                invalidateLayout();
            }
            else
            {
                setColumnWidth(dragColumn, newWidth);
            }
        }

        protected virtual void columnHeaderClicked(int column)
        {
            this.ColumnHeaderClick.Invoke(this, new TableColumnHeaderClickEventArgs(column));
        }

        protected void updateAllColumnWidth()
        {
            if (getInnerWidth() > 0)
            {
                columnModel.initializeAll(numColumns);
                bUpdateAllColumnWidth = false;
            }
        }

        protected void updateAll()
        {
            if (!widgetGrid.isEmpty())
            {
                removeAllCellWidgets();
                widgetGrid.clear();
            }

            if (rowModel != null)
            {
                bAutoSizeAllRows = true;
            }

            bUpdateAllCellWidgets = true;
            bUpdateAllColumnWidth = true;
            invalidateLayout();
        }

        protected void modelAllChanged()
        {
            if (columnHeaders != null)
            {
                removeColumnHeaders(0, columnHeaders.Length);
            }

            dropMarkerRow = -1;
            columnHeaders = new ColumnHeader[numColumns];
            for (int i = 0; i < numColumns; i++)
            {
                columnHeaders[i] = createColumnHeader(i);
                updateColumnHeader(i);
            }
            updateColumnHeaderNumbers();

            if (selectionManager != null)
            {
                selectionManager.modelChanged();
            }

            updateAll();
        }

        protected void modelRowChanged(int row)
        {
            if (rowModel != null)
            {
                if (autoSizeRow(row))
                {
                    invalidateLayout();
                }
            }
            for (int col = 0; col < numColumns; col++)
            {
                updateCellWidget(row, col);
            }
            invalidateLayoutLocally();
        }

        protected void modelRowsChanged(int idx, int count)
        {
            checkRowRange(idx, count);
            bool rowHeightChanged = false;
            for (int i = 0; i < count; i++)
            {
                if (rowModel != null)
                {
                    rowHeightChanged |= autoSizeRow(idx + i);
                }
                for (int col = 0; col < numColumns; col++)
                {
                    updateCellWidget(idx + i, col);
                }
            }
            invalidateLayoutLocally();
            if (rowHeightChanged)
            {
                invalidateLayout();
            }
        }

        protected void modelCellChanged(int row, int column)
        {
            checkRowIndex(row);
            checkColumnIndex(column);
            if (rowModel != null)
            {
                autoSizeRow(row);
            }
            updateCellWidget(row, column);
            invalidateLayout();
        }

        protected void modelRowsInserted(int row, int count)
        {
            checkRowRange(row, count);
            if (rowModel != null)
            {
                rowModel.insert(row, count);
            }
            if (dropMarkerRow > row || (dropMarkerRow == row && dropMarkerBeforeRow))
            {
                dropMarkerRow += count;
            }
            if (!widgetGrid.isEmpty() || hasCellWidgetCreators)
            {
                removeAllCellWidgets();
                widgetGrid.insertRows(row, count);

                for (int i = 0; i < count; i++)
                {
                    for (int col = 0; col < numColumns; col++)
                    {
                        updateCellWidget(row + i, col);
                    }
                }
            }
            // invalidateLayout() before sp.setScrollPositionY() as this may cause a
            // call to invalidateLayoutLocally() which is redundant.
            invalidateLayout();
            if (row < getRowFromPosition(scrollPosY))
            {
                ScrollPane sp = ScrollPane.getContainingScrollPane(this);
                if (sp != null)
                {
                    int rowsStart = getRowStartPosition(row);
                    int rowsEnd = getRowEndPosition(row + count - 1);
                    sp.setScrollPositionY(scrollPosY + rowsEnd - rowsStart);
                }
            }
            if (selectionManager != null)
            {
                selectionManager.rowsInserted(row, count);
            }
        }

        protected void modelRowsDeleted(int row, int count)
        {
            if (row + count <= getRowFromPosition(scrollPosY))
            {
                ScrollPane sp = ScrollPane.getContainingScrollPane(this);
                if (sp != null)
                {
                    int rowsStart = getRowStartPosition(row);
                    int rowsEnd = getRowEndPosition(row + count - 1);
                    sp.setScrollPositionY(scrollPosY - rowsEnd + rowsStart);
                }
            }
            if (rowModel != null)
            {
                rowModel.remove(row, count);
            }
            if (dropMarkerRow >= row)
            {
                if (dropMarkerRow < (row + count))
                {
                    dropMarkerRow = -1;
                }
                else
                {
                    dropMarkerRow -= count;
                }
            }
            if (!widgetGrid.isEmpty())
            {
                widgetGrid.iterate(row, 0, row + count - 1, numColumns, removeCellWidgetsFunction);
                widgetGrid.removeRows(row, count);
            }
            if (selectionManager != null)
            {
                selectionManager.rowsDeleted(row, count);
            }
            invalidateLayout();
        }

        protected void modelColumnsInserted(int column, int count)
        {
            checkColumnRange(column, count);
            ColumnHeader[] newColumnHeaders = new ColumnHeader[numColumns];
            Array.Copy(columnHeaders, 0, newColumnHeaders, 0, column);
            Array.Copy(columnHeaders, column, newColumnHeaders, column + count,
                    numColumns - (column + count));
            for (int i = 0; i < count; i++)
            {
                newColumnHeaders[column + i] = createColumnHeader(column + i);
            }
            columnHeaders = newColumnHeaders;
            updateColumnHeaderNumbers();

            columnModel.insert(column, count);

            if (!widgetGrid.isEmpty() || hasCellWidgetCreators)
            {
                removeAllCellWidgets();
                widgetGrid.insertColumns(column, count);

                for (int row = 0; row < numRows; row++)
                {
                    for (int i = 0; i < count; i++)
                    {
                        updateCellWidget(row, column + i);
                    }
                }
            }
            if (column < getColumnStartPosition(scrollPosX))
            {
                ScrollPane sp = ScrollPane.getContainingScrollPane(this);
                if (sp != null)
                {
                    int columnsStart = getColumnStartPosition(column);
                    int columnsEnd = getColumnEndPosition(column + count - 1);
                    sp.setScrollPositionX(scrollPosX + columnsEnd - columnsStart);
                }
            }
            invalidateLayout();
        }

        protected void modelColumnsDeleted(int column, int count)
        {
            if (column + count <= getColumnStartPosition(scrollPosX))
            {
                ScrollPane sp = ScrollPane.getContainingScrollPane(this);
                if (sp != null)
                {
                    int columnsStart = getColumnStartPosition(column);
                    int columnsEnd = getColumnEndPosition(column + count - 1);
                    sp.setScrollPositionY(scrollPosX - columnsEnd + columnsStart);
                }
            }
            columnModel.remove(column, count);
            if (!widgetGrid.isEmpty())
            {
                widgetGrid.iterate(0, column, numRows, column + count - 1, removeCellWidgetsFunction);
                widgetGrid.removeColumns(column, count);
            }

            removeColumnHeaders(column, count);

            ColumnHeader[] newColumnHeaders = new ColumnHeader[numColumns];
            Array.Copy(columnHeaders, 0, newColumnHeaders, 0, column);
            Array.Copy(columnHeaders, column + count, newColumnHeaders, column, numColumns - count);
            columnHeaders = newColumnHeaders;
            updateColumnHeaderNumbers();

            invalidateLayout();
        }

        protected void modelColumnHeaderChanged(int column)
        {
            checkColumnIndex(column);
            updateColumnHeader(column);
        }

        class RowSizeSequence : SizeSequence
        {
            private TableBase tableBase;
            public RowSizeSequence(TableBase tableBase, int initialCapacity) : base(initialCapacity)
            {
                this.tableBase = tableBase;
            }

            protected internal override void initializeSizes(int index, int count)
            {
                for (int i = 0; i < count; i++, index++)
                {
                    table[index] = this.tableBase.computeRowHeight(index);
                }
            }
        }

        protected class ColumnSizeSequence : SizeSequence
        {
            private TableBase tableBase;
            public ColumnSizeSequence(TableBase tableBase)
            {
                this.tableBase = tableBase;
            }
            protected internal override void initializeSizes(int index, int count)
            {
                bool useSprings = this.tableBase.isFixedWidthMode();
                if (!useSprings)
                {
                    int sum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int width = this.tableBase.computePreferredColumnWidth(index + i);
                        table[index + i] = width;
                        sum += width;
                    }
                    useSprings = sum < this.tableBase.getInnerWidth();
                }
                if (useSprings)
                {
                    computeColumnHeaderLayout();
                    for (int i = 0; i < count; i++)
                    {
                        table[index + i] = this.tableBase.clampColumnWidth(this.tableBase.columnHeaders[i].springWidth);
                    }
                }
            }
            protected internal bool update(int index)
            {
                int width;
                if (this.tableBase.isFixedWidthMode())
                {
                    computeColumnHeaderLayout();
                    width = this.tableBase.clampColumnWidth(this.tableBase.columnHeaders[index].springWidth);
                }
                else
                {
                    width = this.tableBase.computePreferredColumnWidth(index);
                    if (this.tableBase.ensureColumnHeaderMinWidth)
                    {
                        width = Math.Max(width, this.tableBase.columnHeaders[index].getMinWidth());
                    }
                }
                return setSize(index, width);
            }
            void computeColumnHeaderLayout()
            {
                if (this.tableBase.columnHeaders != null)
                {
                    DialogLayout.SequentialGroup g = (DialogLayout.SequentialGroup)(new DialogLayout()).createSequentialGroup();
                    foreach (ColumnHeader h in this.tableBase.columnHeaders)
                    {
                        g.addSpring(h.spring);
                    }
                    g.setSize(DialogLayout.AXIS_X, 0, this.tableBase.getInnerWidth());
                }
            }
            protected internal int computePreferredWidth()
            {
                int count = this.tableBase.getNumColumns();
                if (!this.tableBase.isFixedWidthMode())
                {
                    int sum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int width = this.tableBase.computePreferredColumnWidth(i);
                        sum += width;
                    }
                    return sum;
                }
                if (this.tableBase.columnHeaders != null)
                {
                    DialogLayout.SequentialGroup g = (DialogLayout.SequentialGroup)(new DialogLayout()).createSequentialGroup();
                    foreach (ColumnHeader h in this.tableBase.columnHeaders)
                    {
                        g.addSpring(h.spring);
                    }
                    return g.getPrefSize(DialogLayout.AXIS_X);
                }
                return 0;
            }
        }

        class RemoveCellWidgets : SparseGrid.GridFunction
        {
            private TableBase tableBase;
            public RemoveCellWidgets(TableBase tableBase)
            {
                this.tableBase = tableBase;
            }
            public void apply(int row, int column, SparseGrid.Entry e)
            {
                WidgetEntry widgetEntry = (WidgetEntry)e;
                Widget widget = widgetEntry.widget;
                if (widget != null)
                {
                    this.tableBase.removeCellWidget(widget);
                }
            }
        }

        class InsertCellWidgets : SparseGrid.GridFunction
        {
            private TableBase tableBase;
            public InsertCellWidgets(TableBase tableBase)
            {
                this.tableBase = tableBase;
            }
            public void apply(int row, int column, SparseGrid.Entry e)
            {
                this.tableBase.insertCellWidget(row, column, (WidgetEntry)e);
            }
        }

        protected class ColumnHeader : Button
        {
            protected internal int column;
            private int columnWidth;
            protected internal int springWidth;
            private TableBase tableBase;
            protected internal DialogLayout.Spring spring;

            public ColumnHeader(TableBase tableBase)
            {
                this.tableBase = tableBase;
                this.Action += (sender, e) =>
                {
                    this.tableBase.columnHeaderClicked(column);
                };
                this.spring = new ColumnHeaderSpring(this);
            }

            class ColumnHeaderSpring : DialogLayout.Spring
            {
                public ColumnHeader columnHeader;
                public ColumnHeaderSpring(ColumnHeader columnHeader)
                {
                    this.columnHeader = columnHeader;
                }

                internal override int getMinSize(int axis)
                {
                    return this.columnHeader.tableBase.clampColumnWidth(this.columnHeader.getMinWidth());
                }

                internal override int getPrefSize(int axis)
                {
                    return this.columnHeader.getPreferredWidth();
                }

                internal override int getMaxSize(int axis)
                {
                    return this.columnHeader.getMaxWidth();
                }

                internal override void setSize(int axis, int pos, int size)
                {
                    this.columnHeader.springWidth = size;
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

            public int getColumnWidth()
            {
                return columnWidth;
            }

            public void setColumnWidth(int columnWidth)
            {
                this.columnWidth = columnWidth;
            }

            public override int getPreferredWidth()
            {
                if (columnWidth > 0)
                {
                    return columnWidth;
                }
                DialogLayout.Gap mpm = this.tableBase.getColumnMPM(column);
                int prefWidth = (mpm != null) ? mpm.preferred : this.tableBase.defaultColumnWidth;
                return Math.Max(prefWidth, base.getPreferredWidth());
            }

            public override int getMinWidth()
            {
                DialogLayout.Gap mpm = this.tableBase.getColumnMPM(column);
                int minWidth = (mpm != null) ? mpm.min : 0;
                return Math.Max(minWidth, base.getPreferredWidth());
            }

            public override int getMaxWidth()
            {
                DialogLayout.Gap mpm = this.tableBase.getColumnMPM(column);
                int maxWidth = (mpm != null) ? mpm.max : 32767;
                return maxWidth;
            }

            public override void adjustSize()
            {
                // don't do anything
            }

            public override bool handleEvent(Event evt)
            {
                if (evt.isMouseEventNoWheel())
                {
                    this.tableBase.mouseLeftTableArea();
                }
                return base.handleEvent(evt);
            }

            protected override void paintWidget(GUI gui)
            {
                Renderer.Renderer renderer = gui.getRenderer();
                renderer.ClipEnter(getX(), getY(), getWidth(), getHeight());
                try
                {
                    paintLabelText(getAnimationState());
                }
                finally
                {
                    renderer.ClipLeave();
                }
            }

            public void run()
            {
                this.tableBase.columnHeaderClicked(column);
            }
        }

        class WidgetEntry : SparseGrid.Entry
        {
            protected internal Widget widget;
            protected internal CellWidgetCreator creator;
        }

        protected internal class CellWidgetContainer : Widget
        {
            protected internal CellWidgetContainer()
            {
                setTheme("");
                setClip(true);
            }

            protected override void childInvalidateLayout(Widget child)
            {
                // always ignore
            }

            protected override void sizeChanged()
            {
                // always ignore
            }

            protected override void childAdded(Widget child)
            {
                // always ignore
            }

            protected override void childRemoved(Widget exChild)
            {
                // always ignore
            }

            protected override void allChildrenRemoved()
            {
                // always ignore
            }
        }

        public class StringCellRenderer : TextWidget, CellRenderer
        {
            public StringCellRenderer()
            {
                setCache(false);
                setClip(true);
            }

            public void applyTheme(ThemeInfo themeInfo)
            {
                base.applyTheme(themeInfo);
            }

            public void setCellData(int row, int column, Object data)
            {
                setCharSequence(data.ToString());
            }

            public int getColumnSpan()
            {
                return 1;
            }

            protected override void sizeChanged()
            {
                // this method is overriden to prevent Widget.sizeChanged() from
                // calling invalidateLayout().
                // StringCellRenderer is used as a stamp and does not participate
                // in layouts - so invalidating the layout would lead to many
                // or even constant relayouts and bad performance
            }

            public Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                setPosition(x, y);
                setSize(width, height);
                getAnimationState().setAnimationState(STATE_SELECTED, isSelected);
                return this;
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
