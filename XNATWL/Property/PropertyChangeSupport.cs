using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Property
{
    public class PropertyChangeSupport
    {
        private Dictionary<string, HashSet<PropertyChangeListener>> propertyListeners;
        private List<PropertyChangeListener> anyPropertyListeners;
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
