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
using static XNATWL.Utils.Logger;
using XNATWL.Renderer;
using XNATWL.Utils;
using XNATWL.IO;

namespace XNATWL.TextAreaModel
{
    public class StyleSheet : StyleSheetResolver
    {
        static Object NULL = new Object();

        private static Selector PRE_SELECTOR = new Selector("pre", null, null, null, null);
        static StyleSheet()
        {
            (PRE_SELECTOR._style = new CSSStyle()).Put(StyleAttribute.PREFORMATTED, true);
            PRE_SELECTOR._score = 0x100;
        }

        private List<Selector> _rules;
        private Dictionary<Style, Object> _cache;
        private List<AtRule> _atRules;

        public StyleSheet()
        {
            this._rules = new List<Selector>();
            this._cache = new Dictionary<Style, Object>();

            _rules.Add(PRE_SELECTOR);
        }

        public void Parse(IO.FileSystemObject fso)
        {
            FileStream stream = File.OpenRead(fso.Path);
            try
            {
                Parse(new StreamReader(stream));
            }
            finally
            {
                stream.Close();
            }
        }

        public void Parse(String style)
        {
            Stream s = new MemoryStream(Encoding.UTF8.GetBytes(style));
            try
            {
                Parse(new StreamReader(s));
            }
            finally
            {
                s.Close();
            }
        }

        public void Parse(StreamReader r)
        {
            Parser parser = new Parser(r);
            List<Selector> selectors = new List<Selector>();
            int what;
            while ((what = parser.YYLex()) != Parser.EOF)
            {
                if (what == Parser.ATRULE)
                {
                    parser.Expect(Parser.IDENT);
                    AtRule atrule = new AtRule(parser.YYText());
                    parser.Expect(Parser.STYLE_BEGIN);

                    while ((what = parser.YYLex()) != Parser.STYLE_END)
                    {
                        if (what != Parser.IDENT)
                        {
                            parser.Unexpected();
                        }
                        String key = parser.YYText();
                        parser.Expect(Parser.COLON);
                        what = parser.YYLex();
                        if (what != Parser.SEMICOLON && what != Parser.STYLE_END)
                        {
                            parser.Unexpected();
                        }
                        String value = TextUtil.trim(parser._stringBuilder.ToString(), 0);
                        try
                        {
                            atrule._entries.Add(key, value);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                        }
                        if (what == Parser.STYLE_END)
                        {
                            break;
                        }
                    }

                    if (_atRules == null)
                    {
                        _atRules = new List<AtRule>();
                    }
                    _atRules.Add(atrule);
                    continue;
                }

                Selector selector = null;
            selectorloop: for (; ; )
                {
                    String element = null;
                    String className = null;
                    String pseudoClass = null;
                    String id = null;
                    parser._sawWhitespace = false;
                    if (what == Parser.DOT || what == Parser.HASH || what == Parser.COLON)
                    {
                        //
                    }
                    else if (what == Parser.IDENT)
                    {
                        element = parser.YYText();
                        what = parser.YYLex();
                    }
                    else if (what == Parser.STAR)
                    {
                        what = parser.YYLex();
                    }
                    else
                    {
                        parser.Unexpected();
                    }

                    while ((what == Parser.DOT || what == Parser.HASH || what == Parser.COLON) && !parser._sawWhitespace)
                    {
                        parser.Expect(Parser.IDENT);
                        String text = parser.YYText();
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
                        what = parser.YYLex();
                    }
                    selector = new Selector(element, className, id, pseudoClass, selector);
                    if (what == Parser.GT)
                    {
                        selector._directChild = true;
                        what = parser.YYLex();
                    }
                    else if (what == Parser.COMMA || what == Parser.STYLE_BEGIN)
                    {
                        break;
                    }
                }

                // to ensure that the head of the selector matches the head of the
                // style and not skip ahead we use the directChild flag
                // this causes an offset of 1 for all scores which doesn't matter
                selector._directChild = true;
                selectors.Add(selector);

                if (what == Parser.STYLE_BEGIN)
                {
                    CSSStyle style = new CSSStyle();

                    while ((what = parser.YYLex()) != Parser.STYLE_END)
                    {
                        if (what != Parser.IDENT)
                        {
                            parser.Unexpected();
                        }
                        String key = parser.YYText();
                        parser.Expect(Parser.COLON);
                        what = parser.YYLex();
                        if (what != Parser.SEMICOLON && what != Parser.STYLE_END)
                        {
                            parser.Unexpected();
                        }
                        String value = TextUtil.trim(parser._stringBuilder.ToString(), 0);
                        try
                        {
                            style.ParseCSSAttribute(key, value);
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
                        if (selector._pseudoClass != null)
                        {
                            selectorStyle = TransformStyle(style, selector._pseudoClass);
                        }
                        _rules.Add(selector);
                        int score = 0;
                        for (Selector s = selector; s != null; s = s._tail)
                        {
                            if (s._directChild)
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
                        selector._score = score;
                        selector._style = selectorStyle;
                    }

                    selectors.Clear();
                    break;
                }
                else if(what == Parser.COMMA)
                {

                }
                else
                {
                    parser.Unexpected();
                }
            }
        }

        public int NumberOfAtRules()
        {
            return (_atRules != null) ? _atRules.Count : 0;
        }

        public AtRule GetAtRule(int idx)
        {
            if (_atRules == null)
            {
                throw new IndexOutOfRangeException();
            }
            return _atRules[idx];
        }

        public void RegisterFonts(FontMapper fontMapper, FileSystemObject baseFso)
        {
            if (_atRules == null)
            {
                return;
            }
            foreach (AtRule atrule in _atRules)
            {
                if ("font-face".Equals(atrule._name))
                {
                    String family = atrule.Get("font-family");
                    String src = atrule.Get("src");

                    if (family != null && src != null)
                    {
                        List<string> srcs = CSSStyle.ParseList(src, 0);
                        foreach(string srcEntry in srcs)
                        {
                            String url = CSSStyle.StripURL(srcEntry);
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
            _cache.Clear();
        }

        public void StartLayout()
        {
            _cache.Clear();
        }

        public Style Resolve(Style style)
        {
            Object cacheData = null;
            if (_cache.ContainsKey(style))
            {
                cacheData = _cache[style];
            }
            if (cacheData == null)
            {
                return ResolveSlow(style);
            }
            if (cacheData == NULL)
            {
                return null;
            }
            return (Style)cacheData;
        }

        private Style ResolveSlow(Style style)
        {
            Selector[] candidates = new Selector[_rules.Count];
            int numCandidates = 0;

            // find all possible candidates
            for (int i = 0, n = _rules.Count; i < n; i++)
            {
                Selector selector = _rules[i];
                if (Matches(selector, style))
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
                Style ruleStyle = candidates[i]._style;
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

            PutIntoCache(style, result);
            return result;
        }

        private void PutIntoCache(Style key, Style style)
        {
            _cache.Add(key, (style == null) ? NULL : style);
        }

        private bool Matches(Selector selector, Style style)
        {
            do
            {
                StyleSheetKey styleSheetKey = style.StyleSheetKey;
                if (styleSheetKey != null)
                {
                    if (selector.Matches(styleSheetKey))
                    {
                        selector = selector._tail;
                        if (selector == null)
                        {
                            return true;
                        }
                    }
                    else if (selector._directChild)
                    {
                        return false;
                    }
                }
                style = style.Parent;
            } while (style != null);
            return false;
        }

        private Style TransformStyle(CSSStyle style, String pseudoClass)
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
            internal String _pseudoClass;
            internal Selector _tail;
            internal bool _directChild;
            internal Style _style;
            internal int _score;

            public Selector(String element, String className, String id, String pseudoClass, Selector tail) : base(element, className, id)
            {
                this._pseudoClass = pseudoClass;
                this._tail = tail;
            }

            public int CompareTo(Selector other)
            {
                return this._score - other._score;
            }
        }

        public class AtRule : Dictionary<string, string>//<DictionaryEntry<String, String>>
        {
            internal String _name;
            internal Dictionary<String, String> _entries;

            public AtRule(String name)
            {
                this._name = name;
                this._entries = new Dictionary<String, String>();
            }

            public String GetName()
            {
                return _name;
            }

            public String Get(String key)
            {
                return _entries[key];
            }
        }
    }
}
