using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    public class ThemeManager
    {
        private static Dictionary<string, Type> enums =
                new Dictionary<string, Type>();

        static ThemeManager()
        {
            registerEnumType<PositionAnimatedPanel.eDirection>("direction", typeof(PositionAnimatedPanel.eDirection));
        }

        static Object NULL = new Object();

        ParameterMapImpl constants;
        private Renderer.Renderer renderer;
        private CacheContext cacheContext;
        private ImageManager imageManager;
        private Dictionary<string, Font> fonts;
        private Dictionary<string, ThemeInfoImpl> themes;
        private Dictionary<string, InputMap> inputMaps;
        private MathInterpreter mathInterpreter;
        private Font defaultFont;
        private Font firstFont;

        internal ParameterMapImpl emptyMap;
        internal ParameterListImpl emptyList;

        private ThemeManager(Renderer.Renderer renderer, CacheContext cacheContext)
        {
            this.constants = new ParameterMapImpl(this, null);
            this.renderer = renderer;
            this.cacheContext = cacheContext;
            this.imageManager = new ImageManager(constants, renderer);
            this.fonts = new Dictionary<string, Font>();
            this.themes = new Dictionary<string, ThemeInfoImpl>();
            this.inputMaps = new Dictionary<string, InputMap>();
            this.emptyMap = new ParameterMapImpl(this, null);
            this.emptyList = new ParameterListImpl(this, null);
            this.mathInterpreter = new MathInterpreter(this);
        }

        /**
         * Returns the associated cache context which was used to load this theme.
         * @return the cache context
         * @see #createThemeManager(java.net.URL, de.matthiasmann.twl.renderer.Renderer.Renderer, de.matthiasmann.twl.renderer.CacheContext) 
         */
        public CacheContext getCacheContext()
        {
            return cacheContext;
        }

        /**
         * Destroys the CacheContext and releases all OpenGL resources
         * @see CacheContext#destroy()
         * @see #getCacheContext()
         */
        public void destroy()
        {
            foreach (Font font in fonts.Values)
            {
                font.Dispose();
            }
            cacheContext.Dispose();
        }

        public Font getDefaultFont()
        {
            return defaultFont;
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
        public static ThemeManager createThemeManager(FileSystemObject fso, Renderer.Renderer renderer)
        {
            if (fso == null)
            {
                throw new ArgumentNullException("url is null");
            }
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer is null");
            }
            return createThemeManager(fso, renderer, renderer.CreateNewCacheContext());
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
        public static ThemeManager createThemeManager(FileSystemObject fso, Renderer.Renderer renderer, CacheContext cacheContext)
        {
            return createThemeManager(fso, renderer, cacheContext, null);
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
        public static ThemeManager createThemeManager(FileSystemObject fso, Renderer.Renderer renderer, CacheContext cacheContext, Dictionary<string, Object> constants)
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
                tm.insertDefaultConstants();
                if (constants != null && constants.Count != 0)
                {
                    tm.insertConstants(constants);
                }
                tm.parseThemeFile(fso);
                if (tm.defaultFont == null)
                {
                    tm.defaultFont = tm.firstFont;
                }
                return tm;
            }
            catch (XmlPullParserException ex)
            {
                throw (IOException)(new IOException("createThemeManager", ex));
            }
        }

        public static void registerEnumType<E>(string name, Type enumClazz) where E : struct, IConvertible
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

        public ThemeInfo findThemeInfo(string themePath)
        {
            return findThemeInfo(themePath, true, true);
        }

        private ThemeInfo findThemeInfo(string themePath, bool warn, bool useFallback)
        {
            int start = TextUtil.indexOf(themePath, '.', 0);
            ThemeInfo info = themes[themePath.Substring(0, start)];
            if (info == null)
            {
                info = themes["*"];
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
                info = info.getChildTheme(themePath.Substring(start, next - start));
                start = next;
            }
            if (info == null && warn)
            {
                DebugHook.getDebugHook().missingTheme(themePath);
            }
            return info;
        }

        public Image getImageNoWarning(string name)
        {
            return imageManager.getImage(name);
        }

        public Image getImage(string name)
        {
            Image img = imageManager.getImage(name);
            if (img == null)
            {
                DebugHook.getDebugHook().missingImage(name);
            }
            return img;
        }

        public Object getCursor(string name)
        {
            return imageManager.getCursor(name);
        }

        public Font getFont(string name)
        {
            return fonts[name];
        }

        public ParameterMap getConstants()
        {
            return constants;
        }

        private void insertDefaultConstants()
        {
            //constants.put("SINGLE_COLUMN", ListBox.SINGLE_COLUMN); // TODO
            constants.put("MAX", short.MaxValue);
        }

        private void insertConstants(Dictionary<string, Object> src)
        {
            constants.put(src);
        }

        private void parseThemeFile(FileSystemObject fso)
        {
            try
            {
                XMLParser xmlp = new XMLParser(fso);
                try
                {
                    xmlp.setLoggerName(typeof(ThemeManager).Name);
                    xmlp.next();
                    xmlp.require(XmlPullParser.XML_DECLARATION, null, null);
                    while(xmlp.next() == XmlPullParser.IGNORABLE_WHITESPACE)
                    {
                    }
                    parseThemeFile(xmlp, fso.Parent);
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

        private void parseThemeFile(XMLParser xmlp, FileSystemObject baseFso)
        {
            xmlp.require(XmlPullParser.START_TAG, null, "themes");
            xmlp.nextTag();

            while (!xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, null);
                string tagName = xmlp.getName();
                if ("images".Equals(tagName) || "textures".Equals(tagName))
                {
                    imageManager.parseImages(xmlp, baseFso);
                }
                else if ("include".Equals(tagName))
                {
                    string fontFileName = xmlp.getAttributeNotNull("filename");
                    try
                    {
                        parseThemeFile(new FileSystemObject(baseFso, fontFileName));
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
                        if (themes.ContainsKey(name))
                        {
                            throw xmlp.error("theme \"" + name + "\" already defined");
                        }
                        themes.Add(name, parseTheme(xmlp, name, null, baseFso));
                    }
                    else if ("inputMapDef".Equals(tagName))
                    {
                        if (inputMaps.ContainsKey(name))
                        {
                            throw xmlp.error("inputMap \"" + name + "\" already defined");
                        }
                        inputMaps.Add(name, parseInputMap(xmlp, name, null));
                    }
                    else if ("fontDef".Equals(tagName))
                    {
                        if (fonts.ContainsKey(name))
                        {
                            throw xmlp.error("font \"" + name + "\" already defined");
                        }
                        bool makeDefault = xmlp.parseBoolFromAttribute("default", false);
                        Font font = parseFont(xmlp, baseFso);
                        fonts.Add(name, font);
                        if (firstFont == null)
                        {
                            firstFont = font;
                        }
                        if (makeDefault)
                        {
                            if (defaultFont != null)
                            {
                                throw xmlp.error("default font already set");
                            }
                            defaultFont = font;
                        }
                    }
                    else if ("constantDef".Equals(tagName))
                    {
                        parseParam(xmlp, baseFso, "constantDef", null, constants);
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

        private InputMap getInputMap(XMLParser xmlp, string name)
        {
            if (!inputMaps.ContainsKey(name))
            {
                throw xmlp.error("Undefined input map: " + name);
            }

            return inputMaps[name];
        }

        private InputMap parseInputMap(XMLParser xmlp, string name, ThemeInfoImpl parent)
        {
            InputMap baseMap = InputMap.empty();
            if (xmlp.parseBoolFromAttribute("merge", false))
            {
                if (parent == null)
                {
                    throw xmlp.error("Can't merge on top level");
                }
                Object o = parent.getParam(name);
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
                baseMap = baseMap.addKeyStrokes(getInputMap(xmlp, baseName));
            }

            xmlp.nextTag();

            LinkedHashSet<KeyStroke> keyStrokes = InputMap.parseBody(xmlp);
            InputMap im = baseMap.addKeyStrokes(keyStrokes);
            return im;
        }

        private Font parseFont(XMLParser xmlp, FileSystemObject baseFso)
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

            List<string> fontFamilies = parseList(xmlp, "families");
            int fontSize = 0;
            int fontStyle = 0;
            if (fontFamilies != null)
            {
                fontSize = parseMath(xmlp, xmlp.getAttributeNotNull("size")).intValue();
                List<string> styles = parseList(xmlp, "style");
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
            parseFontParameter(xmlp, baseParams);
            List<FontParameter> fontParams = new List<FontParameter>();
            List<StateExpression> stateExpr = new List<StateExpression>();

            xmlp.nextTag();
            while (!xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, "fontParam");

                StateExpression cond = ParserUtil.parseCondition(xmlp);
                if (cond == null)
                {
                    throw xmlp.error("Condition required");
                }
                stateExpr.Add(cond);

                FontParameter parameters = new FontParameter(baseParams);
                parseFontParameter(xmlp, parameters);
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
                FontMapper fontMapper = renderer.FontMapper;
                if (fontMapper != null)
                {
                    Font font = fontMapper.GetFont(fontFamilies, fontSize, fontStyle, stateSelect, stateParams);
                    if (font != null)
                    {
                        return font;
                    }
                }
            }

            return renderer.LoadFont(fso, stateSelect, stateParams);
        }

        private void parseFontParameter(XMLParser xmlp, FontParameter fp)
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
                            fp.Put(colorType, ParserUtil.parseColor(xmlp, value, constants));

                        }
                        else if (dataClass == typeof(int))
                        {
                            FontParameter.Parameter<int> intType = (FontParameter.Parameter<int>)type;
                            fp.Put(intType, parseMath(xmlp, value).intValue());

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

        private static List<string> parseList(XMLParser xmlp, string name)
        {
            string value = xmlp.getAttributeValue(null, name);
            if (value != null)
            {
                return parseList(value, 0);
            }
            return null;
        }

        private static List<string> parseList(string value, int idx)
        {
            idx = TextUtil.skipSpaces(value, idx);
            if (idx >= value.Length)
            {
                return null;
            }

            int end = TextUtil.indexOf(value, ',', idx);
            string part = TextUtil.trim(value, idx, end);

            return new List<string> { part, parseList(value, end + 1)[0] };
        }

        private void parseThemeWildcardRef(XMLParser xmlp, ThemeInfoImpl parent)
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
            parent.wildcardImportPath = refPath;
            xmlp.nextTag();
        }

        private ThemeInfoImpl parseTheme(XMLParser xmlp, string themeName, ThemeInfoImpl parent, FileSystemObject baseFso)
        {
            // allow top level theme "*" as fallback theme
            if (!themeName.Equals("*") || parent != null)
            {
                ParserUtil.checkNameNotEmpty(themeName, xmlp);
                if (themeName.IndexOf('.') >= 0)
                {
                    throw xmlp.error("'.' is not allowed in names");
                }
            }
            ThemeInfoImpl ti = new ThemeInfoImpl(this, themeName, parent);
            ThemeInfoImpl oldEnv = mathInterpreter.setEnv(ti);
            try
            {
                if (xmlp.parseBoolFromAttribute("merge", false))
                {
                    if (parent == null)
                    {
                        throw xmlp.error("Can't merge on top level");
                    }
                    ThemeInfoImpl tiPrev = parent.getTheme(themeName);
                    if (tiPrev != null)
                    {
                        ti.themeInfoImplCopy(tiPrev);
                    }
                }
                string reference = xmlp.getAttributeValue(null, "ref");
                if (reference != null)
                {
                    ThemeInfoImpl tiRef = null;
                    if (parent != null)
                    {
                        tiRef = parent.getTheme(reference);
                    }
                    if (tiRef == null)
                    {
                        tiRef = (ThemeInfoImpl)findThemeInfo(reference);
                    }
                    if (tiRef == null)
                    {
                        throw xmlp.error("referenced theme info not found: " + reference);
                    }
                    ti.themeInfoImplCopy(tiRef);
                }
                ti.maybeUsedFromWildcard = xmlp.parseBoolFromAttribute("allowWildcard", true);
                xmlp.nextTag();
                while (!xmlp.isEndTag())
                {
                    xmlp.require(XmlPullParser.START_TAG, null, null);
                    string tagName = xmlp.getName();
                    string name = xmlp.getAttributeNotNull("name");
                    if ("param".Equals(tagName))
                    {
                        parseParam(xmlp, baseFso, "param", ti, ti);
                    }
                    else if ("theme".Equals(tagName))
                    {
                        if (name.Length == 0)
                        {
                            parseThemeWildcardRef(xmlp, ti);
                        }
                        else
                        {
                            ThemeInfoImpl tiChild = parseTheme(xmlp, name, ti, baseFso);
                            ti.putTheme(name, tiChild);
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
                mathInterpreter.setEnv(oldEnv);
            }
            return ti;
        }

        private void parseParam(XMLParser xmlp, FileSystemObject baseFso, string tagName, ThemeInfoImpl parent, ParameterMapImpl target)
        {
            try
            {
                xmlp.require(XmlPullParser.START_TAG, null, tagName);
                string name = xmlp.getAttributeNotNull("name");
                xmlp.nextTag();
                string valueTagName = xmlp.getName();
                Object value = parseValue(xmlp, valueTagName, name, baseFso, parent);
                xmlp.require(XmlPullParser.END_TAG, null, valueTagName);
                xmlp.nextTag();
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                if (value is IDictionary<string, Renderer.Image>)
                {
                    IDictionary<string, Renderer.Image> map = (IDictionary<string, Renderer.Image>)value;
                    foreach (string key in map.Keys)
                    {
                        target.put(key, map[key]);
                    }
                }
                else if (value is IDictionary<string, Renderer.MouseCursor>)
                {
                    IDictionary<string, Renderer.MouseCursor> map = (IDictionary<string, Renderer.MouseCursor>)value;
                    foreach (string key in map.Keys)
                    {
                        target.put(key, map[key]);
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
                    target.put(map);
                }
                else
                {
                    ParserUtil.checkNameNotEmpty(name, xmlp);
                    target.put(name, value);
                }
            }
            catch (FormatException ex)
            {
                throw xmlp.error("unable to parse value", ex);
            }
        }

        private ParameterListImpl parseList(XMLParser xmlp, FileSystemObject baseFso, ThemeInfoImpl parent)
        {
            ParameterListImpl result = new ParameterListImpl(this, parent);
            xmlp.nextTag();
            while (xmlp.isStartTag())
            {
                string tagName = xmlp.getName();
                Object obj = parseValue(xmlp, tagName, null, baseFso, parent);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                result.parameters.Add(obj);
                xmlp.nextTag();
            }
            return result;
        }

        private ParameterMapImpl parseMap(XMLParser xmlp, FileSystemObject baseFso, string name, ThemeInfoImpl parent)
        {
            ParameterMapImpl result = new ParameterMapImpl(this, parent);
            if (xmlp.parseBoolFromAttribute("merge", false))
            {
                if (parent == null)
                {
                    throw xmlp.error("Can't merge on top level");
                }
                Object obj = parent.getParam(name);
                if (obj is ParameterMapImpl)
                {
                    ParameterMapImpl baseMap = (ParameterMapImpl)obj;
                    result.copy(baseMap);
                }
                else if (obj != null)
                {
                    throw xmlp.error("Can only merge with map - found a " + obj.GetType().Name);
                }
            }
            string reference = xmlp.getAttributeValue(null, "ref");
            if (reference != null)
            {
                Object obj = parent.getParam(reference);
                if (obj == null)
                {
                    obj = constants.getParam(reference);
                    if (obj == null)
                    {
                        throw new IOException("Referenced map not found: " + reference);
                    }
                }
                if (obj is ParameterMapImpl)
                {
                    ParameterMapImpl baseMap = (ParameterMapImpl)obj;
                    result.copy(baseMap);
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
                parseParam(xmlp, baseFso, "param", parent, result);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
            }
            return result;
        }

        private Object parseValue(XMLParser xmlp, string tagName, string wildcardName, FileSystemObject baseFso, ThemeInfoImpl parent)
        {
            try
            {
                if ("list".Equals(tagName))
                {
                    return parseList(xmlp, baseFso, parent);
                }
                if ("map".Equals(tagName))
                {
                    return parseMap(xmlp, baseFso, wildcardName, parent);
                }
                if ("inputMapDef".Equals(tagName))
                {
                    return parseInputMap(xmlp, wildcardName, parent);
                }
                if ("fontDef".Equals(tagName))
                {
                    return parseFont(xmlp, baseFso);
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
                    return ParserUtil.parseColor(xmlp, value, constants);
                }
                if ("float".Equals(tagName))
                {
                    return parseMath(xmlp, value).floatValue();
                }
                if ("int".Equals(tagName))
                {
                    return parseMath(xmlp, value).intValue();
                }
                if ("string".Equals(tagName))
                {
                    return value;
                }
                if ("font".Equals(tagName))
                {
                    Font font = fonts[value];
                    if (font == null)
                    {
                        throw xmlp.error("Font \"" + value + "\" not found");
                    }
                    return font;
                }
                if ("border".Equals(tagName))
                {
                    return parseObject<Border>(xmlp, value, typeof(Border));
                }
                if ("dimension".Equals(tagName))
                {
                    return parseObject<Dimension>(xmlp, value, typeof(Dimension));
                }
                if ("gap".Equals(tagName) || "size".Equals(tagName))
                {
                    return parseObject<DialogLayout.Gap>(xmlp, value, typeof(DialogLayout.Gap));
                }
                if ("constant".Equals(tagName))
                {
                    Object result = constants.getParam(value);
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
                        return imageManager.getImages(value, wildcardName);
                    }
                    return imageManager.getReferencedImage(xmlp, value);
                }
                if ("cursor".Equals(tagName))
                {
                    if (value.EndsWith(".*"))
                    {
                        if (wildcardName == null)
                        {
                            throw new ArgumentNullException("Wildcard's not allowed");
                        }
                        return imageManager.getCursors(value, wildcardName);
                    }
                    return imageManager.getReferencedCursor(xmlp, value);
                }
                if ("inputMap".Equals(tagName))
                {
                    return getInputMap(xmlp, value);
                }
                throw xmlp.error("Unknown type \"" + tagName + "\" specified");
            }
            catch (FormatException ex)
            {
                throw xmlp.error("unable to parse value", ex);
            }
        }

        private Number parseMath(XMLParser xmlp, string str)
        {
            try
            {
                return mathInterpreter.execute(str);
            }
            catch (ParseException ex)
            {
                throw xmlp.error("unable to evaluate", unwrap(ex));
            }
        }

        private T parseObject<T>(XMLParser xmlp, string str, Type type)
        {
            try
            {
                return mathInterpreter.executeCreateObject<T>(str, type);
            }
            catch (ParseException ex)
            {
                throw xmlp.error("unable to evaluate", unwrap(ex));
            }
        }

        private Exception unwrap(ParseException ex)
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

        internal ThemeInfo resolveWildcard(string baseStr, string name, bool useFallback)
        {
            if(!(baseStr.Length == 0 || baseStr.EndsWith(".")))
            {
                throw new Exception("Assertion exception");
            }
            string fullPath = baseStr + name;
            ThemeInfo info = findThemeInfo(fullPath, false, useFallback);
            if (info != null && ((ThemeInfoImpl)info).maybeUsedFromWildcard)
            {
                return info;
            }
            return null;
        }

        class MathInterpreter : AbstractMathInterpreter
        {
            private ThemeInfoImpl env;
            private ThemeManager themeManager;

            public MathInterpreter(ThemeManager themeManager)
            {
                this.themeManager = themeManager;
            }

            public ThemeInfoImpl setEnv(ThemeInfoImpl env)
            {
                ThemeInfoImpl oldEnv = this.env;
                this.env = env;
                return oldEnv;
            }

            public override void accessVariable(string name)
            {
                for (ThemeInfoImpl e = env; e != null; e = e.parent)
                {
                    Object objx = e.getParam(name);
                    if (objx != null)
                    {
                        push(objx);
                        return;
                    }
                    objx = e.getChildThemeImpl(name, false);
                    if (objx != null)
                    {
                        push(objx);
                        return;
                    }
                }

                Object obj = this.themeManager.constants.getParam(name);
                if (obj != null)
                {
                    push(obj);
                    return;
                }

                if (this.themeManager.fonts.ContainsKey(name))
                {
                    push(this.themeManager.fonts[name]);
                    return;
                }

                throw new ArgumentNullException("variable not found: " + name);
            }


            protected override Object accessField(Object obj, string field)
            {
                if (obj is ThemeInfoImpl)
                {
                    Object result = ((ThemeInfoImpl)obj).getTheme(field);
                    if (result != null)
                    {
                        return result;
                    }
                }
                if (obj is ParameterMapImpl)
                {
                    Object result = ((ParameterMapImpl)obj).getParam(field);
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
                return base.accessField(obj, field);
            }
        }
    }
}
