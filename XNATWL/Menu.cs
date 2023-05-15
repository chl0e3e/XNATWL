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
            void MenuOpening(Menu menu);

            /**
             * Called after a popup has been opened.
             * <p>When a menu is displayed as menu bar then no events are fired.</p>
             * 
             * @param menu the {@code Menu} which has been opened
             * @see #menuOpening(de.matthiasmann.twl.Menu) 
             */
            void MenuOpened(Menu menu);

            /**
             * Called after a popup has been closed.
             * <p>When a menu is displayed as menu bar then no events are fired.</p>
             * 
             * @param menu the {@code Menu} which has been closed
             */
            void MenuClosed(Menu menu);
        }

        private List<MenuElement> _elements = new List<MenuElement>();
        private TypeMapping _classAlignments = new TypeMapping();
        private String _popupTheme;
        private Listener[] _listeners;

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
        public String GetPopupTheme()
        {
            return _popupTheme;
        }

        /**
         * Sets the theme which is used when this menun is displayed as popup/sub menu.
         * @param popupTheme the popup theme
         */
        public void SetPopupTheme(String popupTheme)
        {
            String oldPopupTheme = this._popupTheme;
            this._popupTheme = popupTheme;
            FirePropertyChange("popupTheme", oldPopupTheme, this._popupTheme);
        }

        /**
         * Sets the default alignment based on menu element subclasses.
         * <p>By default all alignments are {@link Alignment#FILL}</p>
         * 
         * @param clazz the class for which a default alignment should be set
         * @param value the alignment
         */
        public void SetClassAlignment(Type clazz, Alignment value)
        {
            if (value == null)
            {
                throw new NullReferenceException("value");
            }
            if (value == Alignment.FILL)
            {
                _classAlignments.RemoveByType(clazz);
            }
            else
            {
                _classAlignments.SetByType(clazz, value);
            }
        }

        /**
         * Retrieves the default alignment for the given menu element class.
         * <p>By default all alignments are {@link Alignment#FILL}</p>
         * 
         * @param clazz the menu element class
         * @return the alignment
         */
        public Alignment GetClassAlignment(Type clazz)
        {
            Alignment alignment = (Alignment) _classAlignments.GetByType(clazz);
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
        public MenuElement Get(int index)
        {
            return _elements[index];
        }

        /**
         * Returns the number of menu elements in this menu.
         * @return the number of menu elements
         */
        public int GetNumElements()
        {
            return _elements.Count;
        }

        /**
         * Removes all menu elements
         */
        public void Clear()
        {
            _elements.Clear();
        }

        /**
         * Adds the given menu element at the end. It is possible to add the same
         * menu element several times also in different menus.
         *
         * @param e the menu element
         * @return this
         */
        public Menu Add(MenuElement e)
        {
            _elements.Add(e);
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
        public Menu Add(String name, Action cb)
        {
            return Add(new MenuAction(name, cb));
        }

        /**
         * Adds a {code MenuCheckbox} element at the end.  It is equivalent to
         * {code add(new MenuCheckbox(name, model)) }
         *
         * @param name the name of the menu checkbox
         * @param model the bool model which is displayed/modified by the menu checkbox
         * @return this
         */
        public Menu Add(String name, BooleanModel model)
        {
            return Add(new MenuCheckbox(name, model));
        }

        /**
         * Adds a {code MenuSpacer} element at the end.  It is equivalent to
         * {code add(new MenuSpacer()) }
         *
         * @return this
         */
        public Menu AddSpacer()
        {
            return Add(new MenuSpacer());
        }

        /**
         * Creates a menu bar by adding all menu widgets to the specified container.
         *
         * @param container the container for the menu widgets.
         * @see #createMenuBar()
         */
        public void CreateMenuBar(Widget container)
        {
            MenuManager mm = CreateMenuManager(container, true);
            foreach (Widget w in CreateWidgets(mm, 0))
            {
                container.Add(w);
            }
        }

        /**
         * Creates a menu bar with a DialogLayout as conatiner. This is the preferred
         * method to create a menu bar.
         *
         * @return the menu bar conatiner
         */
        public Widget CreateMenuBar()
        {
            DialogLayout l = new DialogLayout();
            SetWidgetTheme(l, "menubar");

            MenuManager mm = CreateMenuManager(l, true);
            Widget[] widgets = CreateWidgets(mm, 0);

            l.SetHorizontalGroup(l.CreateSequentialGroup().AddWidgetsWithGap("menuitem", widgets));
            l.SetVerticalGroup(l.CreateParallelGroup(widgets));

            for (int i = 0, n = _elements.Count; i < n; i++)
            {
                MenuElement e = _elements[i];

                Alignment alignment = e.GetAlignment();
                if (alignment == null)
                {
                    alignment = GetClassAlignment(e.GetType());
                }

                l.SetWidgetAlignment(widgets[i], alignment);
            }

            l.GetHorizontalGroup().AddGap();
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
        public MenuManager OpenPopupMenu(Widget parent)
        {
            MenuManager mm = CreateMenuManager(parent, false);
            mm.OpenSubMenu(0, this, parent, true);
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
        public MenuManager OpenPopupMenu(Widget parent, int x, int y)
        {
            MenuManager mm = CreateMenuManager(parent, false);
            Widget popup = mm.OpenSubMenu(0, this, parent, false);
            if (popup != null)
            {
                popup.SetPosition(x, y);
            }
            return mm;
        }

        //@Override
        protected internal override Widget CreateMenuWidget(MenuManager mm, int level)
        {
            SubMenuBtn smb = new SubMenuBtn(this, mm, level);
            SetWidgetTheme(smb, "submenu");
            return smb;
        }

        protected MenuManager CreateMenuManager(Widget parent, bool isMenuBar)
        {
            return new MenuManager(parent, isMenuBar);
        }

        protected Widget[] CreateWidgets(MenuManager mm, int level)
        {
            Widget[] widgets = new Widget[_elements.Count];
            for (int i = 0, n = _elements.Count; i < n; i++)
            {
                MenuElement e = _elements[i];
                widgets[i] = e.CreateMenuWidget(mm, level);
            }
            return widgets;
        }

        internal DialogLayout CreatePopup(MenuManager mm, int level, Widget btn)
        {
            if (_listeners != null)
            {
                foreach (Listener l in _listeners)
                {
                    l.MenuOpening(this);
                }
            }

            Widget[] widgets = CreateWidgets(mm, level);
            MenuPopup popup = new MenuPopup(btn, level, this);
            if (_popupTheme != null)
            {
                popup.SetTheme(_popupTheme);
            }
            popup.SetHorizontalGroup(popup.CreateParallelGroup(widgets));
            popup.SetVerticalGroup(popup.CreateSequentialGroup().AddWidgetsWithGap("menuitem", widgets));
            return popup;
        }

        void FireMenuOpened()
        {
            if (_listeners != null)
            {
                foreach (Listener l in _listeners)
                {
                    l.MenuOpened(this);
                }
            }
        }

        void FireMenuClosed()
        {
            if (_listeners != null)
            {
                foreach (Listener l in _listeners)
                {
                    l.MenuClosed(this);
                }
            }
        }

        public IEnumerator<MenuElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        internal class MenuPopup : DialogLayout
        {
            private Widget _btn;
            private Menu _menu;
            internal int _level;

            public MenuPopup(Widget btn, int level, Menu menu)
            {
                this._btn = btn;
                this._menu = menu;
                this._level = level;
            }

            //@Override
            protected override void AfterAddToGUI(GUI gui)
            {
                base.AfterAddToGUI(gui);
                _menu.FireMenuOpened();
                _btn.GetAnimationState().SetAnimationState(STATE_HAS_OPEN_MENUS, true);
            }

            //@Override
            protected override void BeforeRemoveFromGUI(GUI gui)
            {
                _btn.GetAnimationState().SetAnimationState(STATE_HAS_OPEN_MENUS, false);
                _menu.FireMenuClosed();
                base.BeforeRemoveFromGUI(gui);
            }

            //@Override
            public override bool HandleEvent(Event evt)
            {
                return base.HandleEvent(evt) || evt.IsMouseEventNoWheel();
            }
        }

        internal class SubMenuBtn : MenuBtn
        {
            private MenuManager _mm;
            private int _level;
            private Menu _menu;

            public SubMenuBtn(Menu menu, MenuManager mm, int level) : base(menu)
            {
                this._menu = menu;
                this._mm = mm;
                this._level = level;

                this.Action += SubMenuBtn_Action;
            }

            public void OpenSubMenu()
            {
                this._mm.OpenSubMenu(_level, this._menu, this, true);
            }

            private void SubMenuBtn_Action(object sender, ButtonActionEventArgs e)
            {
                this.OpenSubMenu();
            }
        }
    }
}
