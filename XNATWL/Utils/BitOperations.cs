namespace XNATWL.Utils
{
    internal class BitOperations
    {
        public static int BitCount(uint value)
        {
            const uint c1 = 0x_55555555u;
            const uint c2 = 0x_33333333u;
            const uint c3 = 0x_0F0F0F0Fu;
            const uint c4 = 0x_01010101u;

            value -= (value >> 1) & c1;
            value = (value & c2) + ((value >> 2) & c2);
            value = (((value + (value >> 4)) & c3) * c4) >> 24;

            return (int)value;
        }

        public static int LowestOneBit(int value)
        {
            return value & -value;
        }

        /// <summary>
        /// Logical (unsigned) right shift — the equivalent of Java's <c>&gt;&gt;&gt;</c> operator.
        /// Shifting an unsigned value zero-fills from the left; C# masks the shift count to the low
        /// 5 bits for 32-bit operands, matching Java's semantics.
        /// </summary>
        public static int RightMove(int value, int pos)
        {
            return (int)((uint)value >> pos);
        }

        public static int NumberOfTrailingZeros(int value)
        {
            if (value == 0)
                return 32;
            int j = 31;
            int i = value << 16;
            if (i != 0)
            {
                j -= 16;
                value = i;
            }
            i = value << 8;
            if (i != 0)
            {
                j -= 8;
                value = i;
            }
            i = value << 4;
            if (i != 0)
            {
                j -= 4;
                value = i;
            }
            i = value << 2;
            if (i != 0)
            {
                j -= 2;
                value = i;
            }

            return j - RightMove(value << 1, 31);
        }
    }
}
