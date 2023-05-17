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
using XNATWL.IO;

namespace XNATWL.Model
{
    /// <summary>
    /// A persistent <see cref="IntegerModel"/>.
    /// </summary>
    public class PersistentIntegerModel : AbstractIntegerModel
    {
        public override int Value
        {
            get
            {
                if (this._preferences == null)
                {
                    return _noPrefValue;
                }

                return (int)this._preferences.Get(this._preferenceKey, this._defaultValue);
            }
            set
            {
                var old = this.Value;
                if (!old.Equals(value))
                {
                    if (this._preferences != null)
                    {
                        this._preferences.Set(this._preferenceKey, value);
                    }
                    else
                    {
                        this._noPrefValue = value;
                    }

                    if (this.Changed != null)
                    {
                        this.Changed.Invoke(this, new IntegerChangedEventArgs(old, value));
                    }
                }
            }
        }

        public override int MinValue
        {
            get
            {
                return this._minValue;
            }
        }

        public override int MaxValue
        {
            get
            {
                return this._maxValue;
            }
        }

        public override event EventHandler<IntegerChangedEventArgs> Changed;

        private Preferences _preferences;
        private string _preferenceKey;
        private int _defaultValue;

        private int _minValue;
        private int _maxValue;

        private int _noPrefValue;

        public PersistentIntegerModel(Preferences preferences, string preferenceKey, int minValue, int maxValue, int defaultValue) : base()
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._defaultValue = defaultValue;

            this._minValue = minValue;
            this._maxValue = maxValue;
        }

        public PersistentIntegerModel(int minValue, int maxValue, int value) : base()
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._preferences = null;
            this._preferenceKey = null;
            this._defaultValue = Int32.MinValue;

            this._minValue = minValue;
            this._maxValue = maxValue;

            this._noPrefValue = value;
        }
    }
}
