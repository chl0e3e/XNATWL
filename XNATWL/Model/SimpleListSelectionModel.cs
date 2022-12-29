using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleListSelectionModel<T> : ListSelectionModel<T>
    {
        private ListModel<T> _listModel;
        private int _selected;

        public SimpleListSelectionModel(ListModel<T> listModel)
        {
            this._listModel = listModel;
        }

        public ListModel<T> Model
        {
            get
            {
                return this._listModel;
            }
        }

        public T SelectedEntry
        {
            get
            {
                if (this._selected >= 0 && this._selected < this._listModel.Entries)
                {
                    return this._listModel.EntryAt(this._selected);
                }
                else
                {
                    return default(T); // TODO
                }
            }

            set
            {
                SetSelectedEntryWithDefault(value, -1);
            }
        }

        public int Value
        {
            get
            {
                return this._selected;
            }

            set
            {
                int old = this._selected;
                if (value != old)
                {
                    this._selected = value;
                    this.Changed.Invoke(this, new IntegerChangedEventArgs(old, value));
                }
            }
        }

        public int MinValue
        {
            get
            {
                return this._listModel.Entries - 1;
            }
        }

        public int MaxValue
        {
            get
            {
                return -1;
            }
        }

        public event EventHandler<IntegerChangedEventArgs> Changed;

        public bool SetSelectedEntryWithDefault(T entry, int defaultIndex)
        {
            for (int i = 0, n = this._listModel.Entries; i < n; i++)
            {
                if (entry.Equals(this._listModel.EntryAt(i)))
                {
                    this.Value = i;
                    return true;
                }
            }

            this.Value = defaultIndex;
            return false;
        }
    }
}
