using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.Logger;
using XNATWL.Utils;
using XNATWL.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using XNATWL.Input.XNA;
using System.Net.Http;

namespace XNATWL.Renderer.XNA
{
    public class XNARenderer : Renderer, LineRenderer
    {
        public static FontParameter.Parameter<int> FONTPARAM_OFFSET_X = FontParameter.NewParameter("offsetX", 0);
        public static FontParameter.Parameter<int> FONTPARAM_OFFSET_Y = FontParameter.NewParameter("offsetY", 0);
        public static FontParameter.Parameter<int> FONTPARAM_UNDERLINE_OFFSET = FontParameter.NewParameter("underlineOffset", 0);

        public long TimeMillis
        {
            get
            {
                return (long) this._gameTime.TotalGameTime.TotalMilliseconds;
            }
        }

        public Input.Input Input
        {
            get
            {
                return new XNAInput();
            }
        }

        public int Width
        {
            get
            {
                return this._graphicsDevice.Viewport.Width;
            }
        }

        public int Height
        {
            get
            {
                return this._graphicsDevice.Viewport.Height;
            }
        }

        public LineRenderer LineRenderer
        {
            get
            {
                return this;
            }
        }

        public OffscreenRenderer OffscreenRenderer => throw new NotImplementedException();

        public FontMapper FontMapper
        {
            get
            {
                return null;
            }
        }

        private GameTime _gameTime;
        private List<TextureArea> textureAreas;
        //private List<TextureAreaRotated> rotatedTextureAreas;
        private List<XNADynamicImage> dynamicImages;
        private GraphicsDevice _graphicsDevice;
        private XNATWL.Renderer.CacheContext _cacheContext;
        private TintStack _tintStack;
        private ClipStack _clipStack;
        private MouseCursor _mouseCursor;
        private XNACursor _defaultCursor;
        protected Rect clipRectTemp;
        private bool hasScissor;
        private Rectangle? _defaultScissor = null;

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return this._graphicsDevice;
            }
        }

        public TintStack TintStack
        {
            get
            {
                return this._tintStack;
            }
        }

        public XNARenderer(GraphicsDevice graphicsDevice)
        {
            this._gameTime = new GameTime();
            this._graphicsDevice = graphicsDevice;
            this._tintStack = new TintStack();
            this._clipStack = new ClipStack();
            this.clipRectTemp = new Rect();

            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABEAAAAZCAMAAADg4DWlAAAAmVBMVEUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD///8jo0yHAAAAMXRSTlMAAQUPA0IfMgInFzlGCwwpGQcJEiIIP04vGDoqDgYbNVFDGjFBMEgWIElFEyw4PjRLq1RDbAAAALhJREFUeF5V0NlygzAMBVDbKK1tgtlJSgrZ93RR/v/jgjwTC/R45o6kuUJIDVqK0SDkSaHGhBhlNhoTPtGkH0QsT4w/iVgGmjGRMAVhCsLEwsTCRDKhsIemTW2hvQyKm8Xp989kCZAgyeL/cKxslFMG11uia7OySQdSIO72Mcl8tvyq/fX6bNKLp7j6Bnqxo34mId+h5tC7Z5Jbs7qrUJxEfLQ/pmQR4HpjegcsUrmydNQHEygFA7wAtBchFf1cSBMAAAAASUVORK5CYII=");
            var contents = new MemoryStream(bytes);
            Texture2D texture = Texture2D.FromStream(this._graphicsDevice, contents);
            contents.Close();

            this._defaultCursor = new XNACursor(new XNATexture(this, 17, 25, texture), 0, 0, 17, 25, Color.WHITE);
        }

        protected void setClipRect()
        {
            Rect rect = clipRectTemp;
            if (_clipStack.getClipRect(rect))
            {
                if (!hasScissor)
                {
                    this._defaultScissor = this._graphicsDevice.ScissorRectangle;
                    this._graphicsDevice.RasterizerState.ScissorTestEnable = true;
                    hasScissor = true;
                }
                Rectangle scissor = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                this._graphicsDevice.ScissorRectangle = scissor;
               /* SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
                Texture2D dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
                dummyTexture.SetData(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

                spriteBatch.Begin();
                spriteBatch.Draw(dummyTexture, scissor, Microsoft.Xna.Framework.Color.Red);
                spriteBatch.End();*/
            }
            else if (hasScissor)
            {
                this._graphicsDevice.ScissorRectangle = (Rectangle) this._defaultScissor;
                this._graphicsDevice.RasterizerState.ScissorTestEnable = false;
                hasScissor = false;
            }
        }

        public void ClipEnter(int x, int y, int w, int h)
        {
            this._clipStack.push(x, y, w, h);
            setClipRect();
        }

        public void ClipEnter(Rect rect)
        {
            this._clipStack.push(rect);
            setClipRect();
        }

        public bool ClipIsEmpty()
        {
            return this._clipStack.isClipEmpty();
            //throw new NotImplementedException();
        }

        public void ClipLeave()
        {
            this._clipStack.pop();
            setClipRect();
        }

        public DynamicImage CreateDynamicImage(int width, int height)
        {
            XNADynamicImage image = new XNADynamicImage(this, width, height, Color.WHITE);
            dynamicImages.Add(image);
            return image;
        }

        public Image CreateGradient(Gradient gradient)
        {
            throw new NotImplementedException();
        }

        public XNATWL.Renderer.CacheContext CreateNewCacheContext()
        {
            return new CacheContext();
        }

        public XNATWL.Renderer.CacheContext GetActiveCacheContext()
        {
            return this._cacheContext;
        }

        public void SetActiveCacheContext(XNATWL.Renderer.CacheContext cc)
        {
            this._cacheContext = cc;
        }

        public Font LoadFont(FileSystemObject baseFile, StateSelect select, params FontParameter[] parameterList)
        {
            return new XNAFont(this, baseFile, select, parameterList);
        }

        public Texture LoadTexture(FileSystemObject file, string format, string filter)
        {
            FileStream titleStream = File.OpenRead(file.Path);
            Texture2D texture = Texture2D.FromStream(this._graphicsDevice, titleStream);
            titleStream.Close();

            return new XNATexture(this, texture.Width, texture.Height, texture);
        }

        public void PopGlobalTintColor()
        {
            _tintStack = _tintStack.pop();
        }

        public void PushGlobalTintColor(float r, float g, float b, float a)
        {
            _tintStack = _tintStack.push(r, g, b, a);
        }

        public void SetCursor(MouseCursor cursor)
        {
            this._mouseCursor = cursor;
        }

        public void SetMouseButton(int button, bool state)
        {
           // throw new NotImplementedException();
        }

        public void SetMousePosition(int mouseX, int mouseY)
        {
            //throw new NotImplementedException();
        }

        public bool StartRendering()
        {
            this._clipStack.clearStack();
            return true;
            //throw new NotImplementedException();
        }

        public void EndRendering()
        {
            //System.Diagnostics.Debug.WriteLine(this._mouseCursor == null);
            XNACursor cursor = this._mouseCursor == null ? this._defaultCursor : ((XNACursor)this._mouseCursor);
            MouseState ms = Mouse.GetState();
            cursor.drawQuad(ms.X, ms.Y, cursor.getWidth(), cursor.getHeight());
            //System.Diagnostics.Debug.WriteLine("x: " + ms.X + ", y: " + ms.Y);
            //throw new NotImplementedException();
        }

        public void DrawLine(float[] pts, int numPts, float width, Color color, bool drawAsLoop)
        {
            //throw new NotImplementedException();
        }
    }
}
