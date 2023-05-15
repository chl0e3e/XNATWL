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
using System.Linq;
using System.Xml;
using XNATWL.IO;
using XNATWL.Utils;

namespace XNATWL
{
    public class InputMap
    {

        private static InputMap EMPTY_MAP = new InputMap(new KeyStroke[0]);

        private KeyStroke[] keyStrokes;

        public InputMap(KeyStroke[] keyStrokes)
        {
            this.keyStrokes = keyStrokes;
        }

        /**
         * Maps the given key event to an action.
         * @param event the key event
         * @return the action or null if no mapping was found
         */
        public String mapEvent(Event @event)
        {
            if (@event.isKeyEvent())
            {
                int mappedEventModifiers = KeyStroke.convertModifier(@event);
                foreach (KeyStroke ks in keyStrokes)
                {
                    if (ks.match(@event, mappedEventModifiers))
                    {
                        return ks.getAction();
                    }
                }
            }
            return null;
        }

        /**
         * Creates a new InputMap containing both the current and the new KeyStrokes.
         * If the new key strokes contain already mapped key strokes then the new mappings will replace the old mappings.
         *
         * @param newKeyStrokes the new key strokes.
         * @return the InputMap containing the resulting mapping
         */
        public InputMap addKeyStrokes(LinkedHashSet<KeyStroke> newKeyStrokes)
        {
            int size = newKeyStrokes.Count;
            if (size == 0)
            {
                return this;
            }

            KeyStroke[] combined = new KeyStroke[keyStrokes.Length + size];
            int i = 0;
            foreach(KeyStroke ks in newKeyStrokes)
            {
                combined[i] = ks;
                i++;
            }   // copy new key strokes
            foreach (KeyStroke ks in keyStrokes)
            {
                if (!newKeyStrokes.Contains(ks))
                {  // append old ones if they have not been replaced
                    combined[i] = ks;
                    i++;
                }
            }

            return new InputMap(shrink(combined, size));
        }

        /**
         * Creates a new InputMap containing both the current and the new KeyStrokes from another InputMap.
         * If the new key strokes contain already mapped key strokes then the new mappings will replace the old mappings.
         *
         * @param map the other InputMap containing the new key strokes.
         * @return the InputMap containing the resulting mapping
         */
        public InputMap addKeyStrokes(InputMap map)
        {
            if (map == this || map.keyStrokes.Length == 0)
            {
                return this;
            }
            if (keyStrokes.Length == 0)
            {
                return map;
            }
            return addKeyStrokes(new LinkedHashSet<KeyStroke>(map.keyStrokes.ToList()));
        }

        /**
         *
         * Creates a new InputMap containing both the current and the new KeyStroke.
         * If the specified key stroke is already mapped then the new mapping will replace the old mapping.
         *
         * @param keyStroke the new key stroke.
         * @return the InputMap containing the resulting mapping
         */
        public InputMap addKeyStroke(KeyStroke keyStroke)
        {
            LinkedHashSet<KeyStroke> newKeyStrokes = new LinkedHashSet<KeyStroke>(1);
            newKeyStrokes.Add(keyStroke);
            return addKeyStrokes(newKeyStrokes);
        }

        /**
         * Remove key strokes from this mapping
         *
         * @param keyStrokes the key strokes to remove
         * @return the InputMap containing the resulting mapping
         */
        public InputMap removeKeyStrokes(HashSet<KeyStroke> keyStrokes)
        {
            if (keyStrokes.Count == 0)
            {
                return this;
            }

            int size = 0;
            KeyStroke[] result = new KeyStroke[this.keyStrokes.Length];
            foreach (KeyStroke ks in this.keyStrokes)
            {
                if (!keyStrokes.Contains(ks))
                {  // append old ones if it has not been removed
                    result[size++] = ks;
                }
            }

            return new InputMap(shrink(result, size));
        }

        /**
         * Returns all key strokes in this InputMap.
         * @return all key strokes in this InputMap.
         */
        public KeyStroke[] getKeyStrokes()
        {
            return (KeyStroke[]) keyStrokes.Clone();
        }

        /**
         * Returns an empty input mapping
         * @return an empty input mapping
         */
        public static InputMap empty()
        {
            return EMPTY_MAP;
        }

        /**
         * Parses a stand alone &lt;inputMapDef&gt; XML file
         *
         * @param url the URL ton the XML file
         * @return the parsed key strokes
         * @throws IOException if an IO related error occured
         */
        public static InputMap parse(FileSystemObject url)
        {
            try
            {
                XMLParser xmlp = new XMLParser(url);
                try
                {
                    xmlp.Require(XmlPullParser.START_DOCUMENT, null, null);
                    xmlp.NextTag();
                    xmlp.Require(XmlPullParser.START_TAG, null, "inputMapDef");
                    xmlp.NextTag();
                    LinkedHashSet<KeyStroke> keyStrokes = parseBody(xmlp);
                    xmlp.Require(XmlPullParser.END_TAG, null, "inputMapDef");
                    return new InputMap(keyStrokes.ToArray());
                }
                finally
                {
                    xmlp.Close();
                }
            }
            catch (XmlPullParserException ex)
            {
                throw (IOException)(new IOException("Can't parse XML", ex));
            }
        }

        /**
         * Writes this input map into a XML file which can be parsed by {@link #parse(java.net.URL)}.
         * The encoding is UTF8
         *
         * @param os the output where the XML will be written to
         * @throws IOException if an IO error occured
         * @see #parse(java.net.URL) 
         */
        public void writeXML(Stream os)
        {
            try
            {
                /*XmlPullParserFactory factory = XmlPullParserFactory.newInstance();
                XmlSerializer serializer = factory.newSerializer();
                serializer.setOutput(os, "UTF8");
                serializer.startDocument("UTF8", Boolean.TRUE);
                serializer.text("\n");
                serializer.startTag(null, "inputMapDef");
                foreach (KeyStroke ks in keyStrokes)
                {
                    serializer.text("\n    ");
                    serializer.startTag(null, "action");
                    serializer.attribute(null, "name", ks.getAction());
                    serializer.text(ks.getStroke());
                    serializer.endTag(null, "action");
                }
                serializer.text("\n");
                serializer.endTag(null, "inputMapDef");
                serializer.endDocument();*/
                XmlDocument xmlDocument = new XmlDocument();
                var inputMapDefNode = xmlDocument.CreateNode(XmlNodeType.Element, "inputMapDef", "");
                foreach (KeyStroke ks in keyStrokes)
                {
                    var keystrokeNode = xmlDocument.CreateNode(XmlNodeType.Element, "action", "");
                    var keystrokeNameAttribute = xmlDocument.CreateAttribute("name");
                    keystrokeNameAttribute.Value = ks.getAction();
                    keystrokeNode.Attributes.Append(keystrokeNameAttribute);
                    keystrokeNode.InnerText = ks.getStroke();
                    inputMapDefNode.AppendChild(keystrokeNode);
                }
                xmlDocument.Save(os);
            }
            catch (XmlPullParserException ex)
            {
                throw (IOException)(new IOException("Can't generate XML", ex));
            }
        }

        /**
         * Parses the child elemets of the current XML tag as input map.
         * This method is only public so that it can be called from ThemeManager.
         *
         * @param xmlp the XML parser
         * @return the found key strokes
         * @throws XmlPullParserException if a parser error occured
         * @throws IOException if an IO error occured
         */
        public static LinkedHashSet<KeyStroke> parseBody(XMLParser xmlp)
        {
            LinkedHashSet<KeyStroke> newStrokes = new LinkedHashSet<KeyStroke>();
            while (!xmlp.IsEndTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, "action");
                String name = xmlp.GetAttributeNotNull("name");
                String key = xmlp.NextText();
                try
                {
                    KeyStroke ks = KeyStroke.parse(key, name);
                    if (!newStrokes.Add(ks))
                    {
                        System.Diagnostics.Debug.WriteLine("InputMap: Duplicate key stroke '" + ks.getStroke() + "'");
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw xmlp.Error("can't parse Keystroke", ex);
                }
                xmlp.Require(XmlPullParser.END_TAG, null, "action");
                xmlp.NextTag();
            }
            return newStrokes;
        }

        private static KeyStroke[] shrink(KeyStroke[] keyStrokes, int size)
        {
            if (size != keyStrokes.Length)
            {
                KeyStroke[] tmp = new KeyStroke[size];
                Array.Copy(keyStrokes, 0, tmp, 0, size);
                keyStrokes = tmp;
            }
            return keyStrokes;
        }
    }
}
