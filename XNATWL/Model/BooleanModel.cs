using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface BooleanModel
    {
        event EventHandler<BooleanChangedEventArgs> Changed;

        bool Value
        {
            get;
        }
    }

    public class BooleanChangedEventArgs : EventArgs
    {
        public bool New;
        public bool Old;

        public BooleanChangedEventArgs(bool @old, bool @new)
        {
            this.Old = @old;
            this.New = @new;
        }
    }
}
