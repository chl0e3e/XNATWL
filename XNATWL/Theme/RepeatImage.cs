using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class RepeatImage : Image, HasBorder, SupportsDrawRepeat
    {
        private Image baseImage;
        private Border border;
        private bool repeatX;
        private bool repeatY;
        private SupportsDrawRepeat sdr;

        public RepeatImage(Image baseImage, Border border, bool repeatX, bool repeatY)
        {
            if (!repeatX && !repeatY)
            {
                throw new Exception("assert exception");
            }

            this.baseImage = baseImage;
            this.border = border;
            this.repeatX = repeatX;
            this.repeatY = repeatY;

            if (baseImage is SupportsDrawRepeat)
            {
                sdr = (SupportsDrawRepeat)baseImage;
            }
            else
            {
                sdr = this;
            }
        }

        public int Width
        {
            get
            {
                return baseImage.Width;
            }
        }

        public int Height
        {
            get
            {
                return baseImage.Height;
            }
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            baseImage.Draw(animationState, x, y);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            int countX = repeatX ? Math.Max(1, width / baseImage.Width) : 1;
            int countY = repeatY ? Math.Max(1, height / baseImage.Height) : 1;
            sdr.Draw(animationState, x, y, width, height, countX, countY);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height, int repeatCountX, int repeatCountY)
        {
            while (repeatCountY > 0)
            {
                int rowHeight = height / repeatCountY;

                int cx = 0;
                for (int xi = 0; xi < repeatCountX;)
                {
                    int nx = ++xi * width / repeatCountX;
                    baseImage.Draw(animationState, x + cx, y, nx - cx, rowHeight);
                    cx = nx;
                }

                y += rowHeight;
                height -= rowHeight;
                repeatCountY--;
            }
        }


        public Border Border
        {
            get
            {
                return border;
            }
        }

        public Image CreateTintedVersion(Color color)
        {
            return new RepeatImage(baseImage.CreateTintedVersion(color), border, repeatX, repeatY);
        }
    }
}
