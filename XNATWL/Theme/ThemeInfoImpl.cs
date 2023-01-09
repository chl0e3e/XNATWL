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
using System.Text;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    public class ThemeInfoImpl : ParameterMapImpl, ThemeInfo
    {
        private String name;
        private CascadedHashMap<string, ThemeInfoImpl> children;
        internal bool maybeUsedFromWildcard;
        internal String wildcardImportPath;

        public ThemeInfoImpl(ThemeManager manager, string name, ThemeInfoImpl parent) : base(manager, parent)
        {
            this.name = name;
            this.children = new CascadedHashMap<string, ThemeInfoImpl>();
        }

        public void themeInfoImplCopy(ThemeInfoImpl src)
        {
            base.copy(src);
            children.CollapseAndSetFallback(src.children);
            wildcardImportPath = src.wildcardImportPath;
        }

        public String getName()
        {
            return name;
        }

        public ThemeInfo getChildTheme(String theme)
        {
            return getChildThemeImpl(theme, true);
        }

        public ThemeInfo getChildThemeImpl(String theme, bool useFallback)
        {
            ThemeInfo info = (ThemeInfo) children.CascadingEntry(theme);
            if (info == null)
            {
                if (wildcardImportPath != null)
                {
                    info = manager.resolveWildcard(wildcardImportPath, theme, useFallback);
                }
                if (info == null && useFallback)
                {
                    DebugHook.getDebugHook().missingChildTheme(this, theme);
                }
            }
            return info;
        }

        public ThemeInfoImpl getTheme(String name)
        {
            return (ThemeInfoImpl) children.CascadingEntry(name);
        }

        public void putTheme(String name, ThemeInfoImpl child)
        {
            children.PutCascadingEntry(name, child);
        }

        public String getThemePath()
        {
            return getThemePath(0).ToString();
        }

        private StringBuilder getThemePath(int length)
        {
            StringBuilder sb;
            length += getName().Length;
            if (parent != null)
            {
                sb = parent.getThemePath(length + 1);
                sb.Append('.');
            }
            else
            {
                sb = new StringBuilder(length);
            }
            sb.Append(getName());
            return sb;
        }
    }
}
