using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleProperty<T> : AbstractProperty<T>
    {
        private Type _type;
        private string _name;
        private bool _readOnly;
        private T _value;

        public SimpleProperty(Type type, string name, T value) : this(type, name, value, false)
        {

        }

        public SimpleProperty(Type type, string name, T value, bool readOnly)
        {
            this._type = type;
            this._name = name;
            this._readOnly = readOnly;
            this._value = value;
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
                return this._readOnly;
            }
        }

        public void SetReadOnly(bool readOnly)
        {
            this._readOnly = readOnly;
        }

        public override bool Nullable
        {
            get
            {
                return false;
            }
        }

        public override T Value
        {
            get
            {
                return this._value;
            }

            set
            {
                if (value == null && !this.Nullable)
                {
                    throw new NullReferenceException();
                }

                if (valueChanged(value))
                {
                    var old = value;
                    this._value = value;
                    this.Changed.Invoke(this, new PropertyChangedEventArgs<T>());
                }
            }
        }

        protected bool valueChanged(T newValue)
        {
            return !this._value.Equals(newValue) && (this._value == null || !this._value.Equals(newValue));
        }

        public override Type Type
        {
            get
            {
                return this._type;
            }
        }

        public override event EventHandler<PropertyChangedEventArgs<T>> Changed;
    }
}
