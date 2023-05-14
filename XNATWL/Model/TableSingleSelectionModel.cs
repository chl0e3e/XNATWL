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
    public class TableSingleSelectionModel : AbstractTableSelectionModel
    {
        public static int NO_SELECTION = -1;

        private int _selection;

        public override int FirstSelected
        {
            get
            {
                return this._selection;
            }
        }

        public override int LastSelected => throw new NotImplementedException();

        public override int[] Selection
        {
            get
            {
                if (this._selection >= 0)
                {
                    return new int[] { this._selection };
                }

                return new int[0];
            }
        }

        public override void RowsDeleted(int index, int count)
        {
            int[] oldSelection = this.Selection;
            bool changed = false;

            if (this._selection >= index)
            {
                if (this._selection < index + count)
                {
                    this._selection = NO_SELECTION;
                }
                else
                {
                    this._selection -= count;
                }

                changed = true;
            }

            base.RowsDeleted(index, count);

            if (changed)
            {
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void RowsInserted(int index, int count)
        {
            int[] oldSelection = this.Selection;
            bool changed = false;
            if (this._selection >= index)
            {
                this._selection += count;
                changed = true;
            }

            base.RowsInserted(index, count);

            if (changed)
            {
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void AddSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;
            this.UpdateLeadAndAnchor(index0, index1);
            this._selection = index1;
            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        public override void ClearSelection()
        {
            int[] oldSelection = this.Selection;

            if (this.HasSelection())
            {
                this._selection = NO_SELECTION;
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override bool HasSelection()
        {
            return this._selection >= 0;
        }

        public override void InvertSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;

            this.UpdateLeadAndAnchor(index0, index1);

            if (this._selection == index1)
            {
                this._selection = NO_SELECTION;
            }
            else
            {
                this._selection = index1;
            }

            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }

        public override bool IsSelected(int index)
        {
            return _selection == index;
        }

        public override void RemoveSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;
            this.UpdateLeadAndAnchor(index0, index1);

            if (this.HasSelection())
            {
                int first = Math.Min(index0, index1);
                int last = Math.Max(index0, index1);
                if (_selection >= first && _selection <= last)
                {
                    _selection = NO_SELECTION;
                }
                this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
            }
        }

        public override void SetSelection(int index0, int index1)
        {
            int[] oldSelection = this.Selection;
            this.UpdateLeadAndAnchor(index0, index1);
            _selection = index1;
            this.FireSelectionChange(this, new SelectionChangedEventArgs(oldSelection, this.Selection));
        }
    }
}
