using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface MRUModel<T> : ListModel<T>
    {
        int MaxEntries
        {
            get;
        }

        int Entries
        {
            get;
        }

        void Add(T entry);

        void RemoveAt(int entry);
    }
}
