using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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

        public FontCache CacheMultiLineText(Color color, FontCache prevCache, string str, int width, HAlignment alignment)
        {
            return new XNAFontCache(this, color, str);
        }

        public AttributedStringFontCache CacheMultiLineText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString)
        {
            throw new NotImplementedException();
        }

        public AttributedStringFontCache CacheMultiLineText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end)
        {
            throw new NotImplementedException();
        }

        public FontCache CacheText(Color color, FontCache prevCache, string str)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@1 " + str);
            return new XNAFontCache(this, color, str);
        }

        public FontCache CacheText(Color color, FontCache prevCache, string str, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@2");
            return new XNAFontCache(this, color, str.Substring(start, end - start));
        }

        public AttributedStringFontCache CacheText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@3");
            return new XNAASFontCache(this, color, attributedString);
        }

        public AttributedStringFontCache CacheText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("CacheText@4");
            return new XNAASFontCache(this, color, attributedString, start, end);
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

        public int DrawMultiLineText(Color color, AnimationState animState, int x, int y, string str, int width, HAlignment alignment)
        {
            FontState fontState = evalFontState(animState);
            x += fontState.offsetX;
            y += fontState.offsetY;
            System.Diagnostics.Debug.WriteLine("DrawMultiLineText");
            return this._bitmapFont.drawMultiLineText(fontState.color, x, y, str, 100, HAlignment.CENTER);
        }

        public void DrawMultiLineText(Color color, int x, int y, AttributedString attributedString)
        {
            throw new NotImplementedException();
        }

        public void DrawMultiLineText(Color color, int x, int y, AttributedString attributedString, int start, int end)
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

        public int DrawText(Color color, AnimationState animState, int x, int y, string str)
        {
            FontState fontState = evalFontState(animState);
            x += fontState.offsetX;
            y += fontState.offsetY;
            //System.Diagnostics.Debug.WriteLine("DrawText@1");
            return this._bitmapFont.drawText(fontState.color, x, y, str, 0, str.Length);
        }

        public int DrawText(Color color, AnimationState animState, int x, int y, string str, int start, int end)
        {
            FontState fontState = evalFontState(animState);
            x += fontState.offsetX;
            y += fontState.offsetY;
            //System.Diagnostics.Debug.WriteLine("DrawText@start,end");
            return this._bitmapFont.drawText(fontState.color, x, y, str, start, end);
        }

        public int DrawText(Color color, int x, int y, AttributedString attributedString)
        {
            //System.Diagnostics.Debug.WriteLine("DrawText@AttributedString");
            // throw new NotImplementedException();
            string str = attributedString.Value;
            return this._bitmapFont.drawText(color, x, y, str, 0, str.Length);
        }

        public int DrawText(Color color, int x, int y, AttributedString attributedString, int start, int end)
        {
            //System.Diagnostics.Debug.WriteLine("DrawText@AttributedString<start,end>");
            string str = attributedString.Value;
            return this._bitmapFont.drawText(color, x, y, str, start, end);
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
            private string _str;

            private int _start;
            private int _end;

            private Color _color;

            public XNAASFontCache(XNAFont font, Color color, string str)
            {
                this._str = str;
                this._font = font;
                this._start = 0;
                this._end = this._str.Length;
                this._color = color;
            }

            public XNAASFontCache(XNAFont font, Color color, string str, int start, int end)
            {
                this._str = str;
                this._font = font;
                this._start = start;
                this._end = end;
                this._color = color;
            }

            public XNAASFontCache(XNAFont font, Color color, AttributedString str)
            {
                this._str = str.Value;
                this._font = font;
                this._start = 0;
                this._end = this._str.Length;
                this._color = color;
            }

            public XNAASFontCache(XNAFont font, Color color, AttributedString str, int start, int end)
            {
                this._str = str.Value;
                this._font = font;
                this._start = start;
                this._end = end;
                this._color = color;
            }

            public int Width
            {
                get
                {
                    return this._font.ComputeTextWidth(this._str);
                }
            }

            public int Height
            {
                get
                {
                    return this._font.LineHeight;
                }
            }

            public void Dispose()
            {

            }

            public void Draw(AnimationState animationState, int x, int y)
            {
                this._font.DrawText(this._color, animationState,  x, y, this._str.Substring(this._start, this._end));
            }

            public void Draw(int x, int y)
            {
                this._font.DrawText(this._color, new XNATWL.AnimationState(), x, y, this._str.Substring(this._start, this._end));
            }
        }

        class XNAFontCache : FontCache
        {
            private XNAFont _font;
            private string _str;

            private Color _color;

            public XNAFontCache(XNAFont font, Color color, string str)
            {
                this._str = str;
                this._font = font;
                this._color = color;
            }

            public XNAFontCache(XNAFont font, Color color, AttributedString str)
            {
                this._str = str.Value;
                this._font = font;
                this._color = color;
            }

            public int Width
            {
                get
                {
                    return this._font.ComputeTextWidth(this._str);
                }
            }

            public int Height
            {
                get
                {
                    return this._font.LineHeight;
                }
            }

            public void Dispose()
            {

            }

            public void Draw(AnimationState animationState, int x, int y)
            {
                this._font.DrawText(this._color, animationState, x, y, this._str);
            }
        }

        class XNAMLFontCache : FontCache
        {
            private XNAFont _font;
            private string _str;
            private int _width;

            private Color _color;

            public XNAMLFontCache(XNAFont font, Color color, string str, int width)
            {
                this._str = str;
                this._font = font;
                this._color = color;
                this._width = width;
            }

            public XNAMLFontCache(XNAFont font, Color color, AttributedString str, int width)
            {
                this._str = str.Value;
                this._font = font;
                this._color = color;
                this._width = width;
            }

            public int Width
            {
                get
                {
                    return this._font.ComputeTextWidth(this._str);
                }
            }

            public int Height
            {
                get
                {
                    return this._font.LineHeight;
                }
            }

            public void Dispose()
            {

            }

            public void Draw(AnimationState animationState, int x, int y)
            {
                this._font.DrawMultiLineText(this._color, animationState, x, y, this._str, 100, HAlignment.LEFT);
            }
        }
    }
}
