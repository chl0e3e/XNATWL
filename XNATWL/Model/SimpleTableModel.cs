using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleTableModel : AbstractTableModel
    {
        private string[] columnHeaders;
        private List<object[]> rows;

        public SimpleTableModel(string[] columnHeaders)
        {
            if (columnHeaders.Length < 1)
            {
                throw new ArgumentOutOfRangeException("must have at least one column");
            }

            this.columnHeaders = (string[]) columnHeaders.Clone();
            this.rows = new List<object[]>();
        }

        public override int Columns
        {
            get
            {
                return columnHeaders.Length;
            }
        }

        public override int Rows
        {
            get
            {
                return rows.Count;
            }
        }

        public override object CellAt(int row, int column)
        {
            return rows[row][column];
        }

        public void SetCellAt(int row, int column, object data)
        {
            rows[row][column] = data;   
            this.FireCellChanged(row, column);
        }

        public override string ColumnHeaderTextFor(int column)
        {
            return columnHeaders[column];
        }

        public void AddRow(params object[] data)
        {
            InsertRow(rows.Count, data);
        }

        public void AddRows(ICollection<object[]> data)
        {
            InsertRows(rows.Count, data);
        }

        public void InsertRow(int index, params object[] data)
        {
            rows.Insert(index, CreateRowData(data));
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
                this.rows.InsertRange(index, rowData);
                this.FireRowsInserted(index, rowData.Count);
            }
        }

        public void DeleteRow(int index)
        {
            rows.RemoveAt(index);
            this.FireRowsDeleted(index, 1);
        }

        public void DeleteRows(int index, int count)
        {
            int numRows = rows.Count;
            if (index < 0 || count < 0 || index >= numRows || count > (numRows - index))
            {
                throw new IndexOutOfRangeException("index=" + index + " count=" + count + " numRows=" + numRows);
            }

            if (count > 0)
            {
                for (int i = count; i-- > 0;)
                {
                    rows.RemoveAt(index + i);
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
