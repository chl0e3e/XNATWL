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
    public enum ListBoxCallbackReason
    {
        MODEL_CHANGED,
        SET_SELECTED,
        MOUSE_CLICK,
        MOUSE_DOUBLE_CLICK,
        KEYBOARD,
        KEYBOARD_RETURN
    }

    public class ListBox<T> : Widget
    {
        public static int NO_SELECTION = int.MinValue;
        public static int DEFAULT_CELL_HEIGHT = 20;
        public static int SINGLE_COLUMN = -1;

        public static bool CallbackReason_ActionRequested(ListBoxCallbackReason listBoxCallbackReason)
        {
            if (listBoxCallbackReason == ListBoxCallbackReason.MOUSE_DOUBLE_CLICK || listBoxCallbackReason == ListBoxCallbackReason.KEYBOARD_RETURN)
            {
                return true;
            }

            return false;
        }

        private static ListBoxDisplay[] EMPTY_LABELS = { };

        private Scrollbar scrollbar;
        private ListBoxDisplay[] labels;
        private ListModel<T> model;
        private IntegerModel selectionModel;
        private Runnable selectionModelCallback;
        private int cellHeight = DEFAULT_CELL_HEIGHT;
        private int cellWidth = SINGLE_COLUMN;
        private bool rowMajor = true;
        private bool fixedCellWidth;
        private bool fixedCellHeight;
        private int minDisplayedRows = 1;

        private int numCols = 1;
        private int firstVisible;
        private int selected = NO_SELECTION;
        private int numEntries;
        private bool needUpdate;
        private bool inSetSelected;

        public event EventHandler<ListBoxEventArgs> Callback;
        //private CallbackWithReason<?>[] callbacks;

        public ListBox()
        {
            scrollbar = new Scrollbar();
            scrollbar.PositionChanged += Scrollbar_PositionChanged;
            labels = EMPTY_LABELS;

            base.insertChild(scrollbar, 0);

            setSize(200, 300);
            setCanAcceptKeyboardFocus(true);
            setDepthFocusTraversal(false);
        }

        private void Scrollbar_PositionChanged(object sender, ScrollbarChangedPositionEventArgs e)
        {
            this.scrollbarChanged();
        }

        public ListBox(ListModel<T> model) : this()
        {
            setModel(model);
        }

        public ListBox(ListSelectionModel<T> model) : this()
        {
            setModel(model);
        }

        public ListModel<T> getModel()
        {
            return model;
        }

        public void setModel(ListModel<T> model)
        {
            if (this.model != model)
            {
                if (this.model != null)
                {
                    this.model.AllChanged -= Model_AllChanged;
                    this.model.EntriesChanged -= Model_EntriesChanged;
                    this.model.EntriesDeleted -= Model_EntriesDeleted;
                    this.model.EntriesInserted -= Model_EntriesInserted;
                }

                this.model = model;

                if (model != null)
                {
                    this.model.AllChanged += Model_AllChanged;
                    this.model.EntriesChanged += Model_EntriesChanged;
                    this.model.EntriesDeleted += Model_EntriesDeleted;
                    this.model.EntriesInserted += Model_EntriesInserted;
                }

                this.allChanged();
            }
        }

        private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            this.entriesInserted(e.First, e.Last);
        }

        private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            this.entriesDeleted(e.First, e.Last);
        }

        private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            this.entriesChanged(e.First, e.Last);
        }

        private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            this.allChanged();
        }

        public IntegerModel getSelectionModel()
        {
            return selectionModel;
        }

        public void setSelectionModel(IntegerModel selectionModel)
        {
            if (this.selectionModel != selectionModel)
            {
                if (this.selectionModel != null)
                {
                    this.selectionModel.Changed -= SelectionModel_Changed;
                }
                this.selectionModel = selectionModel;
                if (selectionModel != null)
                {
                    this.selectionModel.Changed += SelectionModel_Changed;
                    syncSelectionFromModel();
                }
            }
        }

        private void SelectionModel_Changed(object sender, IntegerChangedEventArgs e)
        {
            this.syncSelectionFromModel();
        }

        public void setModel(ListSelectionModel<T> model)
        {
            setSelectionModel(null);
            if (model == null)
            {
                setModel((ListModel<T>)null);
            }
            else
            {
                setModel(model.Model);
                setSelectionModel(model);
            }
        }

        private void doCallback(ListBoxCallbackReason reason)
        {
            if (this.Callback != null)
            {
                this.Callback.Invoke(this, new ListBoxEventArgs(reason));
            }
        }

        public int getCellHeight()
        {
            return cellHeight;
        }

        public void setCellHeight(int cellHeight)
        {
            if (cellHeight < 1)
            {
                throw new ArgumentOutOfRangeException("cellHeight < 1");
            }
            this.cellHeight = cellHeight;
        }

        public int getCellWidth()
        {
            return cellWidth;
        }

        public void setCellWidth(int cellWidth)
        {
            if (cellWidth < 1 && cellWidth != SINGLE_COLUMN)
            {
                throw new ArgumentOutOfRangeException("cellWidth < 1");
            }
            this.cellWidth = cellWidth;
        }

        public bool isFixedCellHeight()
        {
            return fixedCellHeight;
        }

        public void setFixedCellHeight(bool fixedCellHeight)
        {
            this.fixedCellHeight = fixedCellHeight;
        }

        public bool isFixedCellWidth()
        {
            return fixedCellWidth;
        }

        public void setFixedCellWidth(bool fixedCellWidth)
        {
            this.fixedCellWidth = fixedCellWidth;
        }

        public bool isRowMajor()
        {
            return rowMajor;
        }

        public void setRowMajor(bool rowMajor)
        {
            this.rowMajor = rowMajor;
        }

        public int getFirstVisible()
        {
            return firstVisible;
        }

        public int getLastVisible()
        {
            return getFirstVisible() + labels.Length - 1;
        }

        public void setFirstVisible(int firstVisible)
        {
            firstVisible = Math.Max(0, Math.Min(firstVisible, numEntries - 1));
            if (this.firstVisible != firstVisible)
            {
                this.firstVisible = firstVisible;
                scrollbar.setValue(firstVisible / numCols, false);
                needUpdate = true;
            }
        }

        public int getSelected()
        {
            return selected;
        }

        /**
         * Selects the specified entry and scrolls to make it visible
         *
         * @param selected the index or {@link #NO_SELECTION}
         * @throws IllegalArgumentException if index is invalid
         * @see #setSelected(int, bool)
         */
        public void setSelected(int selected)
        {
            setSelected(selected, true, ListBoxCallbackReason.SET_SELECTED);
        }

        /**
         * Selects the specified entry and optionally scrolls to that entry
         *
         * @param selected the index or {@link #NO_SELECTION}
         * @param scroll true if it should scroll to make the entry visible
         * @throws IllegalArgumentException if index is invalid
         */
        public void setSelected(int selected, bool scroll)
        {
            setSelected(selected, scroll, ListBoxCallbackReason.SET_SELECTED);
        }

        void setSelected(int selected, bool scroll, ListBoxCallbackReason reason)
        {
            if (selected < NO_SELECTION || selected >= numEntries)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (scroll)
            {
                validateLayout();
                if (selected == NO_SELECTION)
                {
                    setFirstVisible(0);
                }
                else
                {
                    int delta = getFirstVisible() - selected;
                    if (delta > 0)
                    {
                        int deltaRows = (delta + numCols - 1) / numCols;
                        setFirstVisible(getFirstVisible() - deltaRows * numCols);
                    }
                    else
                    {
                        delta = selected - getLastVisible();
                        if (delta > 0)
                        {
                            int deltaRows = (delta + numCols - 1) / numCols;
                            setFirstVisible(getFirstVisible() + deltaRows * numCols);
                        }
                    }
                }
            }
            if (this.selected != selected)
            {
                this.selected = selected;
                if (selectionModel != null)
                {
                    try
                    {
                        inSetSelected = true;
                        selectionModel.Value = selected;
                    }
                    finally
                    {
                        inSetSelected = false;
                    }
                }
                needUpdate = true;
                doCallback(reason);
            }
            else if (CallbackReason_ActionRequested(reason) || reason == ListBoxCallbackReason.MOUSE_CLICK)
            {
                doCallback(reason);
            }
        }

        public void scrollToSelected()
        {
            setSelected(selected, true, ListBoxCallbackReason.SET_SELECTED);
        }

        public int getNumEntries()
        {
            return numEntries;
        }

        public int getNumRows()
        {
            return (numEntries + numCols - 1) / numCols;
        }

        public int getNumColumns()
        {
            return numCols;
        }

        public int findEntryByName(String prefix)
        {
            for (int i = selected + 1; i < numEntries; i++)
            {
                if (model.EntryMatchesPrefix(i, prefix))
                {
                    return i;
                }
            }
            for (int i = 0; i < selected; i++)
            {
                if (model.EntryMatchesPrefix(i, prefix))
                {
                    return i;
                }
            }
            return NO_SELECTION;
        }

        /**
         * The method always return this.
         * Use getEntryAt(x, y) to locate the listbox entry at the specific coordinates.
         * 
         * @param x the x coordinate
         * @param y the y coordinate
         * @return this.
         */
        //@Override
        public override Widget getWidgetAt(int x, int y)
        {
            return this;
        }

        /**
         * Returns the entry at the specific coordinates or -1 if there is no entry.
         * 
         * @param x the x coordinate
         * @param y the y coordinate
         * @return the index of the entry or -1.
         */
        public int getEntryAt(int x, int y)
        {
            int n = Math.Max(labels.Length, numEntries - firstVisible);
            for (int i = 0; i < n; i++)
            {
                if (labels[i].getWidget().isInside(x, y))
                {
                    return firstVisible + i;
                }
            }
            return -1;
        }

        public override void insertChild(Widget child, int index)
        {
            throw new InvalidOperationException();
        }

        public override void removeAllChildren()
        {
            throw new InvalidOperationException();
        }

        public override Widget removeChild(int index)
        {
            throw new InvalidOperationException();
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            setCellHeight(themeInfo.GetParameter("cellHeight", DEFAULT_CELL_HEIGHT));
            setCellWidth(themeInfo.GetParameter("cellWidth", SINGLE_COLUMN));
            setRowMajor(themeInfo.GetParameter("rowMajor", true));
            setFixedCellWidth(themeInfo.GetParameter("fixedCellWidth", false));
            setFixedCellHeight(themeInfo.GetParameter("fixedCellHeight", false));
            minDisplayedRows = themeInfo.GetParameter("minDisplayedRows", 1);
        }

        protected void goKeyboard(int dir)
        {
            int newPos = selected + dir;
            if (newPos >= 0 && newPos < numEntries)
            {
                setSelected(newPos, true, ListBoxCallbackReason.KEYBOARD);
            }
        }

        protected bool isSearchChar(char ch)
        {
            return (ch != Event.CHAR_NONE) && Char.IsLetterOrDigit(ch);
        }

        protected override void keyboardFocusGained()
        {
            setLabelFocused(true);
        }

        protected override void keyboardFocusLost()
        {
            setLabelFocused(false);
        }

        private void setLabelFocused(bool focused)
        {
            int idx = selected - firstVisible;
            if (idx >= 0 && idx < labels.Length)
            {
                labels[idx].setFocused(focused);
            }
        }

        public override bool handleEvent(Event evt)
        {
            if (evt.getEventType() == EventType.MOUSE_WHEEL)
            {
                scrollbar.scroll(-evt.getMouseWheelDelta());
                return true;
            }
            else
            if (evt.getEventType() == EventType.KEY_PRESSED)
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_UP:
                        goKeyboard(-numCols);
                        break;
                    case Event.KEY_DOWN:
                        goKeyboard(numCols);
                        break;
                    case Event.KEY_LEFT:
                        goKeyboard(-1);
                        break;
                    case Event.KEY_RIGHT:
                        goKeyboard(1);
                        break;
                    case Event.KEY_PRIOR:
                        if (numEntries > 0)
                        {
                            setSelected(Math.Max(0, selected - labels.Length),
                                true, ListBoxCallbackReason.KEYBOARD);
                        }
                        break;
                    case Event.KEY_NEXT:
                        setSelected(Math.Min(numEntries - 1, selected + labels.Length),
                                true, ListBoxCallbackReason.KEYBOARD);
                        break;
                    case Event.KEY_HOME:
                        if (numEntries > 0)
                        {
                            setSelected(0, true, ListBoxCallbackReason.KEYBOARD);
                        }
                        break;
                    case Event.KEY_END:
                        setSelected(numEntries - 1, true, ListBoxCallbackReason.KEYBOARD);
                        break;
                    case Event.KEY_RETURN:
                        setSelected(selected, false, ListBoxCallbackReason.KEYBOARD_RETURN);
                        break;
                    default:
                        if (evt.hasKeyChar() && isSearchChar(evt.getKeyChar()))
                        {
                            int idx = findEntryByName(evt.getKeyChar().ToString());
                            if (idx != NO_SELECTION)
                            {
                                setSelected(idx, true, ListBoxCallbackReason.KEYBOARD);
                            }
                            return true;
                        }
                        return false;
                }
                return true;
            }
            else
            if (evt.getEventType() == EventType.KEY_RELEASED)
            {

                switch (evt.getKeyCode())
                {
                    case Event.KEY_UP:
                    case Event.KEY_DOWN:
                    case Event.KEY_LEFT:
                    case Event.KEY_RIGHT:
                    case Event.KEY_PRIOR:
                    case Event.KEY_NEXT:
                    case Event.KEY_HOME:
                    case Event.KEY_END:
                    case Event.KEY_RETURN:
                        return true;
                }
                return false;
            }

            // delegate to children (listbox, displays, etc...)
            if (base.handleEvent(evt))
            {
                return true;
            }
            // eat all mouse events
            return evt.isMouseEvent();
        }

        public override int getMinWidth()
        {
            return Math.Max(base.getMinWidth(), scrollbar.getMinWidth());
        }

        public override int getMinHeight()
        {
            int minHeight = Math.Max(base.getMinHeight(), scrollbar.getMinHeight());
            if (minDisplayedRows > 0)
            {
                minHeight = Math.Max(minHeight, getBorderVertical() +
                        Math.Min(numEntries, minDisplayedRows) * cellHeight);
            }
            return minHeight;
        }

        public override int getPreferredInnerWidth()
        {
            return Math.Max(base.getPreferredInnerWidth(), scrollbar.getPreferredWidth());
        }

        public override int getPreferredInnerHeight()
        {
            return Math.Max(getNumRows() * getCellHeight(), scrollbar.getPreferredHeight());
        }

        protected override void paint(GUI gui)
        {
            if (needUpdate)
            {
                updateDisplay();
            }
            // always update scrollbar
            int maxFirstVisibleRow = computeMaxFirstVisibleRow();
            scrollbar.setMinMaxValue(0, maxFirstVisibleRow);
            scrollbar.setValue(firstVisible / numCols, false);

            base.paint(gui);
        }

        private int computeMaxFirstVisibleRow()
        {
            int maxFirstVisibleRow = Math.Max(0, numEntries - labels.Length);
            maxFirstVisibleRow = (maxFirstVisibleRow + numCols - 1) / numCols;
            return maxFirstVisibleRow;
        }

        private void updateDisplay()
        {
            needUpdate = false;

            if (selected >= numEntries)
            {
                selected = NO_SELECTION;
            }

            int maxFirstVisibleRow = computeMaxFirstVisibleRow();
            int maxFirstVisible = maxFirstVisibleRow * numCols;
            if (firstVisible > maxFirstVisible)
            {
                firstVisible = Math.Max(0, maxFirstVisible);
            }

            bool hasFocus = hasKeyboardFocus();

            for (int i = 0; i < labels.Length; i++)
            {
                ListBoxDisplay label = labels[i];
                int cell = i + firstVisible;
                if (cell < numEntries)
                {
                    label.setData(model.EntryAt(cell));
                    label.setTooltipContent(model.EntryTooltipAt(cell));
                }
                else
                {
                    label.setData(null);
                    label.setTooltipContent(null);
                }
                label.setSelected(cell == selected);
                label.setFocused(cell == selected && hasFocus);
            }
        }

        protected override void layout()
        {
            scrollbar.setSize(scrollbar.getPreferredWidth(), getInnerHeight());
            scrollbar.setPosition(getInnerRight() - scrollbar.getWidth(), getInnerY());

            int numRows = Math.Max(1, getInnerHeight() / cellHeight);
            if (cellWidth != SINGLE_COLUMN)
            {
                numCols = Math.Max(1, (scrollbar.getX() - getInnerX()) / cellWidth);
            }
            else
            {
                numCols = 1;
            }
            setVisibleCells(numRows);

            needUpdate = true;
        }

        private void setVisibleCells(int numRows)
        {
            int visibleCells = numRows * numCols;
            System.Diagnostics.Debug.Assert(visibleCells >= 1);

            scrollbar.setPageSize(visibleCells);

            int curVisible = labels.Length;
            for (int i = curVisible; i-- > visibleCells;)
            {
                base.removeChild(1 + i);
            }

            ListBoxDisplay[] newLabels = new ListBoxDisplay[visibleCells];
            Array.Copy(labels, 0, newLabels, 0, Math.Min(visibleCells, labels.Length));
            labels = newLabels;

            for (int i = curVisible; i < visibleCells; i++)
            {
                int cellOffset = i;
                ListBoxDisplay lbd = createDisplay();
                lbd.Callback += (sender, e) =>
                {
                    int cell = getFirstVisible() + cellOffset;
                    if (cell < getNumEntries())
                    {
                        setSelected(cell, false, e.Reason);
                    }
                };
                base.insertChild(lbd.getWidget(), 1 + i);
                labels[i] = lbd;
            }

            int innerWidth = scrollbar.getX() - getInnerX();
            int innerHeight = getInnerHeight();
            for (int i = 0; i < visibleCells; i++)
            {
                int row, col;
                if (rowMajor)
                {
                    row = i / numCols;
                    col = i % numCols;
                }
                else
                {
                    row = i % numRows;
                    col = i / numRows;
                }
                int x, y, w, h;
                if (fixedCellHeight)
                {
                    y = row * cellHeight;
                    h = cellHeight;
                }
                else
                {
                    y = row * innerHeight / numRows;
                    h = (row + 1) * innerHeight / numRows - y;
                }
                if (fixedCellWidth && cellWidth != SINGLE_COLUMN)
                {
                    x = col * cellWidth;
                    w = cellWidth;
                }
                else
                {
                    x = col * innerWidth / numCols;
                    w = (col + 1) * innerWidth / numCols - x;
                }
                Widget cell = (Widget)labels[i];
                cell.setSize(Math.Max(0, w), Math.Max(0, h));
                cell.setPosition(x + getInnerX(), y + getInnerY());
            }
        }

        protected virtual ListBoxDisplay createDisplay()
        {
            return new ListBoxLabel();
        }

        protected internal class ListBoxLabel : TextWidget, ListBoxDisplay
        {
            public static StateKey STATE_SELECTED = StateKey.Get("selected");
            public static StateKey STATE_EMPTY = StateKey.Get("empty");

            private bool selected;
            //private CallbackWithReason<?>[] callbacks;

            public event EventHandler<ListBoxEventArgs> Callback;

            public ListBoxLabel()
            {
                setClip(true);
                setTheme("display");
            }

            public bool isSelected()
            {
                return selected;
            }

            public void setSelected(bool selected)
            {
                if (this.selected != selected)
                {
                    this.selected = selected;
                    getAnimationState().setAnimationState(STATE_SELECTED, selected);
                }
            }

            public bool isFocused()
            {
                return getAnimationState().GetAnimationState(STATE_KEYBOARD_FOCUS);
            }

            public void setFocused(bool focused)
            {
                getAnimationState().setAnimationState(STATE_KEYBOARD_FOCUS, focused);
            }

            public void setData(Object data)
            {
                setCharSequence((data == null) ? "" : data.ToString());
                getAnimationState().setAnimationState(STATE_EMPTY, data == null);
            }

            public Widget getWidget()
            {
                return this;
            }

            protected void doListBoxCallback(ListBoxCallbackReason reason)
            {
                this.Callback.Invoke(this, new ListBoxEventArgs(reason));
            }

            protected virtual bool handleListBoxEvent(Event evt)
            {
                if (evt.getEventType() == EventType.MOUSE_BTNDOWN)
                {
                    if (!selected)
                    {
                        doListBoxCallback(ListBoxCallbackReason.MOUSE_CLICK);
                    }
                    return true;
                }
                else if (evt.getEventType() == EventType.MOUSE_CLICKED)
                {
                    if (selected && evt.getMouseClickCount() == 2)
                    {
                        doListBoxCallback(ListBoxCallbackReason.MOUSE_DOUBLE_CLICK);
                    }
                    return true;
                }
                return false;
            }

            public override bool handleEvent(Event evt)
            {
                handleMouseHover(evt);
                if (!evt.isMouseDragEvent())
                {
                    if (handleListBoxEvent(evt))
                    {
                        return true;
                    }
                }
                if (base.handleEvent(evt))
                {
                    return true;
                }
                return evt.isMouseEventNoWheel();
            }

        }

        void entriesInserted(int first, int last)
        {
            int delta = last - first + 1;
            int prevNumEntries = numEntries;
            numEntries += delta;
            int fv = getFirstVisible();
            if (fv >= first && prevNumEntries >= labels.Length)
            {
                fv += delta;
                setFirstVisible(fv);
            }
            int s = getSelected();
            if (s >= first)
            {
                setSelected(s + delta, false, ListBoxCallbackReason.MODEL_CHANGED);
            }
            if (first <= getLastVisible() && last >= fv)
            {
                needUpdate = true;
            }
        }

        void entriesDeleted(int first, int last)
        {
            int delta = last - first + 1;
            numEntries -= delta;
            int fv = getFirstVisible();
            int lv = getLastVisible();
            if (fv > last)
            {
                setFirstVisible(fv - delta);
            }
            else if (fv <= last && lv >= first)
            {
                setFirstVisible(first);
            }
            int s = getSelected();
            if (s > last)
            {
                setSelected(s - delta, false, ListBoxCallbackReason.MODEL_CHANGED);
            }
            else if (s >= first && s <= last)
            {
                setSelected(NO_SELECTION, false, ListBoxCallbackReason.MODEL_CHANGED);
            }
        }

        void entriesChanged(int first, int last)
        {
            int fv = getFirstVisible();
            int lv = getLastVisible();
            if (fv <= last && lv >= first)
            {
                needUpdate = true;
            }
        }

        void allChanged()
        {
            numEntries = (model != null) ? model.Entries : 0;
            setSelected(NO_SELECTION, false, ListBoxCallbackReason.MODEL_CHANGED);
            setFirstVisible(0);
            needUpdate = true;
        }

        void scrollbarChanged()
        {
            setFirstVisible(scrollbar.getValue() * numCols);
        }

        void syncSelectionFromModel()
        {
            if (!inSetSelected)
            {
                setSelected(selectionModel.Value);
            }
        }
    }
}
