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
using System.Collections.Generic;

namespace XNATWL.Model
{
    public class SimplePropertyList<T> : AbstractProperty<PropertyList<T>>, PropertyList<T>
    {
        public int Count
        {
            get
            {
                return this._properties.Count;
            }
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
                return false;
            }
        }

        public override bool Nullable
        {
            get
            {
                return false;
            }
        }

        public override PropertyList<T> Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override Type Type
        {
            get
            {
                return typeof(PropertyList<object>);
            }
        }

        public override event EventHandler<PropertyChangedEventArgs<PropertyList<T>>> Changed;

        public Property<T> PropertyAt(int index)
        {
            return this._properties[index];
        }

        public void AddProperty(Property<T> property)
        {
            this._properties.Add(property);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<T>>());
        }

        public void AddProperty(int idx, Property<T> property)
        {
            this._properties.Insert(idx, property);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<T>>());
        }

        public void RemoveProperty(int idx)
        {
            this._properties.RemoveAt(idx);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<T>>());
        }

        public void RemoveAllProperties()
        {
            this._properties.Clear();
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<T>>());
        }

        private string _name;
        private List<Property<T>> _properties;

        public SimplePropertyList(string name)
        {
            this._properties = new List<Property<T>>();
            this._name = name;
        }

        public SimplePropertyList(string name, params Property<T>[] properties) : this(name)
        {
            this._properties.AddRange(properties);
        }
    }
}
