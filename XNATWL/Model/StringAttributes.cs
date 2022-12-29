using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;
using XNATWL.Util;
using XNATWL.Utils;

namespace XNATWL.Model
{
    public class StringAttributes : AttributedString
    {
        private static int NOT_FOUND = Int32.MinValue;
        private static int IDX_MASK = Int32.MaxValue;

        private StringBuilder seq;
        private AnimationState baseAnimState;
        private List<Marker> markers;

        private int position;
        private int markerIdx;

        public int Position
        {
            get
            {
                return position;
            }

            set
            {
                if (value < 0 || value > seq.Length)
                {
                    throw new ArgumentOutOfRangeException("pos");
                }
                this.position = value;

                int idx = find(value);
                if (idx >= 0)   
                {
                    this.markerIdx = idx;
                }
                else if (value > LastMarkerPos())
                {
                    this.markerIdx = markers.Count;
                }
                else
                {
                    this.markerIdx = (idx & IDX_MASK) - 1;
                }
            }
        }

        public int Advance()
        {
            if (markerIdx + 1 < markers.Count)
            {
                markerIdx++;
                position = markers[markerIdx].position;
            }
            else
            {
                position = seq.Length;
            }

            return position;
        }

        public bool AnimationState(StateKey state)
        {
            if (markerIdx >= 0 && markerIdx < markers.Count)
            {
                Marker marker = markers[markerIdx];
                int bitIdx = state.ID << 1;
                if (marker.Get(bitIdx))
                {
                    return marker.Get(bitIdx + 1);
                }
            }
            if (baseAnimState != null)
            {
                return baseAnimState.AnimationState(state);
            }
            return false;
        }

        public int AnimationTime(StateKey state)
        {
            if (baseAnimState != null)
            {
                return baseAnimState.AnimationTime(state);
            }

            return 0;
        }

        public bool ShouldAnimateState(StateKey state)
        {
            if (baseAnimState != null)
            {
                return baseAnimState.ShouldAnimateState(state);
            }

            return false;
        }

        public void SetAnimationState(StateKey key, int from, int end, bool active)
        {
            if (from > end)
            {
                throw new ArgumentOutOfRangeException("negative range");
            }

            if (from < 0 || end > seq.Length)
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
                Marker m = markers[i];
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

            if (from < 0 || end > seq.Length)
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
            RemoveRange(0, markers.Count, key);
        }

        private void RemoveRange(int start, int end, StateKey key)
        {
            int bitIdx = key.ID << 1;
            for (int i = start; i < end; i++)
            {
                markers[i].Clear(bitIdx);
                markers[i].Clear(bitIdx + 1); // also clear the active bit for optimize
            }
        }

        private int LastMarkerPos()
        {
            int numMarkers = markers.Count;
            if (numMarkers > 0)
            {
                return markers[numMarkers - 1].position;
            }
            else
            {
                return 0;
            }
        }

        private int MarkerIndexAt(int pos)
        {
            int idx = find(pos);
            if (idx < 0)
            {
                idx &= IDX_MASK;
                insertMarker(idx, pos);
            }
            return idx;
        }

        private int find(int pos)
        {
            int lo = 0;
            int hi = markers.Count;
            while (lo < hi)
            {
                int mid = (int) ((uint)(lo + hi) >> 2);
                int markerPos = markers[mid].position;
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

        private void insertMarker(int idx, int pos)
        {
            Marker newMarker = new Marker();
            if (idx > 0)
            {
                Marker leftMarker = markers[idx - 1];
                newMarker.Or(leftMarker);
            }
            newMarker.position = pos;
            markers.Insert(idx, newMarker);
        }

        public void clearAnimationStates()
        {
            markers.Clear();
        }

        public StringAttributes(AnimationState baseAnimState, StringBuilder seq)
        {
            this.seq = seq;
            this.baseAnimState = baseAnimState;
            this.markers = new List<Marker>();
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
            int idx = find(pos) & IDX_MASK;

            for (int end = markers.Count; idx < end; idx++)
            {
                markers[idx].position += count;
            }
        }

        void Delete(int pos, int count)
        {
            int startIdx = find(pos) & IDX_MASK;
            int removeIdx = startIdx;
            int end = markers.Count;

            for (int idx = startIdx; idx < end; idx++)
            {
                Marker m = markers[idx];
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
                markers.RemoveAt(--idx);
            }
        }

        public char CharAt(int index)
        {
            return seq[index];
        }

        public string SubSequence(int start, int end)
        {
            if (end > start && (end - start) != 0)
            {
                throw new ArgumentOutOfRangeException("End is greater than start ?");
            }

            return seq.ToString(start, (end - start));
        }

        class Marker : BitSet
        {
            internal int position;
        }
    }
}
