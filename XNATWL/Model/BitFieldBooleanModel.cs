using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class BitFieldBooleanModel : BooleanModel
    {
        private IntegerModel _bitfield;
        private int _bitmask;

        public event EventHandler<BooleanChangedEventArgs> Changed;

        public bool Value
        {
            get
            {
                return ((this._bitfield.Value & this._bitmask) != 0);
            }
            set
            {
                int oldBFValue = this._bitfield.Value;
                int newBFValue = value ? (oldBFValue | this._bitmask) : (oldBFValue & ~this._bitmask);
                if (oldBFValue != newBFValue)
                {
                    this._bitfield.Value = newBFValue;
                    // bitfield's callback will call our callback
                }
            }
        }

        public BitFieldBooleanModel(IntegerModel bitfield, int bit)
        {
            if (bit < 0 || bit > 30)
            {
                throw new ArgumentOutOfRangeException("invalid bit index");
            }
            if (bitfield.MinValue != 0)
            {
                throw new ArgumentOutOfRangeException("bitfield.getMinValue() != 0");
            }
            int bitfieldMax = bitfield.MaxValue;
            if ((bitfieldMax & (bitfieldMax + 1)) != 0)
            {
                throw new ArgumentOutOfRangeException("bitfield.getmaxValue() must eb 2^x");
            }
            if (bitfieldMax < (1 << bit))
            {
                throw new ArgumentOutOfRangeException("bit index outside of bitfield range");
            }

            _bitfield = bitfield;
            _bitmask = 1 << bit;

            bitfield.Changed += (sender, e) =>
            {
                this.Changed.Invoke(sender, new BooleanChangedEventArgs(((e.Old & this._bitmask) != 0), ((e.New & this._bitmask) != 0)));
            };
        }
    }
}
