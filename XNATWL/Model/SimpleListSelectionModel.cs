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

namespace XNATWL.Model
{
    public class SimpleListSelectionModel<T> : ListSelectionModel<T>
    {
        private ListModel<T> _listModel;
        private int _selected;

        public SimpleListSelectionModel(ListModel<T> listModel)
        {
            this._listModel = listModel;
        }

        public ListModel<T> Model
        {
            get
            {
                return this._listModel;
            }
        }

        public T SelectedEntry
        {
            get
            {
                if (this._selected >= 0 && this._selected < this._listModel.Entries)
                {
                    return this._listModel.EntryAt(this._selected);
                }
                else
                {
                    return default(T); // TODO
                }
            }

            set
            {
                SetSelectedEntryWithDefault(value, -1);
            }
        }

        public int Value
        {
            get
            {
                return this._selected;
            }

            set
            {
                int old = this._selected;
                if (value != old)
                {
                    this._selected = value;
                    this.Changed.Invoke(this, new IntegerChangedEventArgs(old, value));
                }
            }
        }

        public int MinValue
        {
            get
            {
                return this._listModel.Entries - 1;
            }
        }

        public int MaxValue
        {
            get
            {
                return -1;
            }
        }

        public event EventHandler<IntegerChangedEventArgs> Changed;

        public bool SetSelectedEntryWithDefault(T entry, int defaultIndex)
        {
            for (int i = 0, n = this._listModel.Entries; i < n; i++)
            {
                if (entry.Equals(this._listModel.EntryAt(i)))
                {
                    this.Value = i;
                    return true;
                }
            }

            this.Value = defaultIndex;
            return false;
        }
    }
}
