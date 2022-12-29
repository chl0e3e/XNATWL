using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface EnumModel<T> where T : struct, IConvertible
    {
        T Value
        {
            get;
            set;
        }

        Type Class
        {
            get;
        }

        event EventHandler<EnumChangedEventArgs<T>> Changed;
    }

    public class EnumChangedEventArgs<T> : EventArgs where T : struct, IConvertible
    {
        public T New;
        public T Old;

        public EnumChangedEventArgs(T @new, T @old)
        {
            New = @new;
            Old = @old;
        }
    }
}
