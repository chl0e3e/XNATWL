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
using XNATWL.TextAreaModel;

namespace XNATWL.Model
{
    /// <summary>
    /// A simple float data model.
    /// 
    /// <para>Out of range values are limited to MinValue/MaxValue.
    /// If the value is set to NaN then it is converted to minValue.</para>
    /// </summary>
    public class SimpleFloatModel : AbstractFloatModel
    {
        private float _minValue;
        private float _maxValue;
        private float _value;

        public SimpleFloatModel(float minValue, float maxValue, float value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue > maxValue");
            }

            this._minValue = minValue;
            this._maxValue = maxValue;
            this._value = value;
        }

        public override float Value
        {
            get
            {
                return this._value;
            }

            set
            {
                float limitedValue = Math.Max(this._minValue, Math.Min(this._maxValue, value));
                float old = this._value;
                if (value != old)
                {
                    this._value = limitedValue;
                    this.Changed.Invoke(this, new FloatChangedEventArgs(old, limitedValue));
                }
            }
        }

        public override float MinValue
        {
            get
            {
                return this._minValue;
            }
        }

        public override float MaxValue
        {
            get
            {
                return this._maxValue;
            }
        }

        public override event EventHandler<FloatChangedEventArgs> Changed;
    }
}
