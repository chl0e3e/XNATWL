using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public object TooltipAt(int row, int column)
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
