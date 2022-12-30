using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class ComposedImage : Image, HasBorder
    {
        private Image[] layers;
        private Border border;

        public ComposedImage(Image[] layers, Border border) : base()
        {
            this.layers = layers;
            this.border = border;
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y)
        {
            Draw(animationState, x, y, Width, Height);
        }

        public void Draw(Renderer.AnimationState animationState, int x, int y, int width, int height)
        {
            foreach (Image layer in layers)
            {
                layer.Draw(animationState, x, y, width, height);
            }
        }

        public int Height
        {
            get
            {
                return layers[0].Height;
            }
        }

        public int Width
        {
            get
            {
                return layers[0].Width;
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
            Image[] newLayers = new Image[layers.Length];
            for (int i = 0; i < newLayers.Length; i++)
            {
                newLayers[i] = layers[i].CreateTintedVersion(color);
            }
            return new ComposedImage(newLayers, border);
        }
    }
}
