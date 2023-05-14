/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using XNATWL.Utils;
using XNATWL.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using XNATWL.Input.XNA;
using System.Diagnostics;

namespace XNATWL.Renderer.XNA
{
    public class XNARenderer : Renderer, LineRenderer
    {
        public static StateKey STATE_LEFT_MOUSE_BUTTON = StateKey.Get("leftMouseButton");
        public static StateKey STATE_MIDDLE_MOUSE_BUTTON = StateKey.Get("middleMouseButton");
        public static StateKey STATE_RIGHT_MOUSE_BUTTON = StateKey.Get("rightMouseButton");

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

        public DeferredDisposer Disposer
        {
            get
            {
                return this._disposer;
            }
        }

        private GameTime _gameTime;
        private List<TextureArea> textureAreas;
        //private List<TextureAreaRotated> rotatedTextureAreas;
        private List<XNADynamicImage> dynamicImages;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private XNATWL.Renderer.CacheContext _cacheContext;
        private TintStack _tintStack;
        private ClipStack _clipStack;
        private MouseCursor _mouseCursor;
        private XNACursor _defaultCursor;
        private DeferredDisposer _disposer;
        protected Rect clipRectTemp;
        private bool hasScissor;
        private Rectangle? _defaultScissor = null;
        public bool rendering = false;
        private SWCursorAnimState _cursorAnimState;
        private int _mouseX = 0;
        private int _mouseY = 0;

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
        
        public GameTime GameTime
        {
            get
            {
                return this._gameTime;
            }
        }

        private StackTrace lastCall = null;
        public SpriteBatch SpriteBatch
        {
            get
            {
                return this._spriteBatch;
            }
        }

        public AnimationState CursorAnimationState
        {
            get
            {
                return this._cursorAnimState;
            }
        }

        public XNARenderer(GraphicsDevice graphicsDevice)
        {
            this._gameTime = new GameTime();
            this._graphicsDevice = graphicsDevice;
            this._tintStack = new TintStack();
            this._clipStack = new ClipStack();
            this._spriteBatch = new SpriteBatch(this._graphicsDevice);
            this.clipRectTemp = new Rect();
            this._disposer = new DeferredDisposer(this);
            this.dynamicImages = new List<XNADynamicImage>();

            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABEAAAAZCAMAAADg4DWlAAAAmVBMVEUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD///8jo0yHAAAAMXRSTlMAAQUPA0IfMgInFzlGCwwpGQcJEiIIP04vGDoqDgYbNVFDGjFBMEgWIElFEyw4PjRLq1RDbAAAALhJREFUeF5V0NlygzAMBVDbKK1tgtlJSgrZ93RR/v/jgjwTC/R45o6kuUJIDVqK0SDkSaHGhBhlNhoTPtGkH0QsT4w/iVgGmjGRMAVhCsLEwsTCRDKhsIemTW2hvQyKm8Xp989kCZAgyeL/cKxslFMG11uia7OySQdSIO72Mcl8tvyq/fX6bNKLp7j6Bnqxo34mId+h5tC7Z5Jbs7qrUJxEfLQ/pmQR4HpjegcsUrmydNQHEygFA7wAtBchFf1cSBMAAAAASUVORK5CYII=");
            var contents = new MemoryStream(bytes);
            Texture2D texture = Texture2D.FromStream(this._graphicsDevice, contents);
            contents.Close();

            this._defaultCursor = new XNACursor(new XNATexture(this, 17, 25, texture), 0, 0, 17, 25, 0, 0);

            this._cursorAnimState = new SWCursorAnimState();
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
            if (cursor == null)
            {
                cursor = this._defaultCursor;
            }

            this._mouseCursor = cursor;
        }

        public void SetMouseButton(int button, bool state)
        {
            this._cursorAnimState.SetAnimationState(button, state);
        }

        public void SetUseSWMouseCursors(bool useSWMouseCursors)
        {
            // XNA does not support HW cursors
        }

        public void SetMousePosition(int mouseX, int mouseY)
        {
            this._mouseX = mouseX;
            this._mouseY = mouseY;
        }

        private BasicEffect startEffect;

        public bool StartRendering()
        {
            RasterizerState rasterizerState = new RasterizerState()
            {
                ScissorTestEnable = true,
            };
            this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, rasterizerState);
            this._clipStack.clearStack();
            rendering = true;

            return true;
        }

        public void EndRendering()
        {
            XNACursor cursor = this._mouseCursor == null ? this._defaultCursor : ((XNACursor)this._mouseCursor);
            cursor.drawQuad(Color.WHITE, this._mouseX, this._mouseY, cursor.getWidth(), cursor.getHeight());
            
            rendering = false;
            this.Disposer.Update();

            this._spriteBatch.End();
        }

        public void DrawLine(float[] pts, int numPts, float width, Color color, bool drawAsLoop)
        {
            Vector2 last = new Vector2(0, 0);
            for(int i = 0; i < pts.Length; i+= 2)
            {
                Vector2 next = new Vector2(pts[i], pts[i + 1]);
                Primitives2D.DrawLine(this._spriteBatch, next, last, color.XNA);
                last = next;
            }
        }

        public class SWCursorAnimState : AnimationState
        {
            private long[] lastTime;
            private bool[] active;

            public SWCursorAnimState()
            {
                lastTime = new long[3];
                active = new bool[3];
            }

            public void SetAnimationState(int idx, bool isActive)
            {
                if (idx >= 0 && idx < 3 && active[idx] != isActive)
                {
                    lastTime[idx] = DateTime.Now.Ticks;
                    active[idx] = isActive;
                }
            }

            private int GetMouseButton(StateKey key)
            {
                if (key == STATE_LEFT_MOUSE_BUTTON)
                {
                    return Event.MOUSE_LBUTTON;
                }
                if (key == STATE_MIDDLE_MOUSE_BUTTON)
                {
                    return Event.MOUSE_MBUTTON;
                }
                if (key == STATE_RIGHT_MOUSE_BUTTON)
                {
                    return Event.MOUSE_RBUTTON;
                }
                return -1;
            }

            public int GetAnimationTime(StateKey state)
            {
                long curTime = DateTime.Now.Ticks;
                int idx = this.GetMouseButton(state);
                if (idx >= 0)
                {
                    curTime -= lastTime[idx];
                }
                return (int)curTime & Int32.MaxValue;
            }

            public bool GetAnimationState(StateKey state)
            {
                int idx = this.GetMouseButton(state);
                if (idx >= 0)
                {
                    return active[idx];
                }
                return false;
            }

            public bool ShouldAnimateState(StateKey state)
            {
                return true;
            }
        }
    }
}
