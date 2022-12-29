using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AbstractEnumModel<T> : EnumModel<T> where T : struct, IConvertible
    {
        public abstract T Value { get; set; }

        public abstract event EventHandler<EnumChangedEventArgs<T>> Changed;

        public Type Class
        {
            get
            {
                return this._type;
            }
        }

        private Type _type;

        public AbstractEnumModel(Type type)
        {
            this._type = type;
        }
    }
}
