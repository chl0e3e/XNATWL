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
        TableModel _model;

        public Table()
        {
        }

        public Table(TableModel model) : this()
        {
            SetModel(model);
        }

        public TableModel GetModel()
        {
            return _model;
        }

        public void SetModel(TableModel model)
        {
            if (this._model != null)
            {
                this._model.RowsDeleted -= Model_RowsDeleted;
                this._model.AllChanged -= Model_AllChanged;
                this._model.CellChanged -= Model_CellChanged;
                this._model.RowsChanged -= Model_RowsChanged;
                this._model.RowsInserted -= Model_RowsInserted;
                this._model.ColumnHeaderChanged -= Model_ColumnHeaderChanged;
                this._model.ColumnInserted -= Model_ColumnInserted;
                this._model.ColumnDeleted -= Model_ColumnDeleted;
            }
            this._columnHeaderModel = model;
            this._model = model;
            if (this._model != null)
            {
                _numRows = model.Rows;
                _numColumns = model.Columns;
                this._model.RowsDeleted += Model_RowsDeleted;
                this._model.AllChanged += Model_AllChanged;
                this._model.CellChanged += Model_CellChanged;
                this._model.RowsChanged += Model_RowsChanged;
                this._model.RowsInserted += Model_RowsInserted;
                this._model.ColumnHeaderChanged += Model_ColumnHeaderChanged;
                this._model.ColumnInserted += Model_ColumnInserted;
                this._model.ColumnDeleted += Model_ColumnDeleted;
            }
            else
            {
                _numRows = 0;
                _numColumns = 0;
            }
            ModelAllChanged();
        }

        private void Model_ColumnDeleted(object sender, ColumnsChangedEventArgs e)
        {
            CheckColumnRange(e.Index, e.Count);
            _numColumns = _model.Columns;
            ModelColumnsDeleted(e.Count, e.Count);
        }

        private void Model_ColumnInserted(object sender, ColumnsChangedEventArgs e)
        {
            _numColumns = _model.Columns;
            ModelColumnsInserted(e.Count, e.Count);
        }

        private void Model_ColumnHeaderChanged(object sender, ColumnHeaderChangedEventArgs e)
        {
            ModelColumnHeaderChanged(e.Column);
        }

        private void Model_RowsInserted(object sender, TableRowModificationEventArgs e)
        {
            _numRows = _model.Rows;
            ModelRowsInserted(e.Index, e.Count);
        }

        private void Model_RowsChanged(object sender, TableRowModificationEventArgs e)
        {
            ModelRowsChanged(e.Index, e.Count);
        }

        private void Model_CellChanged(object sender, TableCellModificationEventArgs e)
        {
            ModelCellChanged(e.Row, e.Column);
        }

        private void Model_AllChanged(object sender, TableAllChangedEventArgs e)
        {
            _numRows = _model.Rows;
            _numColumns = _model.Columns;
            ModelAllChanged();
        }

        private void Model_RowsDeleted(object sender, TableRowModificationEventArgs e)
        {
            CheckRowRange(e.Index, e.Count);
            _numRows = _model.Rows;
            ModelRowsDeleted(e.Index, e.Count);
        }

        public override Object GetCellData(int row, int column, TreeTableNode node)
        {
            return _model.CellAt(row, column);
        }

        public override TreeTableNode GetNodeFromRow(int row)
        {
            return null;
        }

        public override Object GetTooltipContentFromRow(int row, int column)
        {
            return _model.TooltipAt(row, column);
        }
    }
}
