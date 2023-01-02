using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Property
{
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

        public string Name
        {
            get
            {
                return this._propertyName;
            }
        }

        public object Source
        {
            get
            {
                return this._source;
            }
        }

        public object New
        {
            get
            {
                return this._newPropertyValue;
            }
        }

        public object Old
        {
            get
            {
                return this._oldPropertyValue;
            }
        }
    }
}
