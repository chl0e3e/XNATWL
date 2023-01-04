using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
