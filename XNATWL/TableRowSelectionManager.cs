using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected ActionMap actionMap;
        protected TableSelectionModel selectionModel;

        protected TableBase tableBase;

        public TableRowSelectionManager(TableSelectionModel selectionModel)
        {
            if (selectionModel == null)
            {
                throw new ArgumentNullException("selectionModel");
            }
            this.selectionModel = selectionModel;
            this.actionMap = new ActionMap();

            actionMap.addMapping(this);
        }

        /**
         * Creates a row selection model with a multi selection model.
         *
         * @see DefaultTableSelectionModel
         */
        public TableRowSelectionManager() : this(new DefaultTableSelectionModel())
        {
            
        }

        public TableSelectionModel getSelectionModel()
        {
            return selectionModel;
        }

        public void setAssociatedTable(TableBase tableBase)
        {
            if (tableBase != tableBase)
            {
                if (tableBase != null && tableBase != null)
                {
                    throw new InvalidOperationException("selection manager still in use");
                }
                this.tableBase = tableBase;
                modelChanged();
            }
        }

        public TableSelectionGranularity getSelectionGranularity()
        {
            return TableSelectionGranularity.ROWS;
        }

        public bool handleKeyStrokeAction(String action, Event evt)
        {
            return actionMap.invoke(action, evt);
        }

        public bool handleMouseEvent(int row, int column, Event evt)
        {
            bool isShift = (evt.getModifiers() & Event.MODIFIER_SHIFT) != 0;
            bool isCtrl = (evt.getModifiers() & Event.MODIFIER_CTRL) != 0;
            if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN && evt.getMouseButton() == Event.MOUSE_LBUTTON)
            {
                handleMouseDown(row, column, isShift, isCtrl);
                return true;
            }
            if (evt.getEventType() == Event.EventType.MOUSE_CLICKED)
            {
                return handleMouseClick(row, column, isShift, isCtrl);
            }
            return false;
        }

        public bool isRowSelected(int row)
        {
            return selectionModel.IsSelected(row);
        }

        /**
         * In a row selection model no cell can be selected. So this method always
         * returns false
         *
         * @param row ignored
         * @param column ignored
         * @return always false
         */
        public bool isCellSelected(int row, int column)
        {
            return false;
        }

        public int getLeadRow()
        {
            return selectionModel.LeadIndex;
        }

        public int getLeadColumn()
        {
            return -1;
        }

        public void modelChanged()
        {
            selectionModel.ClearSelection();
            selectionModel.AnchorIndex = -1;
            selectionModel.LeadIndex = -1;
        }

        public void rowsInserted(int index, int count)
        {
            selectionModel.RowsInserted(index, count);
        }

        public void rowsDeleted(int index, int count)
        {
            selectionModel.RowsDeleted(index, count);
        }

        public void columnInserted(int index, int count)
        {
        }

        public void columnsDeleted(int index, int count)
        {
        }

        [ActionMap.Action]
        public void selectNextRow()
        {
            handleRelativeAction(1, SET);
        }

        [ActionMap.Action]
        public void selectPreviousRow()
        {
            handleRelativeAction(-1, SET);
        }

        [ActionMap.Action]
        public void selectNextPage()
        {
            handleRelativeAction(getPageSize(), SET);
        }

        [ActionMap.Action]
        public void selectPreviousPage()
        {
            handleRelativeAction(-getPageSize(), SET);
        }

        [ActionMap.Action]
        public void selectFirstRow()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                handleAbsoluteAction(0, SET);
            }
        }

        [ActionMap.Action]
        public void selectLastRow()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                handleRelativeAction(numRows - 1, SET);
            }
        }

        [ActionMap.Action]
        public void extendSelectionToNextRow()
        {
            handleRelativeAction(1, EXTEND);
        }

        [ActionMap.Action]
        public void extendSelectionToPreviousRow()
        {
            handleRelativeAction(-1, EXTEND);
        }

        [ActionMap.Action]
        public void extendSelectionToNextPage()
        {
            handleRelativeAction(getPageSize(), EXTEND);
        }

        [ActionMap.Action]
        public void extendSelectionToPreviousPage()
        {
            handleRelativeAction(-getPageSize(), EXTEND);
        }

        [ActionMap.Action]
        public void extendSelectionToFirstRow()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                handleAbsoluteAction(0, EXTEND);
            }
        }

        [ActionMap.Action]
        public void extendSelectionToLastRow()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                handleRelativeAction(numRows - 1, EXTEND);
            }
        }

        [ActionMap.Action]
        public void moveLeadToNextRow()
        {
            handleRelativeAction(1, MOVE);
        }

        [ActionMap.Action]
        public void moveLeadToPreviousRow()
        {
            handleRelativeAction(-1, MOVE);
        }

        [ActionMap.Action]
        public void moveLeadToNextPage()
        {
            handleRelativeAction(getPageSize(), MOVE);
        }

        [ActionMap.Action]
        public void moveLeadToPreviousPage()
        {
            handleRelativeAction(-getPageSize(), MOVE);
        }

        [ActionMap.Action]
        public void moveLeadToFirstRow()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                handleAbsoluteAction(0, MOVE);
            }
        }

        [ActionMap.Action]
        public void moveLeadToLastRow()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                handleAbsoluteAction(numRows - 1, MOVE);
            }
        }

        [ActionMap.Action]
        public void toggleSelectionOnLeadRow()
        {
            int leadIndex = selectionModel.LeadIndex;
            if (leadIndex > 0)
            {
                selectionModel.InvertSelection(leadIndex, leadIndex);
            }
        }

        [ActionMap.Action]
        public void selectAll()
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                selectionModel.SetSelection(0, numRows - 1);
            }
        }

        [ActionMap.Action]
        public void selectNone()
        {
            selectionModel.ClearSelection();
        }

        protected const int TOGGLE = 0;
        protected const int EXTEND = 1;
        protected const int SET = 2;
        protected const int MOVE = 3;

        protected void handleRelativeAction(int delta, int mode)
        {
            int numRows = getNumRows();
            if (numRows > 0)
            {
                int leadIndex = Math.Max(0, selectionModel.LeadIndex);
                int index = Math.Max(0, Math.Min(numRows - 1, leadIndex + delta));

                handleAbsoluteAction(index, mode);
            }
        }

        protected void handleAbsoluteAction(int index, int mode)
        {
            if (tableBase != null)
            {
                tableBase.adjustScrollPosition(index);
            }

            switch (mode)
            {
                case MOVE:
                    selectionModel.LeadIndex = index;
                    break;
                case EXTEND:
                    int anchorIndex = Math.Max(0, selectionModel.AnchorIndex);
                    selectionModel.SetSelection(anchorIndex, index);
                    break;
                case TOGGLE:
                    selectionModel.InvertSelection(index, index);
                    break;
                default:
                    selectionModel.SetSelection(index, index);
                    break;
            }
        }

        protected void handleMouseDown(int row, int column, bool isShift, bool isCtrl)
        {
            if (row < 0 || row >= getNumRows())
            {
                if (!isShift)
                {
                    selectionModel.ClearSelection();
                }
            }
            else
            {
                tableBase.adjustScrollPosition(row);
                int anchorIndex = selectionModel.AnchorIndex;
                bool anchorSelected;
                if (anchorIndex == -1)
                {
                    anchorIndex = 0;
                    anchorSelected = false;
                }
                else
                {
                    anchorSelected = selectionModel.IsSelected(anchorIndex);
                }

                if (isCtrl)
                {
                    if (isShift)
                    {
                        if (anchorSelected)
                        {
                            selectionModel.AddSelection(anchorIndex, row);
                        }
                        else
                        {
                            selectionModel.RemoveSelection(anchorIndex, row);
                        }
                    }
                    else if (selectionModel.IsSelected(row))
                    {
                        selectionModel.RemoveSelection(row, row);
                    }
                    else
                    {
                        selectionModel.AddSelection(row, row);
                    }
                }
                else if (isShift)
                {
                    selectionModel.SetSelection(anchorIndex, row);
                }
                else
                {
                    selectionModel.SetSelection(row, row);
                }
            }
        }

        protected virtual bool handleMouseClick(int row, int column, bool isShift, bool isCtrl)
        {
            return false;
        }

        protected int getNumRows()
        {
            if (tableBase != null)
            {
                return tableBase.getNumRows();
            }
            return 0;
        }

        protected int getPageSize()
        {
            if (tableBase != null)
            {
                return Math.Max(1, tableBase.getNumVisibleRows());
            }
            return 1;
        }
    }

}
