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
    /// <summary>
    /// A simple XHTML parser.
    /// The following tags are supported:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Tag</term>
    ///         <description>Human name</description>
    ///     </listheader>
    ///     <item>
    ///         <term>a</term>
    ///         <description>Hyperlink<br/>Attributes: <strong>href</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>p</term>
    ///         <description>Paragraph<br/>Attributes: <em>none</em></description>
    ///     </item>
    ///     <item>
    ///         <term>br</term>
    ///         <description>New line<br/>Attributes: <em>none</em></description>
    ///     </item>
    ///     <item>
    ///         <term>img</term>
    ///         <description>Image<br/>Attributes: <strong>src</strong>, <strong>alt</strong><br/>Styles: <strong>float</strong>, <strong>display</strong>, <strong>width</strong>, <strong>height</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>span</term>
    ///         <description>Generic inline element<br/>Attributes: <em>none</em></description>
    ///     </item>
    ///     <item>
    ///         <term>div</term>
    ///         <description>Generic blocking element<br/>Attributes: <em>none</em><br/>Styles: <strong>background-image</strong>, <strong>float</strong>, <strong>width</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>ul</term>
    ///         <description>List<br/>Attributes: <em>none</em></description>
    ///     </item>
    ///     <item>
    ///         <term>li</term>
    ///         <description>List item<br/>Attributes: <em>none</em><br/>Styles: <strong>list-style-image</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>button</term>
    ///         <description>Button<br/>Attributes: <strong>name</strong>, <strong>value</strong><br/>Styles: <strong>float</strong>, <strong>display</strong>, <strong>width</strong>, <strong>height</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>table</term>
    ///         <description>Table<br/>Attributes: <strong>cellspacing</strong>, <strong>cellpadding</strong><br/>Styles: <strong>list-style-image</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>tr</term>
    ///         <description>Table row<br/>Attributes: <em>none</em><br/>Styles:  <strong>height</strong></description>
    ///     </item>
    ///     <item>
    ///         <term>td</term>
    ///         <description>Table cell<br/>Attributes: <strong>colspan</strong><br/>Styles: <strong>width</strong></description>
    ///     </item>
    /// </list>
    ///
    /// The following generic CSS attributes are supported:
    /// <ul>
    ///  <li><strong>font-family</strong>,</li>
    ///  <li><strong>text-align</strong>,</li>
    ///  <li><strong>text-ident</strong>,</li>
    ///  <li><strong>margin</strong>,</li>
    ///  <li><strong>margin-top</strong>,</li>
    ///  <li><strong>margin-left</strong>,</li>
    ///  <li><strong>margin-right</strong>,</li>
    ///  <li><strong>margin-bottom</strong>,</li>
    ///  <li><strong>padding</strong>,</li>
    ///  <li><strong>padding-top</strong>,</li>
    ///  <li><strong>padding-left</strong>,</li>
    ///  <li><strong>padding-right</strong>,</li>
    ///  <li><strong>padding-bottom</strong>,</li>
    ///  <li><strong>clear</strong>,</li>
    ///  <li><strong>vertical-align</strong>,</li>
    ///  <li><strong>white-space</strong></li>
    /// </ul>
    /// <para>You can only use <strong>white-space</strong> on <strong>normal</strong> and <strong>pre</strong></para>
    /// Numeric values must use on of the following units: <strong>em</strong>, <strong>ex</strong>, <strong>px</strong>, <strong>%</strong>
    /// </summary>
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

        /// <summary>
        /// A change to the HTML contents
        /// </summary>
        public event EventHandler<TextAreaChangedEventArgs> Changed;

        /// <summary>
        /// Creates a new <see cref="HTMLTextAreaModel"/> without content.
        /// </summary>
        public HTMLTextAreaModel()
        {
            this._elements = new List<Element>();
            this._styleSheetLinks = new List<String>();
            this._idMap = new Dictionary<String, Element>();
            this._styleStack = new List<Style>();
            this._stringBuilder = new StringBuilder();
            this._startLength = new int[2];
        }

        /// <summary>
        /// Creates a new <see cref="HTMLTextAreaModel"/> and parses the given HTML
        /// </summary>
        /// <param name="html">the HTML to parse</param>
        public HTMLTextAreaModel(string html) : this()
        {
            SetHTML(html);
        }

        /// <summary>
        /// Creates a new <see cref="HTMLTextAreaModel"/> and parses the content of the givev <see cref="Stream"/>
        /// </summary>
        /// <param name="r">the stream to parse the HTML from</param>
        public HTMLTextAreaModel(Stream r) : this()
        {
            ParseXHTML(r);
        }

        /// <summary>
        /// Sets the HTML to parse.
        /// </summary>
        /// <param name="html">the HTML</param>
        public void SetHTML(string html)
        {
            if (!IsXHTML(html))
            {
                html = "<html><body>" + html + "</body></html>";
            }

            ParseXHTML(new MemoryStream(Encoding.UTF8.GetBytes(html)));
        }

        /// <summary>
        /// An Iterable containing all links to CSS style sheets
        /// </summary>
        public IEnumerable<String> StyleSheetLinks
        {
            get
            {
                return _styleSheetLinks;
            }
        }

        /// <summary>
        /// The title of this XHTML document or null if it has no title.
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
        }

        /// <summary>
        /// Get an element by ID
        /// </summary>
        /// <param name="id">ID of the element</param>
        /// <returns>The element with the matching ID</returns>
        public Element this[string id]
        {
            get
            {
                return _idMap[id];
            }
        }

        /// <summary>
        /// Called when the <see cref="HTMLTextAreaModel"/> value has changed
        /// </summary>
        public void DomModified()
        {
            if (this.Changed != null)
            {
                this.Changed.Invoke(this, new TextAreaChangedEventArgs());
            }
        }

        /// <summary>
        /// Parse a XHTML document. The root element must be a HTML tag
        /// </summary>
        /// <param name="stream">the stream used to read the XHTML document.</param>
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
                        BlockElement be = new BlockElement(CurrentStyle);
                        _elements.Add(be);
                        ParseContainer(xpp, be);
                    }
                }

                ParseMain(xpp);
                FinishText();
            }
            catch (Exception ex)
            {
                Logger.GetLogger(typeof(HTMLTextAreaModel)).Log(Level.SEVERE, "Unable to parse XHTML document", ex);
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

        /// <summary>
        /// Parse a <see cref="ContainerElement"/> with a blank slate on the top of the style stack
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="container"></param>
        private void ParseContainer(XmlReader xmlReader, ContainerElement container)
        {
            ContainerElement prevContainer = _curContainer;
            _curContainer = container;
            PushStyle(null);
            ParseMain(xmlReader);
            PopStyle();
            _curContainer = prevContainer;
        }

        /// <summary>
        /// Generic parse most HTML tags
        /// </summary>
        /// <param name="xmlReader">XML reader</param>
        private void ParseMain(XmlReader xmlReader)
        {
            int level = 1;
            while (level > 0 && xmlReader.Read())
            {
                XmlNodeType type = xmlReader.NodeType;
                switch (type)
                {
                    case XmlNodeType.Element:
                        {
                            if ("head".Equals(xmlReader.Name))
                            {
                                ParseHead(xmlReader);
                                break;
                            }
                            ++level;
                            FinishText();
                            Style style = PushStyle(xmlReader);
                            Element element;

                            if ("img".Equals(xmlReader.Name))
                            {
                                String src = TextUtil.NotNull(xmlReader.GetAttribute("src"));
                                String alt = xmlReader.GetAttribute("alt");
                                element = new ImageElement(style, src, alt);
                            }
                            else if ("p".Equals(xmlReader.Name))
                            {
                                ParagraphElement pe = new ParagraphElement(style);
                                ParseContainer(xmlReader, pe);
                                element = pe;
                                --level;
                            }
                            else if ("button".Equals(xmlReader.Name))
                            {
                                String btnName = TextUtil.NotNull(xmlReader.GetAttribute("name"));
                                String btnParam = TextUtil.NotNull(xmlReader.GetAttribute("value"));
                                element = new WidgetElement(style, btnName, btnParam);
                            }
                            else if ("ul".Equals(xmlReader.Name))
                            {
                                ContainerElement ce = new ContainerElement(style);
                                ParseContainer(xmlReader, ce);
                                element = ce;
                                --level;
                            }
                            else if ("ol".Equals(xmlReader.Name))
                            {
                                element = ParseOL(xmlReader, style);
                                --level;
                            }
                            else if ("li".Equals(xmlReader.Name))
                            {
                                ListElement le = new ListElement(style);
                                ParseContainer(xmlReader, le);
                                element = le;
                                --level;
                            }
                            else if ("div".Equals(xmlReader.Name) || IsHeading(xmlReader.Name))
                            {
                                BlockElement be = new BlockElement(style);
                                ParseContainer(xmlReader, be);
                                element = be;
                                --level;
                            }
                            else if ("a".Equals(xmlReader.Name))
                            {
                                String href = xmlReader.GetAttribute("href");
                                if (href == null)
                                {
                                    break;
                                }
                                LinkElement le = new LinkElement(style, href);
                                ParseContainer(xmlReader, le);
                                element = le;
                                --level;
                            }
                            else if ("table".Equals(xmlReader.Name))
                            {
                                element = ParseTable(xmlReader, style);
                                --level;
                            }
                            else if ("br".Equals(xmlReader.Name))
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
                            _stringBuilder.Append(xmlReader.Value);
                            break;
                        }
                    case XmlNodeType.EntityReference:
                        _stringBuilder.Append(xmlReader.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// Parse the head of an XHTML document. The head normally contains LINK tags to a stylesheet or a TITLE tag for the document title.
        /// </summary>
        /// <param name="xmlReader">XMLReader to read from</param>
        /// <exception cref="Exception">The header has nothing to read immediately (invalid XHTML?)</exception>
        private void ParseHead(XmlReader xmlReader)
        {
            int level = 1;
            while (level > 0)
            {
                if (!xmlReader.Read())
                {
                    throw new Exception("Unexpected end of head tag");
                }

                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            ++level;
                            if ("link".Equals(xmlReader.Name))
                            {
                                String linkhref = xmlReader.GetAttribute("href");
                                if ("stylesheet".Equals(xmlReader.GetAttribute("rel")) &&
                                        "text/css".Equals(xmlReader.GetAttribute("type")) &&
                                        linkhref != null)
                                {
                                    _styleSheetLinks.Add(linkhref);
                                }
                            }
                            if ("title".Equals(xmlReader.Name))
                            {
                                _title = xmlReader.Value;
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

        /// <summary>
        /// Parse a table tag from an XHTML document using <see cref="XmlReader"/>
        /// </summary>
        /// <param name="xmlReader">XML reader</param>
        /// <param name="tableStyle">Table styling information</param>
        /// <returns>Table element</returns>
        /// <exception cref="Exception">The table had nothing to parse immediately (invalid XHTML?)</exception>
        private TableElement ParseTable(XmlReader xmlReader, Style tableStyle)
        {
            List<TableCellElement> cells = new List<TableCellElement>();
            List<Style> rowStyles = new List<Style>();
            int numColumns = 0;
            int cellSpacing = ParseInt(xmlReader, "cellspacing", 0);
            int cellPadding = ParseInt(xmlReader, "cellpadding", 0);

            for (; ; )
            {
                if (!xmlReader.Read())
                {
                    throw new Exception("Unexpected end of table");
                }

                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            PushStyle(xmlReader);

                            if ("td".Equals(xmlReader.Name) || "th".Equals(xmlReader.Name))
                            {
                                int colspan = ParseInt(xmlReader, "colspan", 1);
                                TableCellElement cell = new TableCellElement(CurrentStyle, colspan);
                                ParseContainer(xmlReader, cell);
                                RegisterElement(cell);

                                cells.Add(cell);
                                for (int col = 1; col < colspan; col++)
                                {
                                    cells.Add(null);
                                }
                            }
                            if ("tr".Equals(xmlReader.Name))
                            {
                                rowStyles.Add(CurrentStyle);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            PopStyle();
                            if ("tr".Equals(xmlReader.Name))
                            {
                                if (numColumns == 0)
                                {
                                    numColumns = cells.Count;
                                }
                            }
                            if ("table".Equals(xmlReader.Name))
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

        /// <summary>
        /// Parse an OL tag from an XHTML document provided by an <see cref="XmlReader"/>
        /// </summary>
        /// <param name="xmlReader">XML reader to read from</param>
        /// <param name="olStyle">OL styling information</param>
        /// <returns><see cref="OrderedListElement"/></returns>
        /// <exception cref="Exception"></exception>
        private OrderedListElement ParseOL(XmlReader xmlReader, Style olStyle)
        {
            int start = ParseInt(xmlReader, "start", 1);
            OrderedListElement ole = new OrderedListElement(olStyle, start);
            RegisterElement(ole);
            for (; ; )
            {
                if (!xmlReader.Read())
                {
                    throw new Exception("Unexpected end of table");
                }

                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            PushStyle(xmlReader);
                            if ("li".Equals(xmlReader.Name))
                            {
                                ContainerElement ce = new ContainerElement(CurrentStyle);
                                ParseContainer(xmlReader, ce);
                                RegisterElement(ce);
                                ole.Add(ce);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            PopStyle();
                            if ("ol".Equals(xmlReader.Name))
                            {
                                return ole;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Register a new element by it's ID
        /// </summary>
        /// <param name="element">Element to register</param>
        private void RegisterElement(Element element)
        {
            StyleSheetKey styleSheetKey = element.Style.StyleSheetKey;
            if (styleSheetKey != null)
            {
                String id = styleSheetKey.ID;
                if (id != null)
                {
                    _idMap.Add(id, element);
                }
            }
        }

        /// <summary>
        /// Parse XML attribute as an integer
        /// </summary>
        /// <param name="xmlReader">XML reader to read from</param>
        /// <param name="attribute">Name of the attribute</param>
        /// <param name="defaultValue">Default value if the attribute wasn't found</param>
        /// <returns>Parsed <see cref="int"/></returns>
        private static int ParseInt(XmlReader xmlReader, String attribute, int defaultValue)
        {
            string value = xmlReader.GetAttribute(attribute);
            if (value != null)
            {
                return int.Parse(value);
            }
            return defaultValue;
        }

        /// <summary>
        /// Detect if a string contains valid XHTML
        /// </summary>
        /// <param name="doc">document as a string</param>
        /// <returns><strong>true</strong> if valid XHTML</returns>
        private static bool IsXHTML(String doc)
        {
            if (doc.Length > 5 && doc[0] == '<')
            {
                return doc.StartsWith("<?xml") || doc.StartsWith("<!DOCTYPE") || doc.StartsWith("<html>");
            }
            return false;
        }

        /// <summary>
        /// Detect if XHTML tag name is a heading
        /// </summary>
        /// <param name="name">XHTML tag name</param>
        /// <returns><strong>true</strong> if heading</returns>
        private bool IsHeading(string name)
        {
            return name.Length == 2 && name[0] == 'h' && (name[1] >= '0' && name[1] <= '6');
        }

        /// <summary>
        /// The style at the top of the stack
        /// </summary>
        private Style CurrentStyle
        {
            get
            {
                return _styleStack[_styleStack.Count - 1];
            }
        }

        /// <summary>
        /// Push a new style onto the top of the stack
        /// </summary>
        /// <param name="xmlReader">XML reader to read XHTML</param>
        /// <returns>new <see cref="Style"/> at the top of the stack</returns>
        private Style PushStyle(XmlReader xmlReader)
        {
            Style parent = CurrentStyle;
            StyleSheetKey key = null;
            String style = null;

            if (xmlReader != null)
            {
                String className = xmlReader.GetAttribute("class");
                String element = xmlReader.Name;
                String id = xmlReader.GetAttribute("id");
                key = new StyleSheetKey(element, className, id);
                style = xmlReader.GetAttribute("style");
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

        /// <summary>
        /// Pop the last <see cref="Style"/> on the stack
        /// </summary>
        private void PopStyle()
        {
            int stackSize = _styleStack.Count;
            if (stackSize > 1)
            {
                _styleStack.RemoveAt(stackSize - 1);
            }
        }

        /// <summary>
        /// Register the built-up DOM values on the current XHTML tag as an element
        /// </summary>
        private void FinishText()
        {
            if (_stringBuilder.Length > 0)
            {
                Style style = CurrentStyle;
                TextElement e = new TextElement(style, _stringBuilder.ToString());
                RegisterElement(e);
                _curContainer.Add(e);
                _stringBuilder.Length = 0;
            }
        }

        /// <summary>
        /// Enumerate the elements in the HTML model
        /// </summary>
        /// <returns>Element enumerator</returns>
        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        /// <summary>
        /// Enumerate the elements in the HTML model
        /// </summary>
        /// <returns>Element enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
