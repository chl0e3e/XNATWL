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

using System.Security.Policy;
using static XNATWL.Utils.SparseGrid;

namespace XNATWL.Model
{
    /// <summary>
    /// A model for controlling edit fields
    /// </summary>
    public interface EditFieldModel : ObservableCharSequence
    {
        /// <summary>
        /// Replace <paramref name="count"/> characters starting at <paramref name="start"/> with the specified <paramref name="replacement"/> text.
        /// </summary>
        /// <param name="start">the start index</param>
        /// <param name="count">the number of characters to replace, can be 0</param>
        /// <param name="replacement">the replacement text, can be empty</param>
        /// <returns>the number of characters which have been inserted, or -1 if no replacement has been performed.</returns>
        int Replace(int start, int count, string replacement);

        /// <summary>
        /// Replace <paramref name="count"/> characters starting at <paramref name="start"/> with the specified <paramref name="replacement"/> character.
        /// </summary>
        /// <param name="start">the start index</param>
        /// <param name="count">the number of characters to replace, can be 0</param>
        /// <param name="replacement">the replacement character</param>
        /// <returns><b>true</b> if the sequence was changed, <b>false</b> otherwise</returns>
        bool Replace(int start, int count, char replacement);

        /// <summary>
        /// Returns a String containing the specified range from this sequence.
        /// </summary>
        /// <param name="start">the start index</param>
        /// <param name="end">the end index</param>
        /// <returns>string in given range</returns>
        string Substring(int start, int end);
    }
}
