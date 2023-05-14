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
    public class SimpleProperty<T> : AbstractProperty<T>
    {
        private Type _type;
        private string _name;
        private bool _readOnly;
        private T _value;

        public SimpleProperty(Type type, string name, T value) : this(type, name, value, false)
        {

        }

        public SimpleProperty(Type type, string name, T value, bool readOnly)
        {
            this._type = type;
            this._name = name;
            this._readOnly = readOnly;
            this._value = value;
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this._readOnly;
            }
        }

        public void SetReadOnly(bool readOnly)
        {
            this._readOnly = readOnly;
        }

        public override bool Nullable
        {
            get
            {
                return false;
            }
        }

        public override object Value
        {
            get
            {
                return this._value;
            }

            set
            {
                if (value == null && !this.Nullable)
                {
                    throw new NullReferenceException();
                }

                if (valueChanged((T)value))
                {
                    var old = value;
                    this._value = (T)value;
                    this.Changed.Invoke(this, new PropertyChangedEventArgs());
                }
            }
        }

        protected bool valueChanged(T newValue)
        {
            return !this._value.Equals(newValue) && (this._value == null || !this._value.Equals(newValue));
        }

        public override Type Type
        {
            get
            {
                return this._type;
            }
        }

        public override T ValueCast
        {
            get
            {
                return (T)this.Value;
            }
            set
            {
                this.Value = value;
            }
        }

        public override event EventHandler<PropertyChangedEventArgs> Changed;
    }
}
