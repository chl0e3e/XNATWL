using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.Logger;
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;
using System.Collections;

namespace XNATWL
{
    public class Menu : MenuElement, IEnumerable<MenuElement>
    {
        public static StateKey STATE_HAS_OPEN_MENUS = StateKey.Get("hasOpenMenus");

        public interface Listener
        {
            /**
             * Called before a menu popup is created.
             * <p>This is only called once until all open popups are closed.</p>
             * <p>When a menu is displayed as menu bar then no events are fired.</p>
             * 
             * @param menu the {@code Menu} which for which a popup will be opened
             */
            void menuOpening(Menu menu);

            /**
             * Called after a popup has been opened.
             * <p>When a menu is displayed as menu bar then no events are fired.</p>
             * 
             * @param menu the {@code Menu} which has been opened
             * @see #menuOpening(de.matthiasmann.twl.Menu) 
             */
            void menuOpened(Menu menu);

            /**
             * Called after a popup has been closed.
             * <p>When a menu is displayed as menu bar then no events are fired.</p>
             * 
             * @param menu the {@code Menu} which has been closed
             */
            void menuClosed(Menu menu);
        }

        private List<MenuElement> elements = new List<MenuElement>();
        private TypeMapping classAlignments = new TypeMapping();
        private String popupTheme;
        private Listener[] listeners;

        /**
         * Creates a new menu without name.
         * This constructor should be used for top level menus.
         *
         * @see #createMenuBar()
         * @see #createMenuBar(de.matthiasmann.twl.Widget)
         * @see #openPopupMenu(de.matthiasmann.twl.Widget)
         * @see #openPopupMenu(de.matthiasmann.twl.Widget, int, int)
         */
        public Menu()
        {
        }

        /**
         * Creates a new menu with the given name.
         * This constructor should be used used for sub menus. The name is used for
         * the button which opens this sub menu.
         *
         * @param name The name of the popup menu entry
         * @see #add(de.matthiasmann.twl.MenuElement)
         */
        public Menu(String name) : base(name)
        {

        }

        /*public void addListener(Listener listener)
        {
            listeners = CallbackSupport.addCallbackToList(listeners, listener, typeof(Listener));
        }

        public void removeListener(Listener listener)
        {
            listeners = CallbackSupport.removeCallbackFromList(listeners, listener);
        }*/

        /**
         * Returns the theme which is used when this menu is displayed as popup/sub menu.
         * @return the popup theme
         */
        public String getPopupTheme()
        {
            return popupTheme;
        }

        /**
         * Sets the theme which is used when this menun is displayed as popup/sub menu.
         * @param popupTheme the popup theme
         */
        public void setPopupTheme(String popupTheme)
        {
            String oldPopupTheme = this.popupTheme;
            this.popupTheme = popupTheme;
            firePropertyChange("popupTheme", oldPopupTheme, this.popupTheme);
        }

        /**
         * Sets the default alignment based on menu element subclasses.
         * <p>By default all alignments are {@link Alignment#FILL}</p>
         * 
         * @param clazz the class for which a default alignment should be set
         * @param value the alignment
         */
        public void setClassAlignment(Type clazz, Alignment value)
        {
            if (value == null)
            {
                throw new NullReferenceException("value");
            }
            if (value == Alignment.FILL)
            {
                classAlignments.RemoveByType(clazz);
            }
            else
            {
                classAlignments.SetByType(clazz, value);
            }
        }

        /**
         * Retrieves the default alignment for the given menu element class.
         * <p>By default all alignments are {@link Alignment#FILL}</p>
         * 
         * @param clazz the menu element class
         * @return the alignment
         */
        public Alignment getClassAlignment(Type clazz)
        {
            Alignment alignment = (Alignment) classAlignments.GetByType(clazz);
            if (alignment == null)
            {
                return Alignment.FILL;
            }
            return alignment;
        }

        /**
         * Returns the menu element at the given index.
         * @param index the index. Must be &lt; {code getNumElements}
         * @return the menu element
         * @throws IndexOutOfBoundsException if index is invalid
         * @see #getNumElements()
         */
        public MenuElement get(int index)
        {
            return elements[index];
        }

        /**
         * Returns the number of menu elements in this menu.
         * @return the number of menu elements
         */
        public int getNumElements()
        {
            return elements.Count;
        }

        /**
         * Removes all menu elements
         */
        public void clear()
        {
            elements.Clear();
        }

        /**
         * Adds the given menu element at the end. It is possible to add the same
         * menu element several times also in different menus.
         *
         * @param e the menu element
         * @return this
         */
        public Menu add(MenuElement e)
        {
            elements.Add(e);
            return this;
        }

        /**
         * Adds a {code MenuAction} element at the end. It is equivalent to
         * {code add(new MenuAction(name, cb)) }
         *
         * @param name the name of the menu action
         * @param cb the callback when the menu action has been selected
         * @return this
         */
        public Menu add(String name, Action cb)
        {
            return add(new MenuAction(name, cb));
        }

        /**
         * Adds a {code MenuCheckbox} element at the end.  It is equivalent to
         * {code add(new MenuCheckbox(name, model)) }
         *
         * @param name the name of the menu checkbox
         * @param model the bool model which is displayed/modified by the menu checkbox
         * @return this
         */
        public Menu add(String name, BooleanModel model)
        {
            return add(new MenuCheckbox(name, model));
        }

        /**
         * Adds a {code MenuSpacer} element at the end.  It is equivalent to
         * {code add(new MenuSpacer()) }
         *
         * @return this
         */
        public Menu addSpacer()
        {
            return add(new MenuSpacer());
        }

        /**
         * Creates a menu bar by adding all menu widgets to the specified container.
         *
         * @param container the container for the menu widgets.
         * @see #createMenuBar()
         */
        public void createMenuBar(Widget container)
        {
            MenuManager mm = createMenuManager(container, true);
            foreach (Widget w in createWidgets(mm, 0))
            {
                container.add(w);
            }
        }

        /**
         * Creates a menu bar with a DialogLayout as conatiner. This is the preferred
         * method to create a menu bar.
         *
         * @return the menu bar conatiner
         */
        public Widget createMenuBar()
        {
            DialogLayout l = new DialogLayout();
            setWidgetTheme(l, "menubar");

            MenuManager mm = createMenuManager(l, true);
            Widget[] widgets = createWidgets(mm, 0);

            l.setHorizontalGroup(l.createSequentialGroup().addWidgetsWithGap("menuitem", widgets));
            l.setVerticalGroup(l.createParallelGroup(widgets));

            for (int i = 0, n = elements.Count; i < n; i++)
            {
                MenuElement e = elements[i];

                Alignment alignment = e.getAlignment();
                if (alignment == null)
                {
                    alignment = getClassAlignment(e.GetType());
                }

                l.setWidgetAlignment(widgets[i], alignment);
            }

            l.getHorizontalGroup().addGap();
            return l;
        }

        /**
         * Creates a popup menu from this menu. The popup is positioned to the right of
         * the parent widget.
         *
         * @param parent the parent widget for the popup.
         * @return the MenuManager which manages this popup
         * @see MenuManager#closePopup() 
         */
        public MenuManager openPopupMenu(Widget parent)
        {
            MenuManager mm = createMenuManager(parent, false);
            mm.openSubMenu(0, this, parent, true);
            return mm;
        }

        /**
         * Creates a popup menu from this menu at the specified position.
         *
         * @param parent the parent widget for the popup.
         * @param x the absolute X coordinate for the popup
         * @param y the absolute Y coordinate for the popup
         * @return the MenuManager which manages this popup
         * @see MenuManager#closePopup()
         */
        public MenuManager openPopupMenu(Widget parent, int x, int y)
        {
            MenuManager mm = createMenuManager(parent, false);
            Widget popup = mm.openSubMenu(0, this, parent, false);
            if (popup != null)
            {
                popup.setPosition(x, y);
            }
            return mm;
        }

        //@Override
        protected internal override Widget createMenuWidget(MenuManager mm, int level)
        {
            SubMenuBtn smb = new SubMenuBtn(this, mm, level);
            setWidgetTheme(smb, "submenu");
            return smb;
        }

        protected MenuManager createMenuManager(Widget parent, bool isMenuBar)
        {
            return new MenuManager(parent, isMenuBar);
        }

        protected Widget[] createWidgets(MenuManager mm, int level)
        {
            Widget[] widgets = new Widget[elements.Count];
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                MenuElement e = elements[i];
                widgets[i] = e.createMenuWidget(mm, level);
            }
            return widgets;
        }

        internal DialogLayout createPopup(MenuManager mm, int level, Widget btn)
        {
            if (listeners != null)
            {
                foreach (Listener l in listeners)
                {
                    l.menuOpening(this);
                }
            }

            Widget[] widgets = createWidgets(mm, level);
            MenuPopup popup = new MenuPopup(btn, level, this);
            if (popupTheme != null)
            {
                popup.setTheme(popupTheme);
            }
            popup.setHorizontalGroup(popup.createParallelGroup(widgets));
            popup.setVerticalGroup(popup.createSequentialGroup().addWidgetsWithGap("menuitem", widgets));
            return popup;
        }

        void fireMenuOpened()
        {
            if (listeners != null)
            {
                foreach (Listener l in listeners)
                {
                    l.menuOpened(this);
                }
            }
        }

        void fireMenuClosed()
        {
            if (listeners != null)
            {
                foreach (Listener l in listeners)
                {
                    l.menuClosed(this);
                }
            }
        }

        public IEnumerator<MenuElement> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        internal class MenuPopup : DialogLayout
        {
            private Widget btn;
            private Menu menu;
            internal int level;

            public MenuPopup(Widget btn, int level, Menu menu)
            {
                this.btn = btn;
                this.menu = menu;
                this.level = level;
            }

            //@Override
            protected override void afterAddToGUI(GUI gui)
            {
                base.afterAddToGUI(gui);
                menu.fireMenuOpened();
                btn.getAnimationState().setAnimationState(STATE_HAS_OPEN_MENUS, true);
            }

            //@Override
            protected override void beforeRemoveFromGUI(GUI gui)
            {
                btn.getAnimationState().setAnimationState(STATE_HAS_OPEN_MENUS, false);
                menu.fireMenuClosed();
                base.beforeRemoveFromGUI(gui);
            }

            //@Override
            public override bool handleEvent(Event evt)
            {
                return base.handleEvent(evt) || evt.isMouseEventNoWheel();
            }
        }

        internal class SubMenuBtn : MenuBtn
        {
            private MenuManager mm;
            private int level;
            private Menu menu;

            public SubMenuBtn(Menu menu, MenuManager mm, int level) : base(menu)
            {
                this.menu = menu;
                this.mm = mm;
                this.level = level;

                this.Action += SubMenuBtn_Action;
            }

            public void OpenSubMenu()
            {
                this.mm.openSubMenu(level, this.menu, this, true);
            }

            private void SubMenuBtn_Action(object sender, ButtonActionEventArgs e)
            {
                this.OpenSubMenu();
            }
        }
    }
}
