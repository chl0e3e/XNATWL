using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class Border
    {
        public static Border ZERO = new Border(0);

        private int top;
        private int left;
        private int bottom;
        private int right;

        public Border(int all)
        {
            this.top = all;
            this.left = all;
            this.bottom = all;
            this.right = all;
        }

        public Border(Utils.Number all)
        {
            this.top = all.intValue();
            this.left = all.intValue();
            this.bottom = all.intValue();
            this.right = all.intValue();
        }

        public Border(int horz, int vert)
        {
            this.top = vert;
            this.left = horz;
            this.bottom = vert;
            this.right = horz;
        }

        public Border(int top, int left, int bottom, int right)
        {
            this.top = top;
            this.left = left;
            this.bottom = bottom;
            this.right = right;
        }

        public int BorderBottom
        {
            get
            {
                return bottom;
            }
        }

        public int BorderLeft
        {
            get
            {
                return left;
            }
        }

        public int BorderRight
        {
            get
            {
                return right;
            }
        }

        public int BorderTop
        {
            get
            {
                return top;
            }
        }

        public int Bottom
        {
            get
            {
                return bottom;
            }
        }

        public int Left
        {
            get
            {
                return left;
            }
        }

        public int Right
        {
            get
            {
                return right;
            }
        }

        public int Top
        {
            get
            {
                return top;
            }
        }

        public override string ToString()
        {
            return "[Border top=" + top + " left=" + left + " bottom=" + bottom + " right=" + right + "]";
        }
    }
}
