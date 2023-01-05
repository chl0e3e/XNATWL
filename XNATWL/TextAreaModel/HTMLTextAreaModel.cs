using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.Logger;
using XNATWL.Utils;
using XNATWL.IO;
using System.Collections;
using System.Xml;

namespace XNATWL.TextAreaModel
{
    public class HTMLTextAreaModel : TextAreaModel
    {
        private List<Element> elements;
        private List<String> styleSheetLinks;
        private Dictionary<String, Element> idMap;
        private String title;

        private List<Style> styleStack;
        private StringBuilder sb;
        private int[] startLength;

        private ContainerElement curContainer;

        public event EventHandler<TextAreaChangedEventArgs> Changed;

        /**
         * Creates a new {@code HTMLTextAreaModel} without content.
         */
        public HTMLTextAreaModel()
        {
            this.elements = new List<Element>();
            this.styleSheetLinks = new List<String>();
            this.idMap = new Dictionary<String, Element>();
            this.styleStack = new List<Style>();
            this.sb = new StringBuilder();
            this.startLength = new int[2];
        }

        /**
         * Creates a new {@code HTMLTextAreaModel} and parses the given html.
         * @param html the HTML to parse
         * @see #setHtml(java.lang.String)
         */
        public HTMLTextAreaModel(string html) : this()
        {
            setHtml(html);
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
            parseXHTML(r);
        }

        /**
         * Sets the a html to parse.
         * 
         * @param html the html.
         */
        public void setHtml(string html)
        {
            if (!isXHTML(html))
            {
                html = "<html><body>" + html + "</body></html>";
            }
            parseXHTML(new MemoryStream(Encoding.UTF8.GetBytes(html)));
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
        public IEnumerable<String> getStyleSheetLinks()
        {
            return styleSheetLinks;
        }

        /**
         * Returns the title of this XHTML document or null if it has no title.
         * @return the title of this XHTML document or null if it has no title.
         */
        public String getTitle()
        {
            return title;
        }

        public Element getElementById(String id)
        {
            return idMap[id];
        }

        public void domModified()
        {
            this.Changed.Invoke(this, new TextAreaChangedEventArgs());
        }

        /**
         * Parse a XHTML document. The root element must be &lt;html&gt;
         * @param reader the reader used to read the XHTML document.
         */
        public void parseXHTML(Stream stream)
        {
            this.elements.Clear();
            this.styleSheetLinks.Clear();
            this.idMap.Clear();
            this.title = null;

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

                styleStack.Clear();
                styleStack.Add(new Style(null, null));
                curContainer = null;
                sb.Length = 0;

                while (xpp.Read() && xpp.NodeType != XmlNodeType.EndElement)
                {
                    if ("head".Equals(xpp.Name) && !xpp.IsEmptyElement)
                    {
                        parseHead(xpp);
                    }
                    else if ("body".Equals(xpp.Name))
                    {
                        pushStyle(xpp);
                        BlockElement be = new BlockElement(getStyle());
                        elements.Add(be);
                        parseContainer(xpp, be);
                    }
                }

                parseMain(xpp);
                finishText();
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

        private void parseContainer(XmlReader xpp, ContainerElement container)
        {
            ContainerElement prevContainer = curContainer;
            curContainer = container;
            pushStyle(null);
            parseMain(xpp);
            popStyle();
            curContainer = prevContainer;
        }

        private void parseMain(XmlReader xpp)
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
                                parseHead(xpp);
                                break;
                            }
                            ++level;
                            finishText();
                            Style style = pushStyle(xpp);
                            Element element;

                            if ("img".Equals(xpp.Name))
                            {
                                String src = TextUtil.notNull(xpp.GetAttribute("src"));
                                String alt = xpp.GetAttribute("alt");
                                element = new ImageElement(style, src, alt);
                            }
                            else if ("p".Equals(xpp.Name))
                            {
                                ParagraphElement pe = new ParagraphElement(style);
                                parseContainer(xpp, pe);
                                element = pe;
                                --level;
                            }
                            else if ("button".Equals(xpp.Name))
                            {
                                String btnName = TextUtil.notNull(xpp.GetAttribute("name"));
                                String btnParam = TextUtil.notNull(xpp.GetAttribute("value"));
                                element = new WidgetElement(style, btnName, btnParam);
                            }
                            else if ("ul".Equals(xpp.Name))
                            {
                                ContainerElement ce = new ContainerElement(style);
                                parseContainer(xpp, ce);
                                element = ce;
                                --level;
                            }
                            else if ("ol".Equals(xpp.Name))
                            {
                                element = parseOL(xpp, style);
                                --level;
                            }
                            else if ("li".Equals(xpp.Name))
                            {
                                ListElement le = new ListElement(style);
                                parseContainer(xpp, le);
                                element = le;
                                --level;
                            }
                            else if ("div".Equals(xpp.Name) || isHeading(xpp.Name))
                            {
                                BlockElement be = new BlockElement(style);
                                parseContainer(xpp, be);
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
                                parseContainer(xpp, le);
                                element = le;
                                --level;
                            }
                            else if ("table".Equals(xpp.Name))
                            {
                                element = parseTable(xpp, style);
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

                            curContainer.Add(element);
                            registerElement(element);
                            break;
                        }
                    case XmlNodeType.EndElement:
                        {
                            --level;
                            finishText();
                            popStyle();
                            break;
                        }
                    case XmlNodeType.Text:
                        {
                            sb.Append(xpp.Value);
                            break;
                        }
                    case XmlNodeType.EntityReference:
                        sb.Append(xpp.Value);
                        break;
                }
            }
        }

        private void parseHead(XmlReader xpp)
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
                                    styleSheetLinks.Add(linkhref);
                                }
                            }
                            if ("title".Equals(xpp.Name))
                            {
                                title = xpp.Value;
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

        private TableElement parseTable(XmlReader xpp, Style tableStyle)
        {
            List<TableCellElement> cells = new List<TableCellElement>();
            List<Style> rowStyles = new List<Style>();
            int numColumns = 0;
            int cellSpacing = parseInt(xpp, "cellspacing", 0);
            int cellPadding = parseInt(xpp, "cellpadding", 0);

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
                            pushStyle(xpp);

                            if ("td".Equals(xpp.Name) || "th".Equals(xpp.Name))
                            {
                                int colspan = parseInt(xpp, "colspan", 1);
                                TableCellElement cell = new TableCellElement(getStyle(), colspan);
                                parseContainer(xpp, cell);
                                registerElement(cell);

                                cells.Add(cell);
                                for (int col = 1; col < colspan; col++)
                                {
                                    cells.Add(null);
                                }
                            }
                            if ("tr".Equals(xpp.Name))
                            {
                                rowStyles.Add(getStyle());
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            popStyle();
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
                                    tableElement.setRowStyle(row, rowStyles[row]);
                                    for (int col = 0; col < numColumns && idx < cells.Count; col++, idx++)
                                    {
                                        TableCellElement cell = cells[idx];
                                        tableElement.setCell(row, col, cell);
                                    }
                                }
                                return tableElement;
                            }
                        }
                        break;
                }
            }
        }

        private OrderedListElement parseOL(XmlReader xpp, Style olStyle)
        {
            int start = parseInt(xpp, "start", 1);
            OrderedListElement ole = new OrderedListElement(olStyle, start);
            registerElement(ole);
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
                            pushStyle(xpp);
                            if ("li".Equals(xpp.Name))
                            {
                                ContainerElement ce = new ContainerElement(getStyle());
                                parseContainer(xpp, ce);
                                registerElement(ce);
                                ole.Add(ce);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            popStyle();
                            if ("ol".Equals(xpp.Name))
                            {
                                return ole;
                            }
                        }
                        break;
                }
            }
        }

        private void registerElement(Element element)
        {
            StyleSheetKey styleSheetKey = element.getStyle().StyleSheetKey;
            if (styleSheetKey != null)
            {
                String id = styleSheetKey.ID;
                if (id != null)
                {
                    idMap.Add(id, element);
                }
            }
        }

        private static int parseInt(XmlReader xpp, String attribute, int defaultValue)
        {
            String value = xpp.GetAttribute(attribute);
            if (value != null)
            {
                return int.Parse(xpp.GetAttribute(attribute));
            }
            return defaultValue;
        }

        private static bool isXHTML(String doc)
        {
            if (doc.Length > 5 && doc[0] == '<')
            {
                return doc.StartsWith("<?xml") || doc.StartsWith("<!DOCTYPE") || doc.StartsWith("<html>");
            }
            return false;
        }

        private bool isHeading(String name)
        {
            return name.Length == 2 && name[0] == 'h' &&
                    (name[1] >= '0' && name[1] <= '6');
        }

        private Style getStyle()
        {
            return styleStack[styleStack.Count - 1];
        }

        private Style pushStyle(XmlReader xpp)
        {
            Style parent = getStyle();
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

            styleStack.Add(newStyle);
            return newStyle;
        }

        private void popStyle()
        {
            int stackSize = styleStack.Count;
            if (stackSize > 1)
            {
                styleStack.RemoveAt(stackSize - 1);
            }
        }

        private void finishText()
        {
            if (sb.Length > 0)
            {
                Style style = getStyle();
                TextElement e = new TextElement(style, sb.ToString());
                registerElement(e);
                curContainer.Add(e);
                sb.Length = 0;
            }
        }

        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }
}
