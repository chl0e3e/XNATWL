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
    /// A simple graph line model which allows to shift points from right to left.
    /// </summary>
    public class SimpleGraphLineModel : GraphLineModel
    {
        private String _visualStyleName;
        private float _minValue = 0;
        private float _maxValue = 100;
        private float[] _data;

        public SimpleGraphLineModel(string style, int size, float minValue, float maxValue)
        {
            this._visualStyleName = style;
            this._data = new float[size];
            this._minValue = minValue;
            this._maxValue = maxValue;
        }

        public string VisualStyleName
        {
            get
            {
                return this._visualStyleName;
            }

            set
            {
                this._visualStyleName = value;
            }
        }

        public int Points
        {
            get
            {
                return this._data.Length;
            }

            set
            {
                float[] newData = new float[value];
                int overlap = Math.Min(this._data.Length, value);
                Array.Copy(this._data, this._data.Length - overlap, newData, value - overlap, overlap);
                this._data = newData;
            }
        }

        public float MinValue
        {
            get
            {
                return this._minValue;
            }
            set
            {
                this._minValue = value;
            }
        }

        public float MaxValue
        {
            get
            {
                return this._maxValue;
            }
            set
            {
                this._maxValue = value;
            }
        }

        public float Point(int index)
        {
            return this._data[index];
        }

        public void AddPoint(float value)
        {
            Array.Copy(this._data, 1, this._data, 0, this._data.Length - 1);
            this._data[this._data.Length - 1] = value;
        }
    }
}
