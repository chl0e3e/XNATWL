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
            internal short xoffset;
            internal short yoffset;
            internal short xadvance;
            internal byte[][] kerning;

            public GlyphTex(XNATexture texture, int x, int y, int width, int height) : base(texture, x, y, (height <= 0) ? 0 : width, height)
            {

            }

            internal void draw(Color color, bool newDraw, int x, int y)
            {
                drawQuad(color, x + xoffset, y + yoffset, tw, th);
            }

            internal int getKerning(char ch)
            {
                if (kerning != null)
                {
                    byte[] page = kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];
                    if (page != null)
                    {
                        return page[ch & (PAGE_SIZE - 1)];
                    }
                }
                return 0;
            }

            internal void setKerning(int ch, int value)
            {
                if (kerning == null)
                {
                    kerning = new byte[PAGES][];
                }
                byte[] page = kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];
                if (page == null)
                {
                    kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)] = page = new byte[PAGE_SIZE];
                }
                page[ch & (PAGE_SIZE - 1)] = (byte)value;
            }
        }

        internal class Glyph// : TextureAreaBase
        {
            internal short xoffset;
            internal short yoffset;
            internal short xadvance;
            internal byte[][] kerning;
            public Microsoft.Xna.Framework.Color[] colorData;
            private int _width;
            private int _height;

            public Glyph(Microsoft.Xna.Framework.Color[] colorData, int width, int height)//XNATexture texture, int x, int y, int width, int height) : base(texture, x, y, (height <= 0) ? 0 : width, height)
            {
                this.colorData = colorData;
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
                return this.xoffset;
            }

            public short getYOffset()
            {
                return this.yoffset;
            }

            /*internal void draw(Color color, bool newDraw, int x, int y)
            {
                drawQuad(color, newDraw, x + xoffset, y + yoffset, tw, th);
            }*/

            /*internal int getKerning(char ch)
            {
                if (kerning != null)
                {
                    byte[] page = kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];
                    if (page != null)
                    {
                        return page[ch & (PAGE_SIZE - 1)];
                    }
                }
                return 0;
            }

            internal void setKerning(int ch, int value)
            {
                if (kerning == null)
                {
                    kerning = new byte[PAGES][];
                }
                byte[] page = kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)];
                if (page == null)
                {
                    kerning[BitOperations.RightMove(ch, LOG2_PAGE_SIZE)] = page = new byte[PAGE_SIZE];
                }
                page[ch & (PAGE_SIZE - 1)] = (byte)value;
            }*/
        }

        private static int LOG2_PAGE_SIZE = 9;
        private static int PAGE_SIZE = 1 << LOG2_PAGE_SIZE;
        private static int PAGES = 0x10000 / PAGE_SIZE;

        protected internal XNATexture texture;
        private Glyph[][] glyphs;
        private GlyphTex[][] glyphsTex;
        private int lineHeight;
        private int baseLine;
        private int spaceWidth;
        private int ex;
        private bool proportional;

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
            lineHeight = xmlp.parseIntFromAttribute("lineHeight");
            baseLine = xmlp.parseIntFromAttribute("base");
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
            this.texture = (XNATexture) renderer.LoadTexture(new FileSystemObject(baseFso, textureName), "", "");
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

            glyphs = new Glyph[PAGES][];
            glyphsTex = new GlyphTex[PAGES][];
            //Microsoft.Xna.Framework.Color[] textureColorData = new Microsoft.Xna.Framework.Color[this.texture.Width * this.texture.Height];
            //this.texture.Texture2D.GetData<Microsoft.Xna.Framework.Color>(textureColorData, 0, textureColorData.Length);
            SpriteBatch spriteBatch = this.texture.SpriteBatch;
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
                    this.texture.Texture2D.GetData<Microsoft.Xna.Framework.Color>(0, new Microsoft.Xna.Framework.Rectangle(x, y, w, h), textureData, 0, w * h);

                    for (int i = 0; i < textureData.Length; i++)
                    {
                        if (textureData[i] == Microsoft.Xna.Framework.Color.Black)
                        {
                            textureData[i] = Microsoft.Xna.Framework.Color.Transparent; 
                        }
                    }
                    Texture2D xnaGlyph = new Texture2D(this.texture.Renderer.GraphicsDevice, w, h);
                    xnaGlyph.SetData(textureData);
                    Glyph glyph = new Glyph(textureData, w, h);//new XNATexture(this.texture.Renderer, spriteBatch, w, h, xnaGlyph), 0, 0, w, h);
                    GlyphTex glyphTex = new GlyphTex(new XNATexture(this.texture.Renderer, spriteBatch, w, h, xnaGlyph), 0, 0, w, h);
                    glyphTex.xoffset = short.Parse(xmlp.getAttributeNotNull("xoffset"));
                    glyphTex.yoffset = short.Parse(xmlp.getAttributeNotNull("yoffset"));
                    glyphTex.xadvance = xadvance;
                    addGlyphTex(idx, glyphTex);
                    glyph.xoffset = short.Parse(xmlp.getAttributeNotNull("xoffset"));
                    glyph.yoffset = short.Parse(xmlp.getAttributeNotNull("yoffset"));
                    glyph.xadvance = xadvance;
                    addGlyph(idx, glyph);
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
                    addKerning(first, second, amount);
                    xmlp.nextTag();
                    xmlp.require(XmlPullParser.END_TAG, null, "kerning");
                    xmlp.nextTag();
                }
                xmlp.require(XmlPullParser.END_TAG, null, "kernings");
                xmlp.nextTag();
            }
            xmlp.require(XmlPullParser.END_TAG, null, "font");

            Glyph g = getGlyph(' ');
            spaceWidth = (g != null) ? g.xadvance + g.getWidth() : 5;

            Glyph gx = getGlyph('x');
            ex = (gx != null) ? gx.getHeight() : 1;
            proportional = prop;
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

        public static BitmapFont loadFont(XNARenderer renderer, FileSystemObject fso)
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

        public bool isProportional()
        {
            return proportional;
        }

        public int getBaseLine()
        {
            return baseLine;
        }

        public int getLineHeight()
        {
            return lineHeight;
        }

        public int getSpaceWidth()
        {
            return spaceWidth;
        }

        public int getEM()
        {
            return lineHeight;
        }

        public int getEX()
        {
            return ex;
        }

        public void destroy()
        {
            texture.Dispose();
        }

        private void addGlyphTex(int idx, GlyphTex g)
        {
            if (idx <= Char.MaxValue)
            {
                GlyphTex[] page = glyphsTex[idx >> LOG2_PAGE_SIZE];
                if (page == null)
                {
                    glyphsTex[idx >> LOG2_PAGE_SIZE] = page = new GlyphTex[PAGE_SIZE];
                }
                page[idx & (PAGE_SIZE - 1)] = g;
            }
        }

        private void addGlyph(int idx, Glyph g)
        {
            if (idx <= Char.MaxValue)
            {
                Glyph[] page = glyphs[idx >> LOG2_PAGE_SIZE];
                if (page == null)
                {
                    glyphs[idx >> LOG2_PAGE_SIZE] = page = new Glyph[PAGE_SIZE];
                }
                page[idx & (PAGE_SIZE - 1)] = g;
            }
        }

        private void addKerning(int first, int second, int amount)
        {
            if (first >= 0 && first <= Char.MaxValue &&
                    second >= 0 && second <= Char.MaxValue)
            {
                Glyph g = getGlyph((char)first);
                if (g != null)
                {
                    //g.setKerning(second, amount);
                }
            }
        }

        internal GlyphTex getGlyphTex(char ch)
        {
            GlyphTex[] page = glyphsTex[ch >> LOG2_PAGE_SIZE];
            if (page != null)
            {
                int idx = ch & (PAGE_SIZE - 1);
                return page[idx];
            }
            return null;
        }

        internal Glyph getGlyph(char ch)
        {
            Glyph[] page = glyphs[ch >> LOG2_PAGE_SIZE];
            if (page != null)
            {
                int idx = ch & (PAGE_SIZE - 1);
                return page[idx];
            }
            return null;
        }

        public int computeTextWidth(string str, int start, int end)
        {
            int width = 0;
            Glyph lastGlyph = null;
            /*while (start < end)
            {
                lastGlyph = getGlyph(str[start++]);
                if (lastGlyph != null)
                {
                    width = lastGlyph.xadvance;
                    break;
                }
            }*/
            while (start < end)
            {
                char ch = str[start++];
                Glyph g = getGlyph(ch);
                if (g != null)
                {
                    //width += lastGlyph.getKerning(ch);
                    //lastGlyph = g;
                    width += g.xadvance;
                }
                else if (ch == ' ')
                {
                    width += this.spaceWidth;
                }
            }
            return width;
        }

        public int computeVisibleGlpyhs(string str, int start, int end, int availWidth)
        {
            int index = start;
            int width = 0;
            //Glyph lastGlyph = null;
            for (; index < end; index++)
            {
                char ch = str[index];
                Glyph g = getGlyph(ch);
                if (g != null)
                {
                    /*if (lastGlyph != null)
                    {
                        width += lastGlyph.getKerning(ch);
                    }
                    lastGlyph = g;*/
                    if (proportional)
                    {
                        width += g.xadvance;
                        if (width > availWidth)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (width + g.getWidth() + g.xoffset > availWidth)
                        {
                            break;
                        }
                        width += g.xadvance;
                    }
                }
                else if (ch == ' ')
                {
                    width += this.spaceWidth;
                }
            }
            return index - start;
        }

        public struct TexOutput
        {
            public int width;
            public Microsoft.Xna.Framework.Color[] lineColors;
            public TexOutput(int width, Microsoft.Xna.Framework.Color[] lineColors)
            {
                this.width = width;
                this.lineColors = lineColors;
            }
        }

        public int drawText(Color color, int x, int y, string str, int start, int end)
        {
            int startX = x;

            GlyphTex lastGlyph = null;
            //this.texture.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            while (start < end)
            {
                char ch = str[start++];
                GlyphTex g = getGlyphTex(ch);
                if (g != null)
                {
                    //System.Diagnostics.Debug.WriteLine("x0:" + x);
                    //x += lastGlyph.getKerning(ch);
                    //System.Diagnostics.Debug.WriteLine("x1:" + x);
                    //lastGlyph = g;
                    if (g.getWidth() > 0)
                    {
                        g.draw(color, false, x, y);
                    }
                    //System.Diagnostics.Debug.WriteLine("x2:" + x);
                    x += g.xadvance; // + g.getKerning(ch);
                    //System.Diagnostics.Debug.WriteLine("x3:" + x);
                }
                else if (ch == ' ')
                {
                    x += this.spaceWidth;
                }
            }
            //this.texture.SpriteBatch.End();
            return x - startX;
        }
        /*
        public TexOutput cacheBDrawText(Color color, int x, int y, string str, int start, int end)
        {
            //var strLine = str.Replace("\n", " ");
            //TexMultiLineOutput a = cacheBDrawMultiLineText(color, x, y, strLine, start, end, 10000000);
            //return new TexOutput(a.width, a.lineColors);
            int width = computeTextWidth(str, start, end);
            int height = this.getLineHeight();

            //string strWithinBounds = str.Substring(start, end - start);
            Point[] positions = new Point[end - start];
            int tx = 0;
            int theight = this.lineHeight;
            for (int c = start; c < end; c++)
            {
                Glyph g = getGlyph(str[c]);
                positions[c] = new Point(g == null ? tx : (tx + g.getXOffset()), g == null ? y : (y + g.getYOffset()));
                tx += g == null ? this.getSpaceWidth() : g.xoffset;
                if (g != null)
                {
                    theight = Math.Max(theight, g.getHeight() + g.getYOffset());
                }
            }
            theight += 1;
            Microsoft.Xna.Framework.Color[] lineColors = new Microsoft.Xna.Framework.Color[width * theight];

            for (int c = start; c < end; c++)
            {
                Glyph g = getGlyph(str[c]);
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
                        lineColors[destOfs] = g.colorData[srcOfs];
                    }
                }
            }

            TexOutput output = new TexOutput();
            output.xOffset = tx - x;
            output.lineColors = lineColors;
            return output;
        }*/


        public TexOutput cacheBDrawText(Color color, int x, int y, string str, int start, int end)
        {
            int height = this.getLineHeight();

            Point[] positions = new Point[(end - start)];
            int tx = x;
            int theight = this.lineHeight;
            for (int c = start; c < end; c++)
            {
                char ch = str[c];
                Glyph g = getGlyph(ch);
                positions[c] = new Point(g == null ? tx : (tx + g.getXOffset()), g == null ? y : (y + g.getYOffset()));
                tx += g == null ? ((ch == ' ' || ch == '\n') ? this.getSpaceWidth() : 0) : g.xadvance;
                if (g != null)
                    theight = Math.Max(theight, g.getHeight() + g.getYOffset());
            }
            theight += 1;
            int width = (tx - x);
            Microsoft.Xna.Framework.Color[] lineColors = new Microsoft.Xna.Framework.Color[width * theight];

            for (int c = start; c < end; c++)
            {
                Glyph g = getGlyph(str[c]);
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
                        lineColors[destOfs] = g.colorData[srcOfs];
                    }
                }
            }

            TexOutput output = new TexOutput();
            output.width = width;
            output.lineColors = lineColors;
            return output;
        }

        struct GlyphPoint
        {
            public Point point;
            public Glyph glyph;

            public GlyphPoint(Point point, Glyph glyph)
            {
                this.point = point;
                this.glyph = glyph;
            }
        }

        public struct TexMultiLineOutput
        {
            public int numLines;
            public int width;
            public int height;
            public Microsoft.Xna.Framework.Color[] lineColors;
        }

        public TexMultiLineOutput cacheBDrawMultiLineText(Color color, int x, int y, string str, int start, int end, int lineWidth)
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
                int newLineWidth = computeTextWidth(line + strWithinBounds[0], 0, line.Length + 1);
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
                int width = computeTextWidth(lineToPlot, 0, lineToPlot.Length);
                longestLine = Math.Max(width, longestLine);
                int height = this.getLineHeight();

                int tx = 0;
                int theight = this.lineHeight;
                for (int c = 0; c < lineToPlot.Length; c++)
                {
                    Glyph g = getGlyph(lineToPlot[c]);
                    Point point = new Point(g == null ? tx : (tx + g.getXOffset()), g == null ? currentY : (currentY + g.getYOffset()));
                    glyphPoints[currentPoint] = new GlyphPoint(point, g);
                    tx += g == null ? this.getSpaceWidth() : g.xadvance;
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
                if (g.glyph == null)
                {
                    continue;
                }

                for (int j = 0; j < g.glyph.getHeight(); j++)
                {
                    for (int i = 0; i < g.glyph.getWidth(); i++)
                    {
                        var srcOfs = i + j * g.glyph.getWidth();
                        var destOfs = (g.point.X + i) + (g.point.Y + j) * longestLine;
                        lineColors[destOfs] = g.glyph.colorData[srcOfs];
                    }
                }
            }

            TexMultiLineOutput output = new TexMultiLineOutput();
            output.numLines = linesToRender.Count;
            output.lineColors = lineColors;
            output.width = longestLine;
            output.height = currentY;
            return output;
        }

        public int drawMultiLineText(Color color, int x, int y, string str, int width, HAlignment align)
        {
            int start = 0;
            int numLines = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.indexOf(str, '\n', start);
                int xoff = 0;
                if (align != HAlignment.LEFT)
                {
                    int lineWidth = computeTextWidth(str, start, lineEnd);
                    xoff = width - lineWidth;
                    if (align == HAlignment.CENTER)
                    {
                        xoff /= 2;
                    }
                }
                int theight = lineHeight;
                {
                    int startX = x;

                    this.texture.SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                    while (start < lineEnd)
                    {
                        char ch = str[start++];
                        GlyphTex g = getGlyphTex(ch);
                        if (g != null)
                        {
                            //System.Diagnostics.Debug.WriteLine("x0:" + x);
                            //x += lastGlyph.getKerning(ch);
                            //System.Diagnostics.Debug.WriteLine("x1:" + x);
                            //lastGlyph = g;
                            if (g.getWidth() > 0)
                            {
                                g.draw(color, false, x, y);
                            }
                            //System.Diagnostics.Debug.WriteLine("x2:" + x);
                            x += g.xadvance; // + g.getKerning(ch);
                                             //System.Diagnostics.Debug.WriteLine("x3:" + x);
                        }
                        else if (ch == ' ')
                        {
                            x += this.spaceWidth;
                        }

                        //theight = Math.Max(theight, g.getHeight() + g.yoffset);
                    }
                    this.texture.SpriteBatch.End();
                }
                start = lineEnd + 1;
                y += theight;
                numLines++;
            }
            return numLines;
        }

        public void computeMultiLineInfo(string str, int width, HAlignment align, int[] multiLineInfo)
        {
            int start = 0;
            int idx = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.indexOf(str, '\n', start);
                int lineWidth = computeTextWidth(str, start, lineEnd);
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

        /*
        protected void beginLine()
        {
            GL11.glDisable(GL11.GL_TEXTURE_2D);
            GL11.glBegin(GL11.GL_QUADS);
        }

        protected void endLine()
        {
            GL11.glEnd();
            GL11.glEnable(GL11.GL_TEXTURE_2D);
        }

        public void drawMultiLineLines(int x, int y, int[] multiLineInfo, int numLines)
        {
            beginLine();
            try
            {
                for (int i = 0; i < numLines; ++i)
                {
                    int info = multiLineInfo[i];
                    int xoff = x + (short)info;
                    int lineWidth = info >>> 16;
                    GL11.glVertex2i(xoff, y);
                    GL11.glVertex2i(xoff + lineWidth, y);
                    GL11.glVertex2i(xoff + lineWidth, y + 1);
                    GL11.glVertex2i(xoff, y + 1);
                    y += lineHeight;
                }
            }
            finally
            {
                endLine();
            }
        }

        public void drawLine(int x0, int y, int x1)
        {
            beginLine();
            GL11.glVertex2i(x0, y);
            GL11.glVertex2i(x1, y);
            GL11.glVertex2i(x1, y + 1);
            GL11.glVertex2i(x0, y + 1);
            endLine();
        }
        */
        public int computeMultiLineTextWidth(string str)
        {
            int start = 0;
            int width = 0;
            while (start < str.Length)
            {
                int lineEnd = TextUtil.indexOf(str, '\n', start);
                int lineWidth = computeTextWidth(str, start, lineEnd);
                width = Math.Max(width, lineWidth);
                start = lineEnd + 1;
            }
            return width;
        }
        /*

        public FontCache cacheMultiLineText(LWJGLFontCache cache, CharSequence str, int width, HAlignment align)
        {
            if (cache.startCompile())
            {
                int numLines = 0;
                try
                {
                    if (prepare())
                    {
                        try
                        {
                            numLines = drawMultiLineText(0, 0, str, width, align);
                        }
                        finally
                        {
                            cleanup();
                        }
                        computeMultiLineInfo(str, width, align, cache.getMultiLineInfo(numLines));
                    }
                }
                finally
                {
                    cache.endCompile(width, numLines * lineHeight);
                }
                return cache;
            }
            return null;
        }

        public FontCache cacheText(LWJGLFontCache cache, CharSequence str, int start, int end)
        {
            if (cache.startCompile())
            {
                int width = 0;
                try
                {
                    if (prepare())
                    {
                        try
                        {
                            width = drawText(0, 0, str, start, end);
                        }
                        finally
                        {
                            cleanup();
                        }
                    }
                }
                finally
                {
                    cache.endCompile(width, getLineHeight());
                }
                return cache;
            }
            return null;
        }

        boolean bind()
        {
            return texture.bind();
        }

        protected boolean prepare()
        {
            if (texture.bind())
            {
                GL11.glBegin(GL11.GL_QUADS);
                return true;
            }
            return false;
        }

        protected void cleanup()
        {
            GL11.glEnd();
        }*/

        private static String parseFntLine(TextReader br, String tag)
        {
            String line = br.ReadLine();
            if (line == null || line.Length <= tag.Length ||
                    line[tag.Length] != ' ' || !line.StartsWith(tag))
            {
                throw new IOException("'" + tag + "' line expected");
            }
            return line;
        }

        private static void parseFntLine(String line, Dictionary<String, String> parameters)
        {
            parameters.Clear();
            ParameterStringParser psp = new ParameterStringParser(line, ' ', '=');
            while (psp.next())
            {
                parameters.Add(psp.getKey(), psp.getValue());
            }
        }

        private static String getParam(Dictionary<String, String> parameters, String key)
        {
            String value = parameters[key];
            if (value == null)
            {
                throw new IOException("Required parameter '" + key + "' not found");
            }
            return value;
        }

        private static int parseInt(Dictionary<String, String> parameters, String key)
        {
            String value = getParam(parameters, key);
            try
            {
                return int.Parse(value);
            }
            catch (FormatException ex)
            {
                throw new IOException("Can't parse parameter: " + key + '=' + value, ex);
            }
        }

        private static int parseInt(Dictionary<String, String> parameters, String key, int defaultValue)
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
            String value = getParam(parameters, key);
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
