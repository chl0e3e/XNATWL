using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface TableSelectionModel
    {
        void RowsInserted(int index, int count);

        void RowsDeleted(int index, int count);

        void ClearSelection();

        void SetSelection(int index0, int index1);

        void AddSelection(int index0, int index1);

        void InvertSelection(int index0, int index1);

        void RemoveSelection(int index0, int index1);

        int LeadIndex
        {
            get;
            set;
        }

        int AnchorIndex
        {
            get;
            set;
        }

        bool IsSelected(int index);

        bool HasSelection();

        int FirstSelected
        {
            get;
        }

        int LastSelected
        {
            get;
        }

        int[] Selection
        {
            get;
        }

        event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        public int[] OldSelection;
        public int[] NewSelection;

        public SelectionChangedEventArgs(int[] oldSelection, int[] newSelection)
        {
            this.OldSelection = oldSelection;
            this.NewSelection = newSelection;
        }
    }
}
