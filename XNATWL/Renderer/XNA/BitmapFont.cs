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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using XNATWL.IO;
using XNATWL.Utils;

namespace XNATWL.Renderer.XNA
{
    public class BitmapFont
    {
        internal class GlyphTex : TextureAreaBase
        {
            internal short _xOffset;
            internal short _yOffset;
            internal short _xAdvance;
            internal byte[][] _kerning;

            public GlyphTex(XNATexture texture, int x, int y, int width, int height) : base(texture, x, y, (height <= 0) ? 0 : width, height)
            {

            }

            internal void Draw(Color color, bool newDraw, int x, int y)
            {
                DrawQuad(color, x + _xOffset, y + _yOffset, _textureWidth, _textureHeight);
            }

            internal int GetKerning(char ch)
            {
                if (this._kerning != null)
                {
                    byte[] page = this._kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];
                    if (page != null)
                    {
                        return page[ch & (PAGE_SIZE - 1)];
                    }
                }
                return 0;
            }

            internal void SetKerning(int ch, int value)
            {
                if (this._kerning == null)
                {
                    this._kerning = new byte[PAGES][];
                }
                byte[] page = _kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];
                if (page == null)
                {
                    this._kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)] = page = new byte[PAGE_SIZE];
                }
                page[ch & (PAGE_SIZE - 1)] = (byte)value;
            }
        }

        internal class Glyph// : TextureAreaBase
        {
            internal short _xOffset;
            internal short _yOffset;
            internal short _xAdvance;
            private int _width;
            private int _height;

            public Microsoft.Xna.Framework.Color[] ColorData;

            public Glyph(Microsoft.Xna.Framework.Color[] colorData, int width, int height)//XNATexture texture, int x, int y, int width, int height) : base(texture, x, y, (height <= 0) ? 0 : width, height)
            {
                this.ColorData = colorData;
                this._width = width;
                this._height = height;
            }

            public int getWidth()
            {
                return this._width;
            }

            public int getHeight()
            {
                return this._height;
            }

            public short getXOffset()
            {
                return this._xOffset;
            }

            public short getYOffset()
            {
                return this._yOffset;
            }
        }

        private static int LOG2_PAGE_SIZE = 9;
        private static int PAGE_SIZE = 1 << LOG2_PAGE_SIZE;
        private static int PAGES = 0x10000 / PAGE_SIZE;

        protected internal XNATexture _texture;
        private Glyph[][] _glyphs;
        private GlyphTex[][] _glyphsTex;
        private int _lineHeight;
        private int _baseLine;
        private int _spaceWidth;
        private int _ex;
        private bool _proportional;

        public BitmapFont(XNARenderer renderer, XMLParser xmlp, FileSystemObject baseFso)
        {
            xmlp.require(XmlPullParser.START_TAG, null, "font");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.START_TAG, null, "info");
            xmlp.ignoreOtherAttributes();
            xmlp.nextTag();
            xmlp.require(XmlPullParser.END_TAG, null, "info");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.START_TAG, null, "common");
            _lineHeight = xmlp.parseIntFromAttribute("lineHeight");
            _baseLine = xmlp.parseIntFromAttribute("base");
            if (xmlp.parseIntFromAttribute("pages", 1) != 1)
            {
                throw new NotImplementedException("multi page fonts not supported");
            }
            if (xmlp.parseIntFromAttribute("packed", 0) != 0)
            {
                throw new NotImplementedException("packed fonts not supported");
            }
            xmlp.ignoreOtherAttributes();
            xmlp.nextTag();
            xmlp.require(XmlPullParser.END_TAG, null, "common");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.START_TAG, null, "pages");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.START_TAG, null, "page");
            int pageId = int.Parse(xmlp.getAttributeValue(null, "id"));
            if (pageId != 0)
            {
                throw new NotImplementedException("only page id 0 supported");
            }
            String textureName = xmlp.getAttributeValue(null, "file");
            this._texture = (XNATexture) renderer.LoadTexture(new FileSystemObject(baseFso, textureName), "", "");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.END_TAG, null, "page");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.END_TAG, null, "pages");
            xmlp.nextTag();
            xmlp.require(XmlPullParser.START_TAG, null, "chars");
            xmlp.ignoreOtherAttributes();
            xmlp.nextTag();

            int firstXAdvance = int.MinValue;
            bool prop = true;

            _glyphs = new Glyph[PAGES][];
            _glyphsTex = new GlyphTex[PAGES][];
            //Microsoft.Xna.Framework.Color[] textureColorData = new Microsoft.Xna.Framework.Color[this.texture.Width * this.texture.Height];
            //this.texture.Texture2D.GetData<Microsoft.Xna.Framework.Color>(textureColorData, 0, textureColorData.Length);
            SpriteBatch spriteBatch = this._texture.SpriteBatch;
            while (!xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, "char");
                int idx = xmlp.parseIntFromAttribute("id");
                int x = xmlp.parseIntFromAttribute("x");
                int y = xmlp.parseIntFromAttribute("y");
                int w = xmlp.parseIntFromAttribute("width");
                int h = xmlp.parseIntFromAttribute("height");
                if (xmlp.parseIntFromAttribute("page", 0) != 0)
                {
                    throw xmlp.error("Multiple pages not supported");
                }
                int chnl = xmlp.parseIntFromAttribute("chnl", 0);
                short xadvance = short.Parse(xmlp.getAttributeNotNull("xadvance"));
                if (w > 0 && h > 0)
                {
                    Microsoft.Xna.Framework.Color[] textureData = new Microsoft.Xna.Framework.Color[w * h];
                    this._texture.Texture2D.GetData<Microsoft.Xna.Framework.Color>(0, new Microsoft.Xna.Framework.Rectangle(x, y, w, h), textureData, 0, w * h);

                    for (int i = 0; i < textureData.Length; i++)
                    {
                        if (textureData[i] == Microsoft.Xna.Framework.Color.Black)
                        {
                            textureData[i] = Microsoft.Xna.Framework.Color.Transparent; 
                        }
                    }
                    Texture2D xnaGlyph = new Texture2D(this._texture.Renderer.GraphicsDevice, w, h);
                    xnaGlyph.SetData(textureData);
                    Glyph glyph = new Glyph(textureData, w, h);//new XNATexture(this.texture.Renderer, spriteBatch, w, h, xnaGlyph), 0, 0, w, h);
                    GlyphTex glyphTex = new GlyphTex(new XNATexture(this._texture.Renderer, spriteBatch, w, h, xnaGlyph), 0, 0, w, h);
                    glyphTex._xOffset = short.Parse(xmlp.getAttributeNotNull("xoffset"));
                    glyphTex._yOffset = short.Parse(xmlp.getAttributeNotNull("yoffset"));
                    glyphTex._xAdvance = xadvance;
                    AddGlyphTex(idx, glyphTex);
                    glyph._xOffset = short.Parse(xmlp.getAttributeNotNull("xoffset"));
                    glyph._yOffset = short.Parse(xmlp.getAttributeNotNull("yoffset"));
                    glyph._xAdvance = xadvance;
                    AddGlyph(idx, glyph);
                }
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine("Glyph skipped " + idx + " - " + w + "," + h);
                //}
                xmlp.nextTag();
                xmlp.require(XmlPullParser.END_TAG, null, "char");
                xmlp.nextTag();

                if (xadvance != firstXAdvance && xadvance > 0)
                {
                    if (firstXAdvance == Int32.MaxValue)
                    {
                        firstXAdvance = xadvance;
                    }
                    else
                    {
                        prop = false;
                    }
                }
            }

            xmlp.require(XmlPullParser.END_TAG, null, "chars");
            xmlp.nextTag();
            if (xmlp.isStartTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, "kernings");
                xmlp.ignoreOtherAttributes();
                xmlp.nextTag();
                while (!xmlp.isEndTag())
                {
                    xmlp.require(XmlPullParser.START_TAG, null, "kerning");
                    int first = xmlp.parseIntFromAttribute("first");
                    int second = xmlp.parseIntFromAttribute("second");
                    int amount = xmlp.parseIntFromAttribute("amount");
                    AddKerning(first, second, amount);
                    xmlp.nextTag();
                    xmlp.require(XmlPullParser.END_TAG, null, "kerning");
                    xmlp.nextTag();
                }
                xmlp.require(XmlPullParser.END_TAG, null, "kernings");
                xmlp.nextTag();
            }
            xmlp.require(XmlPullParser.END_TAG, null, "font");

            Glyph g = GetGlyph(' ');
            _spaceWidth = (g != null) ? g._xAdvance + g.getWidth() : 5;

            Glyph gx = GetGlyph('x');
            _ex = (gx != null) ? gx.getHeight() : 1;
            _proportional = prop;
        }

        /*
        public BitmapFont(XNARenderer renderer, Reader reader, URL baseUrl)
        {
            BufferedReader br = new BufferedReader(reader);
            Dictionary<String, String> parameters = new HashMap<String, String>();
            parseFntLine(br, "info");
            parseFntLine(parseFntLine(br, "common"), parameters);
            lineHeight = parseInt(parameters, "lineHeight");
            baseLine = parseInt(parameters, "base");
            if (parseInt(parameters, "pages", 1) != 1)
            {
                throw new UnsupportedOperationException("multi page fonts not supported");
            }
            if (parseInt(parameters, "packed", 0) != 0)
            {
                throw new UnsupportedOperationException("packed fonts not supported");
            }
            parseFntLine(parseFntLine(br, "page"), parameters);
            if (parseInt(parameters, "id", 0) != 0)
            {
                throw new UnsupportedOperationException("only page id 0 supported");
            }
            this.texture = renderer.load(new URL(baseUrl, getParam(parameters, "file")),
                    LWJGLTexture.Format.ALPHA, LWJGLTexture.Filter.NEAREST);
            this.glyphs = new Glyph[PAGES][];
            parseFntLine(parseFntLine(br, "chars"), parameters);
            int charCount = parseInt(parameters, "count");
            int firstXAdvance = Int32.MinValue;
            boolean prop = true;
            for (int charIdx = 0; charIdx < charCount; charIdx++)
            {
                parseFntLine(parseFntLine(br, "char"), parameters);
                int idx = parseInt(parameters, "id");
                int x = parseInt(parameters, "x");
                int y = parseInt(parameters, "y");
                int w = parseInt(parameters, "width");
                int h = parseInt(parameters, "height");
                if (parseInt(parameters, "page", 0) != 0)
                {
                    throw new IOException("Multiple pages not supported");
                }
                Glyph g = new Glyph(x, y, w, h, texture.Width, texture.Height);
                g.xoffset = parseShort(parameters, "xoffset");
                g.yoffset = parseShort(parameters, "yoffset");
                g.xadvance = parseShort(parameters, "xadvance");
                addGlyph(idx, g);

                if (g.xadvance != firstXAdvance && g.xadvance > 0)
                {
                    if (firstXAdvance == Int32.MinValue)
                    {
                        firstXAdvance = g.xadvance;
                    }
                    else
                    {
                        prop = false;
                    }
                }
            }
            parseFntLine(parseFntLine(br, "kernings"), parameters);
            int kerningCount = parseInt(parameters, "count");
            for (int kerningIdx = 0; kerningIdx < kerningCount; kerningIdx++)
            {
                parseFntLine(parseFntLine(br, "kerning"), parameters);
                int first = parseInt(parameters, "first");
                int second = parseInt(parameters, "second");
                int amount = parseInt(parameters, "amount");
                addKerning(first, second, amount);
            }

            Glyph g = getGlyph(' ');
            spaceWidth = (g != null) ? g.xadvance + g.width : 1;

            Glyph gx = getGlyph('x');
            ex = (gx != null) ? gx.height : 1;

            this.proportional = prop;
        }*/

        public static BitmapFont LoadFont(XNARenderer renderer, FileSystemObject fso)
        {
            XMLParser xmlp = new XMLParser(fso);
            try
            {
                xmlp.require(XmlPullParser.XML_DECLARATION, null, null);
                xmlp.next();
                int tag = xmlp.nextTag();
                System.Diagnostics.Debug.WriteLine("LoadFont: " + tag);
                return new BitmapFont(renderer, xmlp, fso.Parent);
            }
            finally
            {
                xmlp.close();
            }
        }

        public bool IsProportional()
        {
            return _proportional;
        }

        public int GetBaseLine()
        {
            return _baseLine;
        }

        public int GetLineHeight()
        {
            return _lineHeight;
        }

        public int GetSpaceWidth()
        {
            return _spaceWidth;
        }

        public int GetEM()
        {
            return _lineHeight;
        }

        public int GetEX()
        {
            return _ex;
        }

        public void Destroy()
        {
            _texture.Dispose();
        }

        private void AddGlyphTex(int idx, GlyphTex g)
        {
            if (idx <= Char.MaxValue)
            {
                GlyphTex[] page = this._glyphsTex[idx >> LOG2_PAGE_SIZE];
                if (page == null)
                {
                    this._glyphsTex[idx >> LOG2_PAGE_SIZE] = page = new GlyphTex[PAGE_SIZE];
                }
                page[idx & (PAGE_SIZE - 1)] = g;
            }
        }

        private void AddGlyph(int idx, Glyph g)
        {
            if (idx <= Char.MaxValue)
            {
                Glyph[] page = this._glyphs[idx >> LOG2_PAGE_SIZE];
                if (page == null)
                {
                    this._glyphs[idx >> LOG2_PAGE_SIZE] = page = new Glyph[PAGE_SIZE];
                }
                page[idx & (PAGE_SIZE - 1)] = g;
            }
        }

        private void AddKerning(int first, int second, int amount)
        {
            if (first >= 0 && first <= Char.MaxValue &&
                    second >= 0 && second <= Char.MaxValue)
            {
                Glyph g = this.GetGlyph((char)first);
                if (g != null)
                {
                    //g.setKerning(second, amount);
                }
            }
        }

        internal GlyphTex GetGlyphTex(char ch)
        {
            GlyphTex[] page = this._glyphsTex[ch >> LOG2_PAGE_SIZE];
            if (page != null)
            {
                int idx = ch & (PAGE_SIZE - 1);
                return page[idx];
            }
            return null;
        }

        internal Glyph GetGlyph(char ch)
        {
            Glyph[] page = this._glyphs[ch >> LOG2_PAGE_SIZE];
            if (page != null)
            {
                int idx = ch & (PAGE_SIZE - 1);
                return page[idx];
            }
            return null;
        }

        public int ComputeTextWidth(string str, int start, int end)
        {
            int width = 0;
            Glyph lastGlyph = null;

            while (start < end)
            {
                char ch = str[start++];
                Glyph g = this.GetGlyph(ch);
                if (g != null)
                {
                    width += g._xAdvance;
                }
                else if (ch == ' ')
                {
                    width += this._spaceWidth;
                }
            }
            return width;
        }

        public int ComputeVisibleGlpyhs(string str, int start, int end, int availWidth)
        {
            int index = start;
            int width = 0;

            for (; index < end; index++)
            {
                char ch = str[index];
                Glyph g = this.GetGlyph(ch);
                if (g != null)
                {
                    if (_proportional)
                    {
                        width += g._xAdvance;
                        if (width > availWidth)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (width + g.getWidth() + g._xOffset > availWidth)
                        {
                            break;
                        }
                        width += g._xAdvance;
                    }
                }
                else if (ch == ' ')
                {
                    width += this._spaceWidth;
                }
            }

            return index - start;
        }

        public struct TexOutput
        {
            public int Width;
            public Microsoft.Xna.Framework.Color[] LineColors;
            public TexOutput(int width, Microsoft.Xna.Framework.Color[] lineColors)
            {
                this.Width = width;
                this.LineColors = lineColors;
            }
        }

        public int DrawText(Color color, int x, int y, string str, int start, int end)
        {
            int startX = x;

            while (start < end)
            {
                char ch = str[start++];
                GlyphTex g = this.GetGlyphTex(ch);
                if (g != null)
                {
                    if (g.getWidth() > 0)
                    {
                        g.Draw(color, false, x, y);
                    }

                    x += g._xAdvance; // + g.getKerning(ch);
                }
                else if (ch == ' ')
                {
                    x += this._spaceWidth;
                }
            }

            return x - startX;
        }

        public TexOutput CacheBDrawText(Color color, int x, int y, string str, int start, int end)
        {
            int height = this.GetLineHeight();

            Point[] positions = new Point[(end - start)];
            int tx = x;
            int theight = this._lineHeight;
            for (int c = start; c < end; c++)
            {
                char ch = str[c];
                Glyph g = this.GetGlyph(ch);
                positions[c] = new Point(g == null ? tx : (tx + g.getXOffset()), g == null ? y : (y + g.getYOffset()));
                tx += g == null ? ((ch == ' ' || ch == '\n') ? this.GetSpaceWidth() : 0) : g._xAdvance;
                if (g != null)
                    theight = Math.Max(theight, g.getHeight() + g.getYOffset());
            }
            theight += 1;
            int width = (tx - x);
            Microsoft.Xna.Framework.Color[] lineColors = new Microsoft.Xna.Framework.Color[width * theight];

            for (int c = start; c < end; c++)
            {
                Glyph g = this.GetGlyph(str[c]);
                if (g == null)
                {
                    continue;
                }
                for (int j = 0; j < g.getHeight(); j++)
                {
                    for (int i = 0; i < g.getWidth(); i++)
                    {
                        var srcOfs = i + j * g.getWidth();
                        var destOfs = (positions[c].X + i) + (positions[c].Y + j) * width;
                        lineColors[destOfs] = g.ColorData[srcOfs];
                    }
                }
            }

            TexOutput output = new TexOutput();
            output.Width = width;
            output.LineColors = lineColors;
            return output;
        }

        struct GlyphPoint
        {
            public Point Point;
            public Glyph Glyph;

            public GlyphPoint(Point point, Glyph glyph)
            {
                this.Point = point;
                this.Glyph = glyph;
            }
        }

        public struct TexMultiLineOutput
        {
            public int NumLines;
            public int Width;
            public int Height;
            public Microsoft.Xna.Framework.Color[] LineColors;
        }

        public TexMultiLineOutput CacheBDrawMultiLineText(Color color, int x, int y, string str, int start, int end, int lineWidth)
        {
            List<string> linesToRender = new List<string>();
            string strWithinBounds = str.Substring(start, end - start);
            string line = "";
            int glyphPointsCount = 0;
            while (strWithinBounds.Length > 0)
            {
                if (strWithinBounds[0] == '\r') // we do not support carriage returns
                {
                    strWithinBounds = strWithinBounds.Substring(1);
                    continue;
                }
                bool isNewLine = strWithinBounds[0] == '\n';
                if(isNewLine)
                {
                    linesToRender.Add(line);
                    strWithinBounds = strWithinBounds.Substring(1);
                    line = "";
                    continue;
                }
                int newLineWidth = this.ComputeTextWidth(line + strWithinBounds[0], 0, line.Length + 1);
                if (newLineWidth > lineWidth)
                {
                    linesToRender.Add(line);
                    if (isNewLine)
                    {
                        strWithinBounds = strWithinBounds.Substring(1);
                    }
                    line = "";
                    continue;
                }
                else
                {
                    line += strWithinBounds[0];
                    glyphPointsCount++;
                    strWithinBounds = strWithinBounds.Substring(1);
                    continue;
                }
            }

            if (line.Length != 0)
            {
                linesToRender.Add(line);
                line = "";
            }

            GlyphPoint[] glyphPoints = new GlyphPoint[glyphPointsCount];
            int currentPoint = 0;
            int currentY = 0;
            int longestLine = 0;
            foreach (string lineToPlot in linesToRender)
            {
                int width = this.ComputeTextWidth(lineToPlot, 0, lineToPlot.Length);
                longestLine = Math.Max(width, longestLine);
                int height = this.GetLineHeight();

                int tx = 0;
                int theight = this._lineHeight;
                for (int c = 0; c < lineToPlot.Length; c++)
                {
                    Glyph g = this.GetGlyph(lineToPlot[c]);
                    Point point = new Point(g == null ? tx : (tx + g.getXOffset()), g == null ? currentY : (currentY + g.getYOffset()));
                    glyphPoints[currentPoint] = new GlyphPoint(point, g);
                    tx += g == null ? this.GetSpaceWidth() : g._xAdvance;
                    if (g != null)
                    {
                        theight = Math.Max(theight, g.getHeight() + g.getYOffset());
                    }
                    currentPoint++;
                }
                currentY += theight;
            }
            currentY += 1;

            Microsoft.Xna.Framework.Color[] lineColors = new Microsoft.Xna.Framework.Color[currentY * longestLine];

            for (int a = 0; a < glyphPoints.Length; a++)
            {
                GlyphPoint g = glyphPoints[a];
                if (g.Glyph == null)
                {
                    continue;
                }

                for (int j = 0; j < g.Glyph.getHeight(); j++)
                {
                    for (int i = 0; i < g.Glyph.getWidth(); i++)
                    {
                        var srcOfs = i + j * g.Glyph.getWidth();
                        var destOfs = (g.Point.X + i) + (g.Point.Y + j) * longestLine;
                        lineColors[destOfs] = g.Glyph.ColorData[srcOfs];
                    }
                }
            }

            TexMultiLineOutput output = new TexMultiLineOutput();
            output.NumLines = linesToRender.Count;
            output.LineColors = lineColors;
            output.Width = longestLine;
            output.Height = currentY;
            return output;
        }

        public int DrawMultiLineText(Color color, int x, int y, string str, int width, HAlignment align)
        {
            int start = 0;
            int numLines = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.indexOf(str, '\n', start);
                int xoff = 0;
                if (align != HAlignment.LEFT)
                {
                    int lineWidth = this.ComputeTextWidth(str, start, lineEnd);
                    xoff = width - lineWidth;
                    if (align == HAlignment.CENTER)
                    {
                        xoff /= 2;
                    }
                }

                int theight = _lineHeight;
                {
                    int startX = x;

                    //this.texture.SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                    while (start < lineEnd)
                    {
                        char ch = str[start++];
                        GlyphTex g = this.GetGlyphTex(ch);
                        if (g != null)
                        {
                            //System.Diagnostics.Debug.WriteLine("x0:" + x);
                            //x += lastGlyph.getKerning(ch);
                            //System.Diagnostics.Debug.WriteLine("x1:" + x);
                            //lastGlyph = g;
                            if (g.getWidth() > 0)
                            {
                                g.Draw(color, false, x, y);
                            }
                            //System.Diagnostics.Debug.WriteLine("x2:" + x);
                            x += g._xAdvance; // + g.getKerning(ch);
                                             //System.Diagnostics.Debug.WriteLine("x3:" + x);
                        }
                        else if (ch == ' ')
                        {
                            x += this._spaceWidth;
                        }

                        //theight = Math.Max(theight, g.getHeight() + g.yoffset);
                    }
                    //this.texture.SpriteBatch.End();
                }
                start = lineEnd + 1;
                y += theight;
                numLines++;
            }
            return numLines;
        }

        public void ComputeMultiLineInfo(string str, int width, HAlignment align, int[] multiLineInfo)
        {
            int start = 0;
            int idx = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.indexOf(str, '\n', start);
                int lineWidth = this.ComputeTextWidth(str, start, lineEnd);
                int xoff = width - lineWidth;

                if (align == HAlignment.LEFT)
                {
                    xoff = 0;
                }
                else if (align == HAlignment.CENTER)
                {
                    xoff /= 2;
                }

                multiLineInfo[idx++] = (lineWidth << 16) | (xoff & 0xFFFF);
                start = lineEnd + 1;
            }
        }

        public int ComputeMultiLineTextWidth(string str)
        {
            int start = 0;
            int width = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.indexOf(str, '\n', start);
                int lineWidth = ComputeTextWidth(str, start, lineEnd);
                width = Math.Max(width, lineWidth);
                start = lineEnd + 1;
            }
            return width;
        }

        private static String ParseFontLine(TextReader br, String tag)
        {
            String line = br.ReadLine();
            if (line == null || line.Length <= tag.Length ||
                    line[tag.Length] != ' ' || !line.StartsWith(tag))
            {
                throw new IOException("'" + tag + "' line expected");
            }
            return line;
        }

        private static void ParseFontLine(String line, Dictionary<String, String> parameters)
        {
            parameters.Clear();
            ParameterStringParser psp = new ParameterStringParser(line, ' ', '=');
            while (psp.next())
            {
                parameters.Add(psp.getKey(), psp.getValue());
            }
        }

        private static String GetParam(Dictionary<String, String> parameters, String key)
        {
            String value = parameters[key];
            if (value == null)
            {
                throw new IOException("Required parameter '" + key + "' not found");
            }

            return value;
        }

        private static int ParseInt(Dictionary<String, String> parameters, String key)
        {
            String value = BitmapFont.GetParam(parameters, key);
            try
            {
                return int.Parse(value);
            }
            catch (FormatException ex)
            {
                throw new IOException("Can't parse parameter: " + key + '=' + value, ex);
            }
        }

        private static int ParseInt(Dictionary<String, String> parameters, String key, int defaultValue)
        {
            String value = parameters[key];
            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                return Int32.Parse(value);
            }
            catch (FormatException ex)
            {
                throw new IOException("Can't parse parameter: " + key + '=' + value, ex);
            }
        }

        private static short parseShort(Dictionary<String, String> parameters, String key)
        {
            String value = GetParam(parameters, key);
            try
            {
                return short.Parse(value);
            }
            catch (FormatException ex)
            {
                throw new IOException("Can't parse parameter: " + key + '=' + value, ex);
            }
        }
    }
}
