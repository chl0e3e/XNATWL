using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleBooleanModel : BooleanModel
    {
        private bool _value;

        public bool Value
        {
            get
            {
                return _value;
            }

            set
            {
                bool old = this._value;
                if (value != old)
                {
                    this._value = value;
                    this.Changed.Invoke(this, new BooleanChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<BooleanChangedEventArgs> Changed;

        public SimpleBooleanModel() : this(false)
        {

        }
        
        public SimpleBooleanModel(bool value)
        {
            this._value = value;
        }
    }
}
