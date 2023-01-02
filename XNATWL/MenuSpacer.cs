using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class MenuSpacer : MenuElement
    {
        protected internal override Widget createMenuWidget(MenuManager mm, int level)
        {
            Widget w = new Widget();
            setWidgetTheme(w, "spacer");
            return w;
        }
    }
}
