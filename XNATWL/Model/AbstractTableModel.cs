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

namespace XNATWL.Model
{
    public abstract class AbstractTableModel : AbstractTableColumnHeaderModel, TableModel
    {
        public override abstract int Columns
        {
            get;
        }

        public abstract int Rows
        {
            get;
        }

        public override event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
        public override event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
        public override event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;
        public event EventHandler<TableRowModificationEventArgs> RowsInserted;
        public event EventHandler<TableRowModificationEventArgs> RowsDeleted;
        public event EventHandler<TableRowModificationEventArgs> RowsChanged;
        public event EventHandler<TableAllChangedEventArgs> AllChanged;
        public event EventHandler<TableCellModificationEventArgs> CellChanged;

        public abstract object CellAt(int row, int column);

        public override abstract string ColumnHeaderTextFor(int column);

        public virtual object TooltipAt(int row, int column)
        {
            return null;
        }

        public void FireRowsInserted(int index, int count)
        {
            this.RowsInserted.Invoke(this, new TableRowModificationEventArgs(index, count));
        }

        public void FireRowsDeleted(int index, int count)
        {
            this.RowsDeleted.Invoke(this, new TableRowModificationEventArgs(index, count));
        }

        public void FireRowsChanged(int index, int count)
        {
            this.RowsChanged.Invoke(this, new TableRowModificationEventArgs(index, count));
        }

        public void FireCellChanged(int row, int column)
        {
            this.CellChanged.Invoke(this, new TableCellModificationEventArgs(row, column));
        }

        public void FireAllChanged()
        {
            this.AllChanged.Invoke(this, new TableAllChangedEventArgs());
        }

        public void FireColumnInserted(int index, int count)
        {
            this.ColumnInserted.Invoke(this, new ColumnsChangedEventArgs(index, count));
        }

        public void FireColumnDeleted(int index, int count)
        {
            this.ColumnDeleted.Invoke(this, new ColumnsChangedEventArgs(index, count));
        }

        public void FireColumnHeaderChanged(int column)
        {
            this.ColumnHeaderChanged.Invoke(this, new ColumnHeaderChangedEventArgs(column));
        }
    }
}
