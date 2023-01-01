using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Property
{
    public class PropertyChangeSupport
    {
        public PropertyChangeSupport()
        {
                
        }

        public PropertyChangeSupport(Object o)
        {

        }

        internal void addPropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            //throw new NotImplementedException();
        }

        internal void addPropertyChangeListener(PropertyChangeListener listener)
        {
            //throw new NotImplementedException();
        }

        internal void firePropertyChange(PropertyChangeEvent evt)
        {
            //throw new NotImplementedException();
        }

        internal void firePropertyChange(string propertyName, object oldValue, object newValue)
        {
            //throw new NotImplementedException();
        }

        internal void firePropertyChange(string propertyName, int oldValue, int newValue)
        {
            //throw new NotImplementedException();
        }

        internal void firePropertyChange(string propertyName, bool oldValue, bool newValue)
        {
            //throw new NotImplementedException();
        }

        internal void removePropertyChangeListener(PropertyChangeListener listener)
        {
            //throw new NotImplementedException();
        }

        internal void removePropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            //throw new NotImplementedException();
        }
    }
}
