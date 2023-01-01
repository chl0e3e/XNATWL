using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer.XNA
{
    public class XNADynamicImage : TextureAreaBase, DynamicImage
    {
        private XNARenderer _renderer;

        public XNADynamicImage(XNARenderer renderer, int x, int y, int width, int height, Color tintColor) : base(null, x, y, width, height)
        {
            this._renderer = renderer;
        }

        public int Width => throw new NotImplementedException();

        public int Height => throw new NotImplementedException();

        public Image CreateTintedVersion(Color color)
        {
            return this;
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Draw(AnimationState state, int x, int y)
        {
            //throw new NotImplementedException();
        }

        public void Draw(AnimationState state, int x, int y, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void Update(byte[] data, DynamicImageFormat format)
        {
            //throw new NotImplementedException();
        }

        public void Update(byte[] data, int stride, DynamicImageFormat format)
        {
            //throw new NotImplementedException();
        }

        public void Update(int xoffset, int yoffset, int width, int height, byte[] data, DynamicImageFormat format)
        {
            //throw new NotImplementedException();
        }

        public void Update(int xoffset, int yoffset, int width, int height, byte[] data, int stride, DynamicImageFormat format)
        {
            //throw new NotImplementedException();
        }
    }
}
