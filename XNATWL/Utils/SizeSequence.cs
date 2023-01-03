using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class SizeSequence
    {
        private static int INITIAL_CAPACITY = 64;

        protected int[] table;
        protected internal int size;
        protected int defaultValue;

        public SizeSequence() : this(INITIAL_CAPACITY)
        {
           
        }

        public SizeSequence(int initialCapacity)
        {
            table = new int[initialCapacity];
        }

        public int getPosition(int index)
        {
            int low = 0;
            int high = size;
            int result = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                if (index <= mid)
                {
                    high = mid;
                }
                else
                {
                    result += table[mid];
                    low = mid + 1;
                }
            }
            return result;
        }

        public int getEndPosition()
        {
            int low = 0;
            int high = size;
            int result = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                result += table[mid];
                low = mid + 1;
            }
            return result;
        }

        public int getIndex(int position)
        {
            int low = 0;
            int high = size;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                int pos = table[mid];
                if (position < pos)
                {
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                    position -= pos;
                }
            }
            return low;
        }

        public int getSize(int index)
        {
            return getPosition(index + 1) - getPosition(index);
        }

        public bool setSize(int index, int size)
        {
            int delta = size - getSize(index);
            if (delta != 0)
            {
                adjustSize(index, delta);
                return true;
            }
            return false;
        }

        protected void adjustSize(int index, int delta)
        {
            int low = 0;
            int high = size;

            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                if (index <= mid)
                {
                    table[mid] += delta;
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }
        }

        protected int toSizes(int low, int high, int[] dst)
        {
            int subResult = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                int pos = table[mid];
                dst[mid] = pos - toSizes(low, mid, dst);
                subResult += pos;
                low = mid + 1;
            }
            return subResult;
        }

        protected int fromSizes(int low, int high)
        {
            int subResult = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                int pos = table[mid] + fromSizes(low, mid);
                table[mid] = pos;
                subResult += pos;
                low = mid + 1;
            }
            return subResult;
        }

        public void insert(int index, int count)
        {
            int newSize = size + count;
            if (newSize >= table.Length)
            {
                int[] sizes = new int[newSize];
                toSizes(0, size, sizes);
                table = sizes;
            }
            else
            {
                toSizes(0, size, table);
            }
            Array.Copy(table, index, table, index + count, size - index);
            size = newSize;
            initializeSizes(index, count);
            fromSizes(0, newSize);
        }

        public void remove(int index, int count)
        {
            toSizes(0, size, table);
            int newSize = size - count;
            Array.Copy(table, index + count, table, index, newSize - index);
            size = newSize;
            fromSizes(0, newSize);
        }

        public void initializeAll(int count)
        {
            if (table.Length < count)
            {
                table = new int[count];
            }
            size = count;
            initializeSizes(0, count);
            fromSizes(0, count);
        }

        public void setDefaultValue(int defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        protected internal virtual void initializeSizes(int index, int count)
        {
            for(int i = index; i < index+count; i++)
            {
                table[i] = defaultValue;
            }
        }
    }
}
