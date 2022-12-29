using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimplePropertyList : AbstractProperty<PropertyList<object>>, PropertyList<object>
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

        public override PropertyList<object> Value
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

        public override event EventHandler<PropertyChangedEventArgs<PropertyList<object>>> Changed;

        public Property<object> PropertyAt(int index)
        {
            return this._properties[index];
        }

        public void AddProperty(Property<object> property)
        {
            this._properties.Add(property);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        public void AddProperty(int idx, Property<object> property)
        {
            this._properties.Insert(idx, property);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        public void RemoveProperty(int idx)
        {
            this._properties.RemoveAt(idx);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        public void RemoveAllProperties()
        {
            this._properties.Clear();
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        private string _name;
        private List<Property<object>> _properties;

        public SimplePropertyList(string name)
        {
            this._properties = new List<Property<object>>();
            this._name = name;
        }

        public SimplePropertyList(string name, params Property<object>[] properties) : this(name)
        {
            this._properties.AddRange(properties);
        }
    }
}
