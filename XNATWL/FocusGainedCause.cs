using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public enum FocusGainedCause
    {
        /**
         * Focus transfer caused by focus key (eg TAB)
         */
        FOCUS_KEY,
        /**
         * Focus transfer caused by mouse down event on the widget
         */
        MOUSE_BTNDOWN,
        /**
         * A child widget requested focus
         */
        CHILD_FOCUSED,
        /**
         * {@link Widget#requestKeyboardFocus() } was invoked
         */
        MANUAL,

        NONE
    }
}
