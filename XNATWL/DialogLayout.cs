using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class DialogLayout
    {
        public class Gap
        {
            public int min;
            public int preferred;
            public int max;

            public Gap() : this(0, 0, 32767)
            {

            }

            public Gap(int size) : this(size, size, size)
            {

            }

            public Gap(int min, int preferred) : this(min, preferred, 32767)
            {

            }

            public Gap(int min, int preferred, int max)
            {
                if (min < 0)
                {
                    throw new ArgumentOutOfRangeException("min");
                }
                if (preferred < min)
                {
                    throw new ArgumentOutOfRangeException("preferred");
                }
                if (max < 0 || (max > 0 && max < preferred))
                {
                    throw new ArgumentOutOfRangeException("max");
                }

                this.min = min;
                this.preferred = preferred;
                this.max = max;
            }
        }

    }
}
