using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL
{
    public class ActionCallback : Runnable
    {
        private Widget widget;
        private ActionMap actionMap;
        private String action;

        /**
         * Creates a callback invoking an action on the widget's actionMap.
         * If the widget has no actionMap then no action is performed.
         * 
         * @param widget the widget
         * @param action the action
         * @throws NullPointerException if either widget or action is null
         * @see Widget#getActionMap() 
         */
        public ActionCallback(Widget widget, String action)
        {
            if (widget == null)
            {
                throw new NullReferenceException("widget");
            }
            if (action == null)
            {
                throw new NullReferenceException("action");
            }
            this.widget = widget;
            this.actionMap = null;
            this.action = action;
        }

        /**
         * Creates a callback invoking an action on actionMap.
         * 
         * @param actionMap the actionMap to use
         * @param action the action
         * @throws NullPointerException if either actionMap or action is null
         */
        public ActionCallback(ActionMap actionMap, String action)
        {
            if (actionMap == null)
            {
                throw new NullReferenceException("actionMap");
            }
            if (action == null)
            {
                throw new NullReferenceException("action");
            }
            this.widget = null;
            this.actionMap = actionMap;
            this.action = action;
        }

        public void run()
        {
            ActionMap am = actionMap;
            if (am == null)
            {
                am = widget.getActionMap();
                if (am == null)
                {
                    return;
                }
            }
            am.invokeDirect(action);
        }
    }

}
