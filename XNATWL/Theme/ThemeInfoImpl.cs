using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        void copy(ThemeInfoImpl src)
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
