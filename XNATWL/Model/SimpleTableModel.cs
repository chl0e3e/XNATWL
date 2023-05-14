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
using System.Collections.Generic;

namespace XNATWL.Model
{
    public class SimpleTableModel : AbstractTableModel
    {
        private string[] _columnHeaders;
        private List<object[]> _rows;

        public SimpleTableModel(string[] columnHeaders)
        {
            if (columnHeaders.Length < 1)
            {
                throw new ArgumentOutOfRangeException("must have at least one column");
            }

            this._columnHeaders = (string[]) columnHeaders.Clone();
            this._rows = new List<object[]>();
        }

        public override int Columns
        {
            get
            {
                return this._columnHeaders.Length;
            }
        }

        public override int Rows
        {
            get
            {
                return this._rows.Count;
            }
        }

        public override object CellAt(int row, int column)
        {
            return this._rows[row][column];
        }

        public void SetCellAt(int row, int column, object data)
        {
            _rows[row][column] = data;   
            this.FireCellChanged(row, column);
        }

        public override string ColumnHeaderTextFor(int column)
        {
            return _columnHeaders[column];
        }

        public void AddRow(params object[] data)
        {
            this.InsertRow(_rows.Count, data);
        }

        public void AddRows(ICollection<object[]> data)
        {
            this.InsertRows(_rows.Count, data);
        }

        public void InsertRow(int index, params object[] data)
        {
            this._rows.Insert(index, CreateRowData(data));
            this.FireRowsInserted(index, 1);
        }

        public void InsertRows(int index, ICollection<object[]> rows)
        {
            if (rows.Count != 0)
            {
                List<object[]> rowData = new List<object[]>();
                foreach (object[] row in rows)
                {
                    rowData.Add(CreateRowData(row));
                }
                this._rows.InsertRange(index, rowData);
                this.FireRowsInserted(index, rowData.Count);
            }
        }

        public void DeleteRow(int index)
        {
            this._rows.RemoveAt(index);
            this.FireRowsDeleted(index, 1);
        }

        public void DeleteRows(int index, int count)
        {
            int numRows = _rows.Count;
            if (index < 0 || count < 0 || index >= numRows || count > (numRows - index))
            {
                throw new IndexOutOfRangeException("index=" + index + " count=" + count + " numRows=" + numRows);
            }

            if (count > 0)
            {
                for (int i = count; i-- > 0;)
                {
                    this._rows.RemoveAt(index + i);
                }

                this.FireRowsDeleted(index, count);
            }
        }

        private object[] CreateRowData(object[] data)
        {
            object[] rowData = new object[this.Columns];
            Array.Copy(data, 0, rowData, 0, Math.Min(rowData.Length, data.Length));
            return rowData;
        }
    }
}
