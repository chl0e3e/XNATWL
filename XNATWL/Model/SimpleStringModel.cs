using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleStringModel : StringModel
    {
        private string _value;

        public string Value
        {
            get
            {
                return this._value;
            }

            set
            {
                string old = this._value;
                if (value != old)
                {
                    this._value = value;
                    this.Changed.Invoke(this, new StringChangedEventArgs(old, value));
                }
            }
        }

        public SimpleStringModel(string value)
        {
            this._value = value;
        }

        public event EventHandler<StringChangedEventArgs> Changed;
    }
}
