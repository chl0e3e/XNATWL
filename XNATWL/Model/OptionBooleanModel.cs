using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class OptionBooleanModel : AbstractOptionModel
    {
        public override bool Value
        {
            get
            {
                return ((IntegerModel)this._optionState).Value.Equals(this._optionCode);
            }
            set
            {
                if (value)
                {
                    ((IntegerModel)this._optionState).Value = (int) this._optionCode;
                }
            }
        }

        public override event EventHandler<BooleanChangedEventArgs> Changed;

        public OptionBooleanModel(IntegerModel optionState, int optionCode) : base(optionState, optionCode)
        {

        }

        public override void WireEventToOptionState()
        {
            ((IntegerModel)this._optionState).Changed += (sender, e) =>
            {
                this.Changed.Invoke(this, new BooleanChangedEventArgs(e.Old.Equals(this._optionCode), e.New.Equals(this._optionCode)));
            };
        }
    }
}
