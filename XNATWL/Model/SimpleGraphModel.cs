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
using System.Linq;

namespace XNATWL.Model
{
    public class SimpleGraphModel : GraphModel
    {
        private List<GraphLineModel> _lines;
        private bool _scaleLinesIndependent;

        public SimpleGraphModel()
        {
            _lines = new List<GraphLineModel>();
        }

        public SimpleGraphModel(GraphLineModel[] lines) : this(lines.ToList())
        {
            
        }

        public SimpleGraphModel(ICollection<GraphLineModel> lines)
        {
            this._lines = new List<GraphLineModel>(lines);
        }

        public GraphLineModel LineAt(int idx)
        {
            return this._lines[idx];
        }

        public int Lines
        {
            get
            {
                return this._lines.Count;
            }
        }

        public bool ScaleLinesIndependent()
        {
            return this._scaleLinesIndependent;
        }

        public void SetScaleLinesIndependent(bool val)
        {
            this._scaleLinesIndependent = val;
        }

        /**
         * Adds a new line at the end of the list
         * @param line the new line
         */
        public void AddLine(GraphLineModel line)
        {
            InsertLine(this._lines.Count, line);
        }

        /**
         * Inserts a new line before the specified index in the list
         * @param idx the index before which the new line will be inserted
         * @param line the new line
         * @throws NullPointerException if line is null
         * @throws IllegalArgumentException if the line is already part of this model
         */
        public void InsertLine(int idx, GraphLineModel line)
        {
            if (IndexOfLine(line) >= 0)
            {
                throw new ArgumentOutOfRangeException("line already added");
            }

            this._lines.Insert(idx, line);
        }

        /**
         * Returns the index of the specified line in this list or -1 if not found.
         * @param line the line to locate
         * @return the index or -1 if not found
         */
        public int IndexOfLine(GraphLineModel line)
        {
            return this._lines.IndexOf(line);
        }

        /**
         * Removes the line at the specified index
         * @param idx the index of the line to remove
         * @return the line that was removed
         */
        public GraphLineModel RemoveLine(int idx)
        {
            GraphLineModel lineModel = this._lines[idx];
            this._lines.RemoveAt(idx);
            return lineModel;
        }
    }
}
