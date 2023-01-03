using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public interface ListBoxDisplay
    {

        bool isSelected();

        void setSelected(bool selected);

        bool isFocused();

        void setFocused(bool focused);

        void setData(Object data);

        void setTooltipContent(Object content);

        Widget getWidget();

        event EventHandler<ListBoxEventArgs> Callback;
    }

    public class ListBoxEventArgs : EventArgs
    {
        private ListBoxCallbackReason _callbackReason;

        public ListBoxCallbackReason Reason
        {
            get
            {
                return this._callbackReason;
            }
        }

        public ListBoxEventArgs(ListBoxCallbackReason callbackReason)
        {
            this._callbackReason = callbackReason;
        }
    }
}
