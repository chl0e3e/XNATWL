using Microsoft.Xna.Framework.Graphics;
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

        public XNADynamicImage(XNARenderer renderer, int width, int height, Color tintColor) : base(null, 0, 0, width, height)
        {
            this._renderer = renderer;
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
        }

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
            this.drawQuad(x, y, this.width, this.height);
        }

        public void Draw(AnimationState state, int x, int y, int width, int height)
        {
            this.drawQuad(x, y, width, height);
        }

        public void Update(Microsoft.Xna.Framework.Color[] data)
        {
            if (this._texture != null)
            {
                this._texture.Dispose();
            }

            Texture2D texture2D = new Texture2D(this._renderer.GraphicsDevice, this.width, this.height);
            texture2D.SetData(data);
            this._texture = new XNATexture(this._renderer, this.width, this.height, texture2D);
        }
    }
}
