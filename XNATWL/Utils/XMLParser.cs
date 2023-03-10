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
using System.Xml;
using XNATWL.IO;
using XNATWL.Util;

namespace XNATWL.Utils
{
    public class XMLParser
    {
        private static Type[] XPP_CLASS = { typeof(XmlPullParser) };
        private static bool hasXMP1 = true;

        private XmlTextReader xmlTextReader;
        internal XmlReader xpp;
        private bool reachedDocumentEnd;
        private string source;
        private Stream stream;
        private BitSet unusedAttributes = new BitSet();
        private string loggerName = typeof(XMLParser).Name;

        /*public XMLParser(XmlPullParser xpp, string source)
        {
            if (xpp == null)
            {
                throw new NullPointerException("xpp");
            }
            this.xpp = xpp;
            this.source = source;
            this.inputStream = null;
        }*/

        /**
         * Creates a XMLParser for the given URL.
         *
         * This method also calls {@code URL.getContent} which allows a custom
         * URLStreamHandler to return a class implementing {@code XmlPullParser}.
         *
         * @param url the URL to parse
         * @throws XmlPullParserException if the resource is not a valid XML file
         * @throws IOException if the resource could not be read
         * @see URLStreamHandler
         * @see URL#getContent(java.lang.Class[])
         * @see org.xmlpull.v1.XmlPullParser
         */
        public XMLParser(FileSystemObject file)
        {
            this.source = file.Name;

            this.stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(file.Path)));
            this.xmlTextReader = new XmlTextReader(stream);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
            this.xpp = XmlReader.Create(xmlTextReader, xmlReaderSettings);
        }

        public XMLParser(Stream stream)
        {
            this.source = "Stream";

            this.stream = stream;
            this.xmlTextReader = new XmlTextReader(stream);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
            this.xpp = XmlReader.Create(xmlTextReader, xmlReaderSettings);
        }

        public void close()
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        public void setLoggerName(string loggerName)
        {
            this.loggerName = loggerName;
        }

        bool isElementParsing = false;
        bool justSeenEndTag = false;

        /**
         * @see XmlPullParser#next() 
         */
        public int next()
        {
            warnUnusedAttributes();
            int type;

            if (isElementParsing)
            {
                isElementParsing = false;
                justSeenEndTag = true;
                return XmlPullParser.END_TAG;
            }

            if (justSeenEndTag)
            {
                justSeenEndTag = false;
            }

            bool read = xpp.Read();

            while ((xpp.NodeType == XmlNodeType.Attribute || xpp.NodeType == XmlNodeType.Comment || xpp.NodeType == XmlNodeType.Whitespace) && xpp.Read())
            {
            }

            if (xpp.NodeType == XmlNodeType.Element && xpp.IsEmptyElement)
            {
                isElementParsing = true;
            }

            if (read)
            {
                type = XmlPullParser.NodeTypeToToken(xpp.NodeType);
            }
            else
            {
                if (!reachedDocumentEnd)
                {
                    reachedDocumentEnd = true;
                    type = XmlPullParser.END_DOCUMENT;
                }
                else
                {
                    throw new XmlPullParserException("XPP failure read after document end");
                }
            }

            handleType(type);
            return type;
        }

        /**
         * @see XmlPullParser#nextTag() 
         */
        public int nextTag()
        {
            //warnUnusedAttributes();
            int type = this.next();

            while (type == XmlPullParser.IGNORABLE_WHITESPACE || type == XmlPullParser.COMMENT)
            {
                type = this.next();
            }

            if (type != XmlPullParser.START_TAG && type != XmlPullParser.END_TAG)
            {
                throw new XmlPullParserException("expected start or end tag, got " + this.xpp.NodeType, this);
            }

            handleType(type);
            return type;
        }

        /**
         * @see XmlPullParser#nextText()
         */
        public string nextText()
        {
            if (xpp.NodeType != XmlNodeType.Element)
            {
                throw new XmlPullParserException("parser must be on START_TAG to read next text");
            }
            int eventType = next();
            //warnUnusedAttributes();
            if (eventType == XmlPullParser.TEXT)
            {
                string result = xpp.Value;
                eventType = next();
                if (eventType != XmlPullParser.END_TAG)
                {
                    throw new XmlPullParserException("event TEXT it must be immediately followed by END_TAG");
                }
                return result;
            }
            else if(eventType == XmlPullParser.END_TAG)
            {
                return "";
            }
            else
            {
                throw new XmlPullParserException("parser must be on START_TAG or TEXT to read text");
            }
        }

        public void skipText()
        {
            int token = XmlPullParser.NodeTypeToToken(this.xpp.NodeType);
            while (token == XmlPullParser.TEXT || token == XmlPullParser.ENTITY_REF || token == XmlPullParser.COMMENT)
            {
                xpp.Skip();
            }
        }

        public bool isStartTag()
        {
            if (xpp.NodeType == XmlNodeType.Element && justSeenEndTag)
            {
                return false;
            }
            return xpp.NodeType == XmlNodeType.Element;
        }

        public bool isEndTag()
        {
            if (xpp.NodeType == XmlNodeType.Element && justSeenEndTag)
            {
                return true;
            }
            return xpp.NodeType == XmlNodeType.EndElement;
        }

        public string getPositionDescription()
        {
            string desc = "Line: " + this.xmlTextReader.LineNumber + ", Position: " + this.xmlTextReader.LinePosition;
            if (source != null)
            {
                return desc + " in " + source;
            }
            return desc;
        }

        public int getLineNumber()
        {
            return this.xmlTextReader.LineNumber;
        }

        public int getColumnNumber()
        {
            return this.xmlTextReader.LinePosition;
        }

        public string getFilePosition()
        {
            if (source != null)
            {
                return source + ":" + getLineNumber();
            }

            return this.getPositionDescription();
        }

        public string getName()
        {
            return this.xpp.Name;
        }

        public bool isEmptyElement()
        {
            return this.xpp.IsEmptyElement;
        }

        /**
         * @see XmlPullParser#require(int, java.lang.string, java.lang.string) 
         */
        public void require(int type, string @namespace, string name)
        {
            if (justSeenEndTag && type == XmlPullParser.END_TAG)
            {
                if (name != null && name != xpp.Name)
                {
                    throw new XmlPullParserException("token name mismatch", this);
                }
                return;
            }
            //xpp.require(type, @namespace, name);
            switch(type)
            {
                case 0:
                    if(this.xpp.NodeType != XmlNodeType.Document)
                    {
                        throw new XmlPullParserException("token is not START_DOCUMENT", this);
                    }
                    break;
                case 1:
                    //if (this.xpp.NodeType != XmlNodeType.End)
                    //{
                    throw new NotImplementedException("end document tokens not supported in C#");
                    //}
                    //break;
                case 2:
                    if (this.xpp.NodeType != XmlNodeType.Element)
                    {
                        throw new XmlPullParserException("token is not START_TAG", this);
                    }
                    if (name != null && name != xpp.Name)
                    {
                        throw new XmlPullParserException("token name mismatch", this);
                    }
                    break;
                case 3:
                    if (this.xpp.NodeType != XmlNodeType.EndElement)
                    {
                        throw new XmlPullParserException("token is not END_TAG", this);
                    }
                    if (name != null && name != xpp.Name)
                    {
                        throw new XmlPullParserException("token name mismatch", this);
                    }
                    break;
                case 4:
                    if (this.xpp.NodeType != XmlNodeType.Text)
                    {
                        throw new XmlPullParserException("token is not TEXT", this);
                    }
                    break;
                case 5:
                    if (this.xpp.NodeType != XmlNodeType.CDATA)
                    {
                        throw new XmlPullParserException("token is not CDATA", this);
                    }
                    break;
                case 6:
                    if (this.xpp.NodeType != XmlNodeType.EntityReference)
                    {
                        throw new XmlPullParserException("token is not ENTITY_REF", this);
                    }
                    break;
                case 7:
                    if (this.xpp.NodeType != XmlNodeType.Whitespace && this.xpp.NodeType != XmlNodeType.SignificantWhitespace)
                    {
                        throw new XmlPullParserException("token is not IGNORABLE_WHITESPACE", this);
                    }
                    break;
                case 8:
                    if (this.xpp.NodeType != XmlNodeType.ProcessingInstruction)
                    {
                        throw new XmlPullParserException("token is not PROCESSING_INSTRUCTION", this);
                    }
                    break;
                case 9:
                    if (this.xpp.NodeType != XmlNodeType.Comment)
                    {
                        throw new XmlPullParserException("token is not COMMENT", this);
                    }
                    break;
                case 10:
                    if (this.xpp.NodeType != XmlNodeType.DocumentType)
                    {
                        throw new XmlPullParserException("token is not DODECL", this);
                    }
                    break;
            }
        }

        public string getAttributeValue(int index)
        {
            unusedAttributes.Clear(index);
            return xpp.GetAttribute(index);
        }

        public string getAttributeNamespace(int index)
        {
            this.xpp.MoveToAttribute(index);
            string namespaceUri = xpp.NamespaceURI;
            this.xpp.MoveToElement();
            return namespaceUri;
        }

        public string getAttributeName(int index)
        {
            this.xpp.MoveToAttribute(index);
            string name = this.xpp.Name;
            this.xpp.MoveToElement();
            return name;
        }

        public int getAttributeCount()
        {
            return this.xpp.AttributeCount;
        }

        public string getAttributeValue(string @namespace, string name)
        {
            for (int i = 0, n = xpp.AttributeCount; i < n; i++)
            {
                this.xpp.MoveToAttribute(i);
                if ((this.xpp.NamespaceURI == "" || @namespace.Equals(xpp.NamespaceURI)) &&
                        name.Equals(xpp.Name))
                {
                    var attrValue = getAttributeValue(i);
                    this.xpp.MoveToElement();
                    return attrValue;
                }
            }

            this.xpp.MoveToElement();
            return null;
        }


        public string getAttributeNotNull(string attribute)
        {
            string value = getAttributeValue(null, attribute);
            this.xpp.MoveToElement();
            if (value == null)
            {
                missingAttribute(attribute);
            }
            return value;
        }

        public bool parseBoolFromAttribute(string attribName)
        {
            return parseBool(getAttributeNotNull(attribName));
        }

        public bool parseBoolFromText()
        {
            return parseBool(nextText());
        }

        public bool parseBoolFromAttribute(string attribName, bool defaultValue)
        {
            string value = getAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return parseBool(value);
        }

        public int parseIntFromAttribute(string attribName)
        {
            return parseInt(getAttributeNotNull(attribName));
        }

        public int parseIntFromAttribute(string attribName, int defaultValue)
        {
            string value = getAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return parseInt(value);
        }

        public float parseFloatFromAttribute(string attribName)
        {
            return parseFloat(getAttributeNotNull(attribName));
        }

        public float parseFloatFromAttribute(string attribName, float defaultValue)
        {
            string value = getAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return parseFloat(value);
        }

        public E parseEnumFromAttribute<E>(string attribName, Type enumClazz) where E : struct, IConvertible
        {
            return parseEnum<E>(enumClazz, getAttributeNotNull(attribName));
        }

        public E parseEnumFromAttribute<E>(string attribName, Type enumClazz, E defaultValue) where E : struct, IConvertible
        {
            string value = getAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return parseEnum<E>(enumClazz, value);
        }

        public E parseEnumFromText<E>(Type enumClazz) where E : struct, IConvertible
        {
            return parseEnum<E>(enumClazz, nextText());
        }

        public object parseEnumFromText(Type enumClazz)
        {
            return parseEnum(enumClazz, nextText());
        }

        public Dictionary<string, string> getUnusedAttributes()
        {
            if (unusedAttributes.IsEmpty())
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = -1; (i = unusedAttributes.NextSetBit(i + 1)) >= 0;)
            {
                result.Add(this.getAttributeName(i), this.getAttributeValue(i));
            }

            unusedAttributes.Clear();

            return result;
        }

        public void ignoreOtherAttributes()
        {
            unusedAttributes.Clear();
        }

        public bool isAttributeUnused(int idx)
        {
            return unusedAttributes.Get(idx);
        }

        public XmlPullParserException error(string msg)
        {
            return new XmlPullParserException(msg, this, null);
        }

        public XmlPullParserException error(string msg, Exception cause)
        {
            return (XmlPullParserException)new XmlPullParserException(msg, this, cause);
        }

        public XmlPullParserException unexpected()
        {
            return new XmlPullParserException("Unexpected '" + xpp.Name + "'", this, null);
        }

        protected E parseEnum<E>(Type enumClazz, string value) where E : struct, IConvertible
        {
            if (!enumClazz.IsEnum)
            {
                throw error("enum class provided reflects that it is not an enum");
            }

            foreach(E e in Enum.GetValues(enumClazz))
            {
                if (e.ToString().ToLower() == value.ToLower())
                {
                    return e;
                }
            }

            throw new XmlPullParserException("Unknown enum value \"" + value + "\" for enum class " + enumClazz, this, null);
        }

        protected Object parseEnum(Type enumClazz, string value)
        {
            if (!enumClazz.IsEnum)
            {
                throw error("enum class provided reflects that it is not an enum");
            }

            foreach (object e in Enum.GetValues(enumClazz))
            {
                if (e.ToString().ToLower() == value.ToLower())
                {
                    return e;
                }
            }

            throw new XmlPullParserException("Unknown enum value \"" + value + "\" for enum class " + enumClazz, this, null);
        }


        public bool parseBool(string value)
        {
            if ("true".Equals(value))
            {
                return true;
            }
            else if ("false".Equals(value))
            {
                return false;
            }
            else
            {
                throw new XmlPullParserException("bool value must be 'true' or 'false'", this, null);
            }
        }

        protected int parseInt(string value)
        {
            try
            {
                return Int32.Parse(value);
            }
            catch (FormatException ex)
            {
                throw (XmlPullParserException)(new XmlPullParserException(
                        "Unable to parse integer", this, ex));
            }
        }

        protected float parseFloat(string value)
        {
            try
            {
                return float.Parse(value);
            }
            catch (FormatException ex)
            {
                throw (XmlPullParserException)(new XmlPullParserException(
                        "Unable to parse float", this, ex));
            }
        }

        protected void missingAttribute(string attribute)
        {
            throw new XmlPullParserException("missing '" + attribute + "' on '" + xpp.Name + "'", this, null);
        }

        protected void handleType(int type)
        {
            unusedAttributes.Clear();
            switch (type)
            {
                case 2:
                    unusedAttributes.Set(0, xpp.AttributeCount);
                    break;
            }
        }

        protected void warnUnusedAttributes()
        {
            if (!unusedAttributes.IsEmpty())
            {
                string positionDescription = getPositionDescription();
                for (int i = -1; (i = unusedAttributes.NextSetBit(i + 1)) >= 0;)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Unused attribute ''{0}'' on ''{1}'' at {2}", new Object[] { xpp.GetAttribute(i), xpp.Name, positionDescription }));
                }
            }
        }
    }

    public class XmlPullParserException : Exception
    {
        public object Parser;
        public int LineNumber = -1;
        public int LinePosition = -1;

        public XmlPullParserException(string message) : base(message)
        {
            this.Parser = null;
        }

        public XmlPullParserException(string message, object parser) : base(((XMLParser)parser).getPositionDescription() + "   " + message)
        {
            this.Parser = parser;
            this.LineNumber = ((XMLParser)parser).getLineNumber();
            this.LinePosition = ((XMLParser)parser).getColumnNumber();
        }

        public XmlPullParserException(string message, object parser, Exception chain) : base(((XMLParser)parser).getPositionDescription() + "   " + message, chain)
        {
            this.Parser = parser;
            this.LineNumber = ((XMLParser)parser).getLineNumber();
            this.LinePosition = ((XMLParser)parser).getColumnNumber();
        }
    }

    internal class XmlPullParser
    {
        public static int START_DOCUMENT = 0;
        public static int END_DOCUMENT = 1;
        public static int START_TAG = 2;
        public static int END_TAG = 3;
        public static int TEXT = 4;
        public static int CDSECT = 5;
        public static int ENTITY_REF = 6;
        public static int IGNORABLE_WHITESPACE = 7;
        public static int PROCESSING_INSTRUCTION = 8;
        public static int COMMENT = 9;
        public static int DODECL = 10;
        public static int XML_DECLARATION = 11;
        public static int END_COMMENT = 12;

        public static int NodeTypeToToken(XmlNodeType nodeType)
        {
            int type;

            switch (nodeType)
            {
                case XmlNodeType.Document:
                    type = XmlPullParser.START_DOCUMENT;
                    break;
                case XmlNodeType.Element:
                    type = XmlPullParser.START_TAG;
                    break;
                case XmlNodeType.EndElement:
                    type = XmlPullParser.END_TAG;
                    break;
                case XmlNodeType.Text:
                    type = XmlPullParser.TEXT;
                    break;
                case XmlNodeType.CDATA:
                    type = XmlPullParser.CDSECT;
                    break;
                case XmlNodeType.EntityReference:
                    type = XmlPullParser.ENTITY_REF;
                    break;
                case XmlNodeType.Whitespace:
                    type = XmlPullParser.IGNORABLE_WHITESPACE;
                    break;
                case XmlNodeType.SignificantWhitespace:
                    type = XmlPullParser.IGNORABLE_WHITESPACE;
                    break;
                case XmlNodeType.ProcessingInstruction:
                    type = XmlPullParser.PROCESSING_INSTRUCTION;
                    break;
                case XmlNodeType.Comment:
                    type = XmlPullParser.COMMENT;
                    break;
                case XmlNodeType.DocumentType:
                    type = XmlPullParser.DODECL;
                    break;
                case XmlNodeType.XmlDeclaration:
                    type = XmlPullParser.XML_DECLARATION;
                    break;
                default:
                    throw new XmlPullParserException("Unused XML tag");
            }

            return type;
        }
    }
}
