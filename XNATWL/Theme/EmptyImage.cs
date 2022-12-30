using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class EmptyImage : Image
    {
        private int width;
        private int height;

        public EmptyImage(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public Image CreateTintedVersion(Color color)
        {
            return this;
        }
    }
}
