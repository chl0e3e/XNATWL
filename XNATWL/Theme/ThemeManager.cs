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
using System.Data;
using System.IO;
using XNATWL.IO;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    /// <summary>
    /// Used for representing a Theme and managing it's usage
    /// </summary>
    public class ThemeManager
    {
        private static Dictionary<string, Type> enums = new Dictionary<string, Type>();

        static ThemeManager()
        {
            RegisterEnumType<PositionAnimatedPanel.eDirection>("direction", typeof(PositionAnimatedPanel.eDirection));
        }

        static Object NULL = new Object();

        ParameterMapImpl _constants;
        private Renderer.Renderer _renderer;
        private CacheContext _cacheContext;
        private ImageManager _imageManager;
        private Dictionary<string, Font> _fonts;
        private Dictionary<string, ThemeInfoImpl> _themes;
        private Dictionary<string, InputMap> _inputMaps;
        private MathInterpreter _mathInterpreter;
        private Font _defaultFont;
        private Font _firstFont;

        internal ParameterMapImpl _emptyMap;
        internal ParameterListImpl _emptyList;

        private ThemeManager(Renderer.Renderer renderer, CacheContext cacheContext)
        {
            this._constants = new ParameterMapImpl(this, null);
            this._renderer = renderer;
            this._cacheContext = cacheContext;
            this._imageManager = new ImageManager(_constants, renderer);
            this._fonts = new Dictionary<string, Font>();
            this._themes = new Dictionary<string, ThemeInfoImpl>();
            this._inputMaps = new Dictionary<string, InputMap>();
            this._emptyMap = new ParameterMapImpl(this, null);
            this._emptyList = new ParameterListImpl(this, null);
            this._mathInterpreter = new MathInterpreter(this);
        }

        /// <summary>
        /// <para>Returns the associated cache context which was used to load this theme.</para>
        /// </summary>
        /// <returns>the cache context</returns>
        public CacheContext GetCacheContext()
        {
            return _cacheContext;
        }

        /// <summary>
        /// <para>Destroys the CacheContext and releases all OpenGL resources</para>
        /// </summary>
        public void Destroy()
        {
            foreach (Font font in _fonts.Values)
            {
                font.Dispose();
            }
            _cacheContext.Dispose();
        }

        /// <summary>
        /// Return the default font for the theme (sometimes used as a fallback)
        /// </summary>
        /// <returns>default <see cref="Font"/></returns>
        public Font GetDefaultFont()
        {
            return _defaultFont;
        }

        /// <summary>
        /// <para>Loads the specified theme using the provided renderer and a new cache context. </para>
        /// <para>This is equivalent to calling <c>CreateThemeManager(fso, renderer, renderer.CreateNewCacheContext())</c></para>
        /// </summary>
        /// <param name="fso">The FSO of the theme</param>
        /// <param name="renderer">The renderer which is used to load and render the resources</param>
        /// <returns>a new ThemeManager</returns>
        /// <exception cref="IOException">if an error occured while loading</exception>
        /// <exception cref="ArgumentNullException">if one of the passed parameters is {@code null}</exception>
        public static ThemeManager CreateThemeManager(FileSystemObject fso, Renderer.Renderer renderer)
        {
            if (fso == null)
            {
                throw new ArgumentNullException("fso is null");
            }
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer is null");
            }
            return CreateThemeManager(fso, renderer, renderer.CreateNewCacheContext());
        }

        /// <summary>
        /// <para>Loads the specified theme using the provided renderer and cache context. </para>
        /// <para>This is equivalent to calling {@code createThemeManager(url, renderer, renderer.createNewCacheContext(), null)} </para>
        /// </summary>
        /// <param name="fso">The FSO of the theme</param>
        /// <param name="renderer">The renderer which is used to load and render the resources</param>
        /// <param name="cacheContext">The cache context into which the resources are loaded</param>
        /// <returns>a new ThemeManager</returns>
        /// <exception cref="IOException">if an error occured while loading</exception>
        /// <exception cref="ArgumentNullException">if one of the passed parameters is {@code null}</exception>
        public static ThemeManager CreateThemeManager(FileSystemObject fso, Renderer.Renderer renderer, CacheContext cacheContext)
        {
            return CreateThemeManager(fso, renderer, cacheContext, null);
        }

        /// <summary>
        /// <para>Loads the specified theme using the provided renderer and cache context. </para>
        /// <para>The provided <paramref name="cacheContext"/> is set active in the provided {@code renderer}. </para>
        /// <para>The cache context is stored inside the created ThemeManager. Calling <c>Destroy()</c> on the returned ThemeManager instance will also destroy the cache context. </para>
        /// </summary>
        /// <param name="fso">The FSO of the theme</param>
        /// <param name="renderer">The renderer which is used to load and render the resources</param>
        /// <param name="cacheContext">The cache context into which the resources are loaded</param>
        /// <param name="constants">A map containing constants which as exposed to the theme</param>
        /// <returns>a new ThemeManager</returns>
        /// <exception cref="IOException">if an error occured while loading</exception>
        /// <exception cref="ArgumentNullException">if one of the passed parameters is <em>null</em></exception>
        public static ThemeManager CreateThemeManager(FileSystemObject fso, Renderer.Renderer renderer, CacheContext cacheContext, Dictionary<string, Object> constants)
        {
            if (fso == null)
            {
                throw new ArgumentNullException("url is null");
            }
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer is null");
            }
            if (cacheContext == null)
            {
                throw new ArgumentNullException("cacheContext is null");
            }
            try
            {
                renderer.SetActiveCacheContext(cacheContext);
                ThemeManager tm = new ThemeManager(renderer, cacheContext);
                tm.InsertDefaultConstants();
                if (constants != null && constants.Count != 0)
                {
                    tm.InsertConstants(constants);
                }
                tm.ParseThemeFile(fso);
                if (tm._defaultFont == null)
                {
                    tm._defaultFont = tm._firstFont;
                }
                return tm;
            }
            catch (XmlPullParserException ex)
            {
                throw (new IOException("createThemeManager", ex));
            }
        }

        /// <summary>
        /// Register an enum class with the type name
        /// </summary>
        /// <typeparam name="E">Enum type</typeparam>
        /// <param name="name">Type name</param>
        /// <param name="enumClazz">Enum type object</param>
        /// <exception cref="ArgumentNullException">if one of the passed parameters is not an enum</exception>
        /// <exception cref="DuplicateNameException">enums already exists under that name</exception>
        public static void RegisterEnumType<E>(string name, Type enumClazz) where E : struct, IConvertible
        {
            if (!enumClazz.IsEnum)
            {
                throw new ArgumentNullException("not an enum class");
            }
            if (enums.ContainsKey(name) && enums[name] != enumClazz)
            {
                throw new DuplicateNameException("Enum type name \"" + name + "\" is already in use by " + enums[name].Name);
            }
            enums.Add(name, enumClazz);
        }

        /// <summary>
        /// Find a ThemeInfo given a theme path, automatically warning and using fallbacks
        /// </summary>
        /// <param name="themePath">Theme path</param>
        /// <returns><see cref="ThemeInfo"/></returns>
        public ThemeInfo FindThemeInfo(string themePath)
        {
            return FindThemeInfo(themePath, true, true);
        }

        /// <summary>
        /// Find a ThemeInfo given a theme path, manually allowing warnings or fallbacks
        /// </summary>
        /// <param name="themePath">Theme path</param>
        /// <param name="warn">Send warning to debugger for missing theme</param>
        /// <param name="useFallback">Use theme fallbacks</param>
        /// <returns><see cref="ThemeInfo"/></returns>
        private ThemeInfo FindThemeInfo(string themePath, bool warn, bool useFallback)
        {
            int start = TextUtil.IndexOf(themePath, '.', 0);
            ThemeInfo info = null;
            string themeKey = themePath.Substring(0, start);
            if (_themes.ContainsKey(themeKey))
            {
                info = _themes[themeKey];
            }
            if (info == null)
            {
                themeKey = "*";
                if (_themes.ContainsKey(themeKey))
                {
                    info = _themes[themeKey];
                }
                else
                {
                    info = null;
                }

                if (info != null)
                {
                    if (!useFallback)
                    {
                        return null;
                    }
                    DebugHook.getDebugHook().UsingFallbackTheme(themePath);
                }
            }
            while (info != null && ++start < themePath.Length)
            {
                int next = TextUtil.IndexOf(themePath, '.', start);
                info = info.GetChildTheme(themePath.Substring(start, next - start));
                start = next;
            }
            if (info == null && warn)
            {
                DebugHook.getDebugHook().MissingTheme(themePath);
            }
            return info;
        }

        /// <summary>
        /// Get an image by name from the <see cref="ImageManager"/> without warning when the image was not found
        /// </summary>
        /// <param name="name">Image name</param>
        /// <returns>Matching <see cref="Image"/></returns>
        public Image GetImageNoWarning(string name)
        {
            return _imageManager[name];
        }

        /// <summary>
        /// Get an image by name from the <see cref="ImageManager"/> and warn the debugger when the image was not found
        /// </summary>
        /// <param name="name">Image name</param>
        /// <returns>Matching <see cref="Image"/></returns>
        public Image GetImage(string name)
        {
            Image img = _imageManager[name];
            if (img == null)
            {
                DebugHook.getDebugHook().MissingImage(name);
            }
            return img;
        }

        /// <summary>
        /// Get cursor by name from the <see cref="ImageManager"/>
        /// </summary>
        /// <param name="name">Name of cursor</param>
        /// <returns><see cref="MouseCursor"/></returns>
        public object GetCursor(string name)
        {
            return _imageManager.GetCursor(name);
        }

        /// <summary>
        /// Get font by name
        /// </summary>
        /// <param name="name">Name of font</param>
        /// <returns><see cref="Font"/></returns>
        public Font GetFont(string name)
        {
            return _fonts[name];
        }

        /// <summary>
        /// Get the <see cref="ParameterMap"/> for theme constants
        /// </summary>
        /// <returns><see cref="ParameterMap"/> holding constants</returns>
        public ParameterMap GetConstants()
        {
            return _constants;
        }

        /// <summary>
        /// Automatically insert these constants to the <see cref="_constants"/> map
        /// </summary>
        private void InsertDefaultConstants()
        {
            _constants.Put("SINGLE_COLUMN", -1);
            _constants.Put("MAX", short.MaxValue);
        }

        /// <summary>
        /// Merge constants map from given dictionary
        /// </summary>
        /// <param name="src">Merged constants</param>
        private void InsertConstants(Dictionary<string, Object> src)
        {
            _constants.Put(src);
        }

        /// <summary>
        /// Parse a theme XML file from the given <see cref="FileSystemObject"/> in the <paramref name="fso"/> parameter
        /// </summary>
        /// <param name="fso">File system object to read XML from</param>
        /// <exception cref="ThemeException">XML-specific exception</exception>
        private void ParseThemeFile(FileSystemObject fso)
        {
            try
            {
                XMLParser xmlp = new XMLParser(fso);
                try
                {
                    xmlp.SetLoggerName(typeof(ThemeManager).Name);
                    xmlp.Next();
                    xmlp.Require(XmlPullParser.XML_DECLARATION, null, null);
                    xmlp.Next();
                    ParseThemeFile(xmlp, fso.Parent);
                }
                finally
                {
                    xmlp.Close();
                }
            }
            catch (XmlPullParserException ex)
            {
                throw new ThemeException(ex.Message, fso, ex.LineNumber, ex.LinePosition, ex);
            }
            catch (ThemeException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw (IOException)(new IOException("while parsing Theme XML: " + fso, ex));
            }
        }

        /// <summary>
        /// Parse from the <see cref="XMLParser"/> beginning with a <c>themes</c> tag
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="baseFso">theme file object used for relative file reads</param>
        private void ParseThemeFile(XMLParser xmlp, FileSystemObject baseFso)
        {
            xmlp.Require(XmlPullParser.START_TAG, null, "themes");
            xmlp.NextTag();

            while (!xmlp.IsEndTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, null);
                string tagName = xmlp.GetName();
                if ("images".Equals(tagName) || "textures".Equals(tagName))
                {
                    _imageManager.ParseImages(xmlp, baseFso);
                }
                else if ("include".Equals(tagName))
                {
                    string fontFileName = xmlp.GetAttributeNotNull("filename");
                    try
                    {
                        ParseThemeFile(new FileSystemObject(baseFso, fontFileName));
                    }
                    catch (ThemeException ex)
                    {
                        ex.AddIncludedBy(baseFso, xmlp.GetLineNumber(), xmlp.GetColumnNumber());
                        throw ex;
                    }
                    xmlp.NextTag();
                }
                else
                {
                    string name = xmlp.GetAttributeNotNull("name");
                    if ("theme".Equals(tagName))
                    {
                        if (_themes.ContainsKey(name))
                        {
                            throw xmlp.Error("theme \"" + name + "\" already defined");
                        }
                        _themes.Add(name, ParseTheme(xmlp, name, null, baseFso));
                    }
                    else if ("inputMapDef".Equals(tagName))
                    {
                        if (_inputMaps.ContainsKey(name))
                        {
                            throw xmlp.Error("inputMap \"" + name + "\" already defined");
                        }
                        _inputMaps.Add(name, ParseInputMap(xmlp, name, null));
                    }
                    else if ("fontDef".Equals(tagName))
                    {
                        if (_fonts.ContainsKey(name))
                        {
                            throw xmlp.Error("font \"" + name + "\" already defined");
                        }
                        bool makeDefault = xmlp.ParseBoolFromAttribute("default", false);
                        Font font = ParseFont(xmlp, baseFso);
                        _fonts.Add(name, font);
                        if (_firstFont == null)
                        {
                            _firstFont = font;
                        }
                        if (makeDefault)
                        {
                            if (_defaultFont != null)
                            {
                                throw xmlp.Error("default font already set");
                            }
                            _defaultFont = font;
                        }
                    }
                    else if ("constantDef".Equals(tagName))
                    {
                        ParseParam(xmlp, baseFso, "constantDef", null, _constants);
                    }
                    else
                    {
                        throw xmlp.Unexpected();
                    }
                }
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                xmlp.NextTag();
            }
            xmlp.Require(XmlPullParser.END_TAG, null, "themes");
        }

        /// <summary>
        /// Fetch an input map allowing an error to be thrown by the theme <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="name">name of input map</param>
        /// <returns><see cref="InputMap"/></returns>
        private InputMap GetInputMap(XMLParser xmlp, string name)
        {
            if (!_inputMaps.ContainsKey(name))
            {
                throw xmlp.Error("Undefined input map: " + name);
            }

            return _inputMaps[name];
        }

        /// <summary>
        /// Parse an input map from the <see cref="XMLParser"/> following any references or parent THemeInfo objects
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="name">Name of the input map</param>
        /// <param name="parent">Parent theme info map</param>
        /// <returns><see cref="InputMap"/></returns>
        private InputMap ParseInputMap(XMLParser xmlp, string name, ThemeInfoImpl parent)
        {
            InputMap baseMap = InputMap.Empty();
            if (xmlp.ParseBoolFromAttribute("merge", false))
            {
                if (parent == null)
                {
                    throw xmlp.Error("Can't merge on top level");
                }
                Object o = parent[name];
                if (o is InputMap)
                {
                    baseMap = (InputMap)o;
                }
                else if (o != null)
                {
                    throw xmlp.Error("Can only merge with inputMap - found a " + o.GetType().FullName);
                }
            }
            string baseName = xmlp.GetAttributeValue(null, "ref");
            if (baseName != null)
            {
                baseMap = baseMap.AddKeyStrokes(GetInputMap(xmlp, baseName));
            }

            xmlp.NextTag();

            LinkedHashSet<KeyStroke> keyStrokes = InputMap.ParseBody(xmlp);
            InputMap im = baseMap.AddKeyStrokes(keyStrokes);
            return im;
        }

        /// <summary>
        /// Parse a font from the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="baseFso">Relative file system object</param>
        /// <returns>Parsed <see cref="Font"/></returns>
        private Font ParseFont(XMLParser xmlp, FileSystemObject baseFso)
        {
            FileSystemObject fso;
            string fileName = xmlp.GetAttributeValue(null, "filename");
            if (fileName != null)
            {
                fso = new FileSystemObject(baseFso, fileName);
            }
            else
            {
                fso = baseFso;
            }

            List<string> fontFamilies = ParseList(xmlp, "families");
            int fontSize = 0;
            int fontStyle = 0;
            if (fontFamilies != null)
            {
                fontSize = ParseMath(xmlp, xmlp.GetAttributeNotNull("size")).IntValue();
                List<string> styles = ParseList(xmlp, "style");
                foreach(string style in styles)
                {
                    if ("bold".Equals(style.ToLower()))
                    {
                        fontStyle |= FontMapperStatics.STYLE_BOLD;
                    }
                    else if ("italic".Equals(style.ToLower()))
                    {
                        fontStyle |= FontMapperStatics.STYLE_ITALIC;
                    }
                }
            }

            FontParameter baseParams = new FontParameter();
            ParseFontParameter(xmlp, baseParams);
            List<FontParameter> fontParams = new List<FontParameter>();
            List<StateExpression> stateExpr = new List<StateExpression>();

            xmlp.NextTag();
            while (!xmlp.IsEndTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, "fontParam");

                StateExpression cond = ParserUtil.ParseCondition(xmlp);
                if (cond == null)
                {
                    throw xmlp.Error("Condition required");
                }
                stateExpr.Add(cond);

                FontParameter parameters = new FontParameter(baseParams);
                ParseFontParameter(xmlp, parameters);
                fontParams.Add(parameters);

                xmlp.NextTag();
                xmlp.Require(XmlPullParser.END_TAG, null, "fontParam");
                xmlp.NextTag();
            }

            fontParams.Add(baseParams);
            StateSelect stateSelect = new StateSelect(stateExpr);
            FontParameter[] stateParams = fontParams.ToArray();

            if (fontFamilies != null)
            {
                FontMapper fontMapper = _renderer.FontMapper;
                if (fontMapper != null)
                {
                    Font font = fontMapper.GetFont(fontFamilies, fontSize, fontStyle, stateSelect, stateParams);
                    if (font != null)
                    {
                        return font;
                    }
                }
            }

            return _renderer.LoadFont(fso, stateSelect, stateParams);
        }

        /// <summary>
        /// Parse <see cref="FontParameter"/> from the current state of the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="fp"><see cref="FontParameter"/> to populate</param>
        private void ParseFontParameter(XMLParser xmlp, FontParameter fp)
        {
            for (int i = 0, n = xmlp.GetAttributeCount(); i < n; i++)
            {
                if (xmlp.IsAttributeUnused(i))
                {
                    string name = xmlp.GetAttributeName(i);
                    FontParameter.Parameter type = (FontParameter.Parameter) FontParameter.ParameterByName(name);
                    if (type != null)
                    {
                        string value = xmlp.GetAttributeValue(i);
                        Type dataClass = type.GetDataClass();

                        if (dataClass == typeof(Color))
                        {
                            FontParameter.Parameter<Color> colorType = (FontParameter.Parameter<Color>)type;
                            fp.Put(colorType, ParserUtil.ParseColor(xmlp, value, _constants));

                        }
                        else if (dataClass == typeof(int))
                        {
                            FontParameter.Parameter<int> intType = (FontParameter.Parameter<int>)type;
                            fp.Put(intType, ParseMath(xmlp, value).IntValue());

                        }
                        else if (dataClass == typeof(bool))
                        {
                            FontParameter.Parameter<bool> boolType = (FontParameter.Parameter<bool>)type;
                            fp.Put(boolType, xmlp.ParseBool(value));

                        }
                        else if (dataClass == typeof(string))
                        {
                            FontParameter.Parameter<string> strType = (FontParameter.Parameter<string>)type;
                            fp.Put(strType, value);

                        }
                        else
                        {
                            throw xmlp.Error("dataClass not yet implemented: " + dataClass);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse list of strings from an XML attribute
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="name">XML attribute name</param>
        /// <returns>Parsed <see cref="List{T}"/> where T is <see cref="string"/></returns>
        private static List<string> ParseList(XMLParser xmlp, string name)
        {
            string value = xmlp.GetAttributeValue(null, name);
            if (value != null)
            {
                return ParseList(value, 0);
            }
            return null;
        }

        /// <summary>
        /// Parse a list from the given index in a string
        /// </summary>
        /// <param name="value">String to lookup</param>
        /// <param name="idx">Index in string</param>
        /// <returns>Parsed <see cref="List{T}"/> where T is <see cref="string"/></returns>
        private static List<string> ParseList(string value, int idx)
        {
            idx = TextUtil.SkipSpaces(value, idx);
            if (idx >= value.Length)
            {
                return null;
            }

            int end = TextUtil.IndexOf(value, ',', idx);
            string part = TextUtil.Trim(value, idx, end);

            return new List<string> { part, ParseList(value, end + 1)[0] };
        }

        /// <summary>
        /// Parse a reference in a theme with or without a wildcard
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parent">ThemeInfo parent context</param>
        private void ParseThemeWildcardRef(XMLParser xmlp, ThemeInfoImpl parent)
        {
            string reference = xmlp.GetAttributeValue(null, "ref");
            if (parent == null)
            {
                throw xmlp.Error("Can't declare wildcard themes on top level");
            }
            if (reference == null)
            {
                throw xmlp.Error("Reference required for wildcard theme");
            }
            if (!reference.EndsWith("*"))
            {
                throw xmlp.Error("Wildcard reference must end with '*'");
            }
            string refPath = reference.Substring(0, reference.Length - 1);
            if (refPath.Length > 0 && !refPath.EndsWith("."))
            {
                throw xmlp.Error("Wildcard must end with \".*\" or be \"*\"");
            }
            parent._wildcardImportPath = refPath;
            xmlp.NextTag();
        }

        /// <summary>
        /// Begin parsing a new <see cref="ThemeInfo"/> from the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="themeName">Theme name</param>
        /// <param name="parent">Parent <see cref="ThemeInfoImpl"/> context</param>
        /// <param name="baseFso">Relative file system object</param>
        /// <returns>parsed <see cref="ThemeInfoImpl"/></returns>
        private ThemeInfoImpl ParseTheme(XMLParser xmlp, string themeName, ThemeInfoImpl parent, FileSystemObject baseFso)
        {
            // allow top level theme "*" as fallback theme
            if (!themeName.Equals("*") || parent != null)
            {
                ParserUtil.CheckNameNotEmpty(themeName, xmlp);
                if (themeName.IndexOf('.') >= 0)
                {
                    throw xmlp.Error("'.' is not allowed in names");
                }
            }
            ThemeInfoImpl ti = new ThemeInfoImpl(this, themeName, parent);
            ThemeInfoImpl oldEnv = _mathInterpreter.SetEnv(ti);
            try
            {
                if (xmlp.ParseBoolFromAttribute("merge", false))
                {
                    if (parent == null)
                    {
                        throw xmlp.Error("Can't merge on top level");
                    }
                    ThemeInfoImpl tiPrev = parent.GetTheme(themeName);
                    if (tiPrev != null)
                    {
                        ti.ThemeInfoImplCopy(tiPrev);
                    }
                }
                string reference = xmlp.GetAttributeValue(null, "ref");
                if (reference != null)
                {
                    ThemeInfoImpl tiRef = null;
                    if (parent != null)
                    {
                        tiRef = parent.GetTheme(reference);
                    }
                    if (tiRef == null)
                    {
                        tiRef = (ThemeInfoImpl)FindThemeInfo(reference);
                    }
                    if (tiRef == null)
                    {
                        throw xmlp.Error("referenced theme info not found: " + reference);
                    }
                    ti.ThemeInfoImplCopy(tiRef);
                }
                ti._maybeUsedFromWildcard = xmlp.ParseBoolFromAttribute("allowWildcard", true);
                xmlp.NextTag();
                while (!xmlp.IsEndTag())
                {
                    xmlp.Require(XmlPullParser.START_TAG, null, null);
                    string tagName = xmlp.GetName();
                    string name = xmlp.GetAttributeNotNull("name");
                    if ("param".Equals(tagName))
                    {
                        ParseParam(xmlp, baseFso, "param", ti, ti);
                    }
                    else if ("theme".Equals(tagName))
                    {
                        if (name.Length == 0)
                        {
                            ParseThemeWildcardRef(xmlp, ti);
                        }
                        else
                        {
                            ThemeInfoImpl tiChild = ParseTheme(xmlp, name, ti, baseFso);
                            ti.PutTheme(name, tiChild);
                        }
                    }
                    else
                    {
                        throw xmlp.Unexpected();
                    }
                    xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                    xmlp.NextTag();
                }
            }
            finally
            {
                _mathInterpreter.SetEnv(oldEnv);
            }
            return ti;
        }

        /// <summary>
        /// Parse a theme parameter from a given <paramref name="tagName"/> parameter
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="baseFso">base file system object</param>
        /// <param name="tagName">Theme parameter tag</param>
        /// <param name="parent">Parent theme context</param>
        /// <param name="target">Target parameter map</param>
        private void ParseParam(XMLParser xmlp, FileSystemObject baseFso, string tagName, ThemeInfoImpl parent, ParameterMapImpl target)
        {
            try
            {
                xmlp.Require(XmlPullParser.START_TAG, null, tagName);
                string name = xmlp.GetAttributeNotNull("name");
                xmlp.NextTag();
                string valueTagName = xmlp.GetName();
                Object value = ParseValue(xmlp, valueTagName, name, baseFso, parent);
                xmlp.Require(XmlPullParser.END_TAG, null, valueTagName);
                xmlp.NextTag();
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                if (value is IDictionary<string, Renderer.Image>)
                {
                    IDictionary<string, Renderer.Image> map = (IDictionary<string, Renderer.Image>)value;
                    foreach (string key in map.Keys)
                    {
                        target.Put(key, map[key]);
                    }
                }
                else if (value is IDictionary<string, Renderer.MouseCursor>)
                {
                    IDictionary<string, Renderer.MouseCursor> map = (IDictionary<string, Renderer.MouseCursor>)value;
                    foreach (string key in map.Keys)
                    {
                        target.Put(key, map[key]);
                    }
                }
                else if (value is IDictionary<string, object>)
                { //TODO
                    //target.put((Dictionary<string, object>)value);
                    Dictionary<string, object> map = (Dictionary<string, object>)value;
                    if (parent == null && map.Count != 1)
                    {
                        throw xmlp.Error("constant definitions must define exactly 1 value");
                    }
                    target.Put(map);
                }
                else
                {
                    ParserUtil.CheckNameNotEmpty(name, xmlp);
                    target.Put(name, value);
                }
            }
            catch (FormatException ex)
            {
                throw xmlp.Error("unable to parse value", ex);
            }
        }

        /// <summary>
        /// Parse a list using <see cref="ParseValue(XMLParser, string, string, FileSystemObject, ThemeInfoImpl)"/> from the currnet state of the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="baseFso">base file system object</param>
        /// <param name="parent">Parent theme context</param>
        /// <returns>resultant <see cref="ParameterListImpl"/></returns>
        private ParameterListImpl ParseList(XMLParser xmlp, FileSystemObject baseFso, ThemeInfoImpl parent)
        {
            ParameterListImpl result = new ParameterListImpl(this, parent);
            xmlp.NextTag();
            while (xmlp.IsStartTag())
            {
                string tagName = xmlp.GetName();
                Object obj = ParseValue(xmlp, tagName, null, baseFso, parent);
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                result._parameters.Add(obj);
                xmlp.NextTag();
            }
            return result;
        }

        /// <summary>
        /// Parse a map from the current position of the <see cref="XMLParser"/>, supports <c>ref</c> and <c>merge</c>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="baseFso">base file system object</param>
        /// <param name="name">map name</param>
        /// <param name="parent">Parent theme context</param>
        /// <returns>resultant <see cref="ParameterMapImpl"/></returns>
        /// <exception cref="IOException">Thrown when referencing another map that does not exist</exception>
        private ParameterMapImpl ParseMap(XMLParser xmlp, FileSystemObject baseFso, string name, ThemeInfoImpl parent)
        {
            ParameterMapImpl result = new ParameterMapImpl(this, parent);
            if (xmlp.ParseBoolFromAttribute("merge", false))
            {
                if (parent == null)
                {
                    throw xmlp.Error("Can't merge on top level");
                }
                Object obj = parent[name];
                if (obj is ParameterMapImpl)
                {
                    ParameterMapImpl baseMap = (ParameterMapImpl)obj;
                    result.Copy(baseMap);
                }
                else if (obj != null)
                {
                    throw xmlp.Error("Can only merge with map - found a " + obj.GetType().Name);
                }
            }
            string reference = xmlp.GetAttributeValue(null, "ref");
            if (reference != null)
            {
                Object obj = parent[reference];
                if (obj == null)
                {
                    obj = _constants[reference];
                    if (obj == null)
                    {
                        throw new IOException("Referenced map not found: " + reference);
                    }
                }
                if (obj is ParameterMapImpl)
                {
                    ParameterMapImpl baseMap = (ParameterMapImpl)obj;
                    result.Copy(baseMap);
                }
                else
                {
                    throw new IOException("Expected a map got a " + obj.GetType().Name);
                }
            }
            xmlp.NextTag();
            while (xmlp.IsStartTag())
            {
                string tagName = xmlp.GetName();
                ParseParam(xmlp, baseFso, "param", parent, result);
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                xmlp.NextTag();
            }
            return result;
        }

        /// <summary>
        /// Parse a value from the current position of the XML parser
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="tagName">Value type (a.k.a. current XML tag name)</param>
        /// <param name="wildcardName">Use potential wildcard</param>
        /// <param name="baseFso">base file system object</param>
        /// <param name="parent">Parent theme context</param>
        /// <returns>Parsed value (arbitrary object)</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private object ParseValue(XMLParser xmlp, string tagName, string wildcardName, FileSystemObject baseFso, ThemeInfoImpl parent)
        {
            try
            {
                if ("list".Equals(tagName))
                {
                    return ParseList(xmlp, baseFso, parent);
                }
                if ("map".Equals(tagName))
                {
                    return ParseMap(xmlp, baseFso, wildcardName, parent);
                }
                if ("inputMapDef".Equals(tagName))
                {
                    return ParseInputMap(xmlp, wildcardName, parent);
                }
                if ("fontDef".Equals(tagName))
                {
                    return ParseFont(xmlp, baseFso);
                }
                if ("enum".Equals(tagName))
                {
                    string enumType = xmlp.GetAttributeNotNull("type");
                    if (enumType.ToUpper() == "ALIGNMENT")
                    {
                        return Alignment.ByName(xmlp.NextText());
                    }
                    if (!enums.ContainsKey(enumType))
                    {
                        throw xmlp.Error("enum type \"" + enumType + "\" not registered");
                    }
                    return xmlp.ParseEnumFromText(enums[enumType]);
                }
                if ("bool".Equals(tagName))
                {
                    return xmlp.ParseBoolFromText();
                }

                string value = xmlp.NextText();

                if ("color".Equals(tagName))
                {
                    return ParserUtil.ParseColor(xmlp, value, _constants);
                }
                if ("float".Equals(tagName))
                {
                    return ParseMath(xmlp, value).FloatValue();
                }
                if ("int".Equals(tagName))
                {
                    return ParseMath(xmlp, value).IntValue();
                }
                if ("string".Equals(tagName))
                {
                    return value;
                }
                if ("font".Equals(tagName))
                {
                    Font font = _fonts[value];
                    if (font == null)
                    {
                        throw xmlp.Error("Font \"" + value + "\" not found");
                    }
                    return font;
                }
                if ("border".Equals(tagName))
                {
                    return ParseObject<Border>(xmlp, value, typeof(Border));
                }
                if ("dimension".Equals(tagName))
                {
                    return ParseObject<Dimension>(xmlp, value, typeof(Dimension));
                }
                if ("gap".Equals(tagName) || "size".Equals(tagName))
                {
                    return ParseObject<DialogLayout.Gap>(xmlp, value, typeof(DialogLayout.Gap));
                }
                if ("constant".Equals(tagName))
                {
                    Object result = _constants[value];
                    if (result == null)
                    {
                        throw xmlp.Error("Unknown constant: " + value);
                    }
                    if (result == NULL)
                    {
                        result = null;
                    }
                    return result;
                }
                if ("image".Equals(tagName))
                {
                    if (value.EndsWith(".*"))
                    {
                        if (wildcardName == null)
                        {
                            throw new ArgumentNullException("Wildcard's not allowed");
                        }
                        return _imageManager.GetImages(value, wildcardName);
                    }
                    return _imageManager.GetReferencedImage(xmlp, value);
                }
                if ("cursor".Equals(tagName))
                {
                    if (value.EndsWith(".*"))
                    {
                        if (wildcardName == null)
                        {
                            throw new ArgumentNullException("Wildcard's not allowed");
                        }
                        return _imageManager.GetCursors(value, wildcardName);
                    }
                    return _imageManager.GetReferencedCursor(xmlp, value);
                }
                if ("inputMap".Equals(tagName))
                {
                    return GetInputMap(xmlp, value);
                }
                throw xmlp.Error("Unknown type \"" + tagName + "\" specified");
            }
            catch (FormatException ex)
            {
                throw xmlp.Error("unable to parse value", ex);
            }
        }

        /// <summary>
        /// Parse a given <paramref name="str"/> using the currently instantiated <see cref="MathInterpreter"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="str">mathematical expression</param>
        /// <returns><see cref="Number"/> value</returns>
        private Number ParseMath(XMLParser xmlp, string str)
        {
            try
            {
                return _mathInterpreter.Execute(str);
            }
            catch (ParseException ex)
            {
                throw xmlp.Error("unable to evaluate", Unwrap(ex));
            }
        }

        /// <summary>
        /// Parse an object by marshalling it through the <see cref="MathInterpreter"/>
        /// </summary>
        /// <typeparam name="T">Object to find</typeparam>
        /// <param name="xmlp">XML parser</param>
        /// <param name="str">mathematical expression</param>
        /// <param name="type">Type of object</param>
        /// <returns>Marshalled object <typeparamref name="T"/></returns>
        private T ParseObject<T>(XMLParser xmlp, string str, Type type)
        {
            try
            {
                return _mathInterpreter.ExecuteCreateObject<T>(str, type);
            }
            catch (ParseException ex)
            {
                throw xmlp.Error("unable to evaluate", Unwrap(ex));
            }
        }

        /// <summary>
        /// Unwrap exceptions
        /// </summary>
        /// <param name="ex">Wrapped exception</param>
        /// <returns>InnerException called on the wrapped exception</returns>
        private Exception Unwrap(ParseException ex)
        {
            if (ex.InnerException != null)
            {
                return ex.InnerException;
            }
            else
            {
                return ex;
            }
        }

        /// <summary>
        /// Resolve a wildcard used by a ThemeInfo lookup
        /// </summary>
        /// <param name="baseStr">Base path</param>
        /// <param name="name">Widget name</param>
        /// <param name="useFallback">Use fallback theme?</param>
        /// <returns>Resolved <see cref="ThemeInfo"/></returns>
        /// <exception cref="Exception">Invalid attempt at constructing a wildcard lookuo</exception>
        internal ThemeInfo ResolveWildcard(string baseStr, string name, bool useFallback)
        {
            if(!(baseStr.Length == 0 || baseStr.EndsWith(".")))
            {
                throw new Exception("Assertion exception");
            }
            string fullPath = baseStr + name;
            ThemeInfo info = FindThemeInfo(fullPath, false, useFallback);
            if (info != null && ((ThemeInfoImpl)info)._maybeUsedFromWildcard)
            {
                return info;
            }
            return null;
        }

        /// <summary>
        /// An implementation of <see cref="AbstractMathInterpreter"/> which can access theme fields and place them on the stack
        /// </summary>
        class MathInterpreter : AbstractMathInterpreter
        {
            private ThemeInfoImpl _env;
            private ThemeManager _themeManager;

            /// <summary>
            /// Initialise the math interpreter that can look up using a <see cref="ThemeManager"/>
            /// </summary>
            /// <param name="themeManager"><see cref="ThemeManager"/> where to access variables</param>
            public MathInterpreter(ThemeManager themeManager)
            {
                this._themeManager = themeManager;
            }

            public ThemeInfoImpl SetEnv(ThemeInfoImpl env)
            {
                ThemeInfoImpl oldEnv = this._env;
                this._env = env;
                return oldEnv;
            }

            public override void AccessVariable(string name)
            {
                for (ThemeInfoImpl e = _env; e != null; e = e._parent)
                {
                    Object objx = e[name];
                    if (objx != null)
                    {
                        Push(objx);
                        return;
                    }
                    objx = e.GetChildThemeImpl(name, false);
                    if (objx != null)
                    {
                        Push(objx);
                        return;
                    }
                }

                Object obj = this._themeManager._constants[name];
                if (obj != null)
                {
                    Push(obj);
                    return;
                }

                if (this._themeManager._fonts.ContainsKey(name))
                {
                    Push(this._themeManager._fonts[name]);
                    return;
                }

                throw new ArgumentNullException("variable not found: " + name);
            }

            protected override Object AccessField(Object obj, string field)
            {
                if (obj is ThemeInfoImpl)
                {
                    Object result = ((ThemeInfoImpl)obj).GetTheme(field);
                    if (result != null)
                    {
                        return result;
                    }
                }
                if (obj is ParameterMapImpl)
                {
                    Object result = ((ParameterMapImpl)obj)[field];
                    if (result == null)
                    {
                        throw new ArgumentNullException("field not found: " + field);
                    }
                    return result;
                }
                if ((obj is Image) && "border".Equals(field))
                {
                    Border border = null;
                    if (obj is HasBorder)
                    {
                        border = ((HasBorder)obj).Border;
                    }
                    return (border != null) ? border : Border.ZERO;
                }
                return base.AccessField(obj, field);
            }
        }
    }
}
