using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class TextUtil
    {
        public static string ToCharListNumber(int value, string list)
        {
            if(value < 1)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            int pos = 16;
            char[] tmp = new char[pos];

            do {
                tmp[--pos] = list[--value % list.Length];
                value /= list.Length;
            }while(value > 0);

            return new String(tmp, pos, tmp.Length - pos);
        }
    }
}
