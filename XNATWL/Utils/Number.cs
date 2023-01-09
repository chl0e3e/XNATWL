namespace XNATWL.Utils
{
    public class Number
    {
        internal BigRational _number;

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

        internal Number(BigRational number)
        {
            this._number = number;
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

        public static Number operator +(Number a) => a;
        public static Number operator -(Number a) => new Number(-a._number);

        public static Number operator +(Number a, Number b)
            => new Number(a._number + b._number);

        public static Number operator -(Number a, Number b)
            => new Number(a._number - b._number);

        public static Number operator *(Number a, Number b)
            => new Number(a._number * b._number);

        public static Number operator /(Number a, Number b)
            => new Number(a._number / b._number);
    }
}
