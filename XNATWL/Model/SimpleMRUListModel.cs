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
    /// <summary>
    /// A non persistent MRU list implementation
    /// </summary>
    /// <typeparam name="T">the data type stored in this MRU model</typeparam>
    public class SimpleMRUListModel<T> : MRUListModel<T>
    {
        public int MaxEntries
        {
            get
            {
                return this._maxEntries;
            }
        }

        public int MRUEntries
        {
            get
            {
                return this._entries.Count;
            }
        }

        public int Entries
        {
            get
            {
                return this._entries.Count;
            }
        }

        public event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public event EventHandler<ListAllChangedEventArgs> AllChanged;

        public void Add(T entry)
        {
            int idx = this._entries.IndexOf(entry);

            if (idx >= 0)
            {
                DoDeleteEntry(idx);
            }
            else if (_entries.Count == this._maxEntries)
            {
                DoDeleteEntry(this._maxEntries - 1);
            }

            this._entries.Insert(0, entry);
            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(0, 0));
            this.Save();
        }

        public T EntryAt(int index)
        {
            return this._entries[index];
        }

        public bool EntryMatchesPrefix(int index, string prefix)
        {
            return false;
        }

        public object EntryTooltipAt(int index)
        {
            return null;
        }

        public void RemoveAt(int entry)
        {
            if (entry < 0 && entry >= this._entries.Count)
            {
                throw new IndexOutOfRangeException();
            }

            DoDeleteEntry(entry);
            Save();
        }

        protected void DoDeleteEntry(int idx)
        {
            this._entries.RemoveAt(idx);

            this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
        }

        protected virtual void Save()
        {
        }

        protected int _maxEntries;
        protected List<T> _entries;

        public SimpleMRUListModel(int maxEntries)
        {
            this._entries = new List<T>();
            this._maxEntries = maxEntries;
        }
    }
}
