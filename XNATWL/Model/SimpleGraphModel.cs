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
    /// <summary>
    /// A simple <see cref="GraphModel"/>
    /// </summary>
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

        /// <summary>
        /// Adds a new line at the end of the list
        /// </summary>
        /// <param name="line">the new line</param>
        public void AddLine(GraphLineModel line)
        {
            InsertLine(this._lines.Count, line);
        }

        /// <summary>
        /// Inserts a new line before the specified index in the list
        /// </summary>
        /// <param name="idx">the index before which the new line will be inserted</param>
        /// <param name="line">the new line</param>
        /// <exception cref="ArgumentOutOfRangeException">Line already in graph</exception>
        public void InsertLine(int idx, GraphLineModel line)
        {
            if (IndexOfLine(line) >= 0)
            {
                throw new ArgumentOutOfRangeException("line already added");
            }

            this._lines.Insert(idx, line);
        }

        /// <summary>
        /// Returns the index of the specified line in this list or -1 if not found.
        /// </summary>
        /// <param name="line">the line to locate</param>
        /// <returns>the index or -1 if not found</returns>
        public int IndexOfLine(GraphLineModel line)
        {
            return this._lines.IndexOf(line);
        }

        /// <summary>
        /// Removes the line at the specified index
        /// </summary>
        /// <param name="idx">the index of the line to remove</param>
        /// <returns>the line that was removed</returns>
        public GraphLineModel RemoveLine(int idx)
        {
            GraphLineModel lineModel = this._lines[idx];
            this._lines.RemoveAt(idx);
            return lineModel;
        }
    }
}
