using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface IntegerModel
    {
        event EventHandler<IntegerChangedEventArgs> Changed;

        int Value
        {
            get;
            set;
        }

        int MinValue
        {
            get;
        }

        int MaxValue
        {
            get;
        }
    }

    public class IntegerChangedEventArgs : EventArgs
    {
        public int New;
        public int Old;

        public IntegerChangedEventArgs(int _old, int _new)
        {
            this.Old = _old;
            this.New = _new;
        }
    }
}
