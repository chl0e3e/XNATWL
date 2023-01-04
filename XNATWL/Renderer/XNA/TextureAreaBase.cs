using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer.XNA
{
    public class TextureAreaBase
    {
        protected int tx0;
        protected int ty0;
        protected int tw;
        protected int th;
        protected int width;
        protected int height;
        protected XNATexture _texture;

        public TextureAreaBase(XNATexture texture, int x, int y, int width, int height)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("No texture!");
            }
            if (height == 17)
            {
                //throw new Exception("Test");
                System.Diagnostics.Debug.WriteLine("Test");
            }
            // negative size allows for flipping
           // this.width = (short)Math.Abs(width);
            //this.height = (short)Math.Abs(height);
            /*float fx = x;
            float fy = y;
            if (width == 1 || width == -1)
            {
                fx += 0.5f;
                width = 0;
            }
            else if (width < 0)
            {
                fx -= width;
            }
            if (height == 1 || height == -1)
            {
                fy += 0.5f;
                height = 0;
            }
            else if (height < 0)
            {
                fy -= height;
            }*/
            this.tx0 = x;
            this.ty0 = y;
            this.tw = width;
            this.th = height;
            this.width = width;
            this.height = height;
            this._texture = texture;
        }

        public TextureAreaBase(TextureAreaBase src)
        {
            this.tx0 = src.tx0;
            this.ty0 = src.ty0;
            this.tw = src.tw;
            this.th = src.th;
            this.width = src.width;
            this.height = src.height;
            this._texture = src._texture;
        }

        public int getWidth()
        {
            return this.tw;
        }

        public int getHeight()
        {
            return this.th;
        }

        internal virtual void drawQuad(int x, int y, int w, int h)
        {
            this._texture.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            this._texture.SpriteBatch.Draw(this._texture.Texture2D, new Microsoft.Xna.Framework.Rectangle(x, y, w, h), new Microsoft.Xna.Framework.Rectangle(this.tx0, this.ty0, this.tw, this.th), this._texture.Renderer.TintStack.XNAColor);
            this._texture.SpriteBatch.End();
        }
    }
}
