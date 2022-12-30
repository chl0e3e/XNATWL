using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    public class StateSelectImage : Renderer.Image, HasBorder
    {
        public Renderer.Image[] images;
        public StateSelect select;
        public Border border;

        public StateSelectImage(StateSelect select, Border border, params Renderer.Image[] ximages)
        {
            if (!(ximages.Length >= select.Expressions()))
            {
                throw new Exception("Assert exception");
            }
            if (!(images.Length <= select.Expressions() + 1))
            {
                throw new Exception("Assert exception");
            }

            this.images = ximages;
            this.select = select;
            this.border = border;
        }

        public int Width
        {
            get
            {
                return images[0].Width;
            }
        }

        public int Height
        {
            get
            {
                return images[0].Height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            Draw(animationState, x, y, Width, Height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int idx = select.Evaluate(animationState);
            if (idx < images.Length)
            {
                images[idx].Draw(animationState, x, y, width, height);
            }
        }

        public Border Border
        {
            get
            {
                return border;
            }
        }

        public Renderer.Image CreateTintedVersion(Color color)
        {
            Renderer.Image[] newImages = new Renderer.Image[images.Length];
            for (int i = 0; i < newImages.Length; i++)
            {
                newImages[i] = images[i].CreateTintedVersion(color);
            }
            return new StateSelectImage(select, border, newImages);
        }

    }
}
