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

using Microsoft.Xna.Framework.Input;
using System;
using XNATWL.TextAreaModel;
using static XNATWL.ActionMap;
using static XNATWL.Utils.SparseGrid;

namespace XNATWL.Model
{
    /// <summary>
    /// <para>A <see cref="BooleanModel"/> which is true when the underlying <see cref="IntegerModel"/> has the specified 
    /// option code.This can be used for radio/option buttons.</para>
    /// <para>It is not possible to set this <see cref="BooleanModel"/> to false. It can only be set to
    /// false by setting the underlying <see cref="IntegerModel"/> to another value. e.g. by setting
    /// another <see cref="OptionBooleanModel"/> working on the same <see cref="IntegerModel"/> to true.</para>
    /// </summary>
    public class OptionBooleanModel : AbstractOptionModel
    {
        /// <summary>
        /// If the value is <b>true</b>, then the underlying <see cref="IntegerModel"/> is set to the option code of this <see cref="OptionBooleanModel"/>. <br/>
        /// If the value is <b>false</b> then nothing happens.
        /// </summary>
        public override bool Value
        {
            get
            {
                return ((IntegerModel)this._optionState).Value.Equals(this._optionCode);
            }
            set
            {
                if (value)
                {
                    ((IntegerModel)this._optionState).Value = (int) this._optionCode;
                }
            }
        }

        public override event EventHandler<BooleanChangedEventArgs> Changed;

        /// <summary>
        /// Creates a new <see cref="OptionBooleanModel"/> with the specified <see cref="IntegerModel"/> and option code.
        /// </summary>
        /// <param name="optionState">the <see cref="IntegerModel"/> which stores the current active option</param>
        /// <param name="optionCode">the option code of this option in the <see cref="IntegerModel"/></param>
        public OptionBooleanModel(IntegerModel optionState, int optionCode) : base(optionState, optionCode)
        {

        }

        public override void WireEventToOptionState()
        {
            ((IntegerModel)this._optionState).Changed += (sender, e) =>
            {
                this.Changed.Invoke(this, new BooleanChangedEventArgs(e.Old.Equals(this._optionCode), e.New.Equals(this._optionCode)));
            };
        }
    }
}
