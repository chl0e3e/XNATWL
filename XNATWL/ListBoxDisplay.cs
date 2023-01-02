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

        //void addListBoxCallback(CallbackWithReason<ListBox.CallbackReason> cb);

        //void removeListBoxCallback(CallbackWithReason<ListBox.CallbackReason> cb);

    }

}
