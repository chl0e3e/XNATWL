/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace XNATWL.Utils
{
    public class SizeSequence
    {
        private static int INITIAL_CAPACITY = 64;

        protected int[] _table;
        protected internal int _size;
        protected int _defaultValue;

        public SizeSequence() : this(INITIAL_CAPACITY)
        {
           
        }

        public SizeSequence(int initialCapacity)
        {
            _table = new int[initialCapacity];
        }

        public int GetPosition(int index)
        {
            int low = 0;
            int high = _size;
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
                    result += _table[mid];
                    low = mid + 1;
                }
            }
            return result;
        }

        public int GetEndPosition()
        {
            int low = 0;
            int high = _size;
            int result = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                result += _table[mid];
                low = mid + 1;
            }
            return result;
        }

        public int GetIndex(int position)
        {
            int low = 0;
            int high = _size;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                int pos = _table[mid];
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

        public int GetSize(int index)
        {
            return GetPosition(index + 1) - GetPosition(index);
        }

        public bool SetSize(int index, int size)
        {
            int delta = size - GetSize(index);
            if (delta != 0)
            {
                AdjustSize(index, delta);
                return true;
            }
            return false;
        }

        protected void AdjustSize(int index, int delta)
        {
            int low = 0;
            int high = _size;

            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                if (index <= mid)
                {
                    _table[mid] += delta;
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }
        }

        protected int ToSizes(int low, int high, int[] dst)
        {
            int subResult = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                int pos = _table[mid];
                dst[mid] = pos - ToSizes(low, mid, dst);
                subResult += pos;
                low = mid + 1;
            }
            return subResult;
        }

        protected int FromSizes(int low, int high)
        {
            int subResult = 0;
            while (low < high)
            {
                int mid = BitOperations.RightMove(low + high, 1);
                int pos = _table[mid] + FromSizes(low, mid);
                _table[mid] = pos;
                subResult += pos;
                low = mid + 1;
            }
            return subResult;
        }

        public void Insert(int index, int count)
        {
            int newSize = _size + count;
            if (newSize >= _table.Length)
            {
                int[] sizes = new int[newSize];
                ToSizes(0, _size, sizes);
                _table = sizes;
            }
            else
            {
                ToSizes(0, _size, _table);
            }
            Array.Copy(_table, index, _table, index + count, _size - index);
            _size = newSize;
            InitializeSizes(index, count);
            FromSizes(0, newSize);
        }

        public void Remove(int index, int count)
        {
            ToSizes(0, _size, _table);
            int newSize = _size - count;
            Array.Copy(_table, index + count, _table, index, newSize - index);
            _size = newSize;
            FromSizes(0, newSize);
        }

        public void InitializeAll(int count)
        {
            if (_table.Length < count)
            {
                _table = new int[count];
            }
            _size = count;
            InitializeSizes(0, count);
            FromSizes(0, count);
        }

        public void SetDefaultValue(int defaultValue)
        {
            this._defaultValue = defaultValue;
        }

        protected internal virtual void InitializeSizes(int index, int count)
        {
            for(int i = index; i < index+count; i++)
            {
                _table[i] = _defaultValue;
            }
        }
    }
}
