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
    }
}
