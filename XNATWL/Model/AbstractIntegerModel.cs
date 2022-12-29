using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AbstractIntegerModel : IntegerModel
    {
        public abstract int Value { get; set; }
        public abstract int MinValue { get; }
        public abstract int MaxValue { get; }

        public abstract event EventHandler<IntegerChangedEventArgs> Changed;

        public AbstractIntegerModel()
        {

        }
    }
}
