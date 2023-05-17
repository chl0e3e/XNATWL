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
    /// <summary>
    /// An abstract container for auto completion results.
    /// </summary>
    public abstract class AutoCompletionResult
    {
        public static int DEFAULT_CURSOR_POS = -1;

        /// <summary>
        /// The text which was used for this auto completion
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// The prefix length is the length of prefix which was used to collect the data.
        /// The remaining part of the text is used for high lighting the results.
        /// This is used for things like tree completion.
        /// </summary>
        public readonly int PrefixLength;

        /// <summary>
        /// The number of results
        /// </summary>
        public abstract int Results
        {
            get;
        }

        /// <summary>
        /// Create a new AutoCompletionResult object (abstract)
        /// </summary>
        /// <param name="text">The text which was used for this auto completion</param>
        /// <param name="prefixLength">The prefix length is the length of prefix which was used to collect the data.</param>
        public AutoCompletionResult(string text, int prefixLength)
        {
            Text = text;
            PrefixLength = prefixLength;
        }

        /// <summary>
        /// Returns a selected result entry
        /// </summary>
        /// <param name="index">the index of the desired result entry</param>
        /// <returns>the result entry</returns>
        public abstract string ResultAt(int index);

        /// <summary>
        /// Returns the desired cursor position for the given result entry.
        /// 
        /// The default implementation returns {@link #DEFAULT_CURSOR_POS}
        /// </summary>
        /// <param name="idx">the index of the desired result entry</param>
        /// <returns>the cursor position</returns>
        public virtual int GetCursorPosForResult(int idx)
        {
            return DEFAULT_CURSOR_POS;
        }

        /// <summary>
        /// Tries to refine the results. Refining can result in a different order of results then a new query but is faster.<br/><br/>
        /// If refining resulted in no results then an empty AutoCompletionResult is returned.
        /// </summary>
        /// <param name="text">The new text</param>
        /// <param name="cursorPos">The new cursor position</param>
        /// <returns>The new refined AutoCompletionResult or null if refining was not possible</returns>
        public virtual AutoCompletionResult Refine(String text, int cursorPos)
        {
            return null;
        }
    }
}
