using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface DateModel
    {
        event EventHandler<DateChangedEventArgs> Changed;

        long Value
        {
            get;
            set;
        }
    }

    public class DateChangedEventArgs : EventArgs
    {
        public long New;
        public long Old;

        public DateChangedEventArgs(long @old, long @new)
        {
            this.Old = @old;
            this.New = @new;
        }
    }
}
