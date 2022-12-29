using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimplePropertyList : AbstractProperty<PropertyList<object>>, PropertyList<object>
    {
        public int Count => throw new NotImplementedException();

        public override string Name
        {
            get
            {
                return this.name;
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
            throw new NotImplementedException();
        }

        public void AddProperty(Property<object> property)
        {
            properties.Add(property);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        public void AddProperty(int idx, Property<object> property)
        {
            properties.Insert(idx, property);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        public void RemoveProperty(int idx)
        {
            properties.RemoveAt(idx);
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        public void RemoveAllProperties()
        {
            properties.Clear();
            this.Changed.Invoke(this, new PropertyChangedEventArgs<PropertyList<object>>());
        }

        private string name;
        private List<Property<object>> properties;

        public SimplePropertyList(string name)
        {
            this.properties = new List<Property<object>>();
            this.name = name;
        }

        public SimplePropertyList(string name, params Property<object>[] properties) : this(name)
        {
            this.properties.AddRange(properties);
        }
    }
}
