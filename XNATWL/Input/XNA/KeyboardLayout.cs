using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using XNATWL.IO;
using Microsoft.Xna.Framework.Input;

namespace XNATWL.Input.XNA
{
    /// <summary>
    /// Loads a keyboard layout from an XML file
    /// </summary>
    public class KeyboardLayout
    {
        private XmlDocument _document;
        private Dictionary<Keys, KeyInfo> _keyMap = new Dictionary<Keys, KeyInfo>();
        private KeyInfo _emptyKeyInfo;
        private Dictionary<string, int> _twlKeycodeMap = new Dictionary<string, int>();

        /// <summary>
        /// Loads a keyboard layout from an XML file system object
        /// </summary>
        /// <param name="keyboardLayoutXML">object used for locating keyboard layout XML</param>
        public KeyboardLayout(FileSystemObject keyboardLayoutXML)
        {
            Stream fileStream = keyboardLayoutXML.OpenRead();
            this.Read(fileStream);
            fileStream.Close();

            this._emptyKeyInfo = new KeyInfo();
            this._emptyKeyInfo.TWL = Event.KEY_NONE;
            this._emptyKeyInfo.Char = '\0';
            this._emptyKeyInfo.ShiftChar = '\0';

            this.CopyTWLKeycodeMap();
        }

        /// <summary>
        /// Use reflection to obtain a list of key codes used internally by TWL
        /// </summary>
        private void CopyTWLKeycodeMap()
        {
            // reflect the fields out of XNATWL.Event
            FieldInfo[] fields = typeof(Event).GetFields();

            // iterate over the fields, adding those starting with "KEY_" to the _twlKeycodeMap
            foreach (FieldInfo field in fields)
            {
                if (field.Name.StartsWith("KEY_"))
                {
                    this._twlKeycodeMap.Add(field.Name, (int)field.GetValue(null));
                }
            }
        }

        /// <summary>
        /// Read the keyboard layout from a stream which is used to read XML
        /// </summary>
        /// <param name="stream">A stream buffering the XML of the layout</param>
        /// <exception cref="KeyboardLayoutParseException">Exception describing why parsing the keyboard layout failed</exception>
        private void Read(Stream stream)
        {
            this._document = new XmlDocument();
            this._document.PreserveWhitespace = true;
            this._document.Load(stream);

            // look for a 'KeyboardLayout' XML tag in the root of the document
            XmlNode kbLayoutNode = null;
            foreach (XmlNode xmlNode in this._document.ChildNodes)
            {
                if (xmlNode.Name == "KeyboardLayout")
                {
                    kbLayoutNode = xmlNode;
                    break;
                }
            }

            // if there is no 'KeyboardLayout' tag then throw an exception
            if (kbLayoutNode == null)
            {
                throw new KeyboardLayoutParseException("No KeyboardLayout tag in the root of the XML document'");
            }

            // loop through the KeyboardLayout
            // each key is expressed as a tag name of an xml node
            foreach (XmlNode keyNode in kbLayoutNode.ChildNodes)
            {
                // skip any unnecessary whitespace
                if (keyNode.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

                // get the XNA name of the key
                Keys key = (Keys)Enum.Parse(typeof(Keys), keyNode.Name);
                // create a KeyInfo struct for the key behaviour
                KeyInfo keyInfo = new KeyInfo();

                // loop through the key's corresponding information nodes
                foreach (XmlNode keyInfoNode in keyNode.ChildNodes)
                {
                    if (keyInfoNode.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }

                    // TWL event code
                    if (String.Equals(keyInfoNode.Name, "TWL", StringComparison.OrdinalIgnoreCase))
                    {
                        keyInfo.TWL = _twlKeycodeMap[keyInfoNode.InnerText];
                    }
                    // text input character
                    else if (String.Equals(keyInfoNode.Name, "Char", StringComparison.OrdinalIgnoreCase))
                    {
                        keyInfo.Char = keyInfoNode.InnerText[0];
                    }
                    // text input character when Shift is held
                    else if (String.Equals(keyInfoNode.Name, "ShiftChar", StringComparison.OrdinalIgnoreCase))
                    {
                        keyInfo.ShiftChar = keyInfoNode.InnerText[0];
                    }
                    // unknown info node
                    else
                    {
                        throw new KeyboardLayoutParseException("An unrecognised XML node was found '" + keyInfoNode.Name + "' expected 'TWL', 'Char' or 'ShiftChar'");
                    }
                }

                // Enter always represents a new line for multi-line fields
                if (key == Keys.Enter)
                {
                    keyInfo.Char = '\n';
                    keyInfo.ShiftChar = '\n';
                }

                // finally, add the XNA key with its info into the map
                this._keyMap.Add(key, keyInfo);
            }
        }

        /// <summary>
        /// Return XNATWL struct for the corresponding XNA Keys enum value 
        /// </summary>
        /// <param name="key">XNA Keys enum value</param>
        /// <returns>KeyInfo representing key's behaviour</returns>
        public KeyInfo KeyInfoFor(Keys key)
        {
            if (!this._keyMap.ContainsKey(key))
            {
                // gracefully return an empty key info
                return this._emptyKeyInfo;
            }

            // lookup in the populated key map
            return this._keyMap[key];
        }
    }

    /// <summary>
    /// Thrown when there was an error parsing the XML for a keyboard layout
    /// </summary>
    public class KeyboardLayoutParseException : Exception
    {
        public KeyboardLayoutParseException(string message) : base(message)
        {

        }
    }

}
