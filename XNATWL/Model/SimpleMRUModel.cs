using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleMRUModel<T> : MRUListModel<T>
    {
        public int MaxEntries
        {
            get
            {
                return this._maxEntries;
            }
        }

        public int MRUEntries
        {
            get
            {
                return this._entries.Count;
            }
        }

        public int Entries
        {
            get
            {
                return this._entries.Count;
            }
        }

        public event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public event EventHandler<ListAllChangedEventArgs> AllChanged;

        public void Add(T entry)
        {
            int idx = this._entries.IndexOf(entry);

            if (idx >= 0)
            {
                DoDeleteEntry(idx);
            }
            else if (_entries.Count == this._maxEntries)
            {
                DoDeleteEntry(this._maxEntries - 1);
            }

            this._entries.Insert(0, entry);
            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(0, 0));
            this.Save();
        }

        public T EntryAt(int index)
        {
            return this._entries[index];
        }

        public bool EntryMatchesPrefix(int index, string prefix)
        {
            return false;
        }

        public object EntryTooltipAt(int index)
        {
            return null;
        }

        public void RemoveAt(int entry)
        {
            if (entry < 0 && entry >= this._entries.Count)
            {
                throw new IndexOutOfRangeException();
            }

            DoDeleteEntry(entry);
            Save();
        }

        protected void DoDeleteEntry(int idx)
        {
            this._entries.RemoveAt(idx);

            this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
        }

        protected virtual void Save()
        {
        }

        private int _maxEntries;
        private List<T> _entries;

        public SimpleMRUModel(int maxEntries)
        {
            this._entries = new List<T>();
            this._maxEntries = maxEntries;
        }
    }
}
