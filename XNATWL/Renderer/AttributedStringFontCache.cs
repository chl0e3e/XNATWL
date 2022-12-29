using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface AttributedStringFontCache : Resource
    {
        int Width
        {
            get;
        }

        int Height
        {
            get;
        }

        void Draw(int x, int y);
    }
}
