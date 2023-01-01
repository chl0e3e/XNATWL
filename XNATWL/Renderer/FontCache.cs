using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface FontCache : Resource
    {
        int Width
        {
            get;
        }

        int Height
        {
            get;
        }

        void Draw(AnimationState animationState, int x, int y);
    }
}
