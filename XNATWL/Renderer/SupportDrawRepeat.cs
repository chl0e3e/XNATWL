using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface SupportDrawRepeat
    {
        void Draw(AnimationState animationState, int x, int y, int width, int height,
                int repeatCountX, int repeatCountY);
    }
}
