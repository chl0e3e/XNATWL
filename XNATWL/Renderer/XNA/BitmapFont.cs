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
    public class BitmapFont : IDisposable
    {
        /// <summary>
        /// A font glyph drawn using normal texture rendering routines
        /// </summary>
        protected class Glyph : TextureAreaBase
        {
            private short _xOffset;
            private short _yOffset;
            private short _xAdvance;
            private byte[][] _kerning;
            private Microsoft.Xna.Framework.Color[] _colorData;

            public Glyph(Microsoft.Xna.Framework.Color[] colorData, XNATexture texture, int x, int y, int width, int height, short xOffset, short yOffset, short xAdvance) : base(texture, x, y, (height <= 0) ? 0 : width, height)
            {
                this._xOffset = xOffset;
                this._xAdvance = xAdvance;
                this._yOffset = yOffset;
                this._colorData = colorData;
            }

            /// <summary>
            /// Draw the glyph at the given location
            /// </summary>
            /// <param name="color">Font color</param>
            /// <param name="x">X coordinate</param>
            /// <param name="y">Y coordinate</param>
            public void Draw(Color color, int x, int y)
            {
                DrawQuad(color, x + _xOffset, y + _yOffset, _textureWidth, _textureHeight);
            }

            public short XOffset
            {
                get
                {
                    return _xOffset;
                }
            }

            public short YOffset
            {
                get
                {
                    return _yOffset;
                }
            }

            public short XAdvance
            {
                get
                {
                    return _xAdvance;
                }
            }

            public Microsoft.Xna.Framework.Color[] ColorData
            {
                get
                {
                    return this._colorData;
                }
            }

            public int GetKerning(char ch)
            {
                if (_kerning != null)
                {
                    byte[] page = this._kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];

                    if (page != null)
                    {
                        return page[ch & (PAGE_SIZE - 1)];
                    }
                }
                return 0;
            }

            public void SetKerning(int ch, int value)
            {
                if (_kerning == null)
                {
                    _kerning = new byte[PAGES][];
                }
                int kerningIndex = BitOperations.RightMove(ch, LOG2_PAGE_SIZE);
                byte[] page = _kerning[kerningIndex];
                if (page == null)
                {
                    _kerning[kerningIndex] = page = new byte[PAGE_SIZE];
                }
                page[ch & (PAGE_SIZE - 1)] = (byte)value;
            }
        }

        private static int LOG2_PAGE_SIZE = 9;
        private static int PAGE_SIZE = 1 << LOG2_PAGE_SIZE;
        private static int PAGES = 0x10000 / PAGE_SIZE;

        protected internal XNATexture _texture;
        private Glyph[][] _glyphs;
        private int _lineHeight;
        private int _baseLine;
        private int _spaceWidth;
        private int _ex;
        private bool _proportional;

        /// <summary>
        /// Parse the BitmapFont from a given XMLParser
        /// </summary>
        /// <param name="renderer">XNA renderer the font belongs to</param>
        /// <param name="xmlp">XML parser streaming the font's XML</param>
        /// <param name="baseFso">Relative directory to find included assets</param>
        /// <exception cref="NotImplementedException">XML references unsupported features</exception>
        public BitmapFont(XNARenderer renderer, XMLParser xmlp, FileSystemObject baseFso)
        {
            xmlp.Require(XmlPullParser.START_TAG, null, "font");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.START_TAG, null, "info");
            xmlp.IgnoreOtherAttributes();
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.END_TAG, null, "info");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.START_TAG, null, "common");
            _lineHeight = xmlp.ParseIntFromAttribute("lineHeight");
            _baseLine = xmlp.ParseIntFromAttribute("base");
            if (xmlp.ParseIntFromAttribute("pages", 1) != 1)
            {
                throw new NotImplementedException("multi page fonts not supported");
            }
            if (xmlp.ParseIntFromAttribute("packed", 0) != 0)
            {
                throw new NotImplementedException("packed fonts not supported");
            }
            xmlp.IgnoreOtherAttributes();
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.END_TAG, null, "common");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.START_TAG, null, "pages");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.START_TAG, null, "page");
            int pageId = int.Parse(xmlp.GetAttributeValue(null, "id"));
            if (pageId != 0)
            {
                throw new NotImplementedException("only page id 0 supported");
            }
            String textureName = xmlp.GetAttributeValue(null, "file");
            this._texture = (XNATexture) renderer.LoadTexture(new FileSystemObject(baseFso, textureName), "", "");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.END_TAG, null, "page");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.END_TAG, null, "pages");
            xmlp.NextTag();
            xmlp.Require(XmlPullParser.START_TAG, null, "chars");
            xmlp.IgnoreOtherAttributes();
            xmlp.NextTag();

            int firstXAdvance = int.MinValue;
            bool prop = true;

            _glyphs = new Glyph[PAGES][];
            //Microsoft.Xna.Framework.Color[] textureColorData = new Microsoft.Xna.Framework.Color[this.texture.Width * this.texture.Height];
            //this.texture.Texture2D.GetData<Microsoft.Xna.Framework.Color>(textureColorData, 0, textureColorData.Length);
            SpriteBatch spriteBatch = this._texture.SpriteBatch;
            while (!xmlp.IsEndTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, "char");
                int idx = xmlp.ParseIntFromAttribute("id");
                int x = xmlp.ParseIntFromAttribute("x");
                int y = xmlp.ParseIntFromAttribute("y");
                int w = xmlp.ParseIntFromAttribute("width");
                int h = xmlp.ParseIntFromAttribute("height");
                if (xmlp.ParseIntFromAttribute("page", 0) != 0)
                {
                    throw xmlp.Error("Multiple pages not supported");
                }
                int chnl = xmlp.ParseIntFromAttribute("chnl", 0);
                short xadvance = short.Parse(xmlp.GetAttributeNotNull("xadvance"));
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

                    short xOffset = short.Parse(xmlp.GetAttributeNotNull("xoffset"));
                    short yOffset = short.Parse(xmlp.GetAttributeNotNull("yoffset"));
                    short xAdvance = xadvance;

                    Glyph glyphTex = new Glyph(textureData, new XNATexture(this._texture.Renderer, spriteBatch, w, h, xnaGlyph), 0, 0, w, h, xOffset, yOffset, xAdvance);
                    AddGlyph_XNATexture(idx, glyphTex);
                }
                //else 
                //{
                //    System.Diagnostics.Debug.WriteLine("Glyph skipped " + idx + " - " + w + "," + h);
                //}
                xmlp.NextTag();
                xmlp.Require(XmlPullParser.END_TAG, null, "char");
                xmlp.NextTag();

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

            xmlp.Require(XmlPullParser.END_TAG, null, "chars");
            xmlp.NextTag();
            if (xmlp.IsStartTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, "kernings");
                xmlp.IgnoreOtherAttributes();
                xmlp.NextTag();
                while (!xmlp.IsEndTag())
                {
                    xmlp.Require(XmlPullParser.START_TAG, null, "kerning");
                    int first = xmlp.ParseIntFromAttribute("first");
                    int second = xmlp.ParseIntFromAttribute("second");
                    int amount = xmlp.ParseIntFromAttribute("amount");
                    AddKerning(first, second, amount);
                    xmlp.NextTag();
                    xmlp.Require(XmlPullParser.END_TAG, null, "kerning");
                    xmlp.NextTag();
                }
                xmlp.Require(XmlPullParser.END_TAG, null, "kernings");
                xmlp.NextTag();
            }
            xmlp.Require(XmlPullParser.END_TAG, null, "font");

            Glyph g = GetGlyph(' ');
            _spaceWidth = (g != null) ? g.XAdvance + g.Width : 5;

            Glyph gx = GetGlyph('x');
            _ex = (gx != null) ? gx.Height : 1;
            _proportional = prop;
        }

        /// <summary>
        /// Load a BitmapFont as XML from a file system object
        /// </summary>
        /// <param name="renderer">The responsible XNA renderer</param>
        /// <param name="fso">a FileSystemObject pointing to the XML file</param>
        /// <returns>A new BitmapFont if successful</returns>
        public static BitmapFont LoadFont(XNARenderer renderer, FileSystemObject fso)
        {
            XMLParser xmlp = new XMLParser(fso);
            try
            {
                xmlp.Require(XmlPullParser.XML_DECLARATION, null, null);
                xmlp.Next();
                int tag = xmlp.NextTag();
                System.Diagnostics.Debug.WriteLine("LoadFont: " + tag);
                return new BitmapFont(renderer, xmlp, fso.Parent);
            }
            finally
            {
                xmlp.Close();
            }
        }

        /// <summary>
        /// <strong>true</strong> if the font is proportional or <strong>false</strong> if it's fixed width.
        /// </summary>
        public bool Proportional
        {
            get
            {
                return _proportional;
            }
        }

        /// <summary>
        /// The base line of the font measured in pixels from the top of the text bounding box
        /// </summary>
        public int BaseLine
        {
            get
            {
                return _baseLine;
            }
        }

        /// <summary>
        /// The line height in pixels for this font
        /// </summary>
        public int LineHeight
        {
            get
            {
                return _lineHeight;
            }
        }

        /// <summary>
        /// The width of a ' '
        /// </summary>
        public int SpaceWidth
        {
            get
            {
                return _spaceWidth;
            }
        }

        /// <summary>
        /// The width of a ' '
        /// </summary>
        public int EM
        {
            get
            {
                return _lineHeight;
            }
        }

        /// <summary>
        /// The width of a 'x'
        /// </summary>
        public int EX
        {
            get
            {
                return _ex;
            }
        }

        /// <summary>
        /// Dispose the <see cref="BitmapFont"/> object and other related objects
        /// </summary>
        public void Dispose()
        {
            _texture.Dispose();
        }

        /// <summary>
        /// Add a glyph using an <see cref="XNATexture"/> sprite to the _spriteGlyphs array
        /// </summary>
        /// <param name="idx">index of glyph</param>
        /// <param name="g"><see cref="Glyph"/> instance</param>
        private void AddGlyph_XNATexture(int idx, Glyph g)
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

        /// <summary>
        /// Set a kerning for a given glyph
        /// </summary>
        /// <param name="first">First character in line</param>
        /// <param name="second">Second character in line</param>
        /// <param name="amount">Kerning in pixels</param>
        private void AddKerning(int first, int second, int amount)
        {
            if (first >= 0 && first <= Char.MaxValue &&
                    second >= 0 && second <= Char.MaxValue)
            {
                Glyph g = this.GetGlyph((char)first);
                if (g != null)
                {
                    g.SetKerning(second, amount);
                }
            }
        }

        /// <summary>
        /// Get a Glyph object representing texture data for a given character
        /// </summary>
        /// <param name="ch">the given character</param>
        /// <returns>Glyph object representing texture data</returns>
        protected Glyph GetGlyph(char ch)
        {
            Glyph[] page = this._glyphs[ch >> LOG2_PAGE_SIZE];
            if (page != null)
            {
                int idx = ch & (PAGE_SIZE - 1);
                return page[idx];
            }
            return null;
        }

        /// <summary>
        /// Calculate the width in pixels of a given string/substring
        /// </summary>
        /// <param name="str">The string to measure</param>
        /// <param name="start">Substring start</param>
        /// <param name="end">Substring end</param>
        /// <returns>text width of given string/substring</returns>
        public int ComputeTextWidth(string str, int start, int end)
        {
            int width = 0;

            while (start < end)
            {
                char ch = str[start++];
                Glyph g = this.GetGlyph(ch);
                if (g != null)
                {
                    width += g.XAdvance;
                }
                else if (ch == ' ')
                {
                    width += this._spaceWidth;
                }
            }
            return width;
        }

        /// <summary>
        /// Calculate the number of visible glyphs in pixels of a given string/substring. This width has a maximum specified by <paramref name="availWidth"/>.
        /// </summary>
        /// <param name="str">The string to measure</param>
        /// <param name="start">Substring start</param>
        /// <param name="end">Substring end</param>
        /// <param name="availWidth">Maximum width to return</param>
        /// <returns>number of visible glyphs</returns>
        public int ComputeVisibleGlyphs(string str, int start, int end, int availWidth)
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
                        width += g.XAdvance;
                        if (width > availWidth)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (width + g.Width + g.XOffset > availWidth)
                        {
                            break;
                        }
                        width += g.XAdvance;
                    }
                }
                else if (ch == ' ')
                {
                    width += this._spaceWidth;
                }
            }

            return index - start;
        }

        /// <summary>
        /// A texel cache for drawing a single line of text
        /// </summary>
        public struct SingleLineTexelCache
        {
            /// <summary>
            /// Width of the line of text
            /// </summary>
            public int Width;
            /// <summary>
            /// Texel array of XNA Colors
            /// </summary>
            public Microsoft.Xna.Framework.Color[] LineColors;
            public SingleLineTexelCache(int width, Microsoft.Xna.Framework.Color[] lineColors)
            {
                this.Width = width;
                this.LineColors = lineColors;
            }
        }

        /// <summary>
        /// Traditional text rendering used for drawing a single line of text
        /// </summary>
        /// <param name="color">Font colour</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="str">Text to draw</param>
        /// <param name="start">Beginning of text substring</param>
        /// <param name="end">End of text substring</param>
        /// <returns>Width of line drawn</returns>
        public int DrawText(Color color, int x, int y, string str, int start, int end)
        {
            int startX = x;

            while (start < end)
            {
                char ch = str[start++];
                Glyph g = this.GetGlyph(ch);
                if (g != null)
                {
                    if (g.Width > 0)
                    {
                        g.Draw(color, x, y);
                    }

                    x += g.XAdvance; // + g.getKerning(ch);
                }
                else if (ch == ' ')
                {
                    x += this._spaceWidth;
                }
            }

            return x - startX;
        }

        /// <summary>
        /// Compositive text rendering used for caching a texture representing a single line of text
        /// </summary>
        /// <param name="color">Font colour</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="str">Text to draw</param>
        /// <param name="start">Beginning of text substring</param>
        /// <param name="end">End of text substring</param>
        /// <returns>cache struct of type <see cref="SingleLineTexelCache"/></returns>
        public SingleLineTexelCache CacheBDrawText(Color color, int x, int y, string str, int start, int end)
        {
            int height = this.LineHeight;

            Point[] positions = new Point[(end - start)];
            int tx = x;
            int theight = this._lineHeight;
            for (int c = start; c < end; c++)
            {
                char ch = str[c];
                Glyph g = this.GetGlyph(ch);
                positions[c] = new Point(g == null ? tx : (tx + g.XOffset), g == null ? y : (y + g.YOffset));
                //tx += g == null ? ((ch == ' ' || ch == '\n') ? this.SpaceWidth : 0) : g.XAdvance;
                if (g == null)
                {
                    if (ch == ' ')
                    {
                        tx += this.SpaceWidth;
                    }
                    else if (ch == '\n')
                    {
                        tx += 1;
                    }
                }
                else
                {
                    tx += g.XAdvance;
                }
                if (g != null)
                    theight = Math.Max(theight, g.Height + g.YOffset);
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
                for (int j = 0; j < g.Height; j++)
                {
                    for (int i = 0; i < g.Width; i++)
                    {
                        var srcOfs = i + j * g.Width;
                        var destOfs = (positions[c].X + i) + (positions[c].Y + j) * width;
                        lineColors[destOfs] = g.ColorData[srcOfs];
                    }
                }
            }

            SingleLineTexelCache output = new SingleLineTexelCache();
            output.Width = width;
            output.LineColors = lineColors;
            return output;
        }

        /// <summary>
        /// A glyph alongside a point to draw it in two-dimensional space
        /// </summary>
        struct GlyphPoint
        {
            /// <summary>
            /// Location on the screen
            /// </summary>
            public Point Point;
            /// <summary>
            /// Character to draw at given location
            /// </summary>
            public Glyph Glyph;

            /// <summary>
            /// Create a structure attributing a 2D point to a character on screen
            /// </summary>
            /// <param name="point">Location on the screen</param>
            /// <param name="glyph">Character to draw at given location</param>
            public GlyphPoint(Point point, Glyph glyph)
            {
                this.Point = point;
                this.Glyph = glyph;
            }
        }

        /// <summary>
        /// A cache storing the output of an attempt at rendering multiple lines of text
        /// </summary>
        public struct MultiLineTexelCache
        {
            /// <summary>
            /// Number of lines describing the cache
            /// </summary>
            public int NumLines;
            /// <summary>
            /// Pixel width of the cache
            /// </summary>
            public int Width;
            /// <summary>
            /// Pixel height of the cache
            /// </summary>
            public int Height;
            /// <summary>
            /// Colors representing the amalgamated texture
            /// </summary>
            public Microsoft.Xna.Framework.Color[] LineColors;
        }

        /// <summary>
        /// Compositive text rendering used for caching a texture representing a multiple lines of text fixed at a given <paramref name="lineWidth"/>
        /// </summary>
        /// <param name="color">Font colour</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="str">Text to draw</param>
        /// <param name="start">Beginning of text substring</param>
        /// <param name="end">End of text substring</param>
        /// <param name="lineWidth">Maximum line width</param>
        /// <returns>cache struct of type <see cref="MultiLineTexelCache"/></returns>
        public MultiLineTexelCache CacheBDrawMultiLineText(Color color, int x, int y, string str, int start, int end, int lineWidth)
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
                int height = this.LineHeight;

                int tx = 0;
                int theight = this._lineHeight;
                for (int c = 0; c < lineToPlot.Length; c++)
                {
                    Glyph g = this.GetGlyph(lineToPlot[c]);
                    Point point = new Point(g == null ? tx : (tx + g.XOffset), g == null ? currentY : (currentY + g.YOffset));
                    glyphPoints[currentPoint] = new GlyphPoint(point, g);
                    tx += g == null ? this.SpaceWidth : g.XAdvance;
                    if (g != null)
                    {
                        theight = Math.Max(theight, g.Height + g.YOffset);
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

                for (int j = 0; j < g.Glyph.Height; j++)
                {
                    for (int i = 0; i < g.Glyph.Width; i++)
                    {
                        var srcOfs = i + j * g.Glyph.Width;
                        var destOfs = (g.Point.X + i) + (g.Point.Y + j) * longestLine;
                        lineColors[destOfs] = g.Glyph.ColorData[srcOfs];
                    }
                }
            }

            MultiLineTexelCache output = new MultiLineTexelCache();
            output.NumLines = linesToRender.Count;
            output.LineColors = lineColors;
            output.Width = longestLine;
            output.Height = currentY;
            return output;
        }

        /// <summary>
        /// Traditional text rendering method used for drawing a multiple lines of text
        /// </summary>
        /// <param name="color">Font colour</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="str">Text to draw</param>
        /// <param name="width">Beginning of text substring</param>
        /// <param name="align">Horizontal alignment</param>
        /// <returns>Number of lines drawn</returns>
        public int DrawMultiLineText(Color color, int x, int y, string str, int width, HAlignment align)
        {
            int start = 0;
            int numLines = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.IndexOf(str, '\n', start);
                int xoff = 0;
                if (align != HAlignment.Left)
                {
                    int lineWidth = this.ComputeTextWidth(str, start, lineEnd);
                    xoff = width - lineWidth;
                    if (align == HAlignment.Center)
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
                        Glyph g = GetGlyph(ch);
                        if (g != null)
                        {
                            //System.Diagnostics.Debug.WriteLine("x0:" + x);
                            //x += lastGlyph.getKerning(ch);
                            //System.Diagnostics.Debug.WriteLine("x1:" + x);
                            //lastGlyph = g;
                            if (g.Width > 0)
                            {
                                g.Draw(color, x, y);
                            }
                            //System.Diagnostics.Debug.WriteLine("x2:" + x);
                            x += g.XAdvance; // + g.getKerning(ch);
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

        /// <summary>
        /// Compute multi-line dimensions for given string
        /// </summary>
        /// <param name="str">Rendered string</param>
        /// <param name="width">Maximum width of string</param>
        /// <param name="align">Horizontal alignment</param>
        /// <param name="multiLineInfo">Output sizes array (height per line)</param>
        public void ComputeMultiLineInfo(string str, int width, HAlignment align, int[] multiLineInfo)
        {
            int start = 0;
            int idx = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.IndexOf(str, '\n', start);
                int lineWidth = this.ComputeTextWidth(str, start, lineEnd);
                int xoff = width - lineWidth;

                if (align == HAlignment.Left)
                {
                    xoff = 0;
                }
                else if (align == HAlignment.Center)
                {
                    xoff /= 2;
                }

                multiLineInfo[idx++] = (lineWidth << 16) | (xoff & 0xFFFF);
                start = lineEnd + 1;
            }
        }

        /// <summary>
        /// Compute the text width for a multi-line text render 
        /// </summary>
        /// <param name="str">String to measure</param>
        /// <returns>Longest line width</returns>
        public int ComputeMultiLineTextWidth(string str)
        {
            int start = 0;
            int width = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.IndexOf(str, '\n', start);
                int lineWidth = ComputeTextWidth(str, start, lineEnd);
                width = Math.Max(width, lineWidth);
                start = lineEnd + 1;
            }
            return width;
        }
    }
}
