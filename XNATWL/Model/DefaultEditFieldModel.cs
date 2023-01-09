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
using System.Text;

namespace XNATWL.Model
{
    public class DefaultEditFieldModel : EditFieldModel
    {
        private StringBuilder stringBuilder;

        public event EventHandler<CharSequenceChangedEventArgs> CharSequenceChanged;

        public DefaultEditFieldModel()
        {
            this.stringBuilder = new StringBuilder();
        }

        public int Length
        {
            get
            {
                return stringBuilder.Length;
            }
        }

        public string Value
        {
            get
            {
                return stringBuilder.ToString();
            }
        }

        public char CharAt(int index)
        {
            return stringBuilder[index];
        }

        private void CheckRange(int start, int count)
        {
            int len = this.Length;

            if (start < 0 || start > len)
            {
                throw new IndexOutOfRangeException();
            }

            if (count < 0 || count > len - start)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public int Replace(int start, int count, string replacement)
        {
            CheckRange(start, count);

            int replacementLength = replacement.Length;

            if (count > 0 || replacementLength > 0)
            {
                this.stringBuilder.Remove(start, count);
                this.stringBuilder.Insert(start, replacement);
                if (this.CharSequenceChanged != null)
                {
                    this.CharSequenceChanged.Invoke(this, new CharSequenceChangedEventArgs(start, count, replacementLength));
                }
            }

            return replacementLength;
        }

        public bool Replace(int start, int count, char replacement)
        {
            CheckRange(start, count);

            if (count == 0)
            {
                this.stringBuilder.Insert(start, replacement);
            }
            else
            {
                this.stringBuilder.Remove(start, count - 1); // TODO: Does the minus one actually mean anything? It is unclear.
                this.stringBuilder.Insert(start, replacement);
            }

            if (this.CharSequenceChanged != null)
            {
                this.CharSequenceChanged.Invoke(this, new CharSequenceChangedEventArgs(start, count, 1));
            }
            return true;
        }

        public string SubSequence(int start, int end)
        {
            return this.stringBuilder.ToString(start, end - start);
        }

        public string Substring(int start, int end)
        {
            return this.stringBuilder.ToString(start, end - start);
        }

        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }

        public StringBuilder AsStringBuilder()
        {
            return this.stringBuilder;
        }
    }
}
