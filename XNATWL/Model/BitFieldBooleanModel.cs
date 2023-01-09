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
    public class BitFieldBooleanModel : BooleanModel
    {
        private IntegerModel _bitfield;
        private int _bitmask;

        public event EventHandler<BooleanChangedEventArgs> Changed;

        public bool Value
        {
            get
            {
                return ((this._bitfield.Value & this._bitmask) != 0);
            }
            set
            {
                int oldBFValue = this._bitfield.Value;
                int newBFValue = value ? (oldBFValue | this._bitmask) : (oldBFValue & ~this._bitmask);
                if (oldBFValue != newBFValue)
                {
                    this._bitfield.Value = newBFValue;
                    // bitfield's callback will call our callback
                }
            }
        }

        public BitFieldBooleanModel(IntegerModel bitfield, int bit)
        {
            if (bit < 0 || bit > 30)
            {
                throw new ArgumentOutOfRangeException("invalid bit index");
            }
            if (bitfield.MinValue != 0)
            {
                throw new ArgumentOutOfRangeException("bitfield.getMinValue() != 0");
            }
            int bitfieldMax = bitfield.MaxValue;
            if ((bitfieldMax & (bitfieldMax + 1)) != 0)
            {
                throw new ArgumentOutOfRangeException("bitfield.getmaxValue() must eb 2^x");
            }
            if (bitfieldMax < (1 << bit))
            {
                throw new ArgumentOutOfRangeException("bit index outside of bitfield range");
            }

            _bitfield = bitfield;
            _bitmask = 1 << bit;

            bitfield.Changed += (sender, e) =>
            {
                this.Changed.Invoke(sender, new BooleanChangedEventArgs(((e.Old & this._bitmask) != 0), ((e.New & this._bitmask) != 0)));
            };
        }
    }
}
