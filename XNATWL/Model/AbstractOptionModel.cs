using System;

namespace XNATWL.Model
{
    public abstract class AbstractOptionModel : BooleanModel
    {
        public abstract event EventHandler<BooleanChangedEventArgs> Changed;

        public abstract bool Value { get; set; }

        protected object _optionState;
        protected object _optionCode;

        public AbstractOptionModel(object optionState, object optionCode)
        {
            this._optionState = optionState;
            this._optionCode = optionCode;

            this.WireEventToOptionState();
        }

        public abstract void WireEventToOptionState();
    }
}
