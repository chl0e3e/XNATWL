using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class Dimension
    {
        public static Dimension ZERO = new Dimension(0, 0);

        private int x;
        private int y;

        public Dimension(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Dimension))
            {
                return false;
            }

            Dimension other = (Dimension)obj;

            return (this.x == other.x) && (this.y == other.y);
        }

        public override int GetHashCode()
        {
            int hash = 3;
            hash = 71 * hash + this.x;
            hash = 71 * hash + this.y;
            return hash;
        }

        public override string ToString()
        {
            return "Dimension[x=" + x + ", y=" + y + "]";
        }
    }
}
