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
    /// <summary>
    /// Port of java.beans.PropertyChangeSupport
    /// </summary>
    public class PropertyChangeSupport
    {
        private Dictionary<string, HashSet<PropertyChangeListener>> _propertyListeners = new Dictionary<string, HashSet<PropertyChangeListener>>();
        private List<PropertyChangeListener> _anyPropertyListeners = new List<PropertyChangeListener>();
        private object _source;

        public PropertyChangeSupport()
        {
            this._source = null;
        }

        /// <summary>
        /// Support intialised for given object
        /// </summary>
        /// <param name="o">Object to support</param>
        public PropertyChangeSupport(Object o)
        {
            this._source = o;
        }

        /// <summary>
        /// Adds a property change listener for the given property name
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="listener">Object implementing <see cref="PropertyChangeListener"/></param>
        public void AddPropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            // create the listeners a home if we haven't used this key in the _propertyListeners Dictionary yet
            if (!this._propertyListeners.ContainsKey(propertyName))
            {
                this._propertyListeners[propertyName] = new HashSet<PropertyChangeListener>();
            }

            this._propertyListeners[propertyName].Add(listener);
        }

        /// <summary>
        /// Add a property change listener for any property change
        /// </summary>
        /// <param name="listener"></param>
        public void AddPropertyChangeListener(PropertyChangeListener listener)
        {
            this._anyPropertyListeners.Add(listener);
        }

        /// <summary>
        /// Fire a property change event on all supported listeners
        /// </summary>
        /// <param name="evt">Event detailling the change of the property</param>
        public void FirePropertyChange(PropertyChangeEvent evt)
        {
            // fire the name-specific property handlers
            foreach (string key in this._propertyListeners.Keys)
            {
                if (evt.Name == key)
                {
                    foreach (PropertyChangeListener listener in this._propertyListeners[key])
                    {
                        listener.PropertyChange(evt);
                    }
                }
            }

            // fire all _anyPropertyListeners without any checks
            foreach (PropertyChangeListener listener in this._anyPropertyListeners)
            {
                listener.PropertyChange(evt);
            }
        }

        /// <summary>
        /// Fire a property change event given the event information
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        public void FirePropertyChange(string propertyName, object oldValue, object newValue)
        {
            this.FirePropertyChange(new PropertyChangeEvent(this._source, propertyName, oldValue, newValue));
        }

        /// <summary>
        /// Fire a property change event given the event information
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        public void FirePropertyChange(string propertyName, int oldValue, int newValue)
        {
            this.FirePropertyChange(propertyName, oldValue, newValue);
        }

        /// <summary>
        /// Fire a property change event given the event information
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        public void FirePropertyChange(string propertyName, bool oldValue, bool newValue)
        {
            this.FirePropertyChange(propertyName, oldValue, newValue);
        }

        /// <summary>
        /// Remove a listener from the catch-all listener list
        /// </summary>
        /// <param name="listener">Listener to remove</param>
        public void RemovePropertyChangeListener(PropertyChangeListener listener)
        {
            this._anyPropertyListeners.Remove(listener);
        }

        /// <summary>
        /// Removes a listener for a specific property name
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="listener">Listener to remove</param>
        public void RemovePropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            this._propertyListeners[propertyName].Remove(listener);
        }
    }
}
