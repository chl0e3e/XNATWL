using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Property;

namespace XNATWL
{
    public abstract class MenuElement
    {
        private String name;
        private String theme;
        private bool enabled = true;
        private Object tooltipContent;
        private PropertyChangeSupport pcs;
        private Alignment alignment;

        public MenuElement()
        {
        }

        public MenuElement(String name)
        {
            this.name = name;
        }

        public String getName()
        {
            return name;
        }

        public MenuElement setName(String name)
        {
            String oldName = this.name;
            this.name = name;
            firePropertyChange("name", oldName, name);
            return this;
        }

        public String getTheme()
        {
            return theme;
        }

        public MenuElement setTheme(String theme)
        {
            String oldTheme = this.theme;
            this.theme = theme;
            firePropertyChange("theme", oldTheme, theme);
            return this;
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public MenuElement setEnabled(bool enabled)
        {
            bool oldEnabled = this.enabled;
            this.enabled = enabled;
            firePropertyChange("enabled", oldEnabled, enabled);
            return this;
        }

        public Object getTooltipContent()
        {
            return tooltipContent;
        }

        public MenuElement setTooltipContent(Object tooltip)
        {
            Object oldTooltip = this.tooltipContent;
            this.tooltipContent = tooltip;
            firePropertyChange("tooltipContent", oldTooltip, tooltip);
            return this;
        }

        public Alignment getAlignment()
        {
            return alignment;
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
        public MenuElement setAlignment(Alignment alignment)
        {
            Alignment oldAlignment = this.alignment;
            this.alignment = alignment;
            firePropertyChange("alignment", oldAlignment, alignment);
            return this;
        }

        protected internal abstract Widget createMenuWidget(MenuManager mm, int level);

        public void addPropertyChangeListener(PropertyChangeListener listener)
        {
            if (pcs == null)
            {
                pcs = new PropertyChangeSupport(this);
            }
            pcs.addPropertyChangeListener(listener);
        }

        public void addPropertyChangeListener(String propertyName, PropertyChangeListener listener)
        {
            if (pcs == null)
            {
                pcs = new PropertyChangeSupport(this);
            }
            pcs.addPropertyChangeListener(propertyName, listener);
        }

        public void removePropertyChangeListener(String propertyName, PropertyChangeListener listener)
        {
            if (pcs != null)
            {
                pcs.removePropertyChangeListener(propertyName, listener);
            }
        }

        public void removePropertyChangeListener(PropertyChangeListener listener)
        {
            if (pcs != null)
            {
                pcs.removePropertyChangeListener(listener);
            }
        }

        protected void firePropertyChange(String propertyName, bool oldValue, bool newValue)
        {
            if (pcs != null)
            {
                pcs.firePropertyChange(propertyName, oldValue, newValue);
            }
        }

        protected void firePropertyChange(String propertyName, int oldValue, int newValue)
        {
            if (pcs != null)
            {
                pcs.firePropertyChange(propertyName, oldValue, newValue);
            }
        }

        protected void firePropertyChange(String propertyName, Object oldValue, Object newValue)
        {
            if (pcs != null)
            {
                pcs.firePropertyChange(propertyName, oldValue, newValue);
            }
        }

        /**
         * Helper method to apply the theme from the menu element to the widget
         * if it was set, otherwise the defaultTheme is used.
         * @param w the Widget to which the theme should be applied
         * @param defaultTheme the defaultTheme when none was set 
         */
        protected void setWidgetTheme(Widget w, String defaultTheme)
        {
            if (theme != null)
            {
                w.setTheme(theme);
            }
            else
            {
                w.setTheme(defaultTheme);
            }
        }

        internal class MenuBtn : Button, PropertyChangeListener
        {
            private MenuElement _menuElement;
            public MenuBtn(MenuElement menuElement)
            {
                this._menuElement = menuElement;
                sync();
            }
            
            protected override void afterAddToGUI(GUI gui)
            {
                base.afterAddToGUI(gui);
                this._menuElement.addPropertyChangeListener(this);
            }

            protected override void beforeRemoveFromGUI(GUI gui)
            {
                this._menuElement.removePropertyChangeListener(this);
                base.beforeRemoveFromGUI(gui);
            }

            public virtual void propertyChange(PropertyChangeEvent evt)
            {
                sync();
            }

            protected void sync()
            {
                setEnabled(this._menuElement.isEnabled());
                setTooltipContent(this._menuElement.getTooltipContent());
                setText(this._menuElement.getName());
            }
        }
    }
}
