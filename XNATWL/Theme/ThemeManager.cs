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
using XNATWL.IO;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL.Theme
{
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

        /**
         * Returns the associated cache context which was used to load this theme.
         * @return the cache context
         * @see #createThemeManager(java.net.URL, de.matthiasmann.twl.renderer.Renderer.Renderer, de.matthiasmann.twl.renderer.CacheContext) 
         */
        public CacheContext GetCacheContext()
        {
            return _cacheContext;
        }

        /**
         * Destroys the CacheContext and releases all OpenGL resources
         * @see CacheContext#destroy()
         * @see #getCacheContext()
         */
        public void Destroy()
        {
            foreach (Font font in _fonts.Values)
            {
                font.Dispose();
            }
            _cacheContext.Dispose();
        }

        public Font GetDefaultFont()
        {
            return _defaultFont;
        }

        /**
         * Loads the specified theme using the provided renderer and a new cache context.
         *
         * This is equivalent to calling {@code createThemeManager(url, renderer, renderer.createNewCacheContext())}
         *
         * @param url The URL of the theme
         * @param renderer The renderer which is used to load and render the resources
         * @return a new ThemeManager
         * @throws IOException if an error occured while loading
         * @throws NullPointerException if one of the passed parameters is {@code null}
         * @see #createThemeManager(java.net.URL, de.matthiasmann.twl.renderer.Renderer.Renderer, de.matthiasmann.twl.renderer.CacheContext)
         * @see #destroy() 
         */
        public static ThemeManager CreateThemeManager(FileSystemObject fso, Renderer.Renderer renderer)
        {
            if (fso == null)
            {
                throw new ArgumentNullException("url is null");
            }
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer is null");
            }
            return CreateThemeManager(fso, renderer, renderer.CreateNewCacheContext());
        }

        /**
         * Loads the specified theme using the provided renderer and cache context.
         *
         * This is equivalent to calling {@code createThemeManager(url, renderer, renderer.createNewCacheContext(), null)}
         * 
         * @param url The URL of the theme
         * @param renderer The renderer which is used to load and render the resources
         * @param cacheContext The cache context into which the resources are loaded
         * @return a new ThemeManager
         * @throws IOException if an error occured while loading
         * @throws ArgumentNullException if one of the passed parameters is {@code null}
         * @see #createThemeManager(java.net.URL, de.matthiasmann.twl.renderer.Renderer.Renderer, de.matthiasmann.twl.renderer.CacheContext, java.util.Map) 
         * @see #destroy() 
         */
        public static ThemeManager CreateThemeManager(FileSystemObject fso, Renderer.Renderer renderer, CacheContext cacheContext)
        {
            return CreateThemeManager(fso, renderer, cacheContext, null);
        }

        /**
         * Loads the specified theme using the provided renderer and cache context.
         *
         * The provided {@code cacheContext} is set active in the provided {@code renderer}.
         *
         * The cache context is stored inside the created ThemeManager. Calling
         * {@code destroy()} on the returned ThemeManager instance will also destroy
         * the cache context.
         *
         * @param url The URL of the theme
         * @param renderer The renderer which is used to load and render the resources
         * @param cacheContext The cache context into which the resources are loaded
         * @param constants A map containing constants which as exposed to the theme
         *                  as if defined by &lt;constantDef/&gt;, can be null.
         * @return a new ThemeManager
         * @throws IOException if an error occured while loading
         * @throws ArgumentNullException if one of the passed parameters is {@code null}
         * @see Renderer.Renderer#setActiveCacheContext(de.matthiasmann.twl.renderer.CacheContext)
         * @see #destroy() 
         */
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
                throw (IOException)(new IOException("createThemeManager", ex));
            }
        }

        public static void RegisterEnumType<E>(string name, Type enumClazz) where E : struct, IConvertible
        {
            if (!enumClazz.IsEnum)
            {
                throw new ArgumentNullException("not an enum class");
            }
            if (enums.ContainsKey(name) && enums[name] != enumClazz)
            {
                throw new ArgumentNullException("Enum type name \"" + name + "\" is already in use by " + enums[name].Name);
            }
            enums.Add(name, enumClazz);
        }

        public ThemeInfo FindThemeInfo(string themePath)
        {
            return FindThemeInfo(themePath, true, true);
        }

        private ThemeInfo FindThemeInfo(string themePath, bool warn, bool useFallback)
        {
            int start = TextUtil.indexOf(themePath, '.', 0);
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
                    DebugHook.getDebugHook().usingFallbackTheme(themePath);
                }
            }
            while (info != null && ++start < themePath.Length)
            {
                int next = TextUtil.indexOf(themePath, '.', start);
                info = info.GetChildTheme(themePath.Substring(start, next - start));
                start = next;
            }
            if (info == null && warn)
            {
                DebugHook.getDebugHook().missingTheme(themePath);
            }
            return info;
        }

        public Image GetImageNoWarning(string name)
        {
            return _imageManager.GetImage(name);
        }

        public Image GetImage(string name)
        {
            Image img = _imageManager.GetImage(name);
            if (img == null)
            {
                DebugHook.getDebugHook().missingImage(name);
            }
            return img;
        }

        public Object GetCursor(string name)
        {
            return _imageManager.GetCursor(name);
        }

        public Font GetFont(string name)
        {
            return _fonts[name];
        }

        public ParameterMap GetConstants()
        {
            return _constants;
        }

        private void InsertDefaultConstants()
        {
            _constants.Put("SINGLE_COLUMN", -1);
            _constants.Put("MAX", short.MaxValue);
        }

        private void InsertConstants(Dictionary<string, Object> src)
        {
            _constants.Put(src);
        }

        private void ParseThemeFile(FileSystemObject fso)
        {
            try
            {
                XMLParser xmlp = new XMLParser(fso);
                try
                {
                    xmlp.setLoggerName(typeof(ThemeManager).Name);
                    xmlp.next();
                    xmlp.require(XmlPullParser.XML_DECLARATION, null, null);
                    xmlp.next();
                    ParseThemeFile(xmlp, fso.Parent);
                }
                finally
                {
                    xmlp.close();
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

        private void ParseThemeFile(XMLParser xmlp, FileSystemObject baseFso)
        {
            xmlp.require(XmlPullParser.START_TAG, null, "themes");
            xmlp.nextTag();

            while (!xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, null);
                string tagName = xmlp.getName();
                if ("images".Equals(tagName) || "textures".Equals(tagName))
                {
                    _imageManager.ParseImages(xmlp, baseFso);
                }
                else if ("include".Equals(tagName))
                {
                    string fontFileName = xmlp.getAttributeNotNull("filename");
                    try
                    {
                        ParseThemeFile(new FileSystemObject(baseFso, fontFileName));
                    }
                    catch (ThemeException ex)
                    {
                        ex.addIncludedBy(baseFso, xmlp.getLineNumber(), xmlp.getColumnNumber());
                        throw ex;
                    }
                    xmlp.nextTag();
                }
                else
                {
                    string name = xmlp.getAttributeNotNull("name");
                    if ("theme".Equals(tagName))
                    {
                        if (_themes.ContainsKey(name))
                        {
                            throw xmlp.error("theme \"" + name + "\" already defined");
                        }
                        _themes.Add(name, ParseTheme(xmlp, name, null, baseFso));
                    }
                    else if ("inputMapDef".Equals(tagName))
                    {
                        if (_inputMaps.ContainsKey(name))
                        {
                            throw xmlp.error("inputMap \"" + name + "\" already defined");
                        }
                        _inputMaps.Add(name, ParseInputMap(xmlp, name, null));
                    }
                    else if ("fontDef".Equals(tagName))
                    {
                        if (_fonts.ContainsKey(name))
                        {
                            throw xmlp.error("font \"" + name + "\" already defined");
                        }
                        bool makeDefault = xmlp.parseBoolFromAttribute("default", false);
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
                                throw xmlp.error("default font already set");
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
                        throw xmlp.unexpected();
                    }
                }
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
            }
            xmlp.require(XmlPullParser.END_TAG, null, "themes");
        }

        private InputMap GetInputMap(XMLParser xmlp, string name)
        {
            if (!_inputMaps.ContainsKey(name))
            {
                throw xmlp.error("Undefined input map: " + name);
            }

            return _inputMaps[name];
        }

        private InputMap ParseInputMap(XMLParser xmlp, string name, ThemeInfoImpl parent)
        {
            InputMap baseMap = InputMap.empty();
            if (xmlp.parseBoolFromAttribute("merge", false))
            {
                if (parent == null)
                {
                    throw xmlp.error("Can't merge on top level");
                }
                Object o = parent.GetParam(name);
                if (o is InputMap)
                {
                    baseMap = (InputMap)o;
                }
                else if (o != null)
                {
                    throw xmlp.error("Can only merge with inputMap - found a " + o.GetType().FullName);
                }
            }
            string baseName = xmlp.getAttributeValue(null, "ref");
            if (baseName != null)
            {
                baseMap = baseMap.addKeyStrokes(GetInputMap(xmlp, baseName));
            }

            xmlp.nextTag();

            LinkedHashSet<KeyStroke> keyStrokes = InputMap.parseBody(xmlp);
            InputMap im = baseMap.addKeyStrokes(keyStrokes);
            return im;
        }

        private Font ParseFont(XMLParser xmlp, FileSystemObject baseFso)
        {
            FileSystemObject fso;
            string fileName = xmlp.getAttributeValue(null, "filename");
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
                fontSize = ParseMath(xmlp, xmlp.getAttributeNotNull("size")).intValue();
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

            xmlp.nextTag();
            while (!xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, "fontParam");

                StateExpression cond = ParserUtil.ParseCondition(xmlp);
                if (cond == null)
                {
                    throw xmlp.error("Condition required");
                }
                stateExpr.Add(cond);

                FontParameter parameters = new FontParameter(baseParams);
                ParseFontParameter(xmlp, parameters);
                fontParams.Add(parameters);

                xmlp.nextTag();
                xmlp.require(XmlPullParser.END_TAG, null, "fontParam");
                xmlp.nextTag();
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

        private void ParseFontParameter(XMLParser xmlp, FontParameter fp)
        {
            for (int i = 0, n = xmlp.getAttributeCount(); i < n; i++)
            {
                if (xmlp.isAttributeUnused(i))
                {
                    string name = xmlp.getAttributeName(i);
                    FontParameter.Parameter type = (FontParameter.Parameter) FontParameter.ParameterByName(name);
                    if (type != null)
                    {
                        string value = xmlp.getAttributeValue(i);
                        Type dataClass = type.getDataClass();

                        if (dataClass == typeof(Color))
                        {
                            FontParameter.Parameter<Color> colorType = (FontParameter.Parameter<Color>)type;
                            fp.Put(colorType, ParserUtil.ParseColor(xmlp, value, _constants));

                        }
                        else if (dataClass == typeof(int))
                        {
                            FontParameter.Parameter<int> intType = (FontParameter.Parameter<int>)type;
                            fp.Put(intType, ParseMath(xmlp, value).intValue());

                        }
                        else if (dataClass == typeof(bool))
                        {
                            FontParameter.Parameter<bool> boolType = (FontParameter.Parameter<bool>)type;
                            fp.Put(boolType, xmlp.parseBool(value));

                        }
                        else if (dataClass == typeof(string))
                        {
                            FontParameter.Parameter<string> strType = (FontParameter.Parameter<string>)type;
                            fp.Put(strType, value);

                        }
                        else
                        {
                            throw xmlp.error("dataClass not yet implemented: " + dataClass);
                        }
                    }
                }
            }
        }

        private static List<string> ParseList(XMLParser xmlp, string name)
        {
            string value = xmlp.getAttributeValue(null, name);
            if (value != null)
            {
                return ParseList(value, 0);
            }
            return null;
        }

        private static List<string> ParseList(string value, int idx)
        {
            idx = TextUtil.skipSpaces(value, idx);
            if (idx >= value.Length)
            {
                return null;
            }

            int end = TextUtil.indexOf(value, ',', idx);
            string part = TextUtil.trim(value, idx, end);

            return new List<string> { part, ParseList(value, end + 1)[0] };
        }

        private void ParseThemeWildcardRef(XMLParser xmlp, ThemeInfoImpl parent)
        {
            string reference = xmlp.getAttributeValue(null, "ref");
            if (parent == null)
            {
                throw xmlp.error("Can't declare wildcard themes on top level");
            }
            if (reference == null)
            {
                throw xmlp.error("Reference required for wildcard theme");
            }
            if (!reference.EndsWith("*"))
            {
                throw xmlp.error("Wildcard reference must end with '*'");
            }
            string refPath = reference.Substring(0, reference.Length - 1);
            if (refPath.Length > 0 && !refPath.EndsWith("."))
            {
                throw xmlp.error("Wildcard must end with \".*\" or be \"*\"");
            }
            parent._wildcardImportPath = refPath;
            xmlp.nextTag();
        }

        private ThemeInfoImpl ParseTheme(XMLParser xmlp, string themeName, ThemeInfoImpl parent, FileSystemObject baseFso)
        {
            // allow top level theme "*" as fallback theme
            if (!themeName.Equals("*") || parent != null)
            {
                ParserUtil.CheckNameNotEmpty(themeName, xmlp);
                if (themeName.IndexOf('.') >= 0)
                {
                    throw xmlp.error("'.' is not allowed in names");
                }
            }
            ThemeInfoImpl ti = new ThemeInfoImpl(this, themeName, parent);
            ThemeInfoImpl oldEnv = _mathInterpreter.SetEnv(ti);
            try
            {
                if (xmlp.parseBoolFromAttribute("merge", false))
                {
                    if (parent == null)
                    {
                        throw xmlp.error("Can't merge on top level");
                    }
                    ThemeInfoImpl tiPrev = parent.GetTheme(themeName);
                    if (tiPrev != null)
                    {
                        ti.ThemeInfoImplCopy(tiPrev);
                    }
                }
                string reference = xmlp.getAttributeValue(null, "ref");
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
                        throw xmlp.error("referenced theme info not found: " + reference);
                    }
                    ti.ThemeInfoImplCopy(tiRef);
                }
                ti._maybeUsedFromWildcard = xmlp.parseBoolFromAttribute("allowWildcard", true);
                xmlp.nextTag();
                while (!xmlp.isEndTag())
                {
                    xmlp.require(XmlPullParser.START_TAG, null, null);
                    string tagName = xmlp.getName();
                    string name = xmlp.getAttributeNotNull("name");
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
                        throw xmlp.unexpected();
                    }
                    xmlp.require(XmlPullParser.END_TAG, null, tagName);
                    xmlp.nextTag();
                }
            }
            finally
            {
                _mathInterpreter.SetEnv(oldEnv);
            }
            return ti;
        }

        private void ParseParam(XMLParser xmlp, FileSystemObject baseFso, string tagName, ThemeInfoImpl parent, ParameterMapImpl target)
        {
            try
            {
                xmlp.require(XmlPullParser.START_TAG, null, tagName);
                string name = xmlp.getAttributeNotNull("name");
                xmlp.nextTag();
                string valueTagName = xmlp.getName();
                Object value = ParseValue(xmlp, valueTagName, name, baseFso, parent);
                xmlp.require(XmlPullParser.END_TAG, null, valueTagName);
                xmlp.nextTag();
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
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
                        throw xmlp.error("constant definitions must define exactly 1 value");
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
                throw xmlp.error("unable to parse value", ex);
            }
        }

        private ParameterListImpl ParseList(XMLParser xmlp, FileSystemObject baseFso, ThemeInfoImpl parent)
        {
            ParameterListImpl result = new ParameterListImpl(this, parent);
            xmlp.nextTag();
            while (xmlp.isStartTag())
            {
                string tagName = xmlp.getName();
                Object obj = ParseValue(xmlp, tagName, null, baseFso, parent);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                result._parameters.Add(obj);
                xmlp.nextTag();
            }
            return result;
        }

        private ParameterMapImpl ParseMap(XMLParser xmlp, FileSystemObject baseFso, string name, ThemeInfoImpl parent)
        {
            ParameterMapImpl result = new ParameterMapImpl(this, parent);
            if (xmlp.parseBoolFromAttribute("merge", false))
            {
                if (parent == null)
                {
                    throw xmlp.error("Can't merge on top level");
                }
                Object obj = parent.GetParam(name);
                if (obj is ParameterMapImpl)
                {
                    ParameterMapImpl baseMap = (ParameterMapImpl)obj;
                    result.Copy(baseMap);
                }
                else if (obj != null)
                {
                    throw xmlp.error("Can only merge with map - found a " + obj.GetType().Name);
                }
            }
            string reference = xmlp.getAttributeValue(null, "ref");
            if (reference != null)
            {
                Object obj = parent.GetParam(reference);
                if (obj == null)
                {
                    obj = _constants.GetParam(reference);
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
            xmlp.nextTag();
            while (xmlp.isStartTag())
            {
                string tagName = xmlp.getName();
                ParseParam(xmlp, baseFso, "param", parent, result);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
            }
            return result;
        }

        private Object ParseValue(XMLParser xmlp, string tagName, string wildcardName, FileSystemObject baseFso, ThemeInfoImpl parent)
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
                    string enumType = xmlp.getAttributeNotNull("type");
                    if (enumType.ToUpper() == "ALIGNMENT")
                    {
                        return Alignment.ByName(xmlp.nextText());
                    }
                    if (!enums.ContainsKey(enumType))
                    {
                        throw xmlp.error("enum type \"" + enumType + "\" not registered");
                    }
                    return xmlp.parseEnumFromText(enums[enumType]);
                }
                if ("bool".Equals(tagName))
                {
                    return xmlp.parseBoolFromText();
                }

                string value = xmlp.nextText();

                if ("color".Equals(tagName))
                {
                    return ParserUtil.ParseColor(xmlp, value, _constants);
                }
                if ("float".Equals(tagName))
                {
                    return ParseMath(xmlp, value).floatValue();
                }
                if ("int".Equals(tagName))
                {
                    return ParseMath(xmlp, value).intValue();
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
                        throw xmlp.error("Font \"" + value + "\" not found");
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
                    Object result = _constants.GetParam(value);
                    if (result == null)
                    {
                        throw xmlp.error("Unknown constant: " + value);
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
                throw xmlp.error("Unknown type \"" + tagName + "\" specified");
            }
            catch (FormatException ex)
            {
                throw xmlp.error("unable to parse value", ex);
            }
        }

        private Number ParseMath(XMLParser xmlp, string str)
        {
            try
            {
                return _mathInterpreter.execute(str);
            }
            catch (ParseException ex)
            {
                throw xmlp.error("unable to evaluate", Unwrap(ex));
            }
        }

        private T ParseObject<T>(XMLParser xmlp, string str, Type type)
        {
            try
            {
                return _mathInterpreter.executeCreateObject<T>(str, type);
            }
            catch (ParseException ex)
            {
                throw xmlp.error("unable to evaluate", Unwrap(ex));
            }
        }

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

        class MathInterpreter : AbstractMathInterpreter
        {
            private ThemeInfoImpl _env;
            private ThemeManager _themeManager;

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
                    Object objx = e.GetParam(name);
                    if (objx != null)
                    {
                        push(objx);
                        return;
                    }
                    objx = e.GetChildThemeImpl(name, false);
                    if (objx != null)
                    {
                        push(objx);
                        return;
                    }
                }

                Object obj = this._themeManager._constants.GetParam(name);
                if (obj != null)
                {
                    push(obj);
                    return;
                }

                if (this._themeManager._fonts.ContainsKey(name))
                {
                    push(this._themeManager._fonts[name]);
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
                    Object result = ((ParameterMapImpl)obj).GetParam(field);
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
