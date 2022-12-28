using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface ListSelectionModel<T> : IntegerModel
    {
        ListModel<T> Model
        {
            get;
        }

        T SelectedEntry
        {
            get;
            set;
        }

        bool SetSelectedEntryWithDefault(T entry, int defaultIndex);
    }
}
