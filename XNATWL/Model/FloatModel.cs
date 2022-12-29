using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface FloatModel
    {
        event EventHandler<FloatChangedEventArgs> Changed;

        float Value
        {
            get;
            set;
        }

        float MinValue
        {
            get;
        }

        float MaxValue
        {
            get;
        }
    }

    public class FloatChangedEventArgs : EventArgs
    {
        public float New;
        public float Old;

        public FloatChangedEventArgs(float _old, float _new)
        {
            this.Old = _old;
            this.New = _new;
        }
    }
}
