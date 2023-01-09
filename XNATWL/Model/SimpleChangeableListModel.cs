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
using System.Collections.ObjectModel;

namespace XNATWL.Model
{
    public class SimpleChangeableListModel<T> : SimpleListModel<T>
    {
        private List<T> _content;

        public SimpleChangeableListModel()
        {
            this._content = new List<T>();
        }

        public SimpleChangeableListModel(ICollection<T> content)
        {
            this._content = new List<T>(content);
        }

        public SimpleChangeableListModel(params T[] content)
        {
            this._content = new List<T>(content);
        }

        public override int Entries
        {
            get
            {
                return this._content.Count;
            }
        }

        public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public override event EventHandler<ListAllChangedEventArgs> AllChanged;

        public override T EntryAt(int index)
        {
            return this._content[index];
        }

        public void AddElement(T element)
        {
            InsertElement(this.Entries, element);
        }

        public void AddElements(Collection<T> elements)
        {
            InsertElements(this.Entries, elements);
        }

        public void AddElements(params T[] elements)
        {
            InsertElements(this.Entries, elements);
        }

        public void InsertElement(int idx, T element)
        {
            this._content.Insert(idx, element);
            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
        }

        public void InsertElements(int idx, ICollection<T> elements)
        {
            this._content.InsertRange(idx, elements);
            this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx + elements.Count - 1));
        }

        public void InsertElements(int idx, params T[] elements)
        {
            InsertElements(idx, new List<T>(elements));
        }

        public T RemoveElement(int idx)
        {
            T result = this._content[idx];
            this._content.RemoveAt(idx);
            this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
            return result;
        }

        public T SetElement(int idx, T element)
        {
            this._content[idx] = element;
            this.EntriesChanged.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
            return element;
        }

        public int FindElement(object element)
        {
            return this._content.IndexOf((T) element);
        }

        public void clear()
        {
            this._content.Clear();
            this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
        }
    }
}
