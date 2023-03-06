using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using XNATWL.IO;
using Microsoft.Xna.Framework.Input;

namespace XNATWL.Input.XNA
{
    public class KeyboardLayout
    {
        private XmlDocument _document;
        private Dictionary<Keys, KeyInfo> _keyMap = new Dictionary<Keys, KeyInfo>();
        private KeyInfo _emptyKeyInfo;

        public class KeyboardLayoutParseException : Exception
        {

        }

        public KeyboardLayout(FileSystemObject keyboardLayoutXML)
        {
            Stream fileStream = keyboardLayoutXML.OpenRead();
            this._Read(fileStream);
            fileStream.Close();

            this._emptyKeyInfo = new KeyInfo();
            this._emptyKeyInfo.TWL = Event.KEY_NONE;
            this._emptyKeyInfo.Char = '\0';
            this._emptyKeyInfo.ShiftChar = '\0';
        }

        private void _Read(Stream stream)
        {
            this._document = new XmlDocument();
            this._document.PreserveWhitespace = true;
            this._document.Load(stream);

            XmlNode kbLayoutNode = null;
            foreach (XmlNode xmlNode in this._document.ChildNodes)
            {
                if (xmlNode.Name == "KeyboardLayout")
                {
                    kbLayoutNode = xmlNode;
                    break;
                }
            }

            if (kbLayoutNode != null)
            {
                FieldInfo[] fields = typeof(Event).GetFields();
                Dictionary<string, int> twlKeycodeMap = new Dictionary<string, int>();
                
                foreach(FieldInfo field in fields)
                {
                    if (field.Name.StartsWith("KEY_"))
                    {
                        twlKeycodeMap.Add(field.Name, (int) field.GetValue(null));
                    }
                }

                foreach (XmlNode keyNode in kbLayoutNode.ChildNodes)
                {
                    if (keyNode.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }

                    Keys key = (Keys)Enum.Parse(typeof(Keys), keyNode.Name);
                    KeyInfo keyInfo = new KeyInfo();

                    foreach (XmlNode keyInfoNode in keyNode.ChildNodes)
                    {
                        if (keyInfoNode.NodeType == XmlNodeType.Whitespace)
                        {
                            continue;
                        }

                        if (keyInfoNode.Name == "TWL")
                        {
                            keyInfo.TWL = twlKeycodeMap[keyInfoNode.InnerText];
                        }
                        else if (keyInfoNode.Name == "Char")
                        {
                            keyInfo.Char = keyInfoNode.InnerText[0];
                        }
                        else if (keyInfoNode.Name == "ShiftChar")
                        {
                            keyInfo.ShiftChar = keyInfoNode.InnerText[0];
                        }
                        else
                        {
                            throw new KeyboardLayoutParseException();
                        }
                    }

                    if (key == Keys.Enter)
                    {
                        keyInfo.Char = '\n';
                        keyInfo.ShiftChar = '\n';
                    }

                    this._keyMap.Add(key, keyInfo);
                }
            }
        }

        public KeyInfo KeyInfoFor(Keys key)
        {
            if (!this._keyMap.ContainsKey(key))
            {
                return this._emptyKeyInfo;
            }
            return this._keyMap[key];
        }
    }
}
