using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AbstractTableSelectionModel : TableSelectionModel
    {
        private int _leadIndex;
        private int _anchorIndex;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public int LeadIndex
        {
            get
            {
                return _leadIndex;
            }

            set
            {
                _leadIndex = value;
            }
        }

        public int AnchorIndex
        {
            get
            {
                return _anchorIndex;
            }

            set
            {
                _anchorIndex = value;
            }
        }

        public abstract int FirstSelected { get; }
        public abstract int LastSelected { get; }
        public abstract int[] Selection { get; }

        public virtual void RowsDeleted(int index, int count)
        {
            if (this._leadIndex >= index)
            {
                this._leadIndex = Math.Max(index, this._leadIndex - count);
            }

            if (this._anchorIndex >= index)
            {
                this._anchorIndex = Math.Max(index, this._anchorIndex - count);
            }
        }

        public virtual void RowsInserted(int index, int count)
        {
            if (this._leadIndex >= index)
            {
                this._leadIndex += count;
            }

            if (this._anchorIndex >= index)
            {
                this._anchorIndex += count;
            }
        }

        protected void UpdateLeadAndAnchor(int index0, int index1)
        {
            this._anchorIndex = index0;
            this._leadIndex = index1;
        }

        public abstract void ClearSelection();

        public abstract void AddSelection(int index0, int index1);

        public abstract void InvertSelection(int index0, int index1);

        public abstract void RemoveSelection(int index0, int index1);

        public abstract bool IsSelected(int index);

        public abstract bool HasSelection();

        public abstract void SetSelection(int index0, int index1);

        public void FireSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            this.SelectionChanged.Invoke(sender, e);
        }
    }
}
