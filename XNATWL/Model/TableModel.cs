﻿/*
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
    public interface TableModel : TableColumnHeaderModel
    {
        /// <summary>
        /// New rows have been inserted.
        /// </summary>
        event EventHandler<TableRowModificationEventArgs> RowsInserted;
        /// <summary>
        /// Rows that were at the range idx to idx+count-1 (inclusive) have been removed.
        /// </summary>
        event EventHandler<TableRowModificationEventArgs> RowsDeleted;
        /// <summary>
        /// Rows in the range idx to idx+count-1 (inclusive) have been changed.
        /// </summary>
        event EventHandler<TableRowModificationEventArgs> RowsChanged;
        /// <summary>
        /// A specified cell has changed
        /// </summary>
        event EventHandler<TableCellModificationEventArgs> CellChanged;
        /// <summary>
        /// The complete table was recreated.
        /// </summary>
        event EventHandler<TableAllChangedEventArgs> AllChanged;

        int Rows
        {
            get;
        }

        object CellAt(int row, int column);

        object TooltipAt(int row, int column);
    }

    public class TableRowModificationEventArgs
    {
        public int Index;
        public int Count;

        public TableRowModificationEventArgs(int idx, int count)
        {
            this.Index = idx;
            this.Count = count;
        }
    }

    public class TableCellModificationEventArgs
    {
        public int Row;
        public int Column;

        public TableCellModificationEventArgs(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }
    }

    public class TableAllChangedEventArgs
    {
        public TableAllChangedEventArgs()
        {

        }
    }
}