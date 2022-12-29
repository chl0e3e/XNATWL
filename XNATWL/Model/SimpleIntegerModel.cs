using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleIntegerModel : IntegerModel
    {
        private int _minValue;
        private int _maxValue;
        private int _value;

        public SimpleIntegerModel(int minValue, int maxValue, int value)
        {
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._value = value;
        }

        public int Value
        {
            get
            {
                return this._value;
            }

            set
            {
                int old = this._value;
                if (value != old)
                {
                    this._value = value;
                    this.Changed.Invoke(this, new IntegerChangedEventArgs(old, value));
                }
            }
        }

        public int MaxValue
        {
            get
            {
                return this._maxValue;
            }
        }

        public int MinValue
        {
            get
            {
                return this._minValue;
            }
        }

        public event EventHandler<IntegerChangedEventArgs> Changed;
    }
}
