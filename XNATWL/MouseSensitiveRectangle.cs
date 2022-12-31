using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public abstract class MouseSensitiveRectangle : Rect
    {
        public MouseSensitiveRectangle()
        {
        }

        /**
         * Test if the mouse is over this area
         * @return true if the mouse is over this area.
         */
        public abstract bool isMouseOver();
    }
}
