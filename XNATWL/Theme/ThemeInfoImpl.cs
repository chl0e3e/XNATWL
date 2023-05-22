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
    /// <summary>
    /// The ThemeInfo implementation. The theme info class contains all theme specific information for a given widget.
    /// </summary>
    public class ThemeInfoImpl : ParameterMapImpl, ThemeInfo
    {
        private string _name;
        private CascadedHashMap<string, ThemeInfoImpl> _children;
        protected internal bool _maybeUsedFromWildcard;
        protected internal String _wildcardImportPath;

        /// <summary>
        /// Initialise a ThemeInfo which contains all theme specific information for a given widget
        /// </summary>
        /// <param name="manager">Parent theme manager</param>
        /// <param name="name">Name of theme</param>
        /// <param name="parent">Parent <see cref="ThemeInfoImpl"/></param>
        public ThemeInfoImpl(ThemeManager manager, string name, ThemeInfoImpl parent) : base(manager, parent)
        {
            this._name = name;
            this._children = new CascadedHashMap<string, ThemeInfoImpl>();
        }

        /// <summary>
        /// Copy theme information parameters from given <paramref name="src"/>
        /// </summary>
        /// <param name="src">Source <see cref="ThemeInfoImpl"/></param>
        public void ThemeInfoImplCopy(ThemeInfoImpl src)
        {
            base.Copy(src);
            _children.CollapseAndSetFallback(src._children);
            _wildcardImportPath = src._wildcardImportPath;
        }

        /// <summary>
        /// Given name of the theme info
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Get child <see cref="ThemeInfo"/> implementation
        /// </summary>
        /// <param name="theme">Child theme identifier</param>
        /// <returns>Child <see cref="ThemeInfo"/></returns>
        public ThemeInfo GetChildTheme(String theme)
        {
            return GetChildThemeImpl(theme, true);
        }

        /// <summary>
        /// Lookup the child theme in this object's <see cref="_children"/> field, with additional logic if contains copied theme info (name stored in _wildcardImportPath)
        /// </summary>
        /// <param name="theme">theme identifier</param>
        /// <param name="useFallback">fallback will gracefully error to console</param>
        /// <returns>Child <see cref="ThemeInfo"/></returns>
        public ThemeInfo GetChildThemeImpl(String theme, bool useFallback)
        {
            ThemeInfo info = (ThemeInfo) _children.CascadingEntry(theme);
            if (info == null)
            {
                if (_wildcardImportPath != null)
                {
                    info = _manager.ResolveWildcard(_wildcardImportPath, theme, useFallback);
                }
                if (info == null && useFallback)
                {
                    DebugHook.getDebugHook().MissingChildTheme(this, theme);
                }
            }
            return info;
        }

        /// <summary>
        /// Lookup theme name in the cascading map
        /// </summary>
        /// <param name="name">Child theme name</param>
        /// <returns><see cref="ThemeInfoImpl"/></returns>
        public ThemeInfoImpl GetTheme(String name)
        {
            return (ThemeInfoImpl) _children.CascadingEntry(name);
        }

        /// <summary>
        /// Add a child theme to the cascading map
        /// </summary>
        /// <param name="name">Name of theme</param>
        /// <param name="child">Child theme object</param>
        public void PutTheme(String name, ThemeInfoImpl child)
        {
            _children.PutCascadingEntry(name, child);
        }

        /// <summary>
        /// Returns the first part of the theme path (period delimited)
        /// </summary>
        /// <returns></returns>
        public String GetThemePath()
        {
            return GetThemePath(0).ToString();
        }

        /// <summary>
        /// Returns a string builder with the parent theme path and this theme's name concatenated.
        /// </summary>
        /// <returns><see cref="StringBuilder"/></returns>
        private StringBuilder GetThemePath(int length)
        {
            StringBuilder sb;
            length += Name.Length;
            if (_parent != null)
            {
                sb = _parent.GetThemePath(length + 1);
                sb.Append('.');
            }
            else
            {
                sb = new StringBuilder(length);
            }
            sb.Append(Name);
            return sb;
        }
    }
}
