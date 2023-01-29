using System;
using System.Collections.Generic;
using System.Xml;

namespace XNATWL.IO
{
    public class Preferences
    {
        private Dictionary<string, object> _preferences;
        private string _path;

        public Preferences(string path)
        {
            this._preferences = new Dictionary<string, object>();
            this._path = path;
        }

        public void Read()
        {
            XmlDocument document = new XmlDocument();
            document.Load(this._path);

            XmlNodeList preferences = document.GetElementsByTagName("Preference");
            foreach (XmlNode node in preferences)
            {
                string key = node.Attributes["Key"].InnerText;
                string type = node.Attributes["Type"].InnerText;

                if (type.ToLower() == "int32")
                {
                    _preferences.Add(key, Int32.Parse(node.InnerText));
                }
                else if (type.ToLower() == "float")
                {
                    _preferences.Add(key, float.Parse(node.InnerText));
                }
                else if (type.ToLower() == "long")
                {
                    _preferences.Add(key, long.Parse(node.InnerText));
                }
                else if (type.ToLower() == "string")
                {
                    _preferences.Add(key, node.InnerText);
                }
                else if (type.ToLower() == "color")
                {
                    _preferences.Add(key, Color.Parse(node.InnerText));
                }
                else
                {
                    throw new Exception("Unknown Preferences type: " + type);
                }
            }
        }

        public void Write()
        {
            XmlDocument output = new XmlDocument();

            foreach (string preference in _preferences.Keys)
            {
                var preferenceValue = _preferences[preference];

                var xmlNode = output.CreateNode(XmlNodeType.Element, "Preference", "");

                var xmlKeyAttribute = output.CreateAttribute("Key");
                xmlKeyAttribute.InnerText = preference;
                xmlNode.Attributes.Append(xmlKeyAttribute);

                var xmlTypeAttribute = output.CreateAttribute("Type");
                xmlTypeAttribute.InnerText = preferenceValue.GetType().Name;
                xmlNode.Attributes.Append(xmlTypeAttribute);

                xmlNode.InnerText = preferenceValue.ToString();

                output.AppendChild(xmlNode);
            }

            System.IO.File.WriteAllText(this._path, output.ToString());
        }

        public void Set(string preferenceKey, object value)
        {
            if (this._preferences.ContainsKey(preferenceKey))
            {
                this._preferences[preferenceKey] = value;
            }
            else
            {
                this._preferences.Add(preferenceKey, value);
            }
        }

        public object Get(string preferenceKey, object defaultValue)
        {
            if (!this._preferences.ContainsKey(preferenceKey))
            {
                return defaultValue;
            }

            return this._preferences[preferenceKey];
        }

        public object Get(string preferenceKey)
        {
            return this._preferences[preferenceKey];
        }
    }
}
