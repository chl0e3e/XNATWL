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
using XNATWL.Property;

namespace XNATWL
{
    public abstract class MenuElement
    {
        private String _name;
        private String _theme;
        private bool _enabled = true;
        private Object _tooltipContent;
        private PropertyChangeSupport _pcs;
        private Alignment _alignment;

        public MenuElement()
        {
        }

        public MenuElement(String name)
        {
            this._name = name;
        }

        public String GetName()
        {
            return _name;
        }

        public MenuElement SetName(String name)
        {
            String oldName = this._name;
            this._name = name;
            FirePropertyChange("name", oldName, name);
            return this;
        }

        public String GetTheme()
        {
            return _theme;
        }

        public MenuElement SetTheme(String theme)
        {
            String oldTheme = this._theme;
            this._theme = theme;
            FirePropertyChange("theme", oldTheme, theme);
            return this;
        }

        public bool IsEnabled()
        {
            return _enabled;
        }

        public MenuElement SetEnabled(bool enabled)
        {
            bool oldEnabled = this._enabled;
            this._enabled = enabled;
            FirePropertyChange("enabled", oldEnabled, enabled);
            return this;
        }

        public Object GetTooltipContent()
        {
            return _tooltipContent;
        }

        public MenuElement SetTooltipContent(Object tooltip)
        {
            Object oldTooltip = this._tooltipContent;
            this._tooltipContent = tooltip;
            FirePropertyChange("tooltipContent", oldTooltip, tooltip);
            return this;
        }

        public Alignment GetAlignment()
        {
            return _alignment;
        }

        /**
         * Sets the alignment used for this element in the menubar.
         * The default value is {@code null} which means that the class based
         * default is used.
         * 
         * @param alignment the alignment or null.
         * @return this
         * @see Menu#setClassAlignment(java.lang.Class, de.matthiasmann.twl.Alignment) 
         * @see Menu#getClassAlignment(java.lang.Class) 
         */
        public MenuElement SetAlignment(Alignment alignment)
        {
            Alignment oldAlignment = this._alignment;
            this._alignment = alignment;
            FirePropertyChange("alignment", oldAlignment, alignment);
            return this;
        }

        protected internal abstract Widget CreateMenuWidget(MenuManager mm, int level);

        public void AddPropertyChangeListener(PropertyChangeListener listener)
        {
            if (_pcs == null)
            {
                _pcs = new PropertyChangeSupport(this);
            }

            _pcs.AddPropertyChangeListener(listener);
        }

        public void AddPropertyChangeListener(String propertyName, PropertyChangeListener listener)
        {
            if (_pcs == null)
            {
                _pcs = new PropertyChangeSupport(this);
            }

            _pcs.AddPropertyChangeListener(propertyName, listener);
        }

        public void RemovePropertyChangeListener(String propertyName, PropertyChangeListener listener)
        {
            if (_pcs != null)
            {
                _pcs.RemovePropertyChangeListener(propertyName, listener);
            }
        }

        public void RemovePropertyChangeListener(PropertyChangeListener listener)
        {
            if (_pcs != null)
            {
                _pcs.RemovePropertyChangeListener(listener);
            }
        }

        protected void FirePropertyChange(String propertyName, bool oldValue, bool newValue)
        {
            if (_pcs != null)
            {
                _pcs.FirePropertyChange(propertyName, oldValue, newValue);
            }
        }

        protected void FirePropertyChange(String propertyName, int oldValue, int newValue)
        {
            if (_pcs != null)
            {
                _pcs.FirePropertyChange(propertyName, oldValue, newValue);
            }
        }

        protected void FirePropertyChange(String propertyName, Object oldValue, Object newValue)
        {
            if (_pcs != null)
            {
                _pcs.FirePropertyChange(propertyName, oldValue, newValue);
            }
        }

        /**
         * Helper method to apply the theme from the menu element to the widget
         * if it was set, otherwise the defaultTheme is used.
         * @param w the Widget to which the theme should be applied
         * @param defaultTheme the defaultTheme when none was set 
         */
        protected void SetWidgetTheme(Widget w, String defaultTheme)
        {
            if (_theme != null)
            {
                w.SetTheme(_theme);
            }
            else
            {
                w.SetTheme(defaultTheme);
            }
        }

        internal class MenuBtn : Button, PropertyChangeListener
        {
            private MenuElement _menuElement;
            public MenuBtn(MenuElement menuElement)
            {
                this._menuElement = menuElement;
                Sync();
            }
            
            protected override void AfterAddToGUI(GUI gui)
            {
                base.AfterAddToGUI(gui);
                this._menuElement.AddPropertyChangeListener(this);
            }

            protected override void BeforeRemoveFromGUI(GUI gui)
            {
                this._menuElement.RemovePropertyChangeListener(this);
                base.BeforeRemoveFromGUI(gui);
            }

            public virtual void PropertyChange(PropertyChangeEvent evt)
            {
                Sync();
            }

            protected void Sync()
            {
                SetEnabled(this._menuElement.IsEnabled());
                SetTooltipContent(this._menuElement.GetTooltipContent());
                SetText(this._menuElement.GetName());
            }
        }
    }
}
