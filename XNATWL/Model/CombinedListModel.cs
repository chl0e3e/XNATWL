using System;
using System.Collections.Generic;

namespace XNATWL.Model
{
    public class CombinedListModel<T> : SimpleListModel<T>
    {
        public event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;

        public event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;

        public event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;

        public event EventHandler<ListAllChangedEventArgs> AllChanged;

        private List<Sublist> _sublists;
        private int[] _sublistStarts;
        private SubListsModel _subListsModel;

        public CombinedListModel()
        {
            this._sublists = new List<Sublist>();
            this._sublistStarts = new int[1];
        }

        public override int Entries
        {
            get
            {
                return this._sublistStarts[this._sublistStarts.Length - 1];
            }
        }

        public override T EntryAt(int index)
        {
            Sublist sl = SublistForIndex(index);
            if (sl != null)
            {
                return sl.EntryAt(index - sl.StartIndex);
            }

            throw new IndexOutOfRangeException();
        }

        public int StartIndexOfSublist(int sublistIndex)
        {
            return this._sublists[sublistIndex].StartIndex;
        }

        public ListModel<ListModel<T>> getModelForSubLists()
        {
            if (this._subListsModel == null)
            {
                this._subListsModel = new SubListsModel(this);
            }
            return this._subListsModel;
        }

        public void AddSubList(ListModel<T> model)
        {
            AddSubList(this._sublists.Count, model);
        }

        public void AddSubList(int index, ListModel<T> model)
        {
            Sublist sl = new Sublist(this, model);
            this._sublists.Insert(index, sl);
            this.AdjustStartOffsets();

            int numEntries = sl.Entries;
            if (numEntries > 0)
            {
                this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(sl.StartIndex, sl.StartIndex + numEntries - 1));
            }

            if (this._subListsModel != null)
            {
                this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(index, index));
            }
        }

        public int findSubList(ListModel<T> model)
        {
            for (int i = 0; i < this._sublists.Count; i++)
            {
                Sublist sl = this._sublists[i];
                if (sl.List == model)
                {
                    return i;
                }
            }
            return -1;
        }

        public void removeAllSubLists()
        {
            for (int i = 0; i < this._sublists.Count; i++)
            {
                this._sublists[i].RemoveChangeListener();
            }
            this._sublists.Clear();
            this.AdjustStartOffsets();
            this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            if (this._subListsModel != null)
            {
                this._subListsModel.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            }
        }

        public bool removeSubList(ListModel<T> model)
        {
            int index = findSubList(model);
            if (index >= 0)
            {
                RemoveSubList(index);
                return true;
            }
            return false;
        }

        public ListModel<T> RemoveSubList(int index)
        {
            Sublist sl = this._sublists[index];

            this._sublists.RemoveAt(index);

            sl.RemoveChangeListener();

            this.AdjustStartOffsets();

            int numEntries = sl.Entries;

            if (numEntries > 0)
            {
                this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(sl.StartIndex, sl.StartIndex + numEntries - 1));
            }

            if (this._subListsModel != null)
            {
                this._subListsModel.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(index, index));
            }

            return sl.List;
        }

        public ListModel<ListModel<T>> ModelForSubLists()
        {
            if (this._subListsModel == null)
            {
                this._subListsModel = new SubListsModel(this);
            }

            return this._subListsModel;
        }

        private Sublist SublistForIndex(int index)
        {
            int[] offsets = this._sublistStarts;
            int lo = 0;
            int hi = offsets.Length - 1;
            while (lo <= hi)
            {
                int mid = (int)((uint)(lo + hi) >> 2);
                int delta = offsets[mid] - index;
                if (delta <= 0)
                {
                    lo = mid + 1;
                }
                if (delta > 0)
                {
                    hi = mid - 1;
                }
            }
            if (lo > 0 && lo <= this._sublists.Count)
            {
                Sublist sl = this._sublists[lo - 1];
                return sl;
            }
            return null;
        }

        void AdjustStartOffsets()
        {
            int[] offsets = new int[this._sublists.Count + 1];
            int startIdx = 0;
            for (int idx = 0; idx < this._sublists.Count;)
            {
                Sublist sl = this._sublists[idx];
                sl.StartIndex = startIdx;
                startIdx += sl.Entries;
                offsets[++idx] = startIdx;
            }
            this._sublistStarts = offsets;
        }

        class Sublist
        {
            public ListModel<T> List;
            public CombinedListModel<T> Parent;
            public int StartIndex = 0;

            public Sublist(CombinedListModel<T> parent, ListModel<T> list)
            {
                this.Parent = parent;

                this.List = list;

                this.List.EntriesInserted += List_EntriesInserted;
                this.List.EntriesChanged += List_EntriesChanged;
                this.List.EntriesDeleted += List_EntriesDeleted;
                this.List.AllChanged += List_AllChanged;
            }

            private void List_AllChanged(object sender, ListAllChangedEventArgs e)
            {
                this.Parent.AllChanged.Invoke(this.Parent, new ListAllChangedEventArgs());
            }

            private void List_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
            {
                this.Parent.AdjustStartOffsets();
                this.Parent.EntriesDeleted.Invoke(this.Parent, new ListSubsetChangedEventArgs(StartIndex + e.First, StartIndex + e.Last));
            }

            private void List_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
            {
                this.Parent.EntriesChanged.Invoke(this.Parent, new ListSubsetChangedEventArgs(StartIndex + e.First, StartIndex + e.Last));
            }

            private void List_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
            {
                this.Parent.AdjustStartOffsets();
                this.Parent.EntriesInserted.Invoke(this.Parent, new ListSubsetChangedEventArgs(StartIndex + e.First, StartIndex + e.Last));
            }

            public int Entries
            {
                get
                {
                    return this.List.Entries;
                }
            }

            public void RemoveChangeListener()
            {
                this.List.EntriesInserted -= List_EntriesInserted;
                this.List.EntriesChanged -= List_EntriesChanged;
                this.List.EntriesDeleted -= List_EntriesDeleted;
            }

            public bool EntryMatchesPrefix(int index, string prefix)
            {
                return this.List.EntryMatchesPrefix(index, prefix);
            }

            public object EntryTooltipAt(int index)
            {
                return this.List.EntryTooltipAt(index);
            }

            public T EntryAt(int index)
            {
                return this.List.EntryAt(index);
            }
        }

        class SubListsModel : SimpleListModel<ListModel<T>>
        {
            private CombinedListModel<T> Parent;

            public SubListsModel(CombinedListModel<T> combinedListModel)
            {
                this.Parent = combinedListModel;
            }

            public override int Entries
            {
                get
                {
                    return this.Parent._sublists.Count;
                }
            }

            public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
            public override event EventHandler<ListAllChangedEventArgs> AllChanged;

            public override ListModel<T> EntryAt(int index)
            {
                return this.Parent._sublists[index].List;
            }

            protected void FireEntriesInserted(object sender, ListSubsetChangedEventArgs e)
            {
                this.EntriesInserted.Invoke(sender, e);
            }

            protected void FireEntriesDeleted(object sender, ListSubsetChangedEventArgs e)
            {
                this.EntriesDeleted.Invoke(sender, e);
            }

            protected void FireEntriesChanged(object sender, ListSubsetChangedEventArgs e)
            {
                this.EntriesChanged.Invoke(sender, e);
            }
        }
    }
}
