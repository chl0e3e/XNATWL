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

namespace XNATWL.Model
{
    /// <summary>
    /// A simple <see cref="IntegerModel"/>. The value is not checked against the min/max values.
    /// </summary>
    public class SimpleIntegerModel : IntegerModel
    {
        private int _minValue;
        private int _maxValue;
        private int _value;

        /// <summary>
        /// Creates a new integer model with the specified min/max and initial value
        /// </summary>
        /// <param name="minValue">the minimum allowed value</param>
        /// <param name="maxValue">the maximum allowed value</param>
        /// <param name="value">the initial value</param>
        public SimpleIntegerModel(int minValue, int maxValue, int value)
        {
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._value = value;
        }

        public int Value
        {
            get
            {
                return this._value;
            }

            set
            {
                int old = this._value;
                if (value != old)
                {
                    this._value = value;
                    if (this.Changed != null)
                    {
                        this.Changed.Invoke(this, new IntegerChangedEventArgs(old, value));
                    }
                }
            }
        }

        public int MaxValue
        {
            get
            {
                return this._maxValue;
            }
        }

        public int MinValue
        {
            get
            {
                return this._minValue;
            }
        }

        public event EventHandler<IntegerChangedEventArgs> Changed;
    }
}
