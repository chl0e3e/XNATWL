using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface PropertyList<T>
    {
        int Count
        {
            get;
        }

        Property<T> PropertyAt(int index);
    }
}
