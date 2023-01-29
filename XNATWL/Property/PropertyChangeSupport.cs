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

namespace XNATWL.Property
{
    public class PropertyChangeSupport
    {
        private Dictionary<string, HashSet<PropertyChangeListener>> propertyListeners = new Dictionary<string, HashSet<PropertyChangeListener>>();
        private List<PropertyChangeListener> anyPropertyListeners = new List<PropertyChangeListener>();
        private object _source;

        public PropertyChangeSupport()
        {
            this._source = null;
        }

        public PropertyChangeSupport(Object o)
        {
            this._source = o;
        }

        internal void addPropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            if (!this.propertyListeners.ContainsKey(propertyName))
            {
                this.propertyListeners[propertyName] = new HashSet<PropertyChangeListener>();
            }

            this.propertyListeners[propertyName].Add(listener);
        }

        internal void addPropertyChangeListener(PropertyChangeListener listener)
        {
            this.anyPropertyListeners.Add(listener);
        }

        internal void firePropertyChange(PropertyChangeEvent evt)
        {
            //throw new NotImplementedException();
            foreach (string key in this.propertyListeners.Keys)
            {
                if (evt.Name == key)
                {
                    foreach (PropertyChangeListener listener in this.propertyListeners[key])
                    {
                        listener.propertyChange(evt);
                    }
                }
            }

            foreach (PropertyChangeListener listener in this.anyPropertyListeners)
            {
                listener.propertyChange(evt);
            }
        }

        internal void firePropertyChange(string propertyName, object oldValue, object newValue)
        {
            this.firePropertyChange(new PropertyChangeEvent(this._source, propertyName, oldValue, newValue));
        }

        internal void firePropertyChange(string propertyName, int oldValue, int newValue)
        {
            this.firePropertyChange(propertyName, oldValue, newValue);
        }

        internal void firePropertyChange(string propertyName, bool oldValue, bool newValue)
        {
            this.firePropertyChange(propertyName, oldValue, newValue);
        }

        internal void removePropertyChangeListener(PropertyChangeListener listener)
        {
            this.anyPropertyListeners.Remove(listener);
        }

        internal void removePropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            List<string> keys = new List<string>();
            
            if (!this.propertyListeners.ContainsKey(propertyName))
            {
                throw new AccessViolationException();
            }

            this.propertyListeners[propertyName].Remove(listener);
            //throw new NotImplementedException();
        }
    }
}
