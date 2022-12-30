using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class Number
    {
        private BigRational _number;

        public Number(float n)
        {
            this._number = n;
        }

        public Number(int n)
        {
            this._number = n;
        }

        public Number(long n)
        {
            this._number = n;
        }

        public bool IsRational()
        {
            string numAsString = _number.ToString();
            if (numAsString.Contains("."))
            {
                foreach(char i in numAsString.Split('.')[1].ToCharArray())
                {
                    if (i != '0')
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int intValue()
        {
            return ((int)_number);
        }

        public float floatValue()
        {
            return ((float)_number);
        }

        public double doubleValue()
        {
            return ((double)_number);
        }
    }
}
