using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface Property<T>
    {
        string Name
        {
            get;
        }

        bool IsReadOnly
        {
            get;
        }

        bool Nullable
        {
            get;
        }

        T Value
        {
            get;
            set;
        }
        
        Type Type
        {
            get;
        }

        event EventHandler<PropertyChangedEventArgs<T>> Changed;
    }

    public class PropertyChangedEventArgs<T> : EventArgs
    {
        public T Old;
        public T New;
        
        public PropertyChangedEventArgs(T @old, T @new)
        {
            this.Old = @old;
            this.New = @new;
        }
    }
}
