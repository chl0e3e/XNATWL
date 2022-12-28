using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface ListModel<T>
    {
        event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;

        event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;

        event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;

        event EventHandler<ListAllChangedEventArgs> AllChanged;

        int Entries
        {
            get;
        }

        T EntryAt(int index);

        object EntryTooltipAt(int index);

        bool EntryMatchesPrefix(int index, string prefix);
    }

    public class ListAllChangedEventArgs : EventArgs
    {
        public ListAllChangedEventArgs()
        {
        }
    }

    public class ListSubsetChangedEventArgs : EventArgs
    {
        public int First;
        public int Last;

        public ListSubsetChangedEventArgs(int first, int last)
        {
            this.First = first;
            this.Last = last;
        }
    }
}
