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
        ModelChanged,
        SetSelected,
        MouseClick,
        MouseDoubleClick,
        Keyboard,
        KeyboardReturn
    }

    public class ListBox<T> : Widget
    {
        public static int NO_SELECTION = int.MinValue;
        public static int DEFAULT_CELL_HEIGHT = 20;
        public static int SINGLE_COLUMN = -1;

        public static bool CallbackReason_ActionRequested(ListBoxCallbackReason listBoxCallbackReason)
        {
            if (listBoxCallbackReason == ListBoxCallbackReason.MouseDoubleClick || listBoxCallbackReason == ListBoxCallbackReason.KeyboardReturn)
            {
                return true;
            }

            return false;
        }

        private static ListBoxDisplay[] EMPTY_LABELS = { };

        private Scrollbar _scrollbar;
        private ListBoxDisplay[] _labels;
        private ListModel<T> _model;
        private IntegerModel _selectionModel;
        private int _cellHeight = DEFAULT_CELL_HEIGHT;
        private int _cellWidth = SINGLE_COLUMN;
        private bool _rowMajor = true;
        private bool _fixedCellWidth;
        private bool _fixedCellHeight;
        private int _minDisplayedRows = 1;

        private int _numCols = 1;
        private int _firstVisible;
        private int _selected = NO_SELECTION;
        private int _numEntries;
        private bool _needUpdate;
        private bool _inSetSelected;

        public event EventHandler<ListBoxEventArgs> Callback;
        //private CallbackWithReason<?>[] callbacks;

        public ListBox()
        {
            _scrollbar = new Scrollbar();
            _scrollbar.PositionChanged += Scrollbar_PositionChanged;
            _labels = EMPTY_LABELS;

            base.InsertChild(_scrollbar, 0);

            SetSize(200, 300);
            SetCanAcceptKeyboardFocus(true);
            SetDepthFocusTraversal(false);
        }

        private void Scrollbar_PositionChanged(object sender, ScrollbarChangedPositionEventArgs e)
        {
            this.ScrollbarChanged();
        }

        public ListBox(ListModel<T> model) : this()
        {
            SetModel(model);
        }

        public ListBox(ListSelectionModel<T> model) : this()
        {
            SetModel(model);
        }

        public ListModel<T> getModel()
        {
            return _model;
        }

        public void SetModel(ListModel<T> model)
        {
            if (this._model != model)
            {
                if (this._model != null)
                {
                    this._model.AllChanged -= Model_AllChanged;
                    this._model.EntriesChanged -= Model_EntriesChanged;
                    this._model.EntriesDeleted -= Model_EntriesDeleted;
                    this._model.EntriesInserted -= Model_EntriesInserted;
                }

                this._model = model;

                if (model != null)
                {
                    this._model.AllChanged += Model_AllChanged;
                    this._model.EntriesChanged += Model_EntriesChanged;
                    this._model.EntriesDeleted += Model_EntriesDeleted;
                    this._model.EntriesInserted += Model_EntriesInserted;
                }

                this.AllChanged();
            }
        }

        private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            this.EntriesInserted(e.First, e.Last);
        }

        private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            this.EntriesDeleted(e.First, e.Last);
        }

        private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            this.EntriesChanged(e.First, e.Last);
        }

        private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            this.AllChanged();
        }

        public IntegerModel GetSelectionModel()
        {
            return _selectionModel;
        }

        public void SetSelectionModel(IntegerModel selectionModel)
        {
            if (this._selectionModel != selectionModel)
            {
                if (this._selectionModel != null)
                {
                    this._selectionModel.Changed -= SelectionModel_Changed;
                }
                this._selectionModel = selectionModel;
                if (selectionModel != null)
                {
                    this._selectionModel.Changed += SelectionModel_Changed;
                    SyncSelectionFromModel();
                }
            }
        }

        private void SelectionModel_Changed(object sender, IntegerChangedEventArgs e)
        {
            this.SyncSelectionFromModel();
        }

        public void SetModel(ListSelectionModel<T> model)
        {
            SetSelectionModel(null);
            if (model == null)
            {
                SetModel((ListModel<T>)null);
            }
            else
            {
                SetModel(model.Model);
                SetSelectionModel(model);
            }
        }

        private void DoCallback(ListBoxCallbackReason reason)
        {
            if (this.Callback != null)
            {
                this.Callback.Invoke(this, new ListBoxEventArgs(reason));
            }
        }

        public int GetCellHeight()
        {
            return _cellHeight;
        }

        public void SetCellHeight(int cellHeight)
        {
            if (cellHeight < 1)
            {
                throw new ArgumentOutOfRangeException("cellHeight < 1");
            }
            this._cellHeight = cellHeight;
        }

        public int GetCellWidth()
        {
            return _cellWidth;
        }

        public void SetCellWidth(int cellWidth)
        {
            if (cellWidth < 1 && cellWidth != SINGLE_COLUMN)
            {
                throw new ArgumentOutOfRangeException("cellWidth < 1");
            }
            this._cellWidth = cellWidth;
        }

        public bool IsFixedCellHeight()
        {
            return _fixedCellHeight;
        }

        public void SetFixedCellHeight(bool fixedCellHeight)
        {
            this._fixedCellHeight = fixedCellHeight;
        }

        public bool IsFixedCellWidth()
        {
            return _fixedCellWidth;
        }

        public void SetFixedCellWidth(bool fixedCellWidth)
        {
            this._fixedCellWidth = fixedCellWidth;
        }

        public bool IsRowMajor()
        {
            return _rowMajor;
        }

        public void SetRowMajor(bool rowMajor)
        {
            this._rowMajor = rowMajor;
        }

        public int GetFirstVisible()
        {
            return _firstVisible;
        }

        public int GetLastVisible()
        {
            return GetFirstVisible() + _labels.Length - 1;
        }

        public void SetFirstVisible(int firstVisible)
        {
            firstVisible = Math.Max(0, Math.Min(firstVisible, _numEntries - 1));
            if (this._firstVisible != firstVisible)
            {
                this._firstVisible = firstVisible;
                _scrollbar.SetValue(firstVisible / _numCols, false);
                _needUpdate = true;
            }
        }

        public int GetSelected()
        {
            return _selected;
        }

        /**
         * Selects the specified entry and scrolls to make it visible
         *
         * @param selected the index or {@link #NO_SELECTION}
         * @throws IllegalArgumentException if index is invalid
         * @see #setSelected(int, bool)
         */
        public void SetSelected(int selected)
        {
            SetSelected(selected, true, ListBoxCallbackReason.SetSelected);
        }

        /**
         * Selects the specified entry and optionally scrolls to that entry
         *
         * @param selected the index or {@link #NO_SELECTION}
         * @param scroll true if it should scroll to make the entry visible
         * @throws IllegalArgumentException if index is invalid
         */
        public void SetSelected(int selected, bool scroll)
        {
            SetSelected(selected, scroll, ListBoxCallbackReason.SetSelected);
        }

        void SetSelected(int selected, bool scroll, ListBoxCallbackReason reason)
        {
            if (selected < NO_SELECTION || selected >= _numEntries)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (scroll)
            {
                ValidateLayout();
                if (selected == NO_SELECTION)
                {
                    SetFirstVisible(0);
                }
                else
                {
                    int delta = GetFirstVisible() - selected;
                    if (delta > 0)
                    {
                        int deltaRows = (delta + _numCols - 1) / _numCols;
                        SetFirstVisible(GetFirstVisible() - deltaRows * _numCols);
                    }
                    else
                    {
                        delta = selected - GetLastVisible();
                        if (delta > 0)
                        {
                            int deltaRows = (delta + _numCols - 1) / _numCols;
                            SetFirstVisible(GetFirstVisible() + deltaRows * _numCols);
                        }
                    }
                }
            }

            if (this._selected != selected)
            {
                this._selected = selected;
                if (_selectionModel != null)
                {
                    try
                    {
                        _inSetSelected = true;
                        _selectionModel.Value = selected;
                    }
                    finally
                    {
                        _inSetSelected = false;
                    }
                }
                _needUpdate = true;
                DoCallback(reason);
            }
            else if (CallbackReason_ActionRequested(reason) || reason == ListBoxCallbackReason.MouseClick)
            {
                DoCallback(reason);
            }
        }

        public void ScrollToSelected()
        {
            SetSelected(_selected, true, ListBoxCallbackReason.SetSelected);
        }

        public int GetNumEntries()
        {
            return _numEntries;
        }

        public int GetNumRows()
        {
            return (_numEntries + _numCols - 1) / _numCols;
        }

        public int GetNumColumns()
        {
            return _numCols;
        }

        public int FindEntryByName(String prefix)
        {
            for (int i = _selected + 1; i < _numEntries; i++)
            {
                if (_model.EntryMatchesPrefix(i, prefix))
                {
                    return i;
                }
            }
            for (int i = 0; i < _selected; i++)
            {
                if (_model.EntryMatchesPrefix(i, prefix))
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
        public override Widget GetWidgetAt(int x, int y)
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
        public int GetEntryAt(int x, int y)
        {
            int n = Math.Max(_labels.Length, _numEntries - _firstVisible);
            for (int i = 0; i < n; i++)
            {
                if (_labels[i].GetWidget().IsInside(x, y))
                {
                    return _firstVisible + i;
                }
            }
            return -1;
        }

        public override void InsertChild(Widget child, int index)
        {
            throw new InvalidOperationException();
        }

        public override void RemoveAllChildren()
        {
            throw new InvalidOperationException();
        }

        public override Widget RemoveChild(int index)
        {
            throw new InvalidOperationException();
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            SetCellHeight(themeInfo.GetParameter("cellHeight", DEFAULT_CELL_HEIGHT));
            SetCellWidth(themeInfo.GetParameter("cellWidth", SINGLE_COLUMN));
            SetRowMajor(themeInfo.GetParameter("rowMajor", true));
            SetFixedCellWidth(themeInfo.GetParameter("fixedCellWidth", false));
            SetFixedCellHeight(themeInfo.GetParameter("fixedCellHeight", false));
            _minDisplayedRows = themeInfo.GetParameter("minDisplayedRows", 1);
        }

        protected void GoKeyboard(int dir)
        {
            int newPos = _selected + dir;
            if (newPos >= 0 && newPos < _numEntries)
            {
                SetSelected(newPos, true, ListBoxCallbackReason.Keyboard);
            }
        }

        protected bool IsSearchChar(char ch)
        {
            return (ch != Event.CHAR_NONE) && Char.IsLetterOrDigit(ch);
        }

        protected override void KeyboardFocusGained()
        {
            SetLabelFocused(true);
        }

        protected override void KeyboardFocusLost()
        {
            SetLabelFocused(false);
        }

        private void SetLabelFocused(bool focused)
        {
            int idx = _selected - _firstVisible;
            if (idx >= 0 && idx < _labels.Length)
            {
                _labels[idx].SetFocused(focused);
            }
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.GetEventType() == EventType.MOUSE_WHEEL)
            {
                _scrollbar.Scroll(-evt.GetMouseWheelDelta());
                return true;
            }
            else
            if (evt.GetEventType() == EventType.KEY_PRESSED)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_UP:
                        GoKeyboard(-_numCols);
                        break;
                    case Event.KEY_DOWN:
                        GoKeyboard(_numCols);
                        break;
                    case Event.KEY_LEFT:
                        GoKeyboard(-1);
                        break;
                    case Event.KEY_RIGHT:
                        GoKeyboard(1);
                        break;
                    case Event.KEY_PRIOR:
                        if (_numEntries > 0)
                        {
                            SetSelected(Math.Max(0, _selected - _labels.Length),
                                true, ListBoxCallbackReason.Keyboard);
                        }
                        break;
                    case Event.KEY_NEXT:
                        SetSelected(Math.Min(_numEntries - 1, _selected + _labels.Length),
                                true, ListBoxCallbackReason.Keyboard);
                        break;
                    case Event.KEY_HOME:
                        if (_numEntries > 0)
                        {
                            SetSelected(0, true, ListBoxCallbackReason.Keyboard);
                        }
                        break;
                    case Event.KEY_END:
                        SetSelected(_numEntries - 1, true, ListBoxCallbackReason.Keyboard);
                        break;
                    case Event.KEY_RETURN:
                        SetSelected(_selected, false, ListBoxCallbackReason.KeyboardReturn);
                        break;
                    default:
                        if (evt.HasKeyChar() && IsSearchChar(evt.GetKeyChar()))
                        {
                            int idx = FindEntryByName(evt.GetKeyChar().ToString());
                            if (idx != NO_SELECTION)
                            {
                                SetSelected(idx, true, ListBoxCallbackReason.Keyboard);
                            }
                            return true;
                        }
                        return false;
                }
                return true;
            }
            else
            if (evt.GetEventType() == EventType.KEY_RELEASED)
            {

                switch (evt.GetKeyCode())
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
            if (base.HandleEvent(evt))
            {
                return true;
            }
            // eat all mouse events
            return evt.IsMouseEvent();
        }

        public override int GetMinWidth()
        {
            return Math.Max(base.GetMinWidth(), _scrollbar.GetMinWidth());
        }

        public override int GetMinHeight()
        {
            int minHeight = Math.Max(base.GetMinHeight(), _scrollbar.GetMinHeight());
            if (_minDisplayedRows > 0)
            {
                minHeight = Math.Max(minHeight, GetBorderVertical() +
                        Math.Min(_numEntries, _minDisplayedRows) * _cellHeight);
            }
            return minHeight;
        }

        public override int GetPreferredInnerWidth()
        {
            return Math.Max(base.GetPreferredInnerWidth(), _scrollbar.GetPreferredWidth());
        }

        public override int GetPreferredInnerHeight()
        {
            return Math.Max(GetNumRows() * GetCellHeight(), _scrollbar.GetPreferredHeight());
        }

        protected override void Paint(GUI gui)
        {
            if (_needUpdate)
            {
                UpdateDisplay();
            }
            // always update scrollbar
            int maxFirstVisibleRow = ComputeMaxFirstVisibleRow();
            _scrollbar.SetMinMaxValue(0, maxFirstVisibleRow);
            _scrollbar.SetValue(_firstVisible / _numCols, false);

            base.Paint(gui);
        }

        private int ComputeMaxFirstVisibleRow()
        {
            int maxFirstVisibleRow = Math.Max(0, _numEntries - _labels.Length);
            maxFirstVisibleRow = (maxFirstVisibleRow + _numCols - 1) / _numCols;
            return maxFirstVisibleRow;
        }

        private void UpdateDisplay()
        {
            _needUpdate = false;

            if (_selected >= _numEntries)
            {
                _selected = NO_SELECTION;
            }

            int maxFirstVisibleRow = ComputeMaxFirstVisibleRow();
            int maxFirstVisible = maxFirstVisibleRow * _numCols;
            if (_firstVisible > maxFirstVisible)
            {
                _firstVisible = Math.Max(0, maxFirstVisible);
            }

            bool hasFocus = HasKeyboardFocus();

            for (int i = 0; i < _labels.Length; i++)
            {
                ListBoxDisplay label = _labels[i];
                int cell = i + _firstVisible;
                if (cell < _numEntries)
                {
                    label.SetData(_model.EntryAt(cell));
                    label.SetTooltipContent(_model.EntryTooltipAt(cell));
                }
                else
                {
                    label.SetData(null);
                    label.SetTooltipContent(null);
                }
                label.SetSelected(cell == _selected);
                label.SetFocused(cell == _selected && hasFocus);
            }
        }

        protected override void Layout()
        {
            _scrollbar.SetSize(_scrollbar.GetPreferredWidth(), GetInnerHeight());
            _scrollbar.SetPosition(GetInnerRight() - _scrollbar.GetWidth(), GetInnerY());

            int numRows = Math.Max(1, GetInnerHeight() / _cellHeight);
            if (_cellWidth != SINGLE_COLUMN)
            {
                _numCols = Math.Max(1, (_scrollbar.GetX() - GetInnerX()) / _cellWidth);
            }
            else
            {
                _numCols = 1;
            }
            SetVisibleCells(numRows);

            _needUpdate = true;
        }

        private void SetVisibleCells(int numRows)
        {
            int visibleCells = numRows * _numCols;
            System.Diagnostics.Debug.Assert(visibleCells >= 1);

            _scrollbar.SetPageSize(visibleCells);

            int curVisible = _labels.Length;
            for (int i = curVisible; i-- > visibleCells;)
            {
                base.RemoveChild(1 + i);
            }

            ListBoxDisplay[] newLabels = new ListBoxDisplay[visibleCells];
            Array.Copy(_labels, 0, newLabels, 0, Math.Min(visibleCells, _labels.Length));
            _labels = newLabels;

            for (int i = curVisible; i < visibleCells; i++)
            {
                int cellOffset = i;
                ListBoxDisplay lbd = CreateDisplay();
                lbd.Callback += (sender, e) =>
                {
                    int cell = GetFirstVisible() + cellOffset;
                    if (cell < GetNumEntries())
                    {
                        SetSelected(cell, false, e.Reason);
                    }
                };
                base.InsertChild(lbd.GetWidget(), 1 + i);
                _labels[i] = lbd;
            }

            int innerWidth = _scrollbar.GetX() - GetInnerX();
            int innerHeight = GetInnerHeight();
            for (int i = 0; i < visibleCells; i++)
            {
                int row, col;
                if (_rowMajor)
                {
                    row = i / _numCols;
                    col = i % _numCols;
                }
                else
                {
                    row = i % numRows;
                    col = i / numRows;
                }
                int x, y, w, h;
                if (_fixedCellHeight)
                {
                    y = row * _cellHeight;
                    h = _cellHeight;
                }
                else
                {
                    y = row * innerHeight / numRows;
                    h = (row + 1) * innerHeight / numRows - y;
                }
                if (_fixedCellWidth && _cellWidth != SINGLE_COLUMN)
                {
                    x = col * _cellWidth;
                    w = _cellWidth;
                }
                else
                {
                    x = col * innerWidth / _numCols;
                    w = (col + 1) * innerWidth / _numCols - x;
                }
                Widget cell = (Widget)_labels[i];
                cell.SetSize(Math.Max(0, w), Math.Max(0, h));
                cell.SetPosition(x + GetInnerX(), y + GetInnerY());
            }
        }

        protected virtual ListBoxDisplay CreateDisplay()
        {
            return new ListBoxLabel();
        }

        protected internal class ListBoxLabel : TextWidget, ListBoxDisplay
        {
            public static StateKey STATE_SELECTED = StateKey.Get("selected");
            public static StateKey STATE_EMPTY = StateKey.Get("empty");

            private bool _selected;
            //private CallbackWithReason<?>[] callbacks;

            public event EventHandler<ListBoxEventArgs> Callback;

            public ListBoxLabel()
            {
                SetClip(true);
                SetTheme("display");
            }

            public bool IsSelected()
            {
                return _selected;
            }

            public void SetSelected(bool selected)
            {
                if (this._selected != selected)
                {
                    this._selected = selected;
                    GetAnimationState().SetAnimationState(STATE_SELECTED, selected);
                }
            }

            public bool IsFocused()
            {
                return GetAnimationState().GetAnimationState(STATE_KEYBOARD_FOCUS);
            }

            public void SetFocused(bool focused)
            {
                GetAnimationState().SetAnimationState(STATE_KEYBOARD_FOCUS, focused);
            }

            public void SetData(Object data)
            {
                SetCharSequence((data == null) ? "" : data.ToString());
                GetAnimationState().SetAnimationState(STATE_EMPTY, data == null);
            }

            public Widget GetWidget()
            {
                return this;
            }

            protected void DoListBoxCallback(ListBoxCallbackReason reason)
            {
                this.Callback.Invoke(this, new ListBoxEventArgs(reason));
            }

            protected virtual bool HandleListBoxEvent(Event evt)
            {
                if (evt.GetEventType() == EventType.MOUSE_BTNDOWN)
                {
                    if (!_selected)
                    {
                        DoListBoxCallback(ListBoxCallbackReason.MouseClick);
                    }
                    return true;
                }
                else if (evt.GetEventType() == EventType.MOUSE_CLICKED)
                {
                    if (_selected && evt.GetMouseClickCount() == 2)
                    {
                        DoListBoxCallback(ListBoxCallbackReason.MouseDoubleClick);
                    }
                    return true;
                }

                return false;
            }

            public override bool HandleEvent(Event evt)
            {
                HandleMouseHover(evt);
                if (!evt.IsMouseDragEvent())
                {
                    if (HandleListBoxEvent(evt))
                    {
                        return true;
                    }
                }
                if (base.HandleEvent(evt))
                {
                    return true;
                }
                return evt.IsMouseEventNoWheel();
            }

        }

        void EntriesInserted(int first, int last)
        {
            int delta = last - first + 1;
            int prevNumEntries = _numEntries;
            _numEntries += delta;
            int fv = GetFirstVisible();
            if (fv >= first && prevNumEntries >= _labels.Length)
            {
                fv += delta;
                SetFirstVisible(fv);
            }
            int s = GetSelected();
            if (s >= first)
            {
                SetSelected(s + delta, false, ListBoxCallbackReason.ModelChanged);
            }
            if (first <= GetLastVisible() && last >= fv)
            {
                _needUpdate = true;
            }
        }

        void EntriesDeleted(int first, int last)
        {
            int delta = last - first + 1;
            _numEntries -= delta;
            int fv = GetFirstVisible();
            int lv = GetLastVisible();
            if (fv > last)
            {
                SetFirstVisible(fv - delta);
            }
            else if (fv <= last && lv >= first)
            {
                SetFirstVisible(first);
            }
            int s = GetSelected();
            if (s > last)
            {
                SetSelected(s - delta, false, ListBoxCallbackReason.ModelChanged);
            }
            else if (s >= first && s <= last)
            {
                SetSelected(NO_SELECTION, false, ListBoxCallbackReason.ModelChanged);
            }
        }

        void EntriesChanged(int first, int last)
        {
            int fv = GetFirstVisible();
            int lv = GetLastVisible();
            if (fv <= last && lv >= first)
            {
                _needUpdate = true;
            }
        }

        void AllChanged()
        {
            _numEntries = (_model != null) ? _model.Entries : 0;
            SetSelected(NO_SELECTION, false, ListBoxCallbackReason.ModelChanged);
            SetFirstVisible(0);
            _needUpdate = true;
        }

        void ScrollbarChanged()
        {
            SetFirstVisible(_scrollbar.GetValue() * _numCols);
        }

        void SyncSelectionFromModel()
        {
            if (!_inSetSelected)
            {
                SetSelected(_selectionModel.Value);
            }
        }
    }
}
