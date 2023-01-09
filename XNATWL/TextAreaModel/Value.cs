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

namespace XNATWL.TextAreaModel
{
    public class Value
    {
        private float _value;
        private Unit _unit;

        public Value(float value, Unit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            if (unit == Unit.AUTO && value != 0f)
            {
                throw new ArgumentOutOfRangeException("value must be 0 for Unit.AUTO");
            }

            this._value = value;
            this._unit = unit;
        }

        public float FloatValue
        {
            get
            {
                return this._value;
            }
        }

        public Unit UnitOfValue
        {
            get
            {
                return this._unit;
            }
        }

        public override string ToString()
        {
            if (this._unit == Unit.AUTO)
            {
                return this._unit.Postfix;
            }

            return this._value + this._unit.Postfix;
        }

        public static Value ZERO_PX = new Value(0, Unit.PX);
        public static Value AUTO = new Value(0, Unit.AUTO);

        public class Unit
        {
            public static Unit PX = new Unit(false, "px");
            public static Unit PT = new Unit(false, "pt");
            public static Unit EM = new Unit(true, "em");
            public static Unit EX = new Unit(true, "ex");
            public static Unit PERCENT = new Unit(false, "%");
            public static Unit AUTO = new Unit(false, "auto");

            private bool _fontBased;
            private string _postfix;

            public Unit(bool fontBased, string postfix)
            {
                this._fontBased = fontBased;
                this._postfix = postfix;
            }

            public bool FontBased
            {
                get
                {
                    return _fontBased;
                }
            }

            public string Postfix
            {
                get
                {
                    return _postfix;
                }
            }
        }
    }
}
