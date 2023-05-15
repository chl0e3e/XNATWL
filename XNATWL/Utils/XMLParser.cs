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

        private XmlTextReader _xmlTextReader;
        internal XmlReader _xpp;
        private bool _reachedDocumentEnd;
        private string _source;
        private Stream _stream;
        private BitSet _unusedAttributes = new BitSet();
        private string _loggerName = typeof(XMLParser).Name;

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
            this._source = file.Name;

            this._stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(file.Path)));
            this._xmlTextReader = new XmlTextReader(_stream);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
            this._xpp = XmlReader.Create(_xmlTextReader, xmlReaderSettings);
        }

        public XMLParser(Stream stream)
        {
            this._source = "Stream";

            this._stream = stream;
            this._xmlTextReader = new XmlTextReader(stream);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
            this._xpp = XmlReader.Create(_xmlTextReader, xmlReaderSettings);
        }

        public void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
            }
        }

        public void SetLoggerName(string loggerName)
        {
            this._loggerName = loggerName;
        }

        bool _isElementParsing = false;
        bool _justSeenEndTag = false;

        /**
         * @see XmlPullParser#next() 
         */
        public int Next()
        {
            WarnUnusedAttributes();
            int type;

            if (_isElementParsing)
            {
                _isElementParsing = false;
                _justSeenEndTag = true;
                return XmlPullParser.END_TAG;
            }

            if (_justSeenEndTag)
            {
                _justSeenEndTag = false;
            }

            bool read = _xpp.Read();

            while ((_xpp.NodeType == XmlNodeType.Attribute || _xpp.NodeType == XmlNodeType.Comment || _xpp.NodeType == XmlNodeType.Whitespace) && _xpp.Read())
            {
            }

            if (_xpp.NodeType == XmlNodeType.Element && _xpp.IsEmptyElement)
            {
                _isElementParsing = true;
            }

            if (read)
            {
                type = XmlPullParser.NodeTypeToToken(_xpp.NodeType);
            }
            else
            {
                if (!_reachedDocumentEnd)
                {
                    _reachedDocumentEnd = true;
                    type = XmlPullParser.END_DOCUMENT;
                }
                else
                {
                    throw new XmlPullParserException("XPP failure read after document end");
                }
            }

            HandleType(type);
            return type;
        }

        /**
         * @see XmlPullParser#nextTag() 
         */
        public int NextTag()
        {
            //warnUnusedAttributes();
            int type = this.Next();

            while (type == XmlPullParser.IGNORABLE_WHITESPACE || type == XmlPullParser.COMMENT)
            {
                type = this.Next();
            }

            if (type != XmlPullParser.START_TAG && type != XmlPullParser.END_TAG)
            {
                throw new XmlPullParserException("expected start or end tag, got " + this._xpp.NodeType, this);
            }

            HandleType(type);
            return type;
        }

        /**
         * @see XmlPullParser#nextText()
         */
        public string NextText()
        {
            if (_xpp.NodeType != XmlNodeType.Element)
            {
                throw new XmlPullParserException("parser must be on START_TAG to read next text");
            }
            int eventType = Next();
            //warnUnusedAttributes();
            if (eventType == XmlPullParser.TEXT)
            {
                string result = _xpp.Value;
                eventType = Next();
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

        public void SkipText()
        {
            int token = XmlPullParser.NodeTypeToToken(this._xpp.NodeType);
            while (token == XmlPullParser.TEXT || token == XmlPullParser.ENTITY_REF || token == XmlPullParser.COMMENT)
            {
                _xpp.Skip();
            }
        }

        public bool IsStartTag()
        {
            if (_xpp.NodeType == XmlNodeType.Element && _justSeenEndTag)
            {
                return false;
            }
            return _xpp.NodeType == XmlNodeType.Element;
        }

        public bool IsEndTag()
        {
            if (_xpp.NodeType == XmlNodeType.Element && _justSeenEndTag)
            {
                return true;
            }
            return _xpp.NodeType == XmlNodeType.EndElement;
        }

        public string GetPositionDescription()
        {
            string desc = "Line: " + this._xmlTextReader.LineNumber + ", Position: " + this._xmlTextReader.LinePosition;
            if (_source != null)
            {
                return desc + " in " + _source;
            }
            return desc;
        }

        public int GetLineNumber()
        {
            return this._xmlTextReader.LineNumber;
        }

        public int GetColumnNumber()
        {
            return this._xmlTextReader.LinePosition;
        }

        public string GetFilePosition()
        {
            if (_source != null)
            {
                return _source + ":" + GetLineNumber();
            }

            return this.GetPositionDescription();
        }

        public string GetName()
        {
            return this._xpp.Name;
        }

        public bool IsEmptyElement()
        {
            return this._xpp.IsEmptyElement;
        }

        /**
         * @see XmlPullParser#require(int, java.lang.string, java.lang.string) 
         */
        public void Require(int type, string @namespace, string name)
        {
            if (_justSeenEndTag && type == XmlPullParser.END_TAG)
            {
                if (name != null && name != _xpp.Name)
                {
                    throw new XmlPullParserException("token name mismatch", this);
                }
                return;
            }
            //xpp.require(type, @namespace, name);
            switch(type)
            {
                case 0:
                    if(this._xpp.NodeType != XmlNodeType.Document)
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
                    if (this._xpp.NodeType != XmlNodeType.Element)
                    {
                        throw new XmlPullParserException("token is not START_TAG", this);
                    }
                    if (name != null && name != _xpp.Name)
                    {
                        throw new XmlPullParserException("token name mismatch", this);
                    }
                    break;
                case 3:
                    if (this._xpp.NodeType != XmlNodeType.EndElement)
                    {
                        throw new XmlPullParserException("token is not END_TAG", this);
                    }
                    if (name != null && name != _xpp.Name)
                    {
                        throw new XmlPullParserException("token name mismatch", this);
                    }
                    break;
                case 4:
                    if (this._xpp.NodeType != XmlNodeType.Text)
                    {
                        throw new XmlPullParserException("token is not TEXT", this);
                    }
                    break;
                case 5:
                    if (this._xpp.NodeType != XmlNodeType.CDATA)
                    {
                        throw new XmlPullParserException("token is not CDATA", this);
                    }
                    break;
                case 6:
                    if (this._xpp.NodeType != XmlNodeType.EntityReference)
                    {
                        throw new XmlPullParserException("token is not ENTITY_REF", this);
                    }
                    break;
                case 7:
                    if (this._xpp.NodeType != XmlNodeType.Whitespace && this._xpp.NodeType != XmlNodeType.SignificantWhitespace)
                    {
                        throw new XmlPullParserException("token is not IGNORABLE_WHITESPACE", this);
                    }
                    break;
                case 8:
                    if (this._xpp.NodeType != XmlNodeType.ProcessingInstruction)
                    {
                        throw new XmlPullParserException("token is not PROCESSING_INSTRUCTION", this);
                    }
                    break;
                case 9:
                    if (this._xpp.NodeType != XmlNodeType.Comment)
                    {
                        throw new XmlPullParserException("token is not COMMENT", this);
                    }
                    break;
                case 10:
                    if (this._xpp.NodeType != XmlNodeType.DocumentType)
                    {
                        throw new XmlPullParserException("token is not DODECL", this);
                    }
                    break;
            }
        }

        public string GetAttributeValue(int index)
        {
            _unusedAttributes.Clear(index);
            return _xpp.GetAttribute(index);
        }

        public string GetAttributeNamespace(int index)
        {
            this._xpp.MoveToAttribute(index);
            string namespaceUri = _xpp.NamespaceURI;
            this._xpp.MoveToElement();
            return namespaceUri;
        }

        public string GetAttributeName(int index)
        {
            this._xpp.MoveToAttribute(index);
            string name = this._xpp.Name;
            this._xpp.MoveToElement();
            return name;
        }

        public int GetAttributeCount()
        {
            return this._xpp.AttributeCount;
        }

        public string GetAttributeValue(string @namespace, string name)
        {
            for (int i = 0, n = _xpp.AttributeCount; i < n; i++)
            {
                this._xpp.MoveToAttribute(i);
                if ((this._xpp.NamespaceURI == "" || @namespace.Equals(_xpp.NamespaceURI)) &&
                        name.Equals(_xpp.Name))
                {
                    var attrValue = GetAttributeValue(i);
                    this._xpp.MoveToElement();
                    return attrValue;
                }
            }

            this._xpp.MoveToElement();
            return null;
        }


        public string GetAttributeNotNull(string attribute)
        {
            string value = GetAttributeValue(null, attribute);
            this._xpp.MoveToElement();
            if (value == null)
            {
                MissingAttribute(attribute);
            }
            return value;
        }

        public bool ParseBoolFromAttribute(string attribName)
        {
            return ParseBool(GetAttributeNotNull(attribName));
        }

        public bool ParseBoolFromText()
        {
            return ParseBool(NextText());
        }

        public bool ParseBoolFromAttribute(string attribName, bool defaultValue)
        {
            string value = GetAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return ParseBool(value);
        }

        public int ParseIntFromAttribute(string attribName)
        {
            return ParseInt(GetAttributeNotNull(attribName));
        }

        public int ParseIntFromAttribute(string attribName, int defaultValue)
        {
            string value = GetAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return ParseInt(value);
        }

        public float ParseFloatFromAttribute(string attribName)
        {
            return ParseFloat(GetAttributeNotNull(attribName));
        }

        public float ParseFloatFromAttribute(string attribName, float defaultValue)
        {
            string value = GetAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return ParseFloat(value);
        }

        public E ParseEnumFromAttribute<E>(string attribName, Type enumClazz) where E : struct, IConvertible
        {
            return ParseEnum<E>(enumClazz, GetAttributeNotNull(attribName));
        }

        public E ParseEnumFromAttribute<E>(string attribName, Type enumClazz, E defaultValue) where E : struct, IConvertible
        {
            string value = GetAttributeValue(null, attribName);
            if (value == null)
            {
                return defaultValue;
            }
            return ParseEnum<E>(enumClazz, value);
        }

        public E ParseEnumFromText<E>(Type enumClazz) where E : struct, IConvertible
        {
            return ParseEnum<E>(enumClazz, NextText());
        }

        public object ParseEnumFromText(Type enumClazz)
        {
            return ParseEnum(enumClazz, NextText());
        }

        public Dictionary<string, string> GetUnusedAttributes()
        {
            if (_unusedAttributes.IsEmpty())
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = -1; (i = _unusedAttributes.NextSetBit(i + 1)) >= 0;)
            {
                result.Add(this.GetAttributeName(i), this.GetAttributeValue(i));
            }

            _unusedAttributes.Clear();

            return result;
        }

        public void IgnoreOtherAttributes()
        {
            _unusedAttributes.Clear();
        }

        public bool IsAttributeUnused(int idx)
        {
            return _unusedAttributes.Get(idx);
        }

        public XmlPullParserException Error(string msg)
        {
            return new XmlPullParserException(msg, this, null);
        }

        public XmlPullParserException Error(string msg, Exception cause)
        {
            return (XmlPullParserException)new XmlPullParserException(msg, this, cause);
        }

        public XmlPullParserException Unexpected()
        {
            return new XmlPullParserException("Unexpected '" + _xpp.Name + "'", this, null);
        }

        protected E ParseEnum<E>(Type enumClazz, string value) where E : struct, IConvertible
        {
            if (!enumClazz.IsEnum)
            {
                throw Error("enum class provided reflects that it is not an enum");
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

        protected Object ParseEnum(Type enumClazz, string value)
        {
            if (!enumClazz.IsEnum)
            {
                throw Error("enum class provided reflects that it is not an enum");
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


        public bool ParseBool(string value)
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

        protected int ParseInt(string value)
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

        protected float ParseFloat(string value)
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

        protected void MissingAttribute(string attribute)
        {
            throw new XmlPullParserException("missing '" + attribute + "' on '" + _xpp.Name + "'", this, null);
        }

        protected void HandleType(int type)
        {
            _unusedAttributes.Clear();
            switch (type)
            {
                case 2:
                    _unusedAttributes.Set(0, _xpp.AttributeCount);
                    break;
            }
        }

        protected void WarnUnusedAttributes()
        {
            if (!_unusedAttributes.IsEmpty())
            {
                string positionDescription = GetPositionDescription();
                for (int i = -1; (i = _unusedAttributes.NextSetBit(i + 1)) >= 0;)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Unused attribute ''{0}'' on ''{1}'' at {2}", new Object[] { _xpp.GetAttribute(i), _xpp.Name, positionDescription }));
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

        public XmlPullParserException(string message, object parser) : base(((XMLParser)parser).GetPositionDescription() + "   " + message)
        {
            this.Parser = parser;
            this.LineNumber = ((XMLParser)parser).GetLineNumber();
            this.LinePosition = ((XMLParser)parser).GetColumnNumber();
        }

        public XmlPullParserException(string message, object parser, Exception chain) : base(((XMLParser)parser).GetPositionDescription() + "   " + message, chain)
        {
            this.Parser = parser;
            this.LineNumber = ((XMLParser)parser).GetLineNumber();
            this.LinePosition = ((XMLParser)parser).GetColumnNumber();
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
