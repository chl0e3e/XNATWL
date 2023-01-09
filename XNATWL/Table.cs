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
    public class Table : TableBase
    {
        TableModel model;

        public Table()
        {
        }

        public Table(TableModel model) : this()
        {
            setModel(model);
        }

        public TableModel getModel()
        {
            return model;
        }

        public void setModel(TableModel model)
        {
            if (this.model != null)
            {
                this.model.RowsDeleted -= Model_RowsDeleted;
                this.model.AllChanged -= Model_AllChanged;
                this.model.CellChanged -= Model_CellChanged;
                this.model.RowsChanged -= Model_RowsChanged;
                this.model.RowsInserted -= Model_RowsInserted;
                this.model.ColumnHeaderChanged -= Model_ColumnHeaderChanged;
                this.model.ColumnInserted -= Model_ColumnInserted;
                this.model.ColumnDeleted -= Model_ColumnDeleted;
            }
            this.columnHeaderModel = model;
            this.model = model;
            if (this.model != null)
            {
                numRows = model.Rows;
                numColumns = model.Columns;
                this.model.RowsDeleted += Model_RowsDeleted;
                this.model.AllChanged += Model_AllChanged;
                this.model.CellChanged += Model_CellChanged;
                this.model.RowsChanged += Model_RowsChanged;
                this.model.RowsInserted += Model_RowsInserted;
                this.model.ColumnHeaderChanged += Model_ColumnHeaderChanged;
                this.model.ColumnInserted += Model_ColumnInserted;
                this.model.ColumnDeleted += Model_ColumnDeleted;
            }
            else
            {
                numRows = 0;
                numColumns = 0;
            }
            modelAllChanged();
        }

        private void Model_ColumnDeleted(object sender, ColumnsChangedEventArgs e)
        {
            checkColumnRange(e.Index, e.Count);
            numColumns = model.Columns;
            modelColumnsDeleted(e.Count, e.Count);
        }

        private void Model_ColumnInserted(object sender, ColumnsChangedEventArgs e)
        {
            numColumns = model.Columns;
            modelColumnsInserted(e.Count, e.Count);
        }

        private void Model_ColumnHeaderChanged(object sender, ColumnHeaderChangedEventArgs e)
        {
            modelColumnHeaderChanged(e.Column);
        }

        private void Model_RowsInserted(object sender, TableRowModificationEventArgs e)
        {
            numRows = model.Rows;
            modelRowsInserted(e.Index, e.Count);
        }

        private void Model_RowsChanged(object sender, TableRowModificationEventArgs e)
        {
            modelRowsChanged(e.Index, e.Count);
        }

        private void Model_CellChanged(object sender, TableCellModificationEventArgs e)
        {
            modelCellChanged(e.Row, e.Column);
        }

        private void Model_AllChanged(object sender, TableAllChangedEventArgs e)
        {
            numRows = model.Rows;
            numColumns = model.Columns;
            modelAllChanged();
        }

        private void Model_RowsDeleted(object sender, TableRowModificationEventArgs e)
        {
            checkRowRange(e.Index, e.Count);
            numRows = model.Rows;
            modelRowsDeleted(e.Index, e.Count);
        }

        public override Object getCellData(int row, int column, TreeTableNode node)
        {
            return model.CellAt(row, column);
        }

        public override TreeTableNode getNodeFromRow(int row)
        {
            return null;
        }

        public override Object getTooltipContentFromRow(int row, int column)
        {
            return model.TooltipAt(row, column);
        }
    }
}
