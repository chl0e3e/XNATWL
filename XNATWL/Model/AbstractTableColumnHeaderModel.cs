using System;
using XNATWL.Renderer;

namespace XNATWL.Model
{
    public abstract class AbstractTableColumnHeaderModel : TableColumnHeaderModel
    {
        private static StateKey[] EMPTY_STATE_ARRAY = {};

        public StateKey[] ColumnHeaderStates
        {
            get
            {
                return EMPTY_STATE_ARRAY;
            }
        }

        public bool ColumnHeaderStateFor(int column, int stateIdx)
        {
            return false;
        }

        public abstract int Columns { get; }

        public abstract event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
        public abstract event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
        public abstract event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;

        public abstract string ColumnHeaderTextFor(int column);
    }
}