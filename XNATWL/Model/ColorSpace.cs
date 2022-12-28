using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface ColorSpace
    {
        string Name
        {
            get;
        }

        int Components
        {
            get;
        }

        string ComponentNameOf(int component);

        string ComponentShortNameOf(int component);

        float ComponentMinValueOf(int component);

        float ComponentMaxValueOf(int component);

        float ComponentDefaultValueOf(int component);

        int RGB(float[] color);

        float[] FromRGB(int rgb);
    }
}
