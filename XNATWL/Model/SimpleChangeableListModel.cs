using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XNATWL.Model
{
    public class SimpleChangeableListModel<T> : SimpleListModel<T>
    {
        private List<T> _content;

        public SimpleChangeableListModel()
        {
            this._content = new List<T>();
        }

        public SimpleChangeableListModel(ICollection<T> content)
        {
            this._content = new List<T>(content);
        }

        public SimpleChangeableListModel(params T[] content)
        {
            this._content = new List<T>(content);
        }

        public override int Entries
        {
            get
            {
                return this._content.Count;
            }
        }

        public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public override event EventHandler<ListAllChangedEventArgs> AllChanged;

        public override T EntryAt(int index)
        {
            return this._content[index];
        }

        public void AddElement(T element)
        {
            InsertElement(this.Entries, element);
        }

        public void AddElements(Collection<T> elements)
        {
            InsertElements(this.Entries, elements);
        }

        public void AddElements(params T[] elements)
        {
            InsertElements(this.Entries, elements);
        }

        public void InsertElement(int idx, T element)
        {
            this._content.Insert(idx, element);
            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
        }

        public void InsertElements(int idx, ICollection<T> elements)
        {
            this._content.InsertRange(idx, elements);
            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx + elements.Count - 1));
        }

        public void InsertElements(int idx, params T[] elements)
        {
            InsertElements(idx, new List<T>(elements));
        }

        public T RemoveElement(int idx)
        {
            T result = this._content[idx];
            this._content.RemoveAt(idx);
            this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
            return result;
        }

        public T SetElement(int idx, T element)
        {
            this._content[idx] = element;
            this.EntriesChanged.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
            return element;
        }

        public int FindElement(object element)
        {
            return this._content.IndexOf((T) element);
        }

        public void clear()
        {
            this._content.Clear();
            this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
        }
    }
}
