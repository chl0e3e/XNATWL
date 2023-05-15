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
using System.IO;
using System.Text;
using static XNATWL.Utils.Logger;
using XNATWL.Utils;
using System.Collections;
using System.Xml;

namespace XNATWL.TextAreaModel
{
    public class HTMLTextAreaModel : TextAreaModel
    {
        private List<Element> _elements;
        private List<String> _styleSheetLinks;
        private Dictionary<String, Element> _idMap;
        private String _title;

        private List<Style> _styleStack;
        private StringBuilder _stringBuilder;
        private int[] _startLength;

        private ContainerElement _curContainer;

        public event EventHandler<TextAreaChangedEventArgs> Changed;

        /**
         * Creates a new {@code HTMLTextAreaModel} without content.
         */
        public HTMLTextAreaModel()
        {
            this._elements = new List<Element>();
            this._styleSheetLinks = new List<String>();
            this._idMap = new Dictionary<String, Element>();
            this._styleStack = new List<Style>();
            this._stringBuilder = new StringBuilder();
            this._startLength = new int[2];
        }

        /**
         * Creates a new {@code HTMLTextAreaModel} and parses the given html.
         * @param html the HTML to parse
         * @see #setHtml(java.lang.String)
         */
        public HTMLTextAreaModel(string html) : this()
        {
            SetHtml(html);
        }

        /**
         * Creates a new {@code HTMLTextAreaModel} and parses the content of the
         * given {@code Reader}.
         *
         * @see #parseXHTML(java.io.Reader)
         * @param r the reader to parse html from
         * @throws IOException if an error occured while reading
         */
        public HTMLTextAreaModel(Stream r) : this()
        {
            ParseXHTML(r);
        }

        /**
         * Sets the a html to parse.
         * 
         * @param html the html.
         */
        public void SetHtml(string html)
        {
            if (!IsXHTML(html))
            {
                html = "<html><body>" + html + "</body></html>";
            }
            ParseXHTML(new MemoryStream(Encoding.UTF8.GetBytes(html)));
        }

        /**
         * Reads HTML from the given {@code Reader}.
         *
         * @param r the reader to parse html from
         * @throws IOException if an error occured while reading
         * @see #setHtml(java.lang.String)
         * @deprecated use {@link #parseXHTML(java.io.Reader)}
         */
       /* public void readHTMLFromStream(Reader r)
        {
            parseXHTML(r);
        }*/

        /**
         * Reads HTML from the given {@code URL}.
         *
         * @param url the URL to parse.
         * @throws IOException if an error occured while reading
         * @see #parseXHTML(java.io.Reader)
         */
        /*public void readHTMLFromFSO(FileSystemObject url)
        {
            InputStream in = url.openStream();
            try
            {
                parseXHTML(new InputStreamReader(in, "UTF8"));
            }
            finally
            {
                try
                {
                in.close();
                }
                catch (IOException ex)
                {
                    Logger.getLogger(typeof(HTMLTextAreaModel)).log(Level.SEVERE, "Exception while closing InputStream", ex);
                }
            }
        }*/

        /**
         * Returns all links to CSS style sheets
         * @return an Iterable containing all hrefs
         */
        public IEnumerable<String> GetStyleSheetLinks()
        {
            return _styleSheetLinks;
        }

        /**
         * Returns the title of this XHTML document or null if it has no title.
         * @return the title of this XHTML document or null if it has no title.
         */
        public String Title()
        {
            return _title;
        }

        public Element GetElementById(String id)
        {
            return _idMap[id];
        }

        public void DomModified()
        {
            this.Changed.Invoke(this, new TextAreaChangedEventArgs());
        }

        /**
         * Parse a XHTML document. The root element must be &lt;html&gt;
         * @param reader the reader used to read the XHTML document.
         */
        public void ParseXHTML(Stream stream)
        {
            this._elements.Clear();
            this._styleSheetLinks.Clear();
            this._idMap.Clear();
            this._title = null;

            try
            {
                XmlReader xpp = XmlReader.Create(stream);
                //xpp.setInput(reader);
                //xpp.defineEntityReplacementText("nbsp", "\u00A0");

                while (xpp.NodeType != XmlNodeType.Element && xpp.Read())
                {


                }

                if (xpp.NodeType != XmlNodeType.Element)
                {
                    throw new Exception("HTML does not contain an element");
                }

                if (xpp.Name != "html")
                {
                    throw new Exception("HTML does not contain a HTML tag");
                }

                _styleStack.Clear();
                _styleStack.Add(new Style(null, null));
                _curContainer = null;
                _stringBuilder.Length = 0;

                while (xpp.Read() && xpp.NodeType != XmlNodeType.EndElement)
                {
                    if ("head".Equals(xpp.Name) && !xpp.IsEmptyElement)
                    {
                        ParseHead(xpp);
                    }
                    else if ("body".Equals(xpp.Name))
                    {
                        PushStyle(xpp);
                        BlockElement be = new BlockElement(GetStyle());
                        _elements.Add(be);
                        ParseContainer(xpp, be);
                    }
                }

                ParseMain(xpp);
                FinishText();
            }
            catch (Exception ex)
            {
                Logger.GetLogger(typeof(HTMLTextAreaModel)).log(Level.SEVERE, "Unable to parse XHTML document", ex);
            }
            finally
            {
                // data was modified
                if (this.Changed != null)
                {
                    this.Changed.Invoke(this, new TextAreaChangedEventArgs());
                }
            }
        }

        private void ParseContainer(XmlReader xpp, ContainerElement container)
        {
            ContainerElement prevContainer = _curContainer;
            _curContainer = container;
            PushStyle(null);
            ParseMain(xpp);
            PopStyle();
            _curContainer = prevContainer;
        }

        private void ParseMain(XmlReader xpp)
        {
            int level = 1;
            while (level > 0 && xpp.Read())
            {
                XmlNodeType type = xpp.NodeType;
                switch (type)
                {
                    case XmlNodeType.Element:
                        {
                            if ("head".Equals(xpp.Name))
                            {
                                ParseHead(xpp);
                                break;
                            }
                            ++level;
                            FinishText();
                            Style style = PushStyle(xpp);
                            Element element;

                            if ("img".Equals(xpp.Name))
                            {
                                String src = TextUtil.NotNull(xpp.GetAttribute("src"));
                                String alt = xpp.GetAttribute("alt");
                                element = new ImageElement(style, src, alt);
                            }
                            else if ("p".Equals(xpp.Name))
                            {
                                ParagraphElement pe = new ParagraphElement(style);
                                ParseContainer(xpp, pe);
                                element = pe;
                                --level;
                            }
                            else if ("button".Equals(xpp.Name))
                            {
                                String btnName = TextUtil.NotNull(xpp.GetAttribute("name"));
                                String btnParam = TextUtil.NotNull(xpp.GetAttribute("value"));
                                element = new WidgetElement(style, btnName, btnParam);
                            }
                            else if ("ul".Equals(xpp.Name))
                            {
                                ContainerElement ce = new ContainerElement(style);
                                ParseContainer(xpp, ce);
                                element = ce;
                                --level;
                            }
                            else if ("ol".Equals(xpp.Name))
                            {
                                element = ParseOL(xpp, style);
                                --level;
                            }
                            else if ("li".Equals(xpp.Name))
                            {
                                ListElement le = new ListElement(style);
                                ParseContainer(xpp, le);
                                element = le;
                                --level;
                            }
                            else if ("div".Equals(xpp.Name) || IsHeading(xpp.Name))
                            {
                                BlockElement be = new BlockElement(style);
                                ParseContainer(xpp, be);
                                element = be;
                                --level;
                            }
                            else if ("a".Equals(xpp.Name))
                            {
                                String href = xpp.GetAttribute("href");
                                if (href == null)
                                {
                                    break;
                                }
                                LinkElement le = new LinkElement(style, href);
                                ParseContainer(xpp, le);
                                element = le;
                                --level;
                            }
                            else if ("table".Equals(xpp.Name))
                            {
                                element = ParseTable(xpp, style);
                                --level;
                            }
                            else if ("br".Equals(xpp.Name))
                            {
                                element = new LineBreakElement(style);
                            }
                            else
                            {
                                break;
                            }

                            _curContainer.Add(element);
                            RegisterElement(element);
                            break;
                        }
                    case XmlNodeType.EndElement:
                        {
                            --level;
                            FinishText();
                            PopStyle();
                            break;
                        }
                    case XmlNodeType.Text:
                        {
                            _stringBuilder.Append(xpp.Value);
                            break;
                        }
                    case XmlNodeType.EntityReference:
                        _stringBuilder.Append(xpp.Value);
                        break;
                }
            }
        }

        private void ParseHead(XmlReader xpp)
        {
            int level = 1;
            while (level > 0)
            {
                if (!xpp.Read())
                {
                    throw new Exception("Unexpected end of head tag");
                }

                switch (xpp.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            ++level;
                            if ("link".Equals(xpp.Name))
                            {
                                String linkhref = xpp.GetAttribute("href");
                                if ("stylesheet".Equals(xpp.GetAttribute("rel")) &&
                                        "text/css".Equals(xpp.GetAttribute("type")) &&
                                        linkhref != null)
                                {
                                    _styleSheetLinks.Add(linkhref);
                                }
                            }
                            if ("title".Equals(xpp.Name))
                            {
                                _title = xpp.Value;
                                --level;
                            }
                            break;
                        }
                    case XmlNodeType.EndElement:
                        {
                            --level;
                            break;
                        }
                }
            }
        }

        private TableElement ParseTable(XmlReader xpp, Style tableStyle)
        {
            List<TableCellElement> cells = new List<TableCellElement>();
            List<Style> rowStyles = new List<Style>();
            int numColumns = 0;
            int cellSpacing = ParseInt(xpp, "cellspacing", 0);
            int cellPadding = ParseInt(xpp, "cellpadding", 0);

            for (; ; )
            {
                if (!xpp.Read())
                {
                    throw new Exception("Unexpected end of table");
                }

                switch (xpp.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            PushStyle(xpp);

                            if ("td".Equals(xpp.Name) || "th".Equals(xpp.Name))
                            {
                                int colspan = ParseInt(xpp, "colspan", 1);
                                TableCellElement cell = new TableCellElement(GetStyle(), colspan);
                                ParseContainer(xpp, cell);
                                RegisterElement(cell);

                                cells.Add(cell);
                                for (int col = 1; col < colspan; col++)
                                {
                                    cells.Add(null);
                                }
                            }
                            if ("tr".Equals(xpp.Name))
                            {
                                rowStyles.Add(GetStyle());
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            PopStyle();
                            if ("tr".Equals(xpp.Name))
                            {
                                if (numColumns == 0)
                                {
                                    numColumns = cells.Count;
                                }
                            }
                            if ("table".Equals(xpp.Name))
                            {
                                TableElement tableElement = new TableElement(tableStyle,
                                        numColumns, rowStyles.Count, cellSpacing, cellPadding);
                                for (int row = 0, idx = 0; row < rowStyles.Count; row++)
                                {
                                    tableElement.SetRowStyle(row, rowStyles[row]);
                                    for (int col = 0; col < numColumns && idx < cells.Count; col++, idx++)
                                    {
                                        TableCellElement cell = cells[idx];
                                        tableElement.SetCell(row, col, cell);
                                    }
                                }
                                return tableElement;
                            }
                        }
                        break;
                }
            }
        }

        private OrderedListElement ParseOL(XmlReader xpp, Style olStyle)
        {
            int start = ParseInt(xpp, "start", 1);
            OrderedListElement ole = new OrderedListElement(olStyle, start);
            RegisterElement(ole);
            for (; ; )
            {
                if (!xpp.Read())
                {
                    throw new Exception("Unexpected end of table");
                }

                switch (xpp.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            PushStyle(xpp);
                            if ("li".Equals(xpp.Name))
                            {
                                ContainerElement ce = new ContainerElement(GetStyle());
                                ParseContainer(xpp, ce);
                                RegisterElement(ce);
                                ole.Add(ce);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            PopStyle();
                            if ("ol".Equals(xpp.Name))
                            {
                                return ole;
                            }
                        }
                        break;
                }
            }
        }

        private void RegisterElement(Element element)
        {
            StyleSheetKey styleSheetKey = element.GetStyle().StyleSheetKey;
            if (styleSheetKey != null)
            {
                String id = styleSheetKey.ID;
                if (id != null)
                {
                    _idMap.Add(id, element);
                }
            }
        }

        private static int ParseInt(XmlReader xpp, String attribute, int defaultValue)
        {
            String value = xpp.GetAttribute(attribute);
            if (value != null)
            {
                return int.Parse(xpp.GetAttribute(attribute));
            }
            return defaultValue;
        }

        private static bool IsXHTML(String doc)
        {
            if (doc.Length > 5 && doc[0] == '<')
            {
                return doc.StartsWith("<?xml") || doc.StartsWith("<!DOCTYPE") || doc.StartsWith("<html>");
            }
            return false;
        }

        private bool IsHeading(String name)
        {
            return name.Length == 2 && name[0] == 'h' &&
                    (name[1] >= '0' && name[1] <= '6');
        }

        private Style GetStyle()
        {
            return _styleStack[_styleStack.Count - 1];
        }

        private Style PushStyle(XmlReader xpp)
        {
            Style parent = GetStyle();
            StyleSheetKey key = null;
            String style = null;

            if (xpp != null)
            {
                String className = xpp.GetAttribute("class");
                String element = xpp.Name;
                String id = xpp.GetAttribute("id");
                key = new StyleSheetKey(element, className, id);
                style = xpp.GetAttribute("style");
            }

            Style newStyle;

            if (style != null)
            {
                newStyle = new CSSStyle(parent, key, style);
            }
            else
            {
                newStyle = new Style(parent, key);
            }

            _styleStack.Add(newStyle);
            return newStyle;
        }

        private void PopStyle()
        {
            int stackSize = _styleStack.Count;
            if (stackSize > 1)
            {
                _styleStack.RemoveAt(stackSize - 1);
            }
        }

        private void FinishText()
        {
            if (_stringBuilder.Length > 0)
            {
                Style style = GetStyle();
                TextElement e = new TextElement(style, _stringBuilder.ToString());
                RegisterElement(e);
                _curContainer.Add(e);
                _stringBuilder.Length = 0;
            }
        }

        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
