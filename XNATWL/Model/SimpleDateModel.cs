using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleDateModel : DateModel
    {
        private long _date;

        public SimpleDateModel()
        {
            this._date = DateTime.Now.Ticks;
        }

        public SimpleDateModel(long date)
        {
            this._date = date;
        }

        public long Value
        {
            get
            {
                return this._date;
            }
            set
            {
                long old = this._date;
                if (value != old)
                {
                    this._date = value;
                    this.Changed.Invoke(this, new DateChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<DateChangedEventArgs> Changed;
    }
}
