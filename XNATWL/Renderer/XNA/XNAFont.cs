using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using XNATWL.IO;
using XNATWL.Utils;

namespace XNATWL.Renderer.XNA
{
    public class XNAFont : Font, Font2
    {
        static int STYLE_UNDERLINE = 1;
        static int STYLE_LINETHROUGH = 2;

        public bool Proportional
        {
            get
            {
                return this._bitmapFont.isProportional();
            }
        }

        public int BaseLine
        {
            get
            {
                return this._bitmapFont.getBaseLine();
            }
        }

        public int LineHeight
        {
            get
            {
                return this._bitmapFont.getLineHeight();
            }
        }

        public int SpaceWidth
        {
            get
            {
                return this._bitmapFont.getSpaceWidth();
            }
        }

        public int MWidth
        {
            get
            {
                return this._bitmapFont.getEM();
            }
        }

        public int xWidth
        {
            get
            {
                return this._bitmapFont.getEX();
            }
        }

        private BitmapFont _bitmapFont;
        private StateSelect stateSelect;
        private FontState[] fontStates;
        private XNARenderer renderer;

        public XNAFont(XNARenderer renderer, FileSystemObject baseFile, StateSelect select, params FontParameter[] parameterList)
        {
            this._bitmapFont = BitmapFont.loadFont(renderer, baseFile);

            this.stateSelect = select;
            this.renderer = renderer;

            this.fontStates = new FontState[parameterList.Count()];
            for (int i = 0; i < parameterList.Count(); i++)
            {
                fontStates[i] = new FontState(parameterList[i]);
            }
        }

        public FontCache CacheMultiLineText(FontCache prevCache, string str, int width, HAlignment alignment)
        {
            return new XNAFontCache(this, str, 0, str.Length, width);
        }

        public AttributedStringFontCache CacheMultiLineText(AttributedStringFontCache prevCache, AttributedString attributedString)
        {
            throw new NotImplementedException();
        }

        public AttributedStringFontCache CacheMultiLineText(AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end)
        {
            throw new NotImplementedException();
        }

        public FontCache CacheText(FontCache prevCache, string str)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@1 " + str);
            if (prevCache != null && !(((XNAFontCache)prevCache).ShouldRedraw(str)))
            {
                return prevCache;
            }
            return new XNAFontCache(this, str);
        }

        public FontCache CacheText(FontCache prevCache, string str, int start, int end)
        {
            if (prevCache != null && !(((XNAFontCache)prevCache).ShouldRedraw(str, start, end)))
            {
                return prevCache;
            }
            //System.Diagnostics.Debug.WriteLine("CacheText@2");
            return new XNAFontCache(this, str.Substring(start, end - start));
        }

        public AttributedStringFontCache CacheText(AttributedStringFontCache prevCache, AttributedString attributedString)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@3");
            return new XNAASFontCache(this, attributedString);
        }

        public AttributedStringFontCache CacheText(AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@4");
            return new XNAASFontCache(this, attributedString, start, end);
        }

        public int ComputeMultiLineTextWidth(string str)
        {
            return this._bitmapFont.computeMultiLineTextWidth(str);
        }

        public int ComputeTextWidth(string str)
        {
            return this._bitmapFont.computeTextWidth(str, 0, str.Length);
        }

        public int ComputeTextWidth(string str, int start, int end)
        {
            return this._bitmapFont.computeTextWidth(str, start, end);
        }

        public int ComputeVisibleGlyphs(string str, int start, int end, int width)
        {
            return this._bitmapFont.computeVisibleGlpyhs(str, start, end, width);
        }

        public void Dispose()
        {
        }

        public int DrawMultiLineText(AnimationState animState, int x, int y, string str, int width, HAlignment alignment)
        {
            FontState fontState = evalFontState(animState);
            x += fontState.offsetX;
            y += fontState.offsetY;
            return this._bitmapFont.drawMultiLineText(fontState.color, x, y, str, 100, HAlignment.CENTER);
        }

        public void DrawMultiLineText(int x, int y, AttributedString attributedString)
        {
            throw new NotImplementedException();
        }

        public void DrawMultiLineText(int x, int y, AttributedString attributedString, int start, int end)
        {
            throw new NotImplementedException();
        }

        /*private int drawText(int x, int y, AttributedString attributedString, int start, int end, bool multiLine)
        {
            int startX = x;
            attributedString.Position = start;
            //if (!font.prepare())
            //{
            //    return 0;
            //}
            try
            {
                BitmapFont.Glyph lastGlyph = null;
                do
                {
                    FontState fontState = evalFontState(attributedString);
                    x += fontState.offsetX;
                    y += fontState.offsetY;
                    int runStart = x;
                    //rendere.setColor(fontState.color);
                    int nextStop = Math.Min(end, attributedString.Advance());
                    if (multiLine)
                    {
                        nextStop = TextUtil.indexOf(attributedString.Value, '\n', start, nextStop);
                    }
                    while (start < nextStop)
                    {
                        char ch = attributedString.CharAt(start++);
                        BitmapFont.Glyph g = this._bitmapFont.getGlyph(ch);
                        if (g != null)
                        {
                            if (lastGlyph != null)
                            {
                                x += lastGlyph.getKerning(ch);
                            }
                            lastGlyph = g;
                            if (g.getWidth() > 0)
                            {
                                g.draw(x, y);
                            }
                            x += g.xadvance;
                        }
                    }
                    drawLine(fontState, x, y, x - runStart);
                    x -= fontState.offsetX;
                    y -= fontState.offsetY;
                    if (multiLine && start < end && attributedString.CharAt(start) == '\n')
                    {
                        attributedString.Position = ++start;
                        x = startX;
                        y += this._bitmapFont.getLineHeight();
                        lastGlyph = null;
                    }
                } while (start < end);
            }
            finally
            {
                //font.cleanup();
            }
            return x - startX;
        }*/

        FontState evalFontState(AnimationState animationState)
        {
            return fontStates[stateSelect.Evaluate(animationState)];
        }

        public int DrawText(AnimationState animState, int x, int y, string str)
        {
            FontState fontState = evalFontState(animState);
            x += fontState.offsetX;
            y += fontState.offsetY;
            //System.Diagnostics.Debug.WriteLine("DrawText@1");
            return this._bitmapFont.drawText(fontState.color, x, y, str, 0, str.Length);
        }

        public int DrawText(AnimationState animState, int x, int y, string str, int start, int end)
        {
            FontState fontState = evalFontState(animState);
            x += fontState.offsetX;
            y += fontState.offsetY;
            //System.Diagnostics.Debug.WriteLine("DrawText@start,end");
            return this._bitmapFont.drawText(fontState.color, x, y, str, start, end);
        }

        public int DrawText(int x, int y, AttributedString attributedString)
        {
            //System.Diagnostics.Debug.WriteLine("DrawText@AttributedString");
            // throw new NotImplementedException();
            FontState fontState = evalFontState(attributedString);
            x += fontState.offsetX;
            y += fontState.offsetY;
            string str = attributedString.Value;
            return this._bitmapFont.drawText(fontState.color, x, y, str, 0, str.Length);
        }

        public int DrawText(int x, int y, AttributedString attributedString, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("DrawText@AttributedString<start,end>");
            FontState fontState = evalFontState(attributedString);
            x += fontState.offsetX;
            y += fontState.offsetY;
            string str = attributedString.Value;
            return this._bitmapFont.drawText(fontState.color, x, y, str, start, end);
        }

        public class FontState
        {
            internal Color color;
            internal int offsetX;
            internal int offsetY;
            internal int style;
            internal int underlineOffset;

            public FontState(FontParameter fontParam)
            {
                int lineStyle = 0;
                if (fontParam.Get(FontParameter.UNDERLINE))
                {
                    lineStyle |= STYLE_UNDERLINE;
                }
                if (fontParam.Get(FontParameter.LINETHROUGH))
                {
                    lineStyle |= STYLE_LINETHROUGH;
                }

                this.color = fontParam.Get(FontParameter.COLOR);
                this.offsetX = fontParam.Get(XNARenderer.FONTPARAM_OFFSET_X);
                this.offsetY = fontParam.Get(XNARenderer.FONTPARAM_OFFSET_Y);
                this.style = lineStyle;
                this.underlineOffset = fontParam.Get(XNARenderer.FONTPARAM_UNDERLINE_OFFSET);
            }
        }

        class XNAASFontCache : AttributedStringFontCache
        {
            private XNAFont _font;
            private AttributedString _str;
            private RenderTarget2D _cachedRenderTarget;
            private XNATexture _cachedXNATexture;
            private TextureAreaBase _cachedImage;
            private FontState _fontState;

            private int _start;
            private int _end;

            private int _width;
            private int _height;

            public XNAASFontCache(XNAFont font, AttributedString str, int start, int end)
            {
                this._str = str;
                this._font = font;
                this._start = start;
                this._end = end;
                this._width = this._font.ComputeTextWidth(this._str.Value);
                this._height = this._font.LineHeight;
                this._fontState = this._font.evalFontState(this._str);

                this._cachedRenderTarget = new RenderTarget2D(this._font.renderer.GraphicsDevice, this._width, this._height);

                this.CacheDraw();
            }

            public XNAASFontCache(XNAFont font, AttributedString str) : this(font, str, 0, str.Length)
            {
            }

            public bool ShouldRedraw(string str)
            {
                return this._str.Value == str;
            }

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

            public void CacheDraw()
            {
                this._font.renderer.GraphicsDevice.SetRenderTarget(this._cachedRenderTarget);
                this._font.renderer.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
                this._font._bitmapFont.drawText(Color.BLACK, 0, 0, this._str.Value, this._start, this._end);
                this._font.renderer.GraphicsDevice.SetRenderTarget(null);
                this._cachedXNATexture = new XNATexture(this._font.renderer, this._width, this._height, this._cachedRenderTarget);

                this._cachedImage = (TextureAreaBase) this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.NONE);
            }

            public void Dispose()
            {
                this._cachedXNATexture.Dispose();
                this._cachedRenderTarget.Dispose();
            }

            public void Draw(int x, int y)
            {
                x += this._fontState.offsetX;
                y += this._fontState.offsetY;
                this._cachedImage.drawQuad(this._fontState.color, x, y, this._width, this._height);
            }
        }

        class XNAFontCache : FontCache
        {
            private XNAFont _font;
            private XNATexture _cachedXNATexture;
            private TextureAreaBase _cachedImage = null;
            //private RenderTarget2D _cachedRenderTarget = null;
            private string _str;

            private int _width;
            private int _height;

            private int _multiLineWidth;

            private int _start;
            private int _end;

            public XNAFontCache(XNAFont font, string str, int start, int end, int multiLineWidth)
            {
                this._str = str;
                this._font = font;

                this._start = start;
                this._end = end;

                this._multiLineWidth = multiLineWidth;
                this.CacheDraw();
            }

            public XNAFontCache(XNAFont font, string str, int start, int end) : this(font, str, start, end, -1)
            {

            }

            public XNAFontCache(XNAFont font, string str) : this(font, str, 0, str.Length, -1)
            {
                
            }

            public void CacheDraw()
            {
                if (this._str.Trim() == "")
                {
                    this._cachedImage = null;
                    return;
                }

                /*System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                bool has = false;
                foreach (StackFrame sf in t.GetFrames())
                {
                    if (sf.GetMethod().Name.Contains("Update") || sf.GetMethod().Name.Contains("setup"))
                    {
                        has = true;
                        break;
                    }
                }
                if (!has)
                {
                    System.Diagnostics.Debug.WriteLine("test");
                }
                */
                //BasicEffect effect = new BasicEffect(this._font.renderer.GraphicsDevice);
                //effect.Begin

                /*this._width = this._font.ComputeTextWidth(this._str);
                this._height = this._font.LineHeight;
                this._cachedRenderTarget = new RenderTarget2D(this._font.renderer.GraphicsDevice, this._width, this._height, true, SurfaceFormat.Color, DepthFormat.None);
                this._font.renderer.GraphicsDevice.SetRenderTarget(this._cachedRenderTarget);
                this._font.renderer.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
                this._font._bitmapFont.drawText(Color.BLACK, 0, 0, this._str, this._start, this._end);
                this._font.renderer.GraphicsDevice.SetRenderTarget(null);
                this._cachedXNATexture = new XNATexture(this._font.renderer, this._width, this._height, this._cachedRenderTarget);

                this._cachedImage = (TextureAreaBase) this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.NONE);
                return;*/
                if (this._multiLineWidth > 0)
                {
                    BitmapFont.TexMultiLineOutput _texOutput = this._font._bitmapFont.cacheBDrawMultiLineText(Color.BLACK, 0, 0, this._str, this._start, this._end, this._multiLineWidth);
                    this._width = _texOutput.width;
                    this._height = _texOutput.height;
                    Texture2D cachedTexture = new Texture2D(this._font.renderer.GraphicsDevice, this._width, this._height);
                    cachedTexture.SetData(_texOutput.lineColors);
                    this._cachedXNATexture = new XNATexture(this._font.renderer, this._width, this._height, cachedTexture);
                    this._cachedImage = (TextureAreaBase)this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.NONE);
                }
                else
                {
                    this._width = this._font.ComputeTextWidth(this._str);
                    this._height = this._font.LineHeight;

                    BitmapFont.TexOutput _texOutput = this._font._bitmapFont.cacheBDrawText(Color.BLACK, 0, 0, this._str, this._start, this._end);
                    Texture2D cachedTexture = new Texture2D(this._font.renderer.GraphicsDevice, this._width, this._height);
                    cachedTexture.SetData(_texOutput.lineColors);
                    this._cachedXNATexture = new XNATexture(this._font.renderer, this._width, this._height, cachedTexture);
                    this._cachedImage = (TextureAreaBase)this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.NONE);
                }
            }

            public bool ShouldRedraw(string str)
            {
                return this._str != str;
            }

            public bool ShouldRedraw(string str, int start, int end)
            {
                return this._str != str && this._start != start && this._end != end;  
            }

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

            public void Dispose()
            {
                if (this._cachedXNATexture != null)
                {
                    this._font.renderer.Disposer.Add(this._cachedXNATexture);
                }
                //this._cachedRenderTarget.Dispose();
            }

            public void Draw(AnimationState animationState, int x, int y)
            {
                if (this._cachedImage == null)
                {
                    return;
                }
                FontState fontState = this._font.evalFontState(animationState);
                x += fontState.offsetX;
                y += fontState.offsetY;

                this._cachedImage.drawQuad(fontState.color, x, y, this._width, this._height);
            }
        }
    }
}
