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

namespace XNATWL.Property
{
    /// <summary>
    /// Port of java.beans.PropertyChangeEvent
    /// </summary>
    public class PropertyChangeEvent
    {
        private object _source;
        private string _propertyName;
        private object _oldPropertyValue;
        private object _newPropertyValue;

        public PropertyChangeEvent(object source, string propertyName, object oldPropertyValue, object newPropertyValue)
        {
            this._source = source;
            this._propertyName = propertyName;
            this._oldPropertyValue = oldPropertyValue;
            this._newPropertyValue = newPropertyValue;
        }

        /// <summary>
        /// Property identifier
        /// </summary>
        public string Name
        {
            get
            {
                return this._propertyName;
            }
        }

        /// <summary>
        /// The source of the property change event
        /// </summary>
        public object Source
        {
            get
            {
                return this._source;
            }
        }
        
        /// <summary>
        /// The property's new value
        /// </summary>
        public object New
        {
            get
            {
                return this._newPropertyValue;
            }
        }

        /// <summary>
        /// The property's old value
        /// </summary>
        public object Old
        {
            get
            {
                return this._oldPropertyValue;
            }
        }
    }
}
