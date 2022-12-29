using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface Image
    {
        int Width
        {
            get;
        }

        int Height
        {
            get;
        }

        void Draw(AnimationState state, int x, int y);

        void Draw(AnimationState state, int x, int y, int width, int height);

        Image CreateTintedVersion(Color color);
    }
}
