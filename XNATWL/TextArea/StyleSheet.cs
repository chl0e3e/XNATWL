using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.DialogLayout;
using static XNATWL.Utils.Logger;
using XNATWL.Renderer;
using XNATWL.Utils;
using System.Collections;
using XNATWL.IO;

namespace XNATWL.TextArea
{
    public class StyleSheet : StyleSheetResolver
    {
        static Object NULL = new Object();

        private static Selector PRE_SELECTOR = new Selector("pre", null, null, null, null);
        static StyleSheet()
        {
            (PRE_SELECTOR.style = new CSSStyle()).Put(StyleAttribute.PREFORMATTED, true);
            PRE_SELECTOR.score = 0x100;
        }

        private List<Selector> rules;
        private Dictionary<Style, Object> cache;
        private List<AtRule> atrules;

        public StyleSheet()
        {
            this.rules = new List<Selector>();
            this.cache = new Dictionary<Style, Object>();

            rules.Add(PRE_SELECTOR);
        }

        public void parse(IO.FileSystemObject fso)
        {
            FileStream stream = File.OpenRead(fso.Path);
            try
            {
                parse(new StreamReader(stream));
            }
            finally
            {
                stream.Close();
            }
        }

        public void parse(String style)
        {
            parse(new StreamReader(style));
        }

        public void parse(StreamReader r)
        {
            Parser parser = new Parser(r);
            List<Selector> selectors = new List<Selector>();
            int what;
            while ((what = parser.yylex()) != Parser.EOF)
            {
                if (what == Parser.ATRULE)
                {
                    parser.expect(Parser.IDENT);
                    AtRule atrule = new AtRule(parser.yytext());
                    parser.expect(Parser.STYLE_BEGIN);

                    while ((what = parser.yylex()) != Parser.STYLE_END)
                    {
                        if (what != Parser.IDENT)
                        {
                            parser.unexpected();
                        }
                        String key = parser.yytext();
                        parser.expect(Parser.COLON);
                        what = parser.yylex();
                        if (what != Parser.SEMICOLON && what != Parser.STYLE_END)
                        {
                            parser.unexpected();
                        }
                        String value = TextUtil.trim(parser.sb.ToString(), 0);
                        try
                        {
                            atrule.entries.Add(key, value);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                        }
                        if (what == Parser.STYLE_END)
                        {
                            break;
                        }
                    }

                    if (atrules == null)
                    {
                        atrules = new List<AtRule>();
                    }
                    atrules.Add(atrule);
                    continue;
                }

                Selector selector = null;
            selectorloop: for (; ; )
                {
                    String element = null;
                    String className = null;
                    String pseudoClass = null;
                    String id = null;
                    parser.sawWhitespace = false;
                    if (what == Parser.DOT || what == Parser.HASH || what == Parser.COLON)
                    {
                        //
                    }
                    else if (what == Parser.IDENT)
                    {
                        element = parser.yytext();
                    }
                    else if (what == Parser.STAR)
                    {
                        what = parser.yylex();
                    }
                    else
                    {
                        parser.unexpected();
                    }

                    while ((what == Parser.DOT || what == Parser.HASH || what == Parser.COLON) && !parser.sawWhitespace)
                    {
                        parser.expect(Parser.IDENT);
                        String text = parser.yytext();
                        if (what == Parser.DOT)
                        {
                            className = text;
                        }
                        else if (what == Parser.COLON)
                        {
                            pseudoClass = text;
                        }
                        else
                        {
                            id = text;
                        }
                        what = parser.yylex();
                    }
                    selector = new Selector(element, className, id, pseudoClass, selector);
                    if (what == Parser.GT)
                    {
                        selector.directChild = true;
                        what = parser.yylex();
                    }
                    else if (what == Parser.COMMA || what == Parser.STYLE_BEGIN)
                    {
                        break;
                    }
                }

                // to ensure that the head of the selector matches the head of the
                // style and not skip ahead we use the directChild flag
                // this causes an offset of 1 for all scores which doesn't matter
                selector.directChild = true;
                selectors.Add(selector);

                if (what == Parser.STYLE_BEGIN)
                {
                    CSSStyle style = new CSSStyle();

                    while ((what = parser.yylex()) != Parser.STYLE_END)
                    {
                        if (what != Parser.IDENT)
                        {
                            parser.unexpected();
                        }
                        String key = parser.yytext();
                        parser.expect(Parser.COLON);
                        what = parser.yylex();
                        if (what != Parser.SEMICOLON && what != Parser.STYLE_END)
                        {
                            parser.unexpected();
                        }
                        String value = TextUtil.trim(parser.sb.ToString(), 0);
                        try
                        {
                            style.parseCSSAttribute(key, value);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                        }
                        if (what == Parser.STYLE_END)
                        {
                            break;
                        }
                    }

                    for (int i = 0, n = selectors.Count; i < n; i++)
                    {
                        Style selectorStyle = style;
                        selector = selectors[i];
                        if (selector.pseudoClass != null)
                        {
                            selectorStyle = transformStyle(style, selector.pseudoClass);
                        }
                        rules.Add(selector);
                        int score = 0;
                        for (Selector s = selector; s != null; s = s.tail)
                        {
                            if (s.directChild)
                            {
                                score += 0x1;
                            }
                            if (s.Element != null)
                            {
                                score += 0x100;
                            }
                            if (s.ClassName != null)
                            {
                                score += 0x10000;
                            }
                            if (s.ID != null)
                            {
                                score += 0x1000000;
                            }
                        }
                        // only needed on head
                        selector.score = score;
                        selector.style = selectorStyle;
                    }

                    selectors.Clear();
                    break;
                }
                else if(what == Parser.COMMA)
                {

                }
                else
                {
                    parser.unexpected();
                }
            }
        }

        public int getNumAtRules()
        {
            return (atrules != null) ? atrules.Count : 0;
        }

        public AtRule getAtRule(int idx)
        {
            if (atrules == null)
            {
                throw new IndexOutOfRangeException();
            }
            return atrules[idx];
        }

        public void registerFonts(FontMapper fontMapper, FileSystemObject baseFso)
        {
            if (atrules == null)
            {
                return;
            }
            foreach (AtRule atrule in atrules)
            {
                if ("font-face".Equals(atrule.name))
                {
                    String family = atrule.get("font-family");
                    String src = atrule.get("src");

                    if (family != null && src != null)
                    {
                        List<string> srcs = CSSStyle.parseList(src, 0);
                        foreach(string srcEntry in srcs)
                        {
                            String url = CSSStyle.stripURL(srcEntry);
                            try
                            {
                                fontMapper.RegisterFont(family, new FileSystemObject(baseFso, url));
                            }
                            catch (IOException ex)
                            {
                                Logger.GetLogger(typeof(StyleSheet)).log(Level.SEVERE,
                                        "Could not register font: " + url, ex);
                            }
                        }
                    }
                }
            }
        }

        public void LayoutFinished()
        {
            cache.Clear();
        }

        public void StartLayout()
        {
            cache.Clear();
        }

        public Style Resolve(Style style)
        {
            Object cacheData = cache[style];
            if (cacheData == null)
            {
                return resolveSlow(style);
            }
            if (cacheData == NULL)
            {
                return null;
            }
            return (Style)cacheData;
        }

        private Style resolveSlow(Style style)
        {
            Selector[] candidates = new Selector[rules.Count];
            int numCandidates = 0;

            // find all possible candidates
            for (int i = 0, n = rules.Count; i < n; i++)
            {
                Selector selector = rules[i];
                if (matches(selector, style))
                {
                    candidates[numCandidates++] = selector;
                }
            }

            // sort according to rule priority - this needs a stable sort
            if (numCandidates > 1)
            {
                Array.Sort(candidates, 0, numCandidates);
            }

            Style result = null;
            bool copy = true;

            // merge all matching rules
            for (int i = 0, n = numCandidates; i < n; i++)
            {
                Style ruleStyle = candidates[i].style;
                if (result == null)
                {
                    result = ruleStyle;
                }
                else
                {
                    if (copy)
                    {
                        result = new Style(result);
                        copy = false;
                    }
                    result.PutAll(ruleStyle);
                }
            }

            putIntoCache(style, result);
            return result;
        }

        private void putIntoCache(Style key, Style style)
        {
            cache.Add(key, (style == null) ? NULL : style);
        }

        private bool matches(Selector selector, Style style)
        {
            do
            {
                StyleSheetKey styleSheetKey = style.StyleSheetKey;
                if (styleSheetKey != null)
                {
                    if (selector.Matches(styleSheetKey))
                    {
                        selector = selector.tail;
                        if (selector == null)
                        {
                            return true;
                        }
                    }
                    else if (selector.directChild)
                    {
                        return false;
                    }
                }
                style = style.Parent;
            } while (style != null);
            return false;
        }

        private Style transformStyle(CSSStyle style, String pseudoClass)
        {
            Style result = new Style(style.Parent, style.StyleSheetKey);
            if ("hover".Equals(pseudoClass))
            {
                result.Put(StyleAttribute.COLOR_HOVER, style.GetRaw(StyleAttribute.COLOR));
                result.Put(StyleAttribute.BACKGROUND_COLOR_HOVER, style.GetRaw(StyleAttribute.BACKGROUND_COLOR));
                result.Put(StyleAttribute.TEXT_DECORATION_HOVER, style.GetRaw(StyleAttribute.TEXT_DECORATION));
            }
            return result;
        }

        public class Selector : StyleSheetKey, IComparable<Selector>
        {
            internal String pseudoClass;
            internal Selector tail;
            internal bool directChild;
            internal Style style;
            internal int score;

            public Selector(String element, String className, String id, String pseudoClass, Selector tail) : base(element, className, id)
            {
                this.pseudoClass = pseudoClass;
                this.tail = tail;
            }

            public int CompareTo(Selector other)
            {
                return this.score - other.score;
            }
        }

        public class AtRule : Dictionary<string, string>//<DictionaryEntry<String, String>>
        {
            internal String name;
            internal Dictionary<String, String> entries;

            public AtRule(String name)
            {
                this.name = name;
                this.entries = new Dictionary<String, String>();
            }

            public String getName()
            {
                return name;
            }

            public String get(String key)
            {
                return entries[key];
            }
        }
    }
}
