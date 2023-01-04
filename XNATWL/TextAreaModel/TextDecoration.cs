using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextAreaModel
{
    public enum eTextDecoration
    {
        NONE,
        UNDERLINE,
        LINE_THROUGH
    }

    public class TextDecoration
    {
        public static TextDecoration NONE = new TextDecoration(eTextDecoration.NONE);
        public static TextDecoration UNDERLINE = new TextDecoration(eTextDecoration.UNDERLINE);
        public static TextDecoration LINE_THROUGH = new TextDecoration(eTextDecoration.LINE_THROUGH);

        public eTextDecoration? Value;
        public TextDecoration(eTextDecoration value)
        {
            this.Value = value;
        }
    }
}
