using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface ColorModel
    {
        event EventHandler<ColorChangedEventArgs> Changed;

        Color Value
        {
            get;
        }
    }

    public class ColorChangedEventArgs : EventArgs
    {
        public Color Old;
        public Color New;

        public ColorChangedEventArgs(Color @old, Color @new)
        {
            Old = @old;
            New = @new;
        }
    }
}
