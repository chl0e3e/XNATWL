using System;

namespace XNATWL.Model
{
    public interface TableModel
    {
        event EventHandler<TableRowModificationEventArgs> RowsInserted;
        event EventHandler<TableRowModificationEventArgs> RowsDeleted;
        event EventHandler<TableRowModificationEventArgs> RowsChanged;
        event EventHandler<TableCellModificationEventArgs> CellChanged;
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