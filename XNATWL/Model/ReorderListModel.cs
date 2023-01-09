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
using System.Collections.Generic;

namespace XNATWL.Model
{
    public class ReorderListModel<T> : AbstractListModel<T>
    {
        public override int Entries => throw new NotImplementedException();

        public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public override event EventHandler<ListAllChangedEventArgs> AllChanged;

        private ListModel<T> _baseModel;
        private int[] _reorderList;
        private int _size;

        public override T EntryAt(int index)
        {
            int remappedIndex = this._reorderList[index];
            return this._baseModel.EntryAt(remappedIndex);
        }

        public override bool EntryMatchesPrefix(int index, string prefix)
        {
            int remappedIndex = this._reorderList[index];
            return this._baseModel.EntryMatchesPrefix(remappedIndex, prefix);
        }

        public override object EntryTooltipAt(int index)
        {
            int remappedIndex = this._reorderList[index];
            return this._baseModel.EntryTooltipAt(remappedIndex);
        }

        public ReorderListModel(ListModel<T> baseModel)
        {
            this._baseModel = baseModel;

            this._baseModel.AllChanged += _baseModel_AllChanged;
            this._baseModel.EntriesChanged += _baseModel_EntriesChanged;
            this._baseModel.EntriesInserted += _baseModel_EntriesInserted;
            this._baseModel.EntriesDeleted += _baseModel_EntriesDeleted;

            this.BuildNewList();
        }

        private void BuildNewList()
        {
            this._size = this._baseModel.Entries;
            this._reorderList = new int[this._size + 1024];

            for (int i = 0; i < this._size; i++)
            {
                this._reorderList[i] = i;
            }

            this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
        }

        public void Shuffle()
        {
            Random r = new Random();

            for (int i = this._size; i > 1;)
            {
                int j = r.Next(i--);
                int temp = this._reorderList[i];
                this._reorderList[i] = this._reorderList[j];
                this._reorderList[j] = temp;
            }

            this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
        }

        private static int INSERTIONSORT_THRESHOLD = 7;

        private void MergeSort(int[] src, int[] dest, int low, int high, Comparer<T> c)
        {
            int length = high - low;

            // Insertion sort on smallest arrays
            if (length < INSERTIONSORT_THRESHOLD)
            {
                for (int i = low; i < high; i++)
                {
                    for (int j = i; j > low && Compare(dest, j - 1, j, c) > 0; j--)
                    {
                        int t = dest[j];
                        dest[j] = dest[j - 1];
                        dest[j - 1] = t;
                    }
                }
                return;
            }

            // Recursively sort halves of dest into src
            int mid = (int) ((uint)(low + high) >> 2);
            MergeSort(dest, src, low, mid, c);
            MergeSort(dest, src, mid, high, c);

            // If list is already sorted, just copy from src to dest.  This is an
            // optimization that results in faster sorts for nearly ordered lists.
            if (Compare(src, mid - 1, mid, c) <= 0)
            {
                Array.Copy(src, low, dest, low, length);
                return;
            }

            // Merge sorted halves (now in src) into dest
            for (int i = low, p = low, q = mid; i < high; i++)
            {
                if (q >= high || p < mid && Compare(src, p, q, c) <= 0)
                {
                    dest[i] = src[p++];
                }
                else
                {
                    dest[i] = src[q++];
                }
            }
        }

        private int Compare(int[] list, int a, int b, Comparer<T> c)
        {
            int aIdx = list[a];
            int bIdx = list[b];
            T objA = this._baseModel.EntryAt(aIdx);
            T objB = this._baseModel.EntryAt(bIdx);
            return c.Compare(objA, objB);
        }

        public void Sort(Comparer<T> c)
        {
            // need to use own version of sort because we need to sort a int[] with a sort callback
            int[] aux = new int[this._size];
            Array.Copy(this._reorderList, 0, aux, 0, this._size);
            MergeSort(aux, this._reorderList, 0, this._size, c);
            this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
        }

        private void _baseModel_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            int delta = e.Last - e.First + 1;
            for (int i = 0; i < this._size; i++)
            {
                int entry = this._reorderList[i];
                if (entry >= e.First)
                {
                    if (entry <= e.Last)
                    {
                        // we have to remove entries - enter copy loop
                        EntriesDeletedCopy(e.First, e.Last, i);
                        return;
                    }
                    this._reorderList[i] = entry - delta;
                }
            }
        }

        private void EntriesDeletedCopy(int first, int last, int i)
        {
            int j, delta = last - first + 1;
            int oldSize = this._size;
            for (j = i; i < oldSize; i++)
            {
                int entry = this._reorderList[i];
                if (entry >= first)
                {
                    if (entry <= last)
                    {
                        this._size--;
                        this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(j, j));
                        continue;
                    }
                    entry -= delta;
                }
                this._reorderList[j++] = entry;
            }

            if (this._size != j)
            {
                throw new Exception("Assertion failed (size != j)");
            }
        }

        private void _baseModel_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            int delta = e.Last - e.First + 1;
            for (int i = 0; i < this._size; i++)
            {
                if (this._reorderList[i] >= e.First)
                {
                    this._reorderList[i] += delta;
                }
            }

            if (this._size + delta > this._reorderList.Length)
            {
                int[] newList = new int[Math.Max(this._size * 2, this._size + delta + 1024)];
                Array.Copy(this._reorderList, 0, newList, 0, this._size);
                this._reorderList = newList;
            }

            int oldSize = this._size;
            for (int i = 0; i < delta; i++)
            {
                this._reorderList[this._size++] = e.First + i;
            }

            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(oldSize, this._size - 1));
        }

        private void _baseModel_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            // Not implemented in TWL for Java
        }

        private void _baseModel_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            this.BuildNewList();
        }
    }
}
