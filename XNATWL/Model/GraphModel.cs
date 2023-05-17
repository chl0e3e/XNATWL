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

namespace XNATWL.Model
{
    /// <summary>
    /// A generic graph data model.
    /// </summary>
    public interface GraphModel
    {
        /// <summary>
        /// The number of lines in this graph
        /// </summary>
        int Lines
        {
            get;
        }

        /// <summary>
        /// Returns the specified line model.
        /// </summary>
        /// <param name="index">The line index. Must be greater than 0 and less than <see cref="Lines"/></param>
        /// <returns></returns>
        GraphLineModel LineAt(int index);

        /// <summary>
        /// <para>The Y axis of the graph is based on min/max values.
        /// The scaling for an axis can be compute from the combined
        /// min/max values or using it's own min/max value.</para>
        /// <para>The combined min values is the smallest min value of all lines.<br/>
        /// The combined max values is the largest max value of all lines.</para> 
        /// </summary>
        /// <returns><b>true</b> if the Y scale is independant or false if it is combined.</returns>
        bool ScaleLinesIndependent();
    }
}
