using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;
using XNATWL.Utils;
using static XNATWL.Renderer.XNA.BitmapFont;

namespace XNATWL.Renderer.XNA
{
    public class BitmapFont
    {
        internal class Glyph : TextureAreaBase
        {
            internal short xoffset;
            internal short yoffset;
            internal short xadvance;
            internal byte[][] kerning;

            public Glyph(XNATexture texture, int x, int y, int width, int height) : base(texture, x, y, (height <= 0) ? 0 : width, height)
            {

            }

            internal void draw(Color color, int x, int y)
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

        private static int LOG2_PAGE_SIZE = 9;
        private static int PAGE_SIZE = 1 << LOG2_PAGE_SIZE;
        private static int PAGES = 0x10000 / PAGE_SIZE;

        private XNATexture texture;
        private Glyph[][] glyphs;
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
            //Microsoft.Xna.Framework.Color[] textureColorData = new Microsoft.Xna.Framework.Color[this.texture.Width * this.texture.Height];
            //this.texture.Texture2D.GetData<Microsoft.Xna.Framework.Color>(textureColorData, 0, textureColorData.Length);
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
                    Glyph glyph = new Glyph(new XNATexture(this.texture.Renderer, w, h, xnaGlyph), 0, 0, w, h);
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
                    g.setKerning(second, amount);
                }
            }
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

        public int drawText(Color color, int x, int y, string str, int start, int end)
        {
            int startX = x;
            Glyph lastGlyph = null;
            /*while (start < end)
            {
                lastGlyph = getGlyph(str[start++]);
                if (lastGlyph != null)
                {
                    System.Diagnostics.Debug.WriteLine("b0:" + x);
                    if (lastGlyph.getWidth() > 0)
                    {
                        lastGlyph.draw(x, y);
                    }
                    x += lastGlyph.xadvance;
                    System.Diagnostics.Debug.WriteLine("b1:" + x);
                    break;
                }
            }*/
            while (start < end)
            {
                char ch = str[start++];
                Glyph g = getGlyph(ch);
                if (g != null)
                {
                    //System.Diagnostics.Debug.WriteLine("x0:" + x);
                    //x += lastGlyph.getKerning(ch);
                    //System.Diagnostics.Debug.WriteLine("x1:" + x);
                    lastGlyph = g;
                    if (g.getWidth() > 0)
                    {
                        g.draw(color, x, y);
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
            return x - startX;
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
                drawText(color, x + xoff, y, str, start, lineEnd);
                start = lineEnd + 1;
                y += lineHeight;
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
