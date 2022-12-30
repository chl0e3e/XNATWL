using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Theme
{
    public class ThemeChildImpl
    {
        internal ThemeManager manager;
        internal ThemeInfoImpl parent;

        internal ThemeChildImpl(ThemeManager manager, ThemeInfoImpl parent)
        {
            this.manager = manager;
            this.parent = parent;
        }

        protected string getParentDescription()
        {
            if (parent != null)
            {
                return ", defined in " + parent.getThemePath();
            }
            else
            {
                return "";
            }
        }
    }
}
