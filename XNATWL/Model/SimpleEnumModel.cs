using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleEnumModel<T> : AbstractEnumModel<T> where T : struct, IConvertible
    {
        private T _value;

        public SimpleEnumModel(Type type, T value) : base(type)
        {
            this._value = value;
        }

        public override T Value
        {
            get
            {
                return this._value;
            }

            set
            {
                T old = this._value;
                if (!value.Equals(old))
                {
                    this._value = value;
                    this.Changed.Invoke(this, new EnumChangedEventArgs<T>(old, value));
                }
            }
        }

        public override event EventHandler<EnumChangedEventArgs<T>> Changed;
    }
}
