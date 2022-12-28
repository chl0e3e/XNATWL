using System;
using XNATWL.Renderer;

namespace XNATWL.Model
{
    public interface TableColumnHeaderModel
    {
        event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
        event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
        event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;

        int Columns
        {
            get;
        }

        StateKey[] ColumnHeaderStates
        {
            get;
        }

        string ColumnHeaderTextFor(int column);

        bool ColumnHeaderStateFor(int column, int stateIdx);
    }

    public class ColumnHeaderChangedEventArgs : EventArgs
    {
        public int Column;

        public ColumnHeaderChangedEventArgs(int column)
        {
            Column = column;
        }
    }

    public class ColumnsChangedEventArgs : EventArgs
    {
        public int Index;
        public int Count;

        public ColumnsChangedEventArgs(int index, int count)
        {
            this.Index = index;
            this.Count = count;
        }
    }
}