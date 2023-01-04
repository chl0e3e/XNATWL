using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextAreaModel
{
    public class BoxAttribute
    {
        public StyleAttribute<Value> Top;
        public StyleAttribute<Value> Left;
        public StyleAttribute<Value> Right;
        public StyleAttribute<Value> Bottom;

        public BoxAttribute(StyleAttribute<Value> top, StyleAttribute<Value> left, StyleAttribute<Value> right, StyleAttribute<Value> bottom)
        {
            this.Top = top;
            this.Left = left;
            this.Right = right;
            this.Bottom = bottom;
        }
    }
}
