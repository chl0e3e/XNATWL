using System;

namespace XNATWL.Model
{
    public abstract class AbstractFloatModel : FloatModel
    {
        public AbstractFloatModel()
        {

        }

        public abstract float Value { get; set; }
        public abstract float MinValue { get; }
        public abstract float MaxValue { get; }

        public abstract event EventHandler<FloatChangedEventArgs> Changed;
    }
}