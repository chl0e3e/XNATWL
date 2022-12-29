using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AbstractProperty<T> : Property<T>
    {
        public abstract string Name { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool Nullable { get; }
        public abstract T Value { get; set; }
        public abstract Type Type { get; }

        public abstract event EventHandler<PropertyChangedEventArgs<T>> Changed;
    }
}
