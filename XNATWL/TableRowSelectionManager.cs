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

namespace XNATWL
{
    /*
     * Copyright (c) 2008-2010, Matthias Mann
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

    /**
     * A row selection model manages table selection on a per row base.
     * Cells are never selected by this model.
     *
     * @see #isCellSelected(int, int)
     * @author Matthias Mann
     */
    public class TableRowSelectionManager : TableSelectionManager
    {
        protected ActionMap _actionMap;
        protected TableSelectionModel _selectionModel;

        protected TableBase _tableBase;

        public TableRowSelectionManager(TableSelectionModel selectionModel)
        {
            if (selectionModel == null)
            {
                throw new ArgumentNullException("selectionModel");
            }
            this._selectionModel = selectionModel;
            this._actionMap = new ActionMap();

            _actionMap.AddMapping(this);
        }

        /**
         * Creates a row selection model with a multi selection model.
         *
         * @see DefaultTableSelectionModel
         */
        public TableRowSelectionManager() : this(new DefaultTableSelectionModel())
        {
            
        }

        public TableSelectionModel GetSelectionModel()
        {
            return _selectionModel;
        }

        public void SetAssociatedTable(TableBase tableBase)
        {
            if (tableBase != _tableBase)
            {
                if (tableBase != null && _tableBase != null)
                {
                    throw new InvalidOperationException("selection manager still in use");
                }
                this._tableBase = tableBase;
                ModelChanged();
            }
        }

        public TableSelectionGranularity GetSelectionGranularity()
        {
            return TableSelectionGranularity.Rows;
        }

        public bool HandleKeyStrokeAction(String action, Event evt)
        {
            return _actionMap.Invoke(action, evt);
        }

        public bool HandleMouseEvent(int row, int column, Event evt)
        {
            bool isShift = (evt.GetModifiers() & Event.MODIFIER_SHIFT) != 0;
            bool isCtrl = (evt.GetModifiers() & Event.MODIFIER_CTRL) != 0;
            if (evt.GetEventType() == EventType.MOUSE_BTNDOWN && evt.GetMouseButton() == Event.MOUSE_LBUTTON)
            {
                HandleMouseDown(row, column, isShift, isCtrl);
                return true;
            }
            if (evt.GetEventType() == EventType.MOUSE_CLICKED)
            {
                return HandleMouseClick(row, column, isShift, isCtrl);
            }
            return false;
        }

        public bool IsRowSelected(int row)
        {
            return _selectionModel.IsSelected(row);
        }

        /**
         * In a row selection model no cell can be selected. So this method always
         * returns false
         *
         * @param row ignored
         * @param column ignored
         * @return always false
         */
        public bool IsCellSelected(int row, int column)
        {
            return false;
        }

        public int GetLeadRow()
        {
            return _selectionModel.LeadIndex;
        }

        public int GetLeadColumn()
        {
            return -1;
        }

        public void ModelChanged()
        {
            _selectionModel.ClearSelection();
            _selectionModel.AnchorIndex = -1;
            _selectionModel.LeadIndex = -1;
        }

        public void RowsInserted(int index, int count)
        {
            _selectionModel.RowsInserted(index, count);
        }

        public void RowsDeleted(int index, int count)
        {
            _selectionModel.RowsDeleted(index, count);
        }

        public void ColumnInserted(int index, int count)
        {
        }

        public void ColumnsDeleted(int index, int count)
        {
        }

        [ActionMap.Action]
        public void SelectNextRow()
        {
            HandleRelativeAction(1, SET);
        }

        [ActionMap.Action]
        public void SelectPreviousRow()
        {
            HandleRelativeAction(-1, SET);
        }

        [ActionMap.Action]
        public void SelectNextPage()
        {
            HandleRelativeAction(GetPageSize(), SET);
        }

        [ActionMap.Action]
        public void SelectPreviousPage()
        {
            HandleRelativeAction(-GetPageSize(), SET);
        }

        [ActionMap.Action]
        public void SelectFirstRow()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                HandleAbsoluteAction(0, SET);
            }
        }

        [ActionMap.Action]
        public void SelectLastRow()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                HandleRelativeAction(numRows - 1, SET);
            }
        }

        [ActionMap.Action]
        public void ExtendSelectionToNextRow()
        {
            HandleRelativeAction(1, EXTEND);
        }

        [ActionMap.Action]
        public void ExtendSelectionToPreviousRow()
        {
            HandleRelativeAction(-1, EXTEND);
        }

        [ActionMap.Action]
        public void ExtendSelectionToNextPage()
        {
            HandleRelativeAction(GetPageSize(), EXTEND);
        }

        [ActionMap.Action]
        public void ExtendSelectionToPreviousPage()
        {
            HandleRelativeAction(-GetPageSize(), EXTEND);
        }

        [ActionMap.Action]
        public void ExtendSelectionToFirstRow()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                HandleAbsoluteAction(0, EXTEND);
            }
        }

        [ActionMap.Action]
        public void ExtendSelectionToLastRow()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                HandleRelativeAction(numRows - 1, EXTEND);
            }
        }

        [ActionMap.Action]
        public void MoveLeadToNextRow()
        {
            HandleRelativeAction(1, MOVE);
        }

        [ActionMap.Action]
        public void MoveLeadToPreviousRow()
        {
            HandleRelativeAction(-1, MOVE);
        }

        [ActionMap.Action]
        public void MoveLeadToNextPage()
        {
            HandleRelativeAction(GetPageSize(), MOVE);
        }

        [ActionMap.Action]
        public void MoveLeadToPreviousPage()
        {
            HandleRelativeAction(-GetPageSize(), MOVE);
        }

        [ActionMap.Action]
        public void MoveLeadToFirstRow()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                HandleAbsoluteAction(0, MOVE);
            }
        }

        [ActionMap.Action]
        public void MoveLeadToLastRow()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                HandleAbsoluteAction(numRows - 1, MOVE);
            }
        }

        [ActionMap.Action]
        public void ToggleSelectionOnLeadRow()
        {
            int leadIndex = _selectionModel.LeadIndex;
            if (leadIndex > 0)
            {
                _selectionModel.InvertSelection(leadIndex, leadIndex);
            }
        }

        [ActionMap.Action]
        public void SelectAll()
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                _selectionModel.SetSelection(0, numRows - 1);
            }
        }

        [ActionMap.Action]
        public void SelectNone()
        {
            _selectionModel.ClearSelection();
        }

        protected const int TOGGLE = 0;
        protected const int EXTEND = 1;
        protected const int SET = 2;
        protected const int MOVE = 3;

        protected void HandleRelativeAction(int delta, int mode)
        {
            int numRows = GetNumRows();
            if (numRows > 0)
            {
                int leadIndex = Math.Max(0, _selectionModel.LeadIndex);
                int index = Math.Max(0, Math.Min(numRows - 1, leadIndex + delta));

                HandleAbsoluteAction(index, mode);
            }
        }

        protected void HandleAbsoluteAction(int index, int mode)
        {
            if (_tableBase != null)
            {
                _tableBase.AdjustScrollPosition(index);
            }

            switch (mode)
            {
                case MOVE:
                    _selectionModel.LeadIndex = index;
                    break;
                case EXTEND:
                    int anchorIndex = Math.Max(0, _selectionModel.AnchorIndex);
                    _selectionModel.SetSelection(anchorIndex, index);
                    break;
                case TOGGLE:
                    _selectionModel.InvertSelection(index, index);
                    break;
                default:
                    _selectionModel.SetSelection(index, index);
                    break;
            }
        }

        protected void HandleMouseDown(int row, int column, bool isShift, bool isCtrl)
        {
            if (row < 0 || row >= GetNumRows())
            {
                if (!isShift)
                {
                    _selectionModel.ClearSelection();
                }
            }
            else
            {
                _tableBase.AdjustScrollPosition(row);
                int anchorIndex = _selectionModel.AnchorIndex;
                bool anchorSelected;
                if (anchorIndex == -1)
                {
                    anchorIndex = 0;
                    anchorSelected = false;
                }
                else
                {
                    anchorSelected = _selectionModel.IsSelected(anchorIndex);
                }

                if (isCtrl)
                {
                    if (isShift)
                    {
                        if (anchorSelected)
                        {
                            _selectionModel.AddSelection(anchorIndex, row);
                        }
                        else
                        {
                            _selectionModel.RemoveSelection(anchorIndex, row);
                        }
                    }
                    else if (_selectionModel.IsSelected(row))
                    {
                        _selectionModel.RemoveSelection(row, row);
                    }
                    else
                    {
                        _selectionModel.AddSelection(row, row);
                    }
                }
                else if (isShift)
                {
                    _selectionModel.SetSelection(anchorIndex, row);
                }
                else
                {
                    _selectionModel.SetSelection(row, row);
                }
            }
        }

        protected virtual bool HandleMouseClick(int row, int column, bool isShift, bool isCtrl)
        {
            return false;
        }

        protected int GetNumRows()
        {
            if (_tableBase != null)
            {
                return _tableBase.GetNumRows();
            }
            return 0;
        }

        protected int GetPageSize()
        {
            if (_tableBase != null)
            {
                return Math.Max(1, _tableBase.GetNumVisibleRows());
            }
            return 1;
        }
    }

}
