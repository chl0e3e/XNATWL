using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Util;

namespace XNATWL.Model
{
    public class DefaultTableSelectionModel : AbstractTableSelectionModel
    {
        private BitSet _value;
        private int _minIndex;
        private int _maxIndex;

        public DefaultTableSelectionModel()
        {
            this._value = new BitSet();
            this._minIndex = Int32.MaxValue;
            this._maxIndex = Int32.MinValue;
        }

        public override int FirstSelected
        {
            get
            {
                return _minIndex;
            }
        }

        public override int LastSelected
        {
            get
            {
                return _maxIndex;
            }
        }

        public override int[] Selection
        {
            get
            {
                int[] result = new int[this._value.Cardinality()];
                int idx = -1;

                for (int i = 0; (idx = this._value.NextSetBit(idx + 1)) >= 0; i++)
                {
                    result[i] = idx;
                }

                return result;
            }
        }

        public override void AddSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;

            this.UpdateLeadAndAnchor(index0, index1);

            int min = Math.Min(index0, index1);
            int max = Math.Max(index0, index1);

            for (int i = min; i <= max; i++)
            {
                this.SetBit(i);
            }

            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        private void ClearBit(int idx)
        {
            if (this._value.Get(idx))
            {
                this._value.Clear(idx);

                if (idx == this._minIndex)
                {
                    this._minIndex = this._value.NextSetBit(this._minIndex + 1);
                    if (this._minIndex < 0)
                    {
                        this._minIndex = Int32.MaxValue;
                        this._maxIndex = Int32.MinValue;
                        return;
                    }
                }

                if (idx == this._maxIndex)
                {
                    do
                    {
                        this._maxIndex--;
                    } while (this._maxIndex >= this._minIndex && !this._value.Get(this._maxIndex));
                }
            }
        }

        private void SetBit(int idx)
        {
            if (!this._value.Get(idx))
            {
                this._value.Set(idx);

                if (idx < this._minIndex)
                {
                    this._minIndex = idx;
                }

                if (idx > this._maxIndex)
                {
                    this._maxIndex = idx;
                }
            }
        }

        private void ToggleBit(int idx)
        {
            if (this._value.Get(idx))
            {
                this.ClearBit(idx);
            }
            else
            {
                this.SetBit(idx);
            }
        }

        public override void ClearSelection()
        {
            if (HasSelection())
            {
                int[] oldSelection = this.Selection;

                this._minIndex = Int32.MaxValue;
                this._maxIndex = Int32.MinValue;

                this._value.Clear();

                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override bool HasSelection()
        {
            return this._maxIndex >= this._minIndex;
        }

        public override void InvertSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;

            this.UpdateLeadAndAnchor(index0, index1);

            int min = Math.Min(index0, index1);
            int max = Math.Max(index0, index1);

            for (int i = min; i <= max; i++)
            {
                this.ToggleBit(i);
            }

            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        public override bool IsSelected(int index)
        {
            return this._value.Get(index);
        }

        public override void RemoveSelection(int index0, int index1)
        {
            this.UpdateLeadAndAnchor(index0, index1);

            if (HasSelection())
            {
                int[] oldSelection = this.Selection;

                int min = Math.Min(index0, index1);
                int max = Math.Max(index0, index1);

                for (int i = min; i <= max; i++)
                {
                    this.ClearBit(i);
                }

                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void SetSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;

            this.UpdateLeadAndAnchor(index0, index1);

            this._minIndex = Math.Min(index0, index1);
            this._maxIndex = Math.Max(index0, index1);

            this._value.Clear();

            this._value.Set(this._minIndex, this._maxIndex + 1);

            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        public override void RowsInserted(int index, int count)
        {
            if (index <= this._maxIndex)
            {
                for (int i = this._maxIndex; i >= index; i--)
                {
                    if (this._value.Get(i))
                    {
                        this._value.Set(i + count);
                    }
                    else
                    {
                        this._value.Clear(i + count);
                    }
                }

                this._value.Clear(index, index + count);

                this._maxIndex += count;

                if (index <= this._minIndex)
                {
                    this._minIndex += count;
                }
            }

            base.RowsInserted(index, count);
        }

        public override void RowsDeleted(int index, int count)
        {
            if (index <= this._maxIndex)
            {
                for (int i = index; i <= this._maxIndex; i++)
                {
                    if (this._value.Get(i + count))
                    {
                        this._value.Set(i);
                    }
                    else
                    {
                        this._value.Clear(i);
                    }
                }

                this._minIndex = this._value.NextSetBit(0);

                if (this._minIndex < 0)
                {
                    this._minIndex = Int32.MaxValue;
                    this._maxIndex = Int32.MinValue;
                }
                else
                {
                    while (this._maxIndex >= this._minIndex && !this._value.Get(this._maxIndex))
                    {
                        this._maxIndex--;
                    }
                }
            }

            base.RowsDeleted(index, count);
        }
    }
}
