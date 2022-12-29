using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class OptionEnumModel<T> : AbstractOptionModel where T : struct, IConvertible
    {
        public override bool Value
        {
            get
            {
                return ((EnumModel<T>)this._optionState).Value.Equals(this._optionCode);
            }
            set
            {
                if (value)
                {
                    ((EnumModel<T>)this._optionState).Value = (T)this._optionCode;
                }
            }
        }

        public override event EventHandler<BooleanChangedEventArgs> Changed;

        public OptionEnumModel(EnumModel<T> optionState, int optionCode) : base(optionState, optionCode)
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException("OptionEnumModel must be given a type of enum.");
            }
        }

        public override void WireEventToOptionState()
        {
            ((EnumModel<T>)this._optionState).Changed += (sender, e) =>
            {
                this.Changed.Invoke(this, new BooleanChangedEventArgs(e.Old.Equals(this._optionCode), e.New.Equals(this._optionCode)));
            };
        }
    }
}
