using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL
{
    public class MenuAction : MenuElement
    {
        private Action _action;

        public MenuAction()
        {
        }

        public MenuAction(String name) : base(name)
        {
            this._action = null;
        }


        /**
         * Creates a menu action which displays the given name and invokes the
         * specified callback when activated.
         * 
         * @param name the name/text of the menu action
         * @param cb the callback to invoke
         * @see #setCallback(java.lang.Runnable) 
         */
        public MenuAction(String name, Action action) : base(name)
        {
            this._action = action;
        }

        protected internal override Widget createMenuWidget(MenuManager mm, int level)
        {
            Button b = new MenuBtn(this);
            setWidgetTheme(b, "button");

            b.Action += (sender, e) => {
                mm.closePopup();
            };

            if (this._action != null)
            {
                b.Action += (sender, e) => {
                    this._action.Invoke();
                };
            }

            return b;
        }
    }
}
