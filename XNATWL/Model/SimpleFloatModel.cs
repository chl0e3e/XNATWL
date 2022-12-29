using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleFloatModel : AbstractFloatModel
    {
        private float _minValue;
        private float _maxValue;
        private float _value;

        public SimpleFloatModel(float minValue, float maxValue, float value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue > maxValue");
            }

            this._minValue = minValue;
            this._maxValue = maxValue;
            this._value = value;
        }

        public override float Value
        {
            get
            {
                return this._value;
            }

            set
            {
                float limitedValue = Math.Max(this._minValue, Math.Min(this._maxValue, value));
                float old = this._value;
                if (value != old)
                {
                    this._value = limitedValue;
                    this.Changed.Invoke(this, new FloatChangedEventArgs(old, limitedValue));
                }
            }
        }

        public override float MinValue
        {
            get
            {
                return this._minValue;
            }
        }

        public override float MaxValue
        {
            get
            {
                return this._maxValue;
            }
        }

        public override event EventHandler<FloatChangedEventArgs> Changed;
    }
}
