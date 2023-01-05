using Microsoft.Xna.Framework.Graphics;

namespace XNATWL.Renderer.XNA
{
    public class XNATexture : Texture, Resource, QueriablePixels
    {
        public int Width
        {
            get
            {
                return this._width;
            }
        }

        public int Height
        {
            get
            {
                return this._height;
            }
        }

        private Texture2D _texture;
        private int _width;
        private int _height;
        private SpriteBatch _batch;
        private XNARenderer _renderer;

        public XNARenderer Renderer
        {
            get
            {
                return _renderer;
            }
        }

        public Texture2D Texture2D
        {
            get
            {
                return _texture;
            }
        }

        public SpriteBatch SpriteBatch
        {
            get
            {
                return _batch;
            }
        }

        public XNATexture(XNARenderer renderer, SpriteBatch spriteBatch, int width, int height, Texture2D texture)
        {
            this._texture = texture;
            this._width = width;
            this._height = height;
            this._batch = spriteBatch;
            this._renderer = renderer;
        }

        public XNATexture(XNARenderer renderer, int width, int height, Texture2D texture) : this(renderer, renderer.SpriteBatch, width, height, texture)
        {
        }

        public MouseCursor CreateCursor(int x, int y, int width, int height, int hotSpotX, int hotSpotY, Image imageRef)
        {
            return new XNACursor(this, x, y, width, height, Color.WHITE);
            //throw new NotImplementedException();
        }

        public Image GetImage(int x, int y, int width, int height, Color tintColor, bool tiled, TextureRotation rotation)
        {
            return new TextureArea(this, x, y, width, height, tintColor);
        }

        public int PixelValueAt(int x, int y)
        {
            return 0;
        }

        public void ThemeLoadingDone()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //this._batch.Dispose();
            this._texture.Dispose();
            //throw new NotImplementedException();
        }
    }
}
