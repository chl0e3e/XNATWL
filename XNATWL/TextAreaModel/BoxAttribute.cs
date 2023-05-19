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

namespace XNATWL.TextAreaModel
{
    /// <summary>
    /// Attribute value for inset boxes (padding and margin)
    /// </summary>
    public class BoxAttribute
    {
        /// <summary>
        /// Pixels for the top inset
        /// </summary>
        public readonly StyleAttribute<Value> Top;
        /// <summary>
        /// Pixels for the left inset
        /// </summary>
        public readonly StyleAttribute<Value> Left;
        /// <summary>
        /// Pixels for the right inset
        /// </summary>
        public readonly StyleAttribute<Value> Right;
        /// <summary>
        /// Pixels for the bottom inset
        /// </summary>
        public readonly StyleAttribute<Value> Bottom;

        /// <summary>
        /// Create a new CSS inset box
        /// </summary>
        /// <param name="top">Pixels for the top inset</param>
        /// <param name="left">Pixels for the left inset</param>
        /// <param name="right">Pixels for the right inset</param>
        /// <param name="bottom">Pixels for the bottom inset</param>
        public BoxAttribute(StyleAttribute<Value> top, StyleAttribute<Value> left, StyleAttribute<Value> right, StyleAttribute<Value> bottom)
        {
            this.Top = top;
            this.Left = left;
            this.Right = right;
            this.Bottom = bottom;
        }
    }
}
