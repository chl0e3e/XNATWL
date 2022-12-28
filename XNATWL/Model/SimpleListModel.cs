using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class SimpleListModel<T> : AbstractListModel<T>
    {
        public override object EntryTooltipAt(int index)
        {
            return null;
        }

        public override bool EntryMatchesPrefix(int index, string prefix)
        {
            object entry = this.EntryAt(index);
            if (entry != null)
            {
                return entry.ToString().ToLower().StartsWith(prefix.ToLower());
            }
            return false;
        }
    }
}
