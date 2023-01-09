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
    public abstract class AbstractTableSelectionModel : TableSelectionModel
    {
        private int _leadIndex;
        private int _anchorIndex;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public int LeadIndex
        {
            get
            {
                return _leadIndex;
            }

            set
            {
                _leadIndex = value;
            }
        }

        public int AnchorIndex
        {
            get
            {
                return _anchorIndex;
            }

            set
            {
                _anchorIndex = value;
            }
        }

        public abstract int FirstSelected { get; }
        public abstract int LastSelected { get; }
        public abstract int[] Selection { get; }

        public virtual void RowsDeleted(int index, int count)
        {
            if (this._leadIndex >= index)
            {
                this._leadIndex = Math.Max(index, this._leadIndex - count);
            }

            if (this._anchorIndex >= index)
            {
                this._anchorIndex = Math.Max(index, this._anchorIndex - count);
            }
        }

        public virtual void RowsInserted(int index, int count)
        {
            if (this._leadIndex >= index)
            {
                this._leadIndex += count;
            }

            if (this._anchorIndex >= index)
            {
                this._anchorIndex += count;
            }
        }

        protected void UpdateLeadAndAnchor(int index0, int index1)
        {
            this._anchorIndex = index0;
            this._leadIndex = index1;
        }

        public abstract void ClearSelection();

        public abstract void AddSelection(int index0, int index1);

        public abstract void InvertSelection(int index0, int index1);

        public abstract void RemoveSelection(int index0, int index1);

        public abstract bool IsSelected(int index);

        public abstract bool HasSelection();

        public abstract void SetSelection(int index0, int index1);

        public void FireSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            this.SelectionChanged.Invoke(sender, e);
        }
    }
}
