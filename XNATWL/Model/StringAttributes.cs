﻿/*
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
using System.Text;
using XNATWL.Renderer;
using XNATWL.Util;

namespace XNATWL.Model
{
    /// <summary>
    /// Attributed string which keeps track of markers in it's character sequence
    /// </summary>
    public class StringAttributes : AttributedString
    {
        private static int NOT_FOUND = Int32.MinValue;
        private static int IDX_MASK = Int32.MaxValue;

        private StringBuilder _seq;
        private AnimationState _baseAnimState;
        private List<Marker> _markers;

        private int _position;
        private int _markerIdx;
        
        public string Value
        {
            get
            {
                return this._seq.ToString();
            }
        }

        public int Position
        {
            get
            {
                return _position;
            }

            set
            {
                if (value < 0 || value > _seq.Length)
                {
                    throw new ArgumentOutOfRangeException("pos");
                }
                this._position = value;

                int idx = Find(value);
                if (idx >= 0)   
                {
                    this._markerIdx = idx;
                }
                else if (value > LastMarkerPos())
                {
                    this._markerIdx = _markers.Count;
                }
                else
                {
                    this._markerIdx = (idx & IDX_MASK) - 1;
                }
            }
        }

        public int Length
        {
            get
            {
                return this._seq.Length;
            }
        }

        /// <summary>
        /// Optimizes the internal representation.
        /// This need O(n) time.
        /// </summary>
        public void Optimize()
        {
            if (this._markers.Count > 1)
            {
                Marker prev = this._markers[0];
                for (int i = 1; i < this._markers.Count;)
                {
                    Marker cur = this._markers[i];
                    if (prev.Equals(cur))
                    {
                        this._markers.RemoveAt(i);
                    }
                    else
                    {
                        prev = cur;
                        i++;
                    }
                }
            }
        }

        public int Advance()
        {
            if (_markerIdx + 1 < _markers.Count)
            {
                _markerIdx++;
                _position = _markers[_markerIdx].position;
            }
            else
            {
                _position = _seq.Length;
            }

            return _position;
        }

        public bool GetAnimationState(StateKey state)
        {
            if (_markerIdx >= 0 && _markerIdx < _markers.Count)
            {
                Marker marker = _markers[_markerIdx];
                int bitIdx = state.ID << 1;
                if (marker.Get(bitIdx))
                {
                    return marker.Get(bitIdx + 1);
                }
            }
            if (_baseAnimState != null)
            {
                return _baseAnimState.GetAnimationState(state);
            }
            return false;
        }

        public int GetAnimationTime(StateKey state)
        {
            if (_baseAnimState != null)
            {
                return _baseAnimState.GetAnimationTime(state);
            }

            return 0;
        }

        public bool ShouldAnimateState(StateKey state)
        {
            if (_baseAnimState != null)
            {
                return _baseAnimState.ShouldAnimateState(state);
            }

            return false;
        }

        public void SetAnimationState(StateKey key, int from, int end, bool active)
        {
            if (from > end)
            {
                throw new ArgumentOutOfRangeException("negative range");
            }

            if (from < 0 || end > _seq.Length)
            {
                throw new ArgumentOutOfRangeException("range outside of sequence");
            }

            if (from == end)
            {
                return;
            }

            int fromIdx = MarkerIndexAt(from);
            int endIdx = MarkerIndexAt(end);
            int bitIdx = key.ID << 1;
            for (int i = fromIdx; i < endIdx; i++)
            {
                Marker m = _markers[i];
                m.Set(bitIdx);
                m.Set(bitIdx + 1, active);
            }
        }


        public void RemoveAnimationState(StateKey key, int from, int end)
        {
            if (from > end)
            {
                throw new ArgumentOutOfRangeException("negative range");
            }

            if (from < 0 || end > _seq.Length)
            {
                throw new ArgumentOutOfRangeException("range outside of sequence");
            }

            if (from == end)
            {
                return;
            }

            int fromIdx = MarkerIndexAt(from);
            int endIdx = MarkerIndexAt(end);
            RemoveRange(fromIdx, endIdx, key);
        }


        public void RemoveAnimationState(StateKey key)
        {
            RemoveRange(0, _markers.Count, key);
        }

        private void RemoveRange(int start, int end, StateKey key)
        {
            int bitIdx = key.ID << 1;
            for (int i = start; i < end; i++)
            {
                _markers[i].Clear(bitIdx);
                _markers[i].Clear(bitIdx + 1); // also clear the active bit for optimize
            }
        }

        private int LastMarkerPos()
        {
            int numMarkers = _markers.Count;
            if (numMarkers > 0)
            {
                return _markers[numMarkers - 1].position;
            }
            else
            {
                return 0;
            }
        }

        private int MarkerIndexAt(int pos)
        {
            int idx = Find(pos);
            if (idx < 0)
            {
                idx &= IDX_MASK;
                InsertMarker(idx, pos);
            }
            return idx;
        }

        private int Find(int pos)
        {
            int lo = 0;
            int hi = _markers.Count;
            while (lo < hi)
            {
                int mid = (int) ((uint)(lo + hi) >> 2);
                int markerPos = _markers[mid].position;
                if (pos < markerPos)
                {
                    hi = mid;
                }
                else if (pos > markerPos)
                {
                    lo = mid + 1;
                }
                else
                {
                    return mid;
                }
            }
            return lo | NOT_FOUND;
        }

        private void InsertMarker(int idx, int pos)
        {
            Marker newMarker = new Marker();
            if (idx > 0)
            {
                Marker leftMarker = _markers[idx - 1];
                newMarker.Or(leftMarker);
            }
            newMarker.position = pos;
            _markers.Insert(idx, newMarker);
        }

        public void clearAnimationStates()
        {
            _markers.Clear();
        }

        public StringAttributes(AnimationState baseAnimState, StringBuilder seq)
        {
            this._seq = seq;
            this._baseAnimState = baseAnimState;
            this._markers = new List<Marker>();
        }

        public StringAttributes(string text, AnimationState baseAnimState): this(baseAnimState, new StringBuilder(text))
        {

        }

        public StringAttributes(ObservableCharSequence observableCharSequence, AnimationState baseAnimState) : this(baseAnimState, new StringBuilder(observableCharSequence.Value))
        {
            observableCharSequence.CharSequenceChanged += ObservableCharSequence_CharSequenceChanged;
        }

        private void ObservableCharSequence_CharSequenceChanged(object sender, CharSequenceChangedEventArgs e)
        {
            if (e.Start < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (e.OldCount > 0)
            {
                Delete(e.Start, e.OldCount);
            }

            if (e.NewCount > 0)
            {
                Insert(e.Start, e.NewCount);
            }
        }

        void Insert(int pos, int count)
        {
            int idx = Find(pos) & IDX_MASK;

            for (int end = _markers.Count; idx < end; idx++)
            {
                _markers[idx].position += count;
            }
        }

        void Delete(int pos, int count)
        {
            int startIdx = Find(pos) & IDX_MASK;
            int removeIdx = startIdx;
            int end = _markers.Count;

            for (int idx = startIdx; idx < end; idx++)
            {
                Marker m = _markers[idx];
                int newPos = m.position - count;
                if (newPos <= pos)
                {
                    newPos = pos;
                    removeIdx = idx;
                }
                m.position = newPos;
            }

            for (int idx = removeIdx; idx > startIdx;)
            {
                _markers.RemoveAt(--idx);
            }
        }

        public char CharAt(int index)
        {
            return _seq[index];
        }

        public string SubSequence(int start, int end)
        {
            if (end > start && (end - start) != 0)
            {
                throw new ArgumentOutOfRangeException("End is greater than start ?");
            }

            return _seq.ToString(start, (end - start));
        }

        class Marker : BitSet
        {
            internal int position;
        }
    }
}
