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
                return this._bitmapFont.Proportional;
            }
        }

        public int BaseLine
        {
            get
            {
                return this._bitmapFont.BaseLine;
            }
        }

        public int LineHeight
        {
            get
            {
                return this._bitmapFont.LineHeight;
            }
        }

        public int SpaceWidth
        {
            get
            {
                return this._bitmapFont.SpaceWidth;
            }
        }

        public int MWidth
        {
            get
            {
                return this._bitmapFont.EM;
            }
        }

        public int xWidth
        {
            get
            {
                return this._bitmapFont.EX;
            }
        }

        private BitmapFont _bitmapFont;
        private StateSelect _stateSelect;
        private FontState[] _fontStates;
        private XNARenderer _renderer;

        /// <summary>
        /// A new <see cref="XNAFont"/> describing a font or font fmaily
        /// </summary>
        /// <param name="renderer">Parent renderer</param>
        /// <param name="baseFile">File to load font from</param>
        /// <param name="select">states to select</param>
        /// <param name="parameterList">array of font parameters</param>
        public XNAFont(XNARenderer renderer, FileSystemObject baseFile, StateSelect select, params FontParameter[] parameterList)
        {
            this._bitmapFont = BitmapFont.LoadFont(renderer, baseFile);

            this._stateSelect = select;
            this._renderer = renderer;

            this._fontStates = new FontState[parameterList.Count()];
            for (int i = 0; i < parameterList.Count(); i++)
            {
                _fontStates[i] = new FontState(parameterList[i]);
            }
        }

        public FontCache CacheMultiLineText(FontCache prevCache, string str, int width, HAlignment alignment)
        {
            return new XNAFontCache(this, str, 0, str.Length, width);
        }

        /// <summary>
        /// Caches multiple lines of an <see cref="AttributedString"/> for faster drawing
        /// </summary>
        /// <param name="prevCache">previous cache returned</param>
        /// <param name="attributedString">attributed string to draw</param>
        /// <returns>new cache to draw</returns>
        public AttributedStringFontCache CacheMultiLineText(AttributedStringFontCache prevCache, AttributedString attributedString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Caches a substring of multiple lines of an <see cref="AttributedString"/> for faster drawing
        /// </summary>
        /// <param name="prevCache">previous cache returned</param>
        /// <param name="attributedString">attributed string to draw</param>
        /// <param name="start">start of the substring to draw</param>
        /// <param name="end">end of the substring to draw</param>
        /// <returns>new cache to use when drawing</returns>
        public AttributedStringFontCache CacheMultiLineText(AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cache a single line of text for faster drawing
        /// </summary>
        /// <param name="prevCache">previous cache returned</param>
        /// <param name="str">string to draw</param>
        /// <returns>new cache to use when drawing</returns>
        public FontCache CacheText(FontCache prevCache, string str)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@1 " + str);
            if (prevCache != null && !(((XNAFontCache)prevCache).ShouldRedraw(str)))
            {
                return prevCache;
            }

            return new XNAFontCache(this, str);
        }

        /// <summary>
        /// Cache a substring of a single line of text for faster drawing
        /// </summary>
        /// <param name="prevCache">previous cache returned</param>
        /// <param name="str">string to draw</param>
        /// <param name="start">start of the substring to draw</param>
        /// <param name="end">end of the substring to draw</param>
        /// <returns>new cache to use when drawing</returns>
        public FontCache CacheText(FontCache prevCache, string str, int start, int end)
        {
            if (prevCache != null && !(((XNAFontCache)prevCache).ShouldRedraw(str, start, end)))
            {
                return prevCache;
            }
            //System.Diagnostics.Debug.WriteLine("CacheText@2");
            return new XNAFontCache(this, str.Substring(start, end - start));
        }

        /// <summary>
        /// Cache a single line of an <see cref="AttributedString"/> for faster drawing
        /// </summary>
        /// <param name="prevCache">previous cache returned</param>
        /// <param name="attributedString">attributed string to draw</param>
        /// <returns>new cache to use when drawing</returns>
        public AttributedStringFontCache CacheText(AttributedStringFontCache prevCache, AttributedString attributedString)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@3");
            return new XNAASFontCache(this, attributedString);
        }

        /// <summary>
        /// Cache a substring of a single line of an <see cref="AttributedString"/> for faster drawing
        /// </summary>
        /// <param name="prevCache">previous cache returned</param>
        /// <param name="attributedString">attributed string to draw</param>
        /// <param name="start">start of the substring to draw</param>
        /// <param name="end">end of the substring to draw</param>
        /// <returns>new cache to use when drawing</returns>
        public AttributedStringFontCache CacheText(AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@4");
            return new XNAASFontCache(this, attributedString, start, end);
        }

        public int ComputeMultiLineTextWidth(string str)
        {
            return this._bitmapFont.ComputeMultiLineTextWidth(str);
        }

        public int ComputeTextWidth(string str)
        {
            return this._bitmapFont.ComputeTextWidth(str, 0, str.Length);
        }

        public int ComputeTextWidth(string str, int start, int end)
        {
            return this._bitmapFont.ComputeTextWidth(str, start, end);
        }

        public int ComputeVisibleGlyphs(string str, int start, int end, int width)
        {
            return this._bitmapFont.ComputeVisibleGlyphs(str, start, end, width);
        }

        public void Dispose()
        {
        }

        public int DrawMultiLineText(AnimationState animState, int x, int y, string str, int width, HAlignment alignment)
        {
            FontState fontState = this.EvalFontState(animState);
            x += fontState._offsetX;
            y += fontState._offsetY;
            return this._bitmapFont.DrawMultiLineText(fontState._color, x, y, str, 100, HAlignment.Center);
        }

        /// <summary>
        /// Draws multi line text - lines are splitted at '\n'
        /// </summary>
        /// <param name="x">X coordinate to draw at</param>
        /// <param name="y">Y coordinate to draw at</param>
        /// <param name="attributedString">attributed string to draw</param>
        public void DrawMultiLineText(int x, int y, AttributedString attributedString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws substring of multi line text - lines are splitted at '\n'
        /// </summary>
        /// <param name="x">X coordinate to draw at</param>
        /// <param name="y">Y coordinate to draw at</param>
        /// <param name="attributedString">attributed string to draw</param>
        /// <param name="start">start of the substring to draw</param>
        /// <param name="end">end of the substring to draw</param>
        public void DrawMultiLineText(int x, int y, AttributedString attributedString, int start, int end)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Evaluate and return a <see cref="FontState"/> from a given <see cref="AnimationState"/>
        /// </summary>
        /// <param name="animationState">Animation source</param>
        /// <returns><see cref="FontState"/> based on the <see cref="AnimationState"/></returns>
        FontState EvalFontState(AnimationState animationState)
        {
            return this._fontStates[this._stateSelect.Evaluate(animationState)];
        }

        public int DrawText(AnimationState animState, int x, int y, string str)
        {
            FontState fontState = this.EvalFontState(animState);
            x += fontState._offsetX;
            y += fontState._offsetY;
            //System.Diagnostics.Debug.WriteLine("DrawText@1");
            return this._bitmapFont.DrawText(fontState._color, x, y, str, 0, str.Length);
        }

        public int DrawText(AnimationState animState, int x, int y, string str, int start, int end)
        {
            FontState fontState = this.EvalFontState(animState);
            x += fontState._offsetX;
            y += fontState._offsetY;
            //System.Diagnostics.Debug.WriteLine("DrawText@start,end");
            return this._bitmapFont.DrawText(fontState._color, x, y, str, start, end);
        }

        /// <summary>
        /// Draws a single line text
        /// </summary>
        /// <param name="x">left coordinate of the text block</param>
        /// <param name="y">top coordinate of the text block</param>
        /// <param name="attributedString">the attributed string to draw</param>
        /// <returns>the width in pixels of the text</returns>
        public int DrawText(int x, int y, AttributedString attributedString)
        {
            //System.Diagnostics.Debug.WriteLine("DrawText@AttributedString");
            // throw new NotImplementedException();
            FontState fontState = this.EvalFontState(attributedString);
            x += fontState._offsetX;
            y += fontState._offsetY;
            string str = attributedString.Value;
            return this._bitmapFont.DrawText(fontState._color, x, y, str, 0, str.Length);
        }

        /// <summary>
        /// Draws a substring of a single line of text
        /// </summary>
        /// <param name="x">left coordinate of the text block</param>
        /// <param name="y">top coordinate of the text block</param>
        /// <param name="attributedString">the attributed string to draw</param>
        /// <param name="start">start of the substring to draw</param>
        /// <param name="end">end of the substring to draw</param>
        /// <returns>the width in pixels of the text</returns>
        public int DrawText(int x, int y, AttributedString attributedString, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("DrawText@AttributedString<start,end>");
            FontState fontState = EvalFontState(attributedString);
            x += fontState._offsetX;
            y += fontState._offsetY;
            string str = attributedString.Value;
            return this._bitmapFont.DrawText(fontState._color, x, y, str, start, end);
        }

        /// <summary>
        /// Font state which describes how to draw the font from a <see cref="FontParameter"/>
        /// </summary>
        public class FontState
        {
            internal Color _color;
            internal int _offsetX;
            internal int _offsetY;
            internal int _style;
            internal int _underlineOffset;

            /// <summary>
            /// Create a new font state
            /// </summary>
            /// <param name="fontParam">font parameter</param>
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

                this._color = fontParam.Get(FontParameter.COLOR);
                this._offsetX = fontParam.Get(XNARenderer.FONTPARAM_OFFSET_X);
                this._offsetY = fontParam.Get(XNARenderer.FONTPARAM_OFFSET_Y);
                this._style = lineStyle;
                this._underlineOffset = fontParam.Get(XNARenderer.FONTPARAM_UNDERLINE_OFFSET);
            }
        }

        /// <summary>
        /// Font cache implementation for an AttributedString
        /// </summary>
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
                this._fontState = this._font.EvalFontState(this._str);

                this._cachedRenderTarget = new RenderTarget2D(this._font._renderer.GraphicsDevice, this._width, this._height);

                this.CacheDraw();
            }

            public XNAASFontCache(XNAFont font, AttributedString str) : this(font, str, 0, str.Length)
            {
            }

            /// <summary>
            /// Should we redraw this cache given it's new str
            /// </summary>
            /// <param name="str">New string to test cache equals</param>
            /// <returns><b>true</b> if redraw</returns>
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

            /// <summary>
            /// Render the font to a cache
            /// </summary>
            public void CacheDraw()
            {
                this._font._renderer.GraphicsDevice.SetRenderTarget(this._cachedRenderTarget);
                this._font._renderer.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
                this._font._bitmapFont.DrawText(Color.BLACK, 0, 0, this._str.Value, this._start, this._end);
                this._font._renderer.GraphicsDevice.SetRenderTarget(null);
                this._cachedXNATexture = new XNATexture(this._font._renderer, this._width, this._height, this._cachedRenderTarget);

                this._cachedImage = (TextureAreaBase) this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.None);
            }

            public void Dispose()
            {
                this._cachedXNATexture.Dispose();
                this._cachedRenderTarget.Dispose();
            }

            public void Draw(int x, int y)
            {
                x += this._fontState._offsetX;
                y += this._fontState._offsetY;
                this._cachedImage.DrawQuad(this._fontState._color, x, y, this._width, this._height);
            }
        }

        /// <summary>
        /// Font cache for strings that aren't described by objects
        /// </summary>
        class XNAFontCache : FontCache
        {
            private XNAFont _font;
            private XNATexture _cachedXNATexture;
            private TextureAreaBase _cachedImage = null;
            private RenderTarget2D _cachedRenderTarget = null;
            private string _str;

            private int _width;
            private int _height;

            private int _multiLineWidth;

            private int _start;
            private int _end;

            /// <summary>
            /// New FontCache instance for a string
            /// </summary>
            /// <param name="font">Font to draw using</param>
            /// <param name="str">String to draw</param>
            /// <param name="start">Position in string to draw from</param>
            /// <param name="end">Position in string to draw to</param>
            /// <param name="multiLineWidth">Maximum length of a line</param>
            public XNAFontCache(XNAFont font, string str, int start, int end, int multiLineWidth)
            {
                this._str = str;
                this._font = font;

                this._start = start;
                this._end = end;

                this._multiLineWidth = multiLineWidth;
                this.CacheDraw();
            }

            /// <summary>
            /// New FontCache instance for a string
            /// </summary>
            /// <param name="font">Font to draw using</param>
            /// <param name="str">String to draw</param>
            /// <param name="start">Position in string to draw from</param>
            /// <param name="end">Position in string to draw to</param>
            public XNAFontCache(XNAFont font, string str, int start, int end) : this(font, str, start, end, -1)
            {

            }

            /// <summary>
            /// New FontCache instance for a string
            /// </summary>
            /// <param name="font">Font to draw using</param>
            /// <param name="str">String to draw</param>
            public XNAFontCache(XNAFont font, string str) : this(font, str, 0, str.Length, -1)
            {
                
            }

            /// <summary>
            /// Render the font to a cache
            /// </summary>
            public void CacheDraw()
            {
                if (this._str == "\n")
                {
                    this._height = this._font.LineHeight;
                    if (this._multiLineWidth > 0)
                    {
                        this._width = this._multiLineWidth;
                    }
                    else
                    {
                        this._width = this._font.SpaceWidth;
                    }
                    return;
                }

                if (this._multiLineWidth > 0)
                {
                    BitmapFont.MultiLineTexelCache _texOutput = this._font._bitmapFont.CacheBDrawMultiLineText(Color.BLACK, 0, 0, this._str, this._start, this._end, this._multiLineWidth);
                    this._width = _texOutput.Width;
                    this._height = _texOutput.Height;
                    Texture2D cachedTexture = new Texture2D(this._font._renderer.GraphicsDevice, this._width, this._height);
                    cachedTexture.SetData(_texOutput.LineColors);
                    this._cachedXNATexture = new XNATexture(this._font._renderer, this._width, this._height, cachedTexture);
                    this._cachedImage = (TextureAreaBase)this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.None);
                }
                else
                {
                    BitmapFont.SingleLineTexelCache _texOutput = this._font._bitmapFont.CacheBDrawText(Color.BLACK, 0, 0, this._str, this._start, this._end);
                    this._width = _texOutput.Width;
                    this._height = this._font.LineHeight;
                    Texture2D cachedTexture = new Texture2D(this._font._renderer.GraphicsDevice, this._width, this._height);
                    cachedTexture.SetData(_texOutput.LineColors);
                    this._cachedXNATexture = new XNATexture(this._font._renderer, this._width, this._height, cachedTexture);
                    this._cachedImage = (TextureAreaBase)this._cachedXNATexture.GetImage(0, 0, this._width, this._height, Color.BLACK, false, TextureRotation.None);
                }
            }

            /// <summary>
            /// Should we redraw this cache given it's new str
            /// </summary>
            /// <param name="str">New string to test cache equals</param>
            /// <returns><b>true</b> if redraw</returns>
            public bool ShouldRedraw(string str)
            {
                return this._str != str;
            }

            /// <summary>
            /// Should we redraw this cache given it's new substring/string range
            /// </summary>
            /// <param name="str">New string to test cache equals</param>
            /// <param name="start">Start of substring of 'str'</param>
            /// <param name="end">End of substring of 'str'</param>
            /// <returns><b>true</b> if redraw</returns>
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
                    this._font._renderer.Disposer.Add(this._cachedXNATexture);
                }
                //this._cachedRenderTarget.Dispose();
            }

            public void Draw(AnimationState animationState, int x, int y)
            {
                if (this._cachedImage == null)
                {
                    return;
                }
                FontState fontState = this._font.EvalFontState(animationState);
                x += fontState._offsetX;
                y += fontState._offsetY;

                this._cachedImage.DrawQuad(fontState._color, x, y, this._width, this._height);
            }
        }
    }
}
