using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Test
{
    public class StyleItem
    {
        public String theme;
        public String name;

        public StyleItem(String theme, String name)
        {
            this.theme = theme;
            this.name = name;
        }

        public override String ToString()
        {
            return name;
        }
    }
}
