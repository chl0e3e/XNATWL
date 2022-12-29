using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class TableSingleSelectionModel : AbstractTableSelectionModel
    {
        public static int NO_SELECTION = -1;

        private int _selection;

        public override int FirstSelected
        {
            get
            {
                return this._selection;
            }
        }

        public override int LastSelected => throw new NotImplementedException();

        public override int[] Selection
        {
            get
            {
                if (_selection >= 0)
                {
                    return new int[] { _selection };
                }

                return new int[0];
            }
        }

        public override void RowsDeleted(int index, int count)
        {
            int[] oldSelection = this.Selection;
            bool changed = false;

            if (_selection >= index)
            {
                if (_selection < index + count)
                {
                    _selection = NO_SELECTION;
                }
                else
                {
                    _selection -= count;
                }

                changed = true;
            }

            base.RowsDeleted(index, count);

            if (changed)
            {
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void RowsInserted(int index, int count)
        {
            int[] oldSelection = this.Selection;
            bool changed = false;
            if (_selection >= index)
            {
                _selection += count;
                changed = true;
            }

            base.RowsInserted(index, count);

            if (changed)
            {
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void AddSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;
            UpdateLeadAndAnchor(index0, index1);
            _selection = index1;
            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        public override void ClearSelection()
        {
            int[] oldSelection = this.Selection;

            if (this.HasSelection())
            {
                _selection = NO_SELECTION;
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override bool HasSelection()
        {
            return _selection >= 0;
        }

        public override void InvertSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;

            UpdateLeadAndAnchor(index0, index1);

            if (_selection == index1)
            {
                _selection = NO_SELECTION;
            }
            else
            {
                _selection = index1;
            }

            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        public override bool IsSelected(int index)
        {
            return _selection == index;
        }

        public override void RemoveSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;
            UpdateLeadAndAnchor(index0, index1);

            if (HasSelection())
            {
                int first = Math.Min(index0, index1);
                int last = Math.Max(index0, index1);
                if (_selection >= first && _selection <= last)
                {
                    _selection = NO_SELECTION;
                }
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void SetSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;
            UpdateLeadAndAnchor(index0, index1);
            _selection = index1;
            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }
    }
}
