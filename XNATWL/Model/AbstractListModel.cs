using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AbstractListModel<T> : ListModel<T>
    {
        public abstract int Entries { get; }

        public abstract event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public abstract event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public abstract event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public abstract event EventHandler<ListAllChangedEventArgs> AllChanged;

        public abstract T EntryAt(int index);

        public abstract bool EntryMatchesPrefix(int index, string prefix);

        public abstract object EntryTooltipAt(int index);
    }
}
