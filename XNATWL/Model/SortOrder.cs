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

using System.Collections.Generic;

namespace XNATWL.Model
{
    /// <summary>
    /// An enum which represents basic sort order
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class SortOrderStatics
    {
        public static SortOrder SortOrder_Invert(SortOrder order)
        {
            if (order == SortOrder.Ascending)
            {
                return SortOrder.Descending;
            }

            return SortOrder.Ascending;
        }

        /// <summary>
        /// Mirrors Java's SortOrder.map(Comparator): returns the comparator unchanged for
        /// Ascending, or a reversed view for Descending. Reverses by swapping the operands
        /// (like Collections.reverseOrder) rather than negating, to stay overflow-safe.
        /// </summary>
        public static IComparer<T> Map<T>(SortOrder order, IComparer<T> comparator)
        {
            if (order == SortOrder.Descending)
            {
                return new ReverseComparer<T>(comparator);
            }
            return comparator;
        }

        private class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _comparator;

            public ReverseComparer(IComparer<T> comparator)
            {
                this._comparator = comparator;
            }

            public int Compare(T a, T b)
            {
                return this._comparator.Compare(b, a);
            }
        }
    }
}
