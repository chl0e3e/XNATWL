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

        public FontMapper FontMapper => throw new NotImplementedException();

        private GameTime _gameTime;
        private List<TextureArea> textureAreas;
        //private List<TextureAreaRotated> rotatedTextureAreas;
        private List<XNADynamicImage> dynamicImages;
        private GraphicsDevice _graphicsDevice;
        private XNATWL.Renderer.CacheContext _cacheContext;
        private TintStack _tintStack;

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
        }

        public void ClipEnter(int x, int y, int w, int h)
        {
            //throw new NotImplementedException();
        }

        public void ClipEnter(Rect rect)
        {
            //throw new NotImplementedException();
        }

        public bool ClipIsEmpty()
        {
            return true;
            //throw new NotImplementedException();
        }

        public void ClipLeave()
        {
            //throw new NotImplementedException();
        }

        public DynamicImage CreateDynamicImage(int width, int height)
        {
            XNADynamicImage image = new XNADynamicImage(this, 0, 0, width, height, Color.WHITE);
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
            //throw new NotImplementedException();
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
            return true;
            //throw new NotImplementedException();
        }

        public void EndRendering()
        {
            //throw new NotImplementedException();
        }

        public void DrawLine(float[] pts, int numPts, float width, Color color, bool drawAsLoop)
        {
            //throw new NotImplementedException();
        }
    }
}
